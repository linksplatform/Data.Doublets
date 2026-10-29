"""Demonstrate three contract failures without submitting invalid raw updates.

Run with a local .NET 8 executable as the only argument and no concurrent builds
or source edits. Sources are restored in finally; rebuild afterward. Artifacts
stay under artifacts/issue-517/negative/. The three expected failures show that
the suite detects mandatory core checks and an omitted outer validator.
"""
from pathlib import Path
# This bounded local experiment intentionally launches the selected SDK.
import subprocess  # nosec B404
import sys
import xml.etree.ElementTree as ET


root = Path(__file__).resolve().parents[1]
backends = [
    root / "csharp/Platform.Data.Doublets/Memory/United/Generic/UnitedMemoryLinksBase.cs",
    root / "csharp/Platform.Data.Doublets/Memory/Split/Generic/SplitMemoryLinksBase.cs",
]
tests = root / "csharp/Platform.Data.Doublets.Tests/UpdateContractTests.cs"
original = {path: path.read_bytes() for path in [*backends, tests]}
results = root / "artifacts/issue-517/negative"
results.mkdir(parents=True, exist_ok=True)
try:
    for path in backends:
        source = original[path].decode()
        update = source.index("public virtual TLinkAddress Update(")
        line = source.index("var linkIndex =", update)
        end = source.index(";", line) + 1
        source = source[:end] + '\n            if (!Exists(linkIndex)) throw new System.InvalidOperationException("Compulsory check");' + source[end:]
        path.write_text(source)
    source = original[tests].decode()
    boundary = "var links = new LinksInnerReferenceExistenceValidator<uint>(spy.DecorateWithAutomaticUniquenessAndUsagesResolution());"
    if source.count(boundary) != 1:
        raise ValueError("Expected one outer-validator spy test")
    tests.write_text(source.replace(boundary, "var links = spy.DecorateWithAutomaticUniquenessAndUsagesResolution();"))
    # Only valid raw writes or terminal-spy requests run in this experiment.
    command = [
        sys.argv[1], "test", "csharp/Platform.Data.Doublets.Tests", "-c", "Release",
        "--no-restore", "-m:1", "-p:UseSharedCompilation=false",
        "--blame-hang-timeout", "3m", "--filter",
        "FullyQualifiedName~ValidRawUpdateOmitsExistenceChecksAndPreservesCallbackFlow|FullyQualifiedName~OuterValidatorRejectsBeforeAnyResolverOperation",
        "--logger", "trx;LogFileName=guards.trx", "--results-directory", str(results),
    ]
    # The SDK path is explicitly selected by the local experiment's caller.
    with (results / "run.log").open("w") as log:
        result = subprocess.run(command, cwd=root, stdout=log, stderr=subprocess.STDOUT, check=False)  # nosec B603
    summary = ET.parse(results / "guards.trx").find(".//{*}Counters")
    if result.returncode != 1 or summary is None or summary.get("failed") != "3" or summary.get("total") != "3":
        raise AssertionError(f"Expected exactly three failed contract tests; inspect {results / 'run.log'}")
    print("Verified three expected contract failures; no invalid raw update was executed.")
finally:
    for path, contents in original.items():
        path.write_bytes(contents)
