"""Bounded regressions for routing, release races, fail-closed APIs and gates."""
import io
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch, Mock

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
import changes
import check_required
import release
import versions


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class RoutingTests(unittest.TestCase):
    def test_evidence_and_experiments_do_not_trigger_releases(self):
        self.assertFalse(any(changes.categories(['dev/log/issues/524/report.md', 'experiments/test_probe.py', 'changes/fix.json']).values()))

    def test_native_configuration_runs_native_validation(self):
        self.assertTrue(changes.categories(['.clang-tidy'])['cpp'])

    def test_deleted_files_are_included_from_the_actual_push_base(self):
        with patch.object(changes, 'git', return_value=b'cpp/deleted.h\0csharp/deleted.cs\0') as git:
            self.assertEqual(changes.changed_paths({'before': 'base-sha'}, 'push'), ['cpp/deleted.h', 'csharp/deleted.cs'])
            git.assert_called_once_with('diff', '--no-renames', '--name-only', '-z', 'base-sha', 'HEAD')

    def test_pull_request_base_is_not_last_commit(self):
        with patch.object(changes, 'git', return_value=b'csharp/old-change.cs\0') as git:
            changes.changed_paths({'pull_request': {'base': {'sha': 'actual-base'}}}, 'pull_request')
            self.assertIn('actual-base', git.call_args.args)


class GateTests(unittest.TestCase):
    def jobs(self):
        result = {name: {'result': 'skipped'} for name in ('changes', 'quality', 'credentials', 'csharp', 'cpp', 'docs', 'publish', 'security', 'links')}
        result['changes'] = {'result': 'success', 'outputs': {'cpp': 'true'}}
        result['quality']['result'] = 'success'
        result['security']['result'] = 'success'
        result['links']['result'] = 'success'
        return result

    def test_applicable_skipped_validation_fails(self):
        with self.assertRaisesRegex(ValueError, 'cpp'):
            check_required.validate(self.jobs())

    def test_pull_requests_allow_skipped_publication(self):
        jobs = self.jobs()
        jobs['cpp']['result'] = 'success'
        jobs['docs']['result'] = 'success'
        check_required.validate(jobs)

    def test_main_requires_credentials_and_publication(self):
        jobs = self.jobs()
        jobs['cpp']['result'] = 'success'
        jobs['docs']['result'] = 'success'
        with self.assertRaisesRegex(ValueError, 'credentials.*publish'):
            check_required.validate(jobs, main=True)

    def test_security_and_links_cannot_silently_skip(self):
        for name in ('security', 'links'):
            jobs = self.jobs()
            jobs['cpp']['result'] = 'success'
            jobs['docs']['result'] = 'success'
            jobs[name]['result'] = 'skipped'
            with self.assertRaisesRegex(ValueError, name):
                check_required.validate(jobs)

    def test_cancelled_tests_never_pass(self):
        jobs = self.jobs()
        jobs['cpp']['result'] = 'cancelled'
        with self.assertRaisesRegex(ValueError, 'cpp'):
            check_required.validate(jobs)


class WorkflowSchemaTests(unittest.TestCase):
    def test_queue_policy(self):
        checker = load('workflow_checker', ROOT / 'scripts/check-workflows.py')
        checker.validate_queue({'jobs': {'publish': {'concurrency': {'queue': 'max', 'cancel-in-progress': False}}}})
        for invalid in ({'queue': 'max', 'cancel-in-progress': True}, {'queue': 'all'}):
            with self.assertRaises(ValueError):
                checker.validate_queue({'concurrency': invalid})


class VersionTests(unittest.TestCase):
    def test_prepare_is_idempotent_and_uses_largest_bump(self):
        with tempfile.TemporaryDirectory() as directory, patch.object(versions, 'ROOT', Path(directory)):
            root = Path(directory)
            for language, filename in versions.FILES.items():
                path = root / filename
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text('<VersionPrefix>1.2.3</VersionPrefix>' if language == 'csharp' else '<version>1.2.3</version>')
            (root / 'cpp/CMakeLists.txt').write_text('project(Platform.Data.Doublets VERSION 1.2.3)')
            (root / 'changes').mkdir()
            for i, kind in enumerate(('patch', 'minor')):
                (root / f'changes/{i}.json').write_text(json.dumps({'summary': f'Change {i}', 'releases': {'csharp': kind, 'cpp': kind}}))
            versions.prepare()
            first = {str(p): p.read_bytes() for p in root.rglob('*') if p.is_file()}
            versions.prepare()
            self.assertEqual(first, {str(p): p.read_bytes() for p in root.rglob('*') if p.is_file()})
            self.assertIn('1.3.0', (root / versions.FILES['csharp']).read_text())
            self.assertEqual(list((root / 'changes').glob('*.json')), [])

    def test_release_cannot_commit_pre_staged_source(self):
        with tempfile.TemporaryDirectory() as directory, patch.object(versions, 'ROOT', Path(directory)):
            root = Path(directory)
            subprocess.run(['git', 'init', '-q', str(root)], check=True)
            (root / 'unexpected.py').write_text('print("unvalidated")')
            subprocess.run(['git', '-C', str(root), 'add', 'unexpected.py'], check=True)
            with self.assertRaisesRegex(ValueError, 'allowlist'):
                versions.commit()


class PublicationTests(unittest.TestCase):
    def test_unknown_nuget_response_fails_closed(self):
        with patch.object(release, 'request', return_value=(503, b'')):
            self.assertIn('503', release.verify_nuget('never-print-me', {'id': 'Example'}))

    def test_malformed_github_permission_response_fails_cleanly(self):
        with patch.dict(os.environ, {'NUGET_TOKEN': 'never-print-me', 'GH_TOKEN': 'also-private'}), patch.object(release, 'verify_nuget', return_value=None), patch.object(release, 'request', return_value=(200, b'not json')):
            with patch('sys.stdout', new_callable=io.StringIO), self.assertRaisesRegex(RuntimeError, 'preflight'):
                release.preflight('release', ['csharp'])

    def test_indexing_retry_is_bounded_and_anonymous(self):
        with patch.object(release, 'request', return_value=(404, b'')) as request, patch.object(release.time, 'sleep') as sleep:
            with self.assertRaisesRegex(RuntimeError, '15 minutes'):
                release.wait_for_package({'id': 'Example', 'version': '1.0.0'}, attempts=3, interval=0)
            self.assertEqual(request.call_count, 3)
            self.assertEqual(sleep.call_count, 2)
            self.assertFalse(request.call_args.kwargs)

    def test_unknown_release_lookup_cannot_publish(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            (output / 'Example.1.0.0.nupkg').write_bytes(b'PK')
            with patch.object(release, 'info', return_value={'id': 'Example', 'version': '1.0.0', 'notes': ''}), patch.object(release, 'request', return_value=(503, b'')), patch.dict(os.environ, {'GH_TOKEN': 'private'}), patch.object(release.subprocess, 'run') as run:
                with self.assertRaisesRegex(RuntimeError, 'unknown'):
                    release.publish('cpp', output)
                run.assert_not_called()

    def test_mirror_failure_does_not_prevent_release_asset_repair(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            artifact = output / 'Example.1.0.0.nupkg'
            artifact.write_bytes(b'PK')
            public = json.dumps({'draft': False, 'assets': [{'name': artifact.name, 'size': 2}]}).encode()
            def run(command, **kwargs):
                return Mock(returncode=1 if 'https://nuget.pkg.github.com/linksplatform/index.json' in command else 0)
            with patch.object(release, 'info', return_value={'id': 'Example', 'version': '1.0.0', 'notes': ''}), patch.object(release, 'request', side_effect=[(200, b''), (200, public)]), patch.dict(os.environ, {'GH_TOKEN': 'private', 'NUGET_TOKEN': 'private'}), patch.object(release, 'wait_for_package'), patch.object(release.subprocess, 'run', side_effect=run) as commands:
                with self.assertRaisesRegex(RuntimeError, 'Package push'):
                    release.publish('cpp', output)
                self.assertTrue(any(c.args[0][:3] == ['gh', 'release', 'upload'] for c in commands.call_args_list))
                self.assertTrue(any(c.args[0][:3] == ['gh', 'release', 'edit'] for c in commands.call_args_list))


if __name__ == '__main__':
    unittest.main()
