"""Reproduce compulsory deletion validation in the previous PR implementation."""

# Usage: python experiments/reproduce_deletion_contract.py /path/to/dotnet
# Temporarily reinstates the previous mandatory checks, restores every file in finally,
# and only deletes valid, reset links; it never passes invalid memory addresses.
# Run from the repository root with no concurrent builds or source edits.

from pathlib import Path
# This local test runner intentionally launches the selected SDK.
import subprocess  # nosec B404
import sys


paths = [
    Path("csharp/Platform.Data.Doublets/Memory/United/Generic/UnitedMemoryLinksBase.cs"),
    Path("csharp/Platform.Data.Doublets/Memory/Split/Generic/SplitMemoryLinksBase.cs"),
    Path("csharp/Platform.Data.Doublets/Decorators/NonNullContentsLinkDeletionResolver.cs"),
]
baseline = "136ada5ad641b755011f77f58ca7de17e6cb0386"
original = {path: path.read_bytes() for path in paths}
try:
    for path in paths:
        source = original[path].decode()
        if path.name == "NonNullContentsLinkDeletionResolver.cs":
            needle = "        var linkIndex = _links.GetIndex(link: restriction);"
            replacement = needle + "\n        _links.EnsureLinkExists(link: linkIndex, argumentName: nameof(linkIndex));"
        else:
            # These are the exact existence guards from baseline, applied to
            # current source so nullable/structural API fixes remain in place.
            access = "index: Constants.IndexPart" if "Split" in path.name else "Constants.IndexPart"
            indent = "        " if "Split" in path.name else "            "
            needle = f"{indent}var link = restriction[{access}];"
            replacement = "\n".join([
                needle,
                f"{indent}if (!Exists(link))",
                f"{indent}{{",
                f"{indent}    throw new Platform.Data.Exceptions."
                "ArgumentLinkDoesNotExistsException<TLinkAddress>(link, nameof(link));",
                f"{indent}}}",
            ])
        if source.count(needle) != 1:
            raise ValueError(f"Expected one insertion point in {path} for {baseline}")
        path.write_text(source.replace(needle, replacement))
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
