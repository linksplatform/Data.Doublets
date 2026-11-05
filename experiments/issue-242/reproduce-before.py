"""Run focused regressions on pinned main sources, then restore the fixed source."""
from pathlib import Path
from shutil import which
import subprocess  # nosec B404
import sys

ROOT = Path(__file__).resolve().parents[2]
SOURCE = "csharp/Platform.Data.Doublets/ILinksExtensions.cs"
BASELINE = "68d306f40"
git = which("git")
if git is None or len(sys.argv) < 2:
    raise SystemExit("Usage: python3 reproduce-before.py <test command> [arguments...]; git is required")
saved = (ROOT / SOURCE).read_bytes()
try:
    # Fixed Git executable, revision and path; no shell is involved.
    original = subprocess.check_output([git, "show", f"{BASELINE}:{SOURCE}"], cwd=ROOT)  # nosec B603
    (ROOT / SOURCE).write_bytes(original)
    # Explicit local developer command, passed as arguments without a shell.
    result = subprocess.run(sys.argv[1:], cwd=ROOT, check=False)  # nosec B603
finally:
    (ROOT / SOURCE).write_bytes(saved)
raise SystemExit(result.returncode)
