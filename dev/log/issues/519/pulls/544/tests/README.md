# Local verification

Run from the repository root. Each passing command below exited zero; quiet
linters can produce an empty log. C# uses the repository-selected .NET 8 SDK.

| Command or check | Result / log |
| --- | --- |
| Focused `dotnet test` filter `FullyQualifiedName~UniquenessResultTests`, before edits | Expected failure: 21 failed, 2 passed; csharp-before.log |
| Same filter after the fix and expanded merge coverage | 25 passed; csharp-after.log |
| `bash scripts/check-csharp.sh` | 323 passed, no skips; build, format, examples, packaging and isolated consumer pass; csharp-full.log |
| `bash scripts/check-python.sh` | Ruff, format, mypy and 96 pipeline regressions pass; python-checks.log |
| `python3 scripts/check_repository.py` | repository-checks.log |
| `python3 scripts/check-workflows.py --container` | workflows.log |
| Tracked shell scripts through ShellCheck, excluding archived dev/log | shellcheck.log |
| `python3 scripts/versions.py validate` | changeset.log |
| `bash cpp/setup.sh` | native-setup.log |
| `bash cpp/run_tests.sh` | 46 passed; native-tests.log |
| `bash cpp/run_benchmarks.sh --benchmark_filter=BM_JustCall --benchmark_min_time=0.01` | native-benchmark.log |
| `python3 scripts/check-cpp.py` | native-format.log |
| clang-tidy with cpp/build compile database, both translation units | clang-tidy.log |
| cppcheck with all arguments from check-cpp.py and the same database | cppcheck.log |
| `bash scripts/check-secrets.sh` | 3,321 commits, no findings; secrets.log |
| Pinned Rust default audit (see experiments/issue-519/README.md) | Four stacks pass; rust-audit.log |
| Optional Rust `--logging-break` probe | Expected assertion failure; rust-logging-break.log, reported upstream as issue 86 |
| Source PDF, DocFX with warnings as errors, Doxygen, check_docs.py | 291 PDF pages, 549 HTML pages, zero DocFX warnings/errors; documentation.log |
| Checks repeated after merging main | 105 pipeline regressions, 323 C# tests plus formatting/examples/package consumer, shell lint, repository/version checks and documentation links pass; *-after-main.log |

The full native analysis script completed formatting and clang-tidy, then its
locally extracted cppcheck could not locate std.cfg. After correcting that tool's
local configuration path, its exact command was rerun successfully. The checks
remain enabled with their original warning policies.

Large build/test output is retained here. Compiler work was serialized; Rust used
one worker with debug information disabled. No stress probe or unbounded input
was used. Documentation results and checks repeated after merging current main
are listed in the analysis inventory.
