"""Reproduce two writers publishing identical validated release metadata."""
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch
import sys
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
import versions


def git(root, *args):
    return subprocess.check_output(['git', '-C', str(root), *args], text=True, stderr=subprocess.STDOUT).strip()


class ReleaseRaceTests(unittest.TestCase):
    def test_identical_remote_release_is_repaired_without_a_second_bump(self):
        with tempfile.TemporaryDirectory() as directory:
            directory = Path(directory)
            remote, first, second = [directory / name for name in ('remote.git', 'first', 'second')]
            subprocess.run(['git', 'init', '--bare', '-q', str(remote)], check=True)
            subprocess.run(['git', 'clone', '-q', str(remote), str(first)], check=True, capture_output=True)
            git(first, 'checkout', '-b', 'main')
            git(first, 'config', 'user.name', 'Test')
            git(first, 'config', 'user.email', 'test@example.invalid')
            filename = versions.FILES['csharp']
            path = first / filename
            path.parent.mkdir(parents=True)
            path.write_text('<VersionPrefix>1.0.0</VersionPrefix>')
            git(first, 'add', '.')
            git(first, 'commit', '-m', 'Source validated by both writers')
            git(first, 'push', '-u', 'origin', 'main')
            subprocess.run(['git', 'clone', '-q', '-b', 'main', str(remote), str(second)], check=True)
            path.write_text('<VersionPrefix>1.0.1</VersionPrefix>')
            git(first, 'commit', '-am', 'First writer release')
            git(first, 'push', 'origin', 'main')
            (second / filename).write_text(path.read_text())
            with patch.object(versions, 'ROOT', second):
                versions.commit()
            self.assertEqual(git(second, 'rev-parse', 'HEAD'), git(first, 'rev-parse', 'HEAD'))
            self.assertEqual((second / filename).read_text(), path.read_text())


if __name__ == '__main__':
    unittest.main()
