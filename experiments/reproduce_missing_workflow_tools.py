"""Show why extracting only actionlint loses embedded shell/Python checks."""

import importlib.util
import os
from pathlib import Path
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("checker", ROOT / "scripts/check-workflows.py")
checker = importlib.util.module_from_spec(spec)
spec.loader.exec_module(checker)

fixture = """name: Probe
on: push
jobs:
  probe:
    runs-on: ubuntu-24.04
    steps:
      - run: echo $UNTRUSTED_VALUE
"""

with tempfile.TemporaryDirectory() as directory:
    root = Path(directory)
    root.chmod(0o755)  # The image runs as an unprivileged user.
    fixture_path = root / "probe.yml"
    fixture_path.write_text(fixture)
    container = subprocess.check_output(["docker", "create", checker.IMAGE], text=True).strip()
    try:
        subprocess.run(
            ["docker", "cp", f"{container}:/usr/local/bin/actionlint", str(root / "actionlint")],
            check=True,
        )
    finally:
        subprocess.run(["docker", "rm", container], check=True, stdout=subprocess.DEVNULL)
    missing = subprocess.run(
        [str(root / "actionlint"), "-no-color", str(fixture_path)],
        env={**os.environ, "PATH": str(root)},
        capture_output=True,
        text=True,
    )
    complete = subprocess.run(
        [
            "docker",
            "run",
            "--rm",
            "-v",
            f"{root}:/probe",
            checker.IMAGE,
            "-no-color",
            "/probe/probe.yml",
        ],
        capture_output=True,
        text=True,
    )
    print(
        f"Extracted binary, tools absent: exit {missing.returncode}\n{missing.stdout}{missing.stderr}"
    )
    print(
        f"Complete pinned container: exit {complete.returncode}\n{complete.stdout}{complete.stderr}"
    )
    assert missing.returncode == 0, "Expected the old silent skip"
    assert complete.returncode != 0 and "SC2086" in complete.stdout, (
        "Expected embedded shell finding"
    )
