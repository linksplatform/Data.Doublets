"""Run the regression against main's pre-fix sources, restoring the working files."""
from pathlib import Path
from shutil import which
# Explicit local reproduction commands use argument lists without a shell.
import subprocess  # nosec B404
import sys

ROOT = Path(__file__).resolve().parents[2]
FILES = [
    "csharp/Platform.Data.Doublets/Memory/Split/Generic/SplitMemoryLinksBase.cs",
    "csharp/Platform.Data.Doublets/Memory/United/Generic/UnitedMemoryLinksBase.cs",
    "cpp/Platform.Data.Doublets/Memory/Split/Generic/SplitMemoryLinksBase.h",
    "cpp/Platform.Data.Doublets/Memory/United/Generic/UnitedMemoryLinksBase.h",
]
# The baseline is pinned so the reproduction remains meaningful after main moves.
BASELINE = "80172fcd0"
git = which("git")
if git is None:
    raise SystemExit("git is required")
if len(sys.argv) < 2:
    raise SystemExit("Pass the regression command and its arguments")
saved = {name: (ROOT / name).read_bytes() for name in FILES}
try:
    for name in FILES:
        # Resolved Git executable and fixed baseline/file list, without a shell.
        result = subprocess.run(  # nosec B603
            [git, "show", f"{BASELINE}:{name}"], cwd=ROOT, check=True, capture_output=True
        )
        (ROOT / name).write_bytes(result.stdout)
    # The developer explicitly supplies the local regression command; no shell.
    result = subprocess.run(  # nosec B603
        sys.argv[1:], cwd=ROOT, check=False
    )
finally:
    for name, contents in saved.items():
        (ROOT / name).write_bytes(contents)
raise SystemExit(result.returncode)
