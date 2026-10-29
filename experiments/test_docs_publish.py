"""Publish into local bare Git repositories, without network or real tokens."""
from pathlib import Path
import os
import subprocess
import shutil
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]


def git(root, *args):
    return subprocess.check_output(['git', '-C', str(root), *args], text=True, stderr=subprocess.STDOUT).strip()


class DocsPublishTests(unittest.TestCase):
    def run_publish(self, initialized, race=False):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            remote = root / 'remote.git'
            subprocess.run(['git', 'init', '--bare', '-q', str(remote)], check=True)
            if initialized:
                seed = root / 'seed'
                subprocess.run(['git', 'clone', '-q', str(remote), str(seed)], check=True, capture_output=True)
                git(seed, 'checkout', '-b', 'gh-pages')
                git(seed, 'config', 'user.name', 'Test')
                git(seed, 'config', 'user.email', 'test@example.invalid')
                (seed / 'rust').mkdir()
                (seed / 'rust/index.html').write_text('Preserve this existing API')
                git(seed, 'add', '.')
                git(seed, 'commit', '-m', 'Existing Rust documentation')
                git(seed, 'push', 'origin', 'gh-pages')
            for name in ('csharp/index.html', 'cpp/index.html', 'csharp/Platform.Data.Doublets.pdf'):
                path = root / '_site' / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text('New generated documentation')
            tools = root / 'tools'
            tools.mkdir()
            gh = tools / 'gh'
            gh.write_text('#!/bin/sh\nexit 0\n')
            gh.chmod(0o755)
            if race:
                wrapper = tools / 'git'
                wrapper.write_text(f"""#!{sys.executable}
import os, subprocess, sys
from pathlib import Path
real = {shutil.which('git')!r}
seed = Path({str(root / 'seed')!r})
marker = Path({str(root / 'race-injected')!r})
if 'push' in sys.argv and not marker.exists():
    marker.touch()
    (seed / 'rust/new.html').write_text('Concurrent documentation')
    for args in (['add', '.'], ['commit', '-m', 'Concurrent publisher'], ['push', 'origin', 'gh-pages']):
        subprocess.run([real, '-C', str(seed), *args], check=True)
os.execv(real, [real, *sys.argv[1:]])
""")
                wrapper.chmod(0o755)
            env = {**os.environ, 'GH_TOKEN': 'test-placeholder', 'GITHUB_REPOSITORY': 'example/fixture',
                   'GIT_CONFIG_COUNT': '1', 'GIT_CONFIG_KEY_0': f'url.{remote.as_uri()}.insteadOf',
                   'GIT_CONFIG_VALUE_0': 'https://github.com/example/fixture.git',
                   'PATH': str(tools) + os.pathsep + os.environ['PATH']}
            for attempt in range(2):
                result = subprocess.run(['bash', str(ROOT / 'scripts/publish-docs.sh')], cwd=root, env=env, capture_output=True, text=True)
                self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
            self.assertEqual(git(remote, 'show', 'gh-pages:cpp/index.html'), 'New generated documentation')
            self.assertEqual(git(remote, 'rev-list', '--count', 'gh-pages'), '3' if race else '2' if initialized else '1')
            if race:
                self.assertEqual(git(remote, 'show', 'gh-pages:rust/new.html'), 'Concurrent documentation')
            if initialized:
                self.assertEqual(git(remote, 'show', 'gh-pages:rust/index.html'), 'Preserve this existing API')

    def test_first_deployment_creates_missing_branch_and_is_idempotent(self):
        self.run_publish(False)

    def test_concurrent_deployment_reapplies_only_our_pages(self):
        self.run_publish(True, race=True)

    def test_repeated_deployment_preserves_other_language_docs(self):
        self.run_publish(True)


if __name__ == '__main__':
    unittest.main()
