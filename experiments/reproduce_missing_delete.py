"""
Run the regression tests against the original C# deletion implementations.

Usage: python experiments/reproduce_missing_delete.py /path/to/dotnet
Temporarily substitutes the original implementation, restores every file in finally,
and excludes extreme addresses because the original code reads unchecked memory.
Run from the repository root with no concurrent builds or source edits.
"""

from pathlib import Path
from shutil import which
# This local test runner intentionally launches git and the SDK.
import subprocess  # nosec B404
import sys


paths = [
    Path("csharp/Platform.Data.Doublets/Memory/United/Generic/UnitedMemoryLinksBase.cs"),
    Path("csharp/Platform.Data.Doublets/Memory/Split/Generic/SplitMemoryLinksBase.cs"),
    Path("csharp/Platform.Data.Doublets/Decorators/NonNullContentsLinkDeletionResolver.cs"),
]
baseline = "8d5c7541371957739de8bfa382e04b0c7d84534f"
git = which("git")
if git is None:
    raise FileNotFoundError("git must be on PATH.")
tests = Path("csharp/Platform.Data.Doublets.Tests/MissingLinkDeletionTests.cs")
original = {path: path.read_bytes() for path in [*paths, tests]}
safe_tests = original[tests].replace(b', "out-of-range"', b"")
if safe_tests == original[tests]:
    raise RuntimeError("Cannot safely exclude extreme addresses from the baseline tests.")
try:
    for path in paths:
        # The revision and file paths are fixed above; execution never uses a shell.
        path.write_bytes(subprocess.check_output([git, "show", f"{baseline}:{path}"]))  # nosec B603
    tests.write_bytes(safe_tests)
    # The caller explicitly selects the local SDK; all test arguments are fixed.
    result = subprocess.run([  # nosec B603
        sys.argv[1], "test", "csharp/Platform.Data.Doublets.Tests", "-c", "Release",
        "--no-restore", "--filter", "FullyQualifiedName~MissingLinkDeletionTests",
        "--blame-hang-timeout", "3m", "-m:1", "-p:UseSharedCompilation=false",
    ])
finally:
    for path, contents in original.items():
        path.write_bytes(contents)
sys.exit(result.returncode)
