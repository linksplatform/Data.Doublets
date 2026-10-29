"""Reproduce compulsory deletion validation in the previous PR implementation."""

# Usage: python experiments/reproduce_deletion_contract.py /path/to/dotnet
# Temporarily substitutes the original implementation, restores every file in finally,
# and only deletes valid, reset links; it never passes invalid memory addresses.
# Run from the repository root with no concurrent builds or source edits.

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
baseline = "136ada5ad641b755011f77f58ca7de17e6cb0386"
git = which("git")
if git is None:
    raise FileNotFoundError("git must be on PATH.")
original = {path: path.read_bytes() for path in paths}
try:
    for path in paths:
        # The revision and file paths are fixed above; execution never uses a shell.
        path.write_bytes(subprocess.check_output([git, "show", f"{baseline}:{path}"]))  # nosec B603
    # The caller explicitly selects the local SDK; all test arguments are fixed.
    result = subprocess.run([  # nosec B603
        sys.argv[1], "test", "csharp/Platform.Data.Doublets.Tests", "-c", "Release",
        "--no-restore", "--filter", "FullyQualifiedName~DeletionContractTests",
        "--blame-hang-timeout", "3m", "-m:1", "-p:UseSharedCompilation=false",
    ])
finally:
    for path, contents in original.items():
        path.write_bytes(contents)
sys.exit(result.returncode)
