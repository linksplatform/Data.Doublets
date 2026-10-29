"""Reproductions of issue 524's pipeline failures; run with unittest discover."""

import os
from pathlib import Path
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]


class CppRunnerTests(unittest.TestCase):
    def test_failed_build_cannot_be_hidden_by_stale_passing_binary(self):
        with tempfile.TemporaryDirectory() as directory:
            work = Path(directory)
            tools = work / "tools"
            tools.mkdir()
            for name, body in {
                "conan": "exit 0",
                "cmake": "exit 42",
                "scan-build": "exit 0",
                "ctest": "exit 0",
            }.items():
                path = tools / name
                path.write_text("#!/bin/bash\n" + body + "\n")
                path.chmod(0o755)
            (work / "build/bin").mkdir(parents=True)
            stale = work / "build/bin/stale-test"
            stale.write_text("#!/bin/bash\nexit 0\n")
            stale.chmod(0o755)
            result = subprocess.run(
                ["bash", str(ROOT / "cpp/run_tests.sh")], cwd=work,
                env={**os.environ, "PATH": str(tools) + os.pathsep + os.environ["PATH"]},
                capture_output=True, text=True, check=False,
            )
            self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)


class PipelineHelpersTests(unittest.TestCase):
    def test_release_credentials_are_not_required_on_pull_requests(self):
        result = subprocess.run(
            ["python3", str(ROOT / "scripts/release.py"), "preflight", "--mode", "report"],
            env={k: v for k, v in os.environ.items() if k not in ("NUGET_TOKEN", "GH_TOKEN")},
            capture_output=True, text=True, check=False,
        )
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("NUGET_TOKEN", result.stdout)

    def test_missing_release_credentials_fail_with_actionable_message(self):
        result = subprocess.run(
            ["python3", str(ROOT / "scripts/release.py"), "preflight", "--mode", "release"],
            env={k: v for k, v in os.environ.items() if k not in ("NUGET_TOKEN", "GH_TOKEN")},
            capture_output=True, text=True, check=False,
        )
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("NUGET_TOKEN", result.stdout + result.stderr)
        self.assertIn("rotate", (result.stdout + result.stderr).lower())


if __name__ == "__main__":
    unittest.main()
