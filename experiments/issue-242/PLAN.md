# Issue 242 requirement and verification plan

Issue: <https://github.com/linksplatform/Data.Doublets/issues/242>

PR: <https://github.com/linksplatform/Data.Doublets/pull/443>

Read all five original issue comments, both PR conversation comments, inline
comments (none), reviews (none), and sub-issues (none). The substantive PR
feedback asks to validate the report, fix C# and C++, cover tests/docs, and
comment on the original issue.

| Requirement | Implementation | Verification |
| --- | --- | --- |
| Validate the reported `super_name` conversion | Reproduce the converter's adjacent-pair layers with raw character addresses | Before/after focused C# test and runnable example |
| Fix the actual C# exception | Check endpoint existence before constructing a stored link; route raw leaves through existing formatting | Exact expected notation on united and split stores |
| Validate the PR's recursion diagnosis | Existing visited set already bounds circular traversal; self references have a direct path | Exact cycle, self-reference and shared-subtree tests |
| Preserve nested debug options | Forward `renderDebug` for both source and target | All combinations of index/debug flags on a cycle |
| Cover C++ | Supply absent native FormatStructure/AppendStructure with matching behavior | United uint64 and split uint32 cases in normal CTest suite |
| Preserve custom elements and caller-owned state | Retain predicate/element callback semantics and visited/builder behavior | Exact callback counts, output and visited set |
| Handle absent, deleted, external and reserved addresses | Numeric missing leaves; preserve external points and skip reserved constants | Both language suites; no callbacks on absent internal leaves |
| Document behavior and explain issue | User guide, corrected example, original issue comment | Docs CI and links to verified evidence |
| Prepare release and final PR | Independent C# patch/C++ minor changeset, merge main, final diff review | Current-head CI, clean tree, updated PR marked ready |

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
