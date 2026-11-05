<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# Issue 242 requirement and verification plan", "*"]
  }
}
-->

# Issue 242 requirement and verification plan

Issue: <https://github.com/linksplatform/Data.Doublets/issues/242>

PR: <https://github.com/linksplatform/Data.Doublets/pull/443>

Read all five original issue comments, both PR conversation comments, inline
comments (none), reviews (none), and sub-issues (none). The substantive PR
feedback asks to validate the report, fix C# and C++, cover tests/docs, and
comment on the original issue.

1. Validate the reported `super_name` conversion: reproduce the converter's
   adjacent-pair layers with raw character addresses. Verify before/after
   focused C# tests and the runnable example.
2. Fix the C# exception: check endpoint existence before reading stored links;
   format raw leaves numerically. Check exact united and split store notation.
3. Validate the recursion diagnosis: retain the visited set and direct self
   reference path. Check exact cycle, point and shared-subtree output.
4. Preserve nested debug options: forward `renderDebug` for both endpoints.
   Test every combination of index/debug flags on a cycle.
5. Cover C++: add the absent FormatStructure/AppendStructure APIs with matching
   behavior. Run united uint64 and split uint32 cases in normal CTest discovery.
6. Preserve custom elements and caller-owned state: retain callbacks and shared
   visited/builder behavior. Assert callback counts, output and visited set.
7. Handle absent, deleted, external and reserved addresses: render numeric
   missing leaves, preserve virtual points and skip reserved constants. Test
   both languages and reject callbacks on absent internal leaves.
8. Document behavior and explain the issue: add the guide, corrected example
   and original issue comment. Check docs CI and links to verified evidence.
9. Prepare the release and final PR: add a C# patch/C++ minor changeset, merge
   main and review the diff. Verify current-head CI, clean tree and ready PR.

Local verification is sequential and focused. Full suites, native compilers,
sanitizers, static analysis, package consumers and generated docs run in CI.
Deliberate probes have finite data; no unbounded stack-overflow reproduction.

## Focused commands and results

```sh
python3 experiments/issue-242/reproduce-before.py dotnet test \
  csharp/Platform.Data.Doublets.Tests -c Release --no-restore -m:1 \
  -p:UseSharedCompilation=false --filter FullyQualifiedName~FormatStructureTests
dotnet test csharp/Platform.Data.Doublets.Tests -c Release --no-restore -m:1 \
  -p:UseSharedCompilation=false --filter FullyQualifiedName~FormatStructureTests
dotnet run --project experiments/issue-242/OriginalReport -c Release
dotnet run --project examples/Structure -c Release
python3 experiments/issue-242/focused-native.py --before
python3 experiments/issue-242/focused-native.py
python3 -m unittest experiments.test_pipeline_policies.RoutingTests
```

The first command fails with 16 failures and 7 passes on pinned main. The
fixed suite passes all 23 cases. The two runnable programs assert the same
expected notation. The native `--before` command fails because main has no
structure formatter; both native store checks pass after the fix. Native
setup uses the normal Conan/CMake configuration (see `cpp/setup.sh` and
`cpp/ci-build.sh`); the focused runner reuses its generated paths while
compiling only these cases with GCC, `-O0 -g0` and an address-space cap.

Local native formatting, C# whitespace formatting, repository validation and
the eight routing checks pass. Full validation is delegated to the existing
current-commit CI jobs, without changing or bypassing their gates.
