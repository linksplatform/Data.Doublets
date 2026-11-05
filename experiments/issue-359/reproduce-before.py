"""Run the regression against main's pre-fix sources, restoring the working files."""
from pathlib import Path
import subprocess
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
saved = {name: (ROOT / name).read_bytes() for name in FILES}
try:
    for name in FILES:
        result = subprocess.run(["git", "show", f"{BASELINE}:{name}"], cwd=ROOT, check=True, capture_output=True)
        (ROOT / name).write_bytes(result.stdout)
    result = subprocess.run(sys.argv[1:], cwd=ROOT, check=False)
finally:
    for name, contents in saved.items():
        (ROOT / name).write_bytes(contents)
raise SystemExit(result.returncode)
