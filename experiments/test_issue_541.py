"""Minimal regressions for incomplete scans and unverified release writes."""

import contextlib
import importlib.util
import io
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch
import stat
import re
from zipfile import ZipFile, ZipInfo
import yaml

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
import check_sarif
import versions
import changes
from extract_artifact import extract


class ArtifactTests(unittest.TestCase):
    def test_raw_zip_without_extension_preserves_package_and_nested_files(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / 'download'
            source.mkdir()
            with ZipFile(source / 'artifact', 'w') as archive:
                archive.writestr('packages/library.nupkg', b'PK\x00\xff')
                archive.writestr('test-results/result.trx', '<TestRun />')
            extract(source, root / 'output')
            self.assertEqual((root / 'output/packages/library.nupkg').read_bytes(), b'PK\x00\xff')
            self.assertEqual((root / 'output/test-results/result.trx').read_text(), '<TestRun />')

    def test_rejects_traversal_absolute_paths_and_symlinks_before_writing(self):
        for name in ('../escape', '/escape', 'C:/escape', 'dir\\escape', 'link'):
            with self.subTest(name=name), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                source = root / 'download'
                source.mkdir()
                with ZipFile(source / 'artifact', 'w') as archive:
                    archive.writestr('valid', 'must not be partially written')
                    entry = ZipInfo(name)
                    if name == 'link':
                        entry.external_attr = (stat.S_IFLNK | 0o777) << 16
                    archive.writestr(entry, '../outside')
                with self.assertRaisesRegex(ValueError, 'Unsafe'):
                    extract(source, root / 'output')
                self.assertFalse((root / 'output').exists())

    def test_existing_symlink_cannot_redirect_extraction(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / 'download'
            source.mkdir()
            (root / 'output').mkdir()
            (root / 'outside').mkdir()
            (root / 'output/link').symlink_to(root / 'outside', target_is_directory=True)
            with ZipFile(source / 'artifact', 'w') as archive:
                archive.writestr('link/escape', 'unsafe')
            with self.assertRaisesRegex(ValueError, 'Unsafe'):
                extract(source, root / 'output')

    def test_ambiguous_download_fails(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for count in range(3):
                if count:
                    (root / str(count)).touch()
                if count != 1:
                    with self.assertRaisesRegex(ValueError, 'exactly one'):
                        extract(root, root / 'output')


class PythonRoutingTests(unittest.TestCase):
    def test_python_tool_configuration_runs_pipeline_validation(self):
        self.assertTrue(changes.categories(['pyproject.toml'])['pipeline'])


class PreCommitTests(unittest.TestCase):
    def test_existing_source_files_reach_their_formatting_hooks(self):
        config = yaml.safe_load((ROOT / '.pre-commit-config.yaml').read_text())
        hooks = {hook['id']: hook for hook in config['repos'][0]['hooks']}
        for hook, paths in {
            'native-format': ['cpp/Platform.Data.Doublets/Doublets.h', 'c/ffi.h', 'examples/CppMemoryLinks.cpp'],
            'csharp-format': ['csharp/Platform.Data.Doublets/Link.cs'],
            'python-quality': ['scripts/release.py', 'pyproject.toml'],
        }.items():
            for path in paths:
                with self.subTest(hook=hook, path=path):
                    self.assertIsNotNone(re.search(hooks[hook]['files'], path))


class IncompleteSarifTests(unittest.TestCase):
    def test_missing_or_unknown_invocation_is_not_success(self):
        for invocations in (None, [], [{}], [{'executionSuccessful': None}]):
            with self.subTest(invocations=invocations), tempfile.TemporaryDirectory() as directory:
                run = {'tool': {'driver': {'name': 'CodeQL'}}, 'results': []}
                if invocations is not None:
                    run['invocations'] = invocations
                Path(directory, 'report.sarif').write_text(json.dumps({'version': '2.1.0', 'runs': [run]}))
                with self.assertRaisesRegex(ValueError, 'Incomplete|failed'):
                    check_sarif.check(directory)

    def test_malformed_collections_are_not_clean_reports(self):
        for results in ({}, '', None):
            with self.subTest(results=results), tempfile.TemporaryDirectory() as directory:
                run = {'tool': {'driver': {'name': 'CodeQL'}}, 'results': results,
                       'invocations': [{'executionSuccessful': True}]}
                Path(directory, 'report.sarif').write_text(json.dumps({'version': '2.1.0', 'runs': [run]}))
                with self.assertRaisesRegex(ValueError, 'Incomplete'):
                    check_sarif.check(directory)


class WorkflowToolTests(unittest.TestCase):
    def test_linter_cannot_silently_skip_embedded_shell_checks(self):
        spec = importlib.util.spec_from_file_location('workflow_checker_541', ROOT / 'scripts/check-workflows.py')
        checker = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(checker)
        with patch.object(checker, 'which', return_value=None):
            with self.assertRaisesRegex(RuntimeError, 'shellcheck'):
                checker.check_tools()


class ProtectedReleaseTests(unittest.TestCase):
    def test_rule_rejection_fails_once_without_rebase_or_force(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            remote = root / 'remote.git'
            work = root / 'work'
            subprocess.run(['git', 'init', '--bare', '-q', str(remote)], check=True)
            subprocess.run(['git', 'clone', '-q', str(remote), str(work)], check=True, capture_output=True)
            def git(*args):
                return subprocess.check_output(['git', '-C', str(work), *args], text=True, stderr=subprocess.STDOUT).strip()
            git('checkout', '-b', 'main')
            git('config', 'user.name', 'Test')
            git('config', 'user.email', 'test@example.invalid')
            project = work / versions.FILES['csharp']
            project.parent.mkdir(parents=True)
            project.write_text('<VersionPrefix>1.0.0</VersionPrefix>')
            git('add', '.')
            git('commit', '-m', 'Validated source')
            git('push', '-u', 'origin', 'main')
            original = git('rev-parse', 'HEAD')
            hook = remote / 'hooks/pre-receive'
            hook.write_text('#!/bin/sh\ncat >/dev/null\necho attempt >>"' + str(root / 'attempts') + '"\necho "error: GH006: Protected branch update failed" >&2\nexit 1\n')
            hook.chmod(0o755)
            project.write_text('<VersionPrefix>1.0.1</VersionPrefix>')
            with patch.object(versions, 'ROOT', work), contextlib.redirect_stdout(io.StringIO()) as output:
                with self.assertRaisesRegex(RuntimeError, 'Repository rules'):
                    versions.commit()
            self.assertEqual((root / 'attempts').read_text().splitlines(), ['attempt'])
            self.assertEqual(git('rev-parse', 'origin/main'), original)
            self.assertIn('GH006', output.getvalue())
            self.assertNotEqual(git('rev-parse', 'HEAD'), original)


if __name__ == '__main__':
    unittest.main()
