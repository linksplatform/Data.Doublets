# Reproduction and verification

The GitHub check suite on [PR 557](https://github.com/linksplatform/Data.Doublets/pull/557/checks) is the authoritative current-head result. Heavy native builds, the compiler/sanitizer matrix and full CodeQL run on hosted runners; local work uses one-worker bounded probes within the 2.92 GiB container limit.

## Observed failures and corrected causes

| Run / saved log | Exact failure evidence | Correction and regression |
|---|---|---|
| [Baseline main 42126920990](https://github.com/linksplatform/Data.Doublets/actions/runs/42126920990), research/main-ci-diagnostics.txt | Original log lines 10795–10796: CodeQL upload Server Error; required gate then failed on security. Other ecosystem checks passed. | Rerun only failed jobs. All passed and publication completed. Merge its generated main release commit into this branch; explicitly dispatch full CI on that current main SHA as well. See github/main-ci-runs-current.json, rather than confuse successful Dependabot graph runs with full CI. |
| [First update 42133011724](https://github.com/linksplatform/Data.Doublets/actions/runs/42133011724), ci-logs/ci-42133011724.log | New zizmor requires current self-repository reusable-workflow references. | Adopt native $/ references and strict pin validator accepting only current-repository workflow paths. Two regression cases check accepted paths, external unpinned actions and traversal. |
| [42133631172](https://github.com/linksplatform/Data.Doublets/actions/runs/42133631172), preserved logs | Original actionlint does not implement that new syntax and queue schema. | Maintained current fork handles both natively; five executable positive/negative fixtures cover queue constraints, callee existence and embedded shell/Python errors. |
| [42134877050](https://github.com/linksplatform/Data.Doublets/actions/runs/42134877050), ci-logs/ci-42134877050-gcc.log lines 1948–1973; clang log lines 1978–2005 | GCC 16/Clang 23 reject object() in two interfaces without a CRTP base. CodeQL native build has the same root cause, lines 2004–2029 of its job log. | Proper abstract virtual methods/destructors and concrete overrides. Compile experiments/issue-556/interface_dispatch.cpp against pre-fix headers: fails. After fix: GCC and Clang compile/run successfully. The CI matrix runs all seven dispatch assertions. |
| Same run, Windows job log lines 590–597 | Windows preinstalled Python rejects current multi-exception syntax during package consumer validation. All 479 C# tests already passed. | Install exact Python 3.15.2 in the C# matrix; retain current formatter output. Full Windows package-consumer job validates the complete flow. |
| Same run, docs job log lines 624–625 | Doxygen 1.18 fails on unresolved README.ru.md reference. | Include both README input pages; local Doxygen warnings-as-errors generation succeeds. Full docs job also verifies DocFX and Unicode PDF. |
| Same run, full log line 4099 | rustup warns that implicit active-toolchain auto-installation is deprecated. | Explicit rustup toolchain install, then explicit selected toolchain for cargo-audit installation. No ignored deprecation filter. |
| [42136483676](https://github.com/linksplatform/Data.Doublets/actions/runs/42136483676), ci-logs/ci-42136483676-clang.log lines 2679–2693 | Clang 23's new portability-avoid-pragma-once check rejects the sole pragma-once header, Tools/TemporaryDirectory.h. Its native build/tests already passed. | Replace the directive with a standard include guard. The existing warnings-as-errors clang-tidy integration is the reproducing automated check; retain it for both full translation units. Repository-wide search confirms no remaining pragma-once directive. |
| [42138118540](https://github.com/linksplatform/Data.Doublets/actions/runs/42138118540), ci-logs/ci-42138118540-codeql-python.log lines 2356–2357 | CodeQL identifies potentially uninitialized registry-result variables for an unsupported ecosystem in the new investigation helper. | Reject unknown ecosystems explicitly before returning registry results. experiments/test_dependency_inventory.py fails before the fix and passes after it, preserving package identity and an actionable error. The full suite now contains 111 tests. |
| validation/workflow-image-audit.json | OSV reports 14 advisories in release binary's Go 1.27.0 standard library. | Rebuild unchanged source with Go 1.27.3; validation/workflow-image-audit-final.json has no findings. CI audits every resulting checker image. |
| validation/collections-reproducer.log | Published Collections.Methods 1.0.0 fails CS0234; full trial causes 176 missing-API errors. | Retain compatible 0.7.3 with upstream report and explicit exception. |
| validation/csharp-full-refresh.log and benchmark trial | Parent exact Perfolizer requirement prevents latest override; newest Pragmastat causes runtime missing constructor. | Preserve upstream-supported statistical versions and execute the benchmark, rather than accept build-only validation. |

Some unittest fixtures intentionally print publication errors or rejected pushes from temporary local remotes. These are asserted negative cases; the enclosing suite result and exit code determine success.

## Automated commands and evidence

| Coverage | Command / result evidence |
|---|---|
| Complete C# checks | PATH includes SDK 10.0.403; bash scripts/check-csharp.sh. validation/csharp-final.log: build has zero warnings/errors, 479 tests pass without skips, format, benchmark, five examples, pack and isolated consumer pass. Both CI platforms repeat this. |
| Python suite | Activate artifacts/python-venv; bash scripts/check-python.sh. validation/python-final.log: Ruff, formatter, mypy and the original 110 tests pass; validation/python-complete-final.log includes the new inventory regression, bringing the final suite to 111. The NuGet fallback regression asserts HTTPError response closure. |
| Workflow checker | python3 scripts/check-workflows.py --container; python3 experiments/issue-556/check_workflow_schema.py. validation/workflow-final.log; native ShellCheck/Pyflakes remain enabled. |
| Repository invariants | python3 scripts/check_repository.py; git diff --check; shellcheck all tracked live *.sh; changeset validation in CI. |
| Native interface reproduction | c++ -std=c++20 -Wall -Wextra -Werror experiments/issue-556/interface_dispatch.cpp -o /tmp/interface-dispatch-556; run binary. Same with Clang. validation/interface-before.log retains expected compile errors. |
| Native full checks | cpp CI GCC, Clang and Clang ASan/UBSan; cppcheck, clang-tidy, C11/C++20 FFI, package consumers and bounded benchmark remain required. |
| Rust full checks | bash scripts/check-rust.sh; both owned locks audited, both parity probes run. Job rust succeeded in 42134877050, then repeated on final head. |
| NuGet audit | dotnet list csharp/Platform.Data.Doublets.sln package --vulnerable --include-transitive --format json; scripts/check_audit.py. validation/nuget-audit-final.json has no vulnerable dependencies. |
| Python audit | pip-audit against all three inputs and shared constraints. validation/python-audit-final.json: no known vulnerabilities. |
| Native audit | osv-scanner scan source --lockfile cpp/conan.lock --all-packages --format json. validation/native-osv-audit.json covers all 20 package identities including CMake. Supplement exact source commits with experiments/issue-556/audit_native_source_commits.py; validation/native-audit.json covers 19 native sources with no findings. |
| Checker image audit | osv-scanner scan image --format json data-doublets-workflow-lint:local. validation/workflow-image-audit-final.json: no findings. CI stores full report artifact. |
| CodeQL and secrets | Current full-repository C#, C++, Python and Actions CodeQL reports must contain no findings or incomplete extraction; repository-history Gitleaks check. No baseline findings excluded. |
| Docs and links | DocFX warnings-as-errors, Doxygen warnings-as-errors, Unicode source PDF validation and current lychee check. |

All final checks must apply to the PR's latest commit; snapshots from superseded runs are retained solely for diagnosis. Raw logs remain local under ci-logs, while hosted run links preserve reviewer access without adding multi-megabyte duplicate logs to Git.

The full current-default-branch run [42136956911](https://github.com/linksplatform/Data.Doublets/actions/runs/42136956911) completed successfully at commit b298e0e20f7ec6f365351cd1375dc24ead1c1eab, including publication and the required gate. That commit is merged into this branch.
