"""Exercise current workflow syntax and retained embedded-code checks in CI."""

from pathlib import Path
import subprocess
import tempfile

IMAGE = "data-doublets-workflow-lint:local"
SOURCE = """on: push
concurrency:
  group: schema-regression
  queue: max
  cancel-in-progress: false
jobs:
  audit:
    uses: $/.github/workflows/callee.yml
"""
CALLEE = """on: workflow_call
jobs:
  audit:
    runs-on: ubuntu-latest
    steps:
      - run: echo checked
"""


def main():
    with tempfile.TemporaryDirectory() as directory:
        root = Path(directory)
        (root / ".git").mkdir()
        workflows = root / ".github/workflows"
        workflows.mkdir(parents=True)
        (workflows / "callee.yml").write_text(CALLEE)
        for name, source, expected in (
            ("current syntax", SOURCE, None),
            (
                "queue conflict",
                SOURCE.replace("cancel-in-progress: false", "cancel-in-progress: true"),
                "queue",
            ),
            ("missing callee", SOURCE.replace("callee.yml", "missing.yml"), "missing.yml"),
            ("embedded shell", CALLEE.replace("echo checked", "echo $UNTRUSTED_VALUE"), "SC2086"),
            (
                "embedded Python",
                CALLEE.replace(
                    "- run: echo checked", "- shell: python\n        run: print(undefined_name)"
                ),
                "undefined name",
            ),
        ):
            (workflows / "ci.yml").write_text(source)
            result = subprocess.run(
                [
                    "docker",
                    "run",
                    "--rm",
                    "-v",
                    f"{root}:/work",
                    "-w",
                    "/work",
                    IMAGE,
                    "-no-color",
                    ".github/workflows/ci.yml",
                ],
                capture_output=True,
                text=True,
            )
            output = result.stdout + result.stderr
            if expected is None:
                assert result.returncode == 0, output
            else:
                assert result.returncode != 0 and expected in output, output
            print(f"PASS {name}: exit {result.returncode}")


if __name__ == "__main__":
    main()
