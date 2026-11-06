"""Refresh all Python transitive pins from a clean, upgraded resolution."""

import json
from pathlib import Path
import subprocess
import sys
import tempfile

root = Path(__file__).resolve().parents[2]
with tempfile.TemporaryDirectory() as directory:
    inputs = []
    for source in sorted((root / "scripts").glob("requirements-*.txt")):
        path = Path(directory) / source.name
        path.write_text(
            "\n".join(
                line for line in source.read_text().splitlines() if not line.startswith("-c ")
            )
            + "\n"
        )
        inputs.extend(["-r", str(path)])
    report = Path(directory) / "resolution.json"
    subprocess.run(
        [
            sys.executable,
            "-m",
            "pip",
            "install",
            "--dry-run",
            "--ignore-installed",
            "--upgrade",
            "--report",
            str(report),
            *inputs,
        ],
        check=True,
    )
    pins = sorted(
        f"{row['metadata']['name']}=={row['metadata']['version']}"
        for row in json.loads(report.read_text())["install"]
    )
    (root / "scripts/requirements.lock.txt").write_text(
        "# Full Python 3.15 resolution. Regenerate with experiments/issue-556/lock_python.py.\n"
        + "\n".join(pins)
        + "\n"
    )
