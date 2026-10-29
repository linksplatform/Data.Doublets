# Issue 519 implementation and requirement inventory

Scope: [issue 519](https://github.com/linksplatform/Data.Doublets/issues/519), its
single comment requesting a Rust audit/report, and
[PR 544](https://github.com/linksplatform/Data.Doublets/pull/544).
No required sub-issues were listed. Conversation comments, inline review comments
and submitted reviews were empty at the initial and implementation reviews.

| Requirement | Implementation | Verification |
| --- | --- | --- |
| CreateAndUpdate returns the existing live address on decorated stores | LinksUniquenessResolver reports the survivor after reset/deletion notifications | CreationReturnsLiveAddresses: united/split, raw/unique/automatic, byte/ushort/uint/ulong |
| UpdateOrCreateOrGet missing-pair fallback returns the surviving address | Same resolver notification reaches the common setter | CreationReturnsLiveAddresses exercises the exact two-point reproduction |
| Preserve ordinary store behavior | Raw stores still create duplicate pairs at addresses 3 and 4; no mandatory existence policy is added | Raw cases plus all existing update/deletion contract tests |
| Keep handler Break state through update | CreateAndUpdate routes the update callback through HandleNullable so callback state is recorded even if a primitive/decorator returns Continue | BreakSuppressesLaterCallbacksButCompletesCreation and ContinueAfterUpdate spy |
| Cover all helper branches | Unchanged UpdateOrCreateOrGet pair now notifies its handler; direct unchanged uniqueness updates also report a result | UpdateOrCreateOrGetReturnsAddressForEveryBranch; direct-update assertions in MergeReportsSurvivorAfterDeletionCallbacks |
| Preserve usage redirection during uniqueness cleanup | MergeUsages builds full index/source/target substitutions, preserving the other endpoint | MergePreservesBothEndpointsOfEveryUsage and merge callback tests on both backends |
| Double-check Rust and report analogous defects | Pinned standalone audit of current doublets-rs main; logging-control defect reported upstream | experiments/issue-519, rust-audit.log and rust-logging-break.log; doublets-rs issue 86 |
| Prepare automatic release | Independent C# patch changeset, no manual package version edit | versions.py validate |

## Root causes and choices

The address adapters remember the last non-null `after` snapshot. Previously a
uniqueness conflict reported only cleanup of the redundant link. CreateAndUpdate
therefore retained its temporary address after that link was deleted. The base
resolver now emits a survivor-to-survivor notification last, matching the existing
Rust design. Reporting before deletion is insufficient: resetting a non-null
redundant link produces another non-null snapshot and can overwrite that result.
Cleanup notifications are retained for observers. A Break suppresses later
notifications while the compound operation completes, preserving the existing
atomic-operation convention.

`WriteHandlerState.Handler` in Platform.Data 0.16.1 is a field containing the raw
callback, not a state-recording delegate. Merely replacing `handler` with that
field, as suggested in the issue, is insufficient when an inner layer swallows
callback control. The update wrapper invokes HandleNullable, which records Break
and prevents subsequent calls independently of the primitive's return value.

The unchanged-pair branch of UpdateOrCreateOrGet also returned a number directly
without notifying the setter, so its address overload returned zero. It now
reports the existing snapshot and consistently returns callback control from
the handler overload.

The merge regression exposed existing malformed usage substitutions: the
2-argument Link constructor means index/source, not source/target. Both loops now
supply all three parts. Tests cover usage as source, target and both, ensuring
surviving links still have their intended endpoints.

## Rust audit

Audited main at `645dd2b8e5c9bff1e38335da42e3c008b5d2769e`, released as
0.15.0 on 2026-10-29. Current Rust already reports the surviving link and correctly
replaces usage endpoints. It has no UpdateOrCreateOrGet method; the audit tests
the corresponding absent-pair creation path and get_or_create. Unit and split
stores, bare and automatically decorated, pass live-address, duplicate-creation,
merge, unchanged-update and Break checks.

A separate optional probe demonstrates that Rust create_link_with loses Break
when LoggingDecorator wraps the store. Logging calls Fuse::call but discards its
flow; create_link_with returns only update_links' Continue. This is reported at
https://github.com/linksplatform/doublets-rs/issues/86, with a minimal reproduction.
The issue comment requests checking and reporting Rust, so the upstream report
fulfills that criterion; it does not require implementing the foreign fix here.

## Reproduction and evidence

The initial regression run preceded library edits: **21 failed, 2 passed** in
`tests/csharp-before.log`. Failures include deleted address 3 instead of survivor
1, zero from the unchanged branch, missing survivor callbacks, callbacks invoked
after Break, and lost Break when the inner update returns Continue. Additional
merge endpoint regressions bring the final focused suite to **25 passing cases**.
The four-width creation checks execute within each store/policy case.

`tests/csharp-full.log` records **323 passing tests, no skips**, zero build warnings,
formatting, all examples, NuGet packaging and isolated package consumption.
`tests/python-checks.log` records **96 passing pipeline regressions**. Repository,
workflow, shell, formatting, version policy and credential scan logs are retained.
The credential scan covers 3,321 commits with no findings.

All probes have finite inputs and test execution uses the existing three-minute
hang diagnostic. Rust builds use one worker and omit debug information. Local
native checks use a single build worker; memory-heavy work is serialized.

Native Release tests also pass locally: **46/46** in `tests/native-tests.log`.
The benchmark smoke run passes in `tests/native-benchmark.log`. Native formatting,
clang-tidy and cppcheck also exit successfully. The unprivileged cppcheck package
initially could not find its standard-library configuration; pointing its local
`cfg` directory to the extracted package resolved that environment issue, and the
exact cppcheck command from check-cpp.py then passed.

Documentation validation passes: the Unicode source PDF contains 291 pages,
DocFX builds with zero warnings/errors, and Doxygen plus check_docs.py validate
549 HTML pages and their local links (`tests/documentation.log`). The required
DejaVu Mono font was extracted locally and supplied with source_pdf.py's --font
option because this container does not install system fonts by default.

Current main `263943021` was merged with a regular merge commit. Its changes
concern native package metadata and release repair. C# code and documentation
generation inputs are unaffected. Repeated checks all pass: **105 pipeline
regressions**, **323 C# tests**, formatting, examples, packaging and isolated
consumer, shell lint, repository/version policy and documentation links. Their
logs use the `*-after-main.log` suffix.
