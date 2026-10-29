# Verification record

## Baseline and reproductions

- `tests/reproductions-before.log`: minimal new regressions fail against the
  original SARIF gate, absent artifact helper, missing workflow tool check and
  Python configuration routing. The protected-release test already passes: its
  existing fail-once behavior is correct and must be preserved.
- `tests/baseline-ruff.log`, `baseline-mypy.log`, `baseline-format.log` and
  `baseline-python-audit.json`: original Python omissions and two fpdf2 advisories.
- `tests/artifact-buffer-reproduction.log`: exact pinned upstream MatcherStream,
  two bytes, no network or credentials, reproduces DEP0005 with its stack trace.
- `tests/workflow-tools-reproduction.log`: same malformed workflow passes the
  extracted actionlint binary without integrations (exit 0), fails in the complete
  pinned image with SC2086 (exit 1).
- `tests/original-sarif-after.log`: the stricter validator accepts all three real
  complete reports from the originally cited main run; no real finding is hidden.

## Local checks

| Check | Result and evidence |
| --- | --- |
| Python quality and pipeline regressions | Ruff, formatting, mypy on all 17 maintained Python modules; 96 tests pass after CodeQL repairs (`tests/python-codeql-fixes.log`) |
| Workflows and standalone shell | Full pinned actionlint image with ShellCheck/Pyflakes; standalone maintained shell scripts pass (`tests/workflows-codeql-fixes.log`, `tests/shellcheck-after.log`) |
| Repository configuration, action pins, source-size limits | Pass (`tests/repository-codeql-fixes.log`) |
| Release fragment/version validation | Pass; tooling-only change needs no product version fragment (`tests/versions.log`) |
| Native formatting | Pass (`tests/native-format.log`) |
| .NET 8 Release | 298 tests, zero skips, coverage, all three examples, pack and isolated package consumer pass; zero warnings/errors (`tests/csharp.log`). Consumer rechecked after helper repair (`tests/csharp-consumer-after-codeql.log`) |
| Documentation | Source PDF, DocFX warnings-as-errors, Doxygen and 549 generated HTML pages/local links pass (`tests/documentation.log`) |
| Full-history secrets scan | 3,310 commits, zero findings with existing exact historical exceptions (`tests/secrets.log`) |
| Full Python dependency resolution and audit | All three requirements files and transitives, zero known findings (`tests/python-audit-after.json`, `tests/python-audit-after.log`) |
| Native test build on this container | Setup succeeds. One-worker GCC compile deliberately stopped before the shared 2.92 GiB cap; no OOM kill, no repeated compile. **No local full-native pass claimed** (`tests/cpp-tests.log`, `tests/local-memory-stop.txt`). Hosted matrix below provides native validation |

The memory stop does not relax compiler, sanitizer, coverage, benchmark, FFI or
consumer checks in CI. No dependency advisory is ignored and SARIF findings remain
fatal at every severity.

## Hosted implementation run

[Run 40931698011](https://github.com/linksplatform/Data.Doublets/actions/runs/40931698011)
tested `1de337ed5955a5036475edc4532a85a8cd3494d3`, started 2026-10-29 18:31:59 UTC,
completed 18:43:01 UTC. Raw full log: `ci-logs/CI-40931698011.log`; run/job metadata
and artifact list: `github/implementation-run-40931698011.json` and
`github/run-40931698011-artifacts.json`.

- Passed: quality (including real artifact round trip under `--throw-deprecation`),
  GCC, Clang, Clang sanitizers, C# Ubuntu and Windows, documentation, live links,
  full dependency audits/history scan, C#/C++/Actions CodeQL.
- Failed: newly enabled Python CodeQL's explicit findings gate, with five findings
  preserved at log lines 7930–7935. The analyzer action itself succeeded. Their
  classification and repairs are in `ANALYSIS.md`; this is not a passing run.
- Expected skipped jobs: trusted-main credentials and publish on a PR. The
  required aggregate correctly fails because Python CodeQL failed.

## Successful follow-up and final evidence packaging

[Run 40934271519](https://github.com/linksplatform/Data.Doublets/actions/runs/40934271519)
tested `7136487ec6b91e43b41f40ff0f98345149364382`, started 2026-10-29 18:48:32 UTC,
completed **successfully at 19:01:02 UTC**. Every applicable job passed, including
all four CodeQL languages, three native configurations, both C# platforms, audits,
links, documentation, quality and the terminal required gate. Trusted-main-only
credentials and publication correctly skip on PRs.

Complete output is retained in `ci-logs/CI-40934271519.log`, with run/job metadata
in `github/implementation-run-40934271519.json`, all 13 original artifacts under
`research/followup-run-artifacts/`, and their metadata/digests in
`github/run-40934271519-artifacts.json`. The complete log has zero warning/error
annotations, DEP0005 occurrences or matched compiler/Conan warning diagnostics.
All 13 artifact SHA-256 hashes match the published GitHub metadata and their ZIP
CRC checks pass (`tests/followup-artifact-integrity.log`). All four unchanged
CodeQL reports pass the local strict gate (`tests/followup-sarif-verification.log`).

The final packaging commit adds only research and verification evidence under
`dev/log/**`. `research/validated-executable-tree.txt` records every Git tree entry
outside that directory; it is unchanged from the successful follow-up run. This
avoids claiming a new executable implementation merely because evidence was
appended. The packaging commit's own status is checked on GitHub before ending
the task; its complete post-commit log is also retained locally in `ci-logs/`.
The PR description links the final run and records its exact SHA and result.

Default branch `c1a96a1aeb577e57d3e0b459bcf26e5fcc5e3efb` is in branch ancestry,
confirmed with a fresh fetch. PR title/body are updated, `Fixes #541` is preserved,
the PR is ready for review, and auto-merge is disabled. It remains unmerged.

## Criteria that remain unfinished

1. Real trusted-main release: existing branch protection rejects metadata writes
   with GH006. An administrator must authorize the intended actor or supply an
   allowed release App/token, then rerun and verify public publication. The task
   only authorizes writes to this PR branch. No policy weakening or main write is
   attempted; PR validation cannot exercise this authorization.
2. Native dependency advisory-service coverage: Conan's provider requires an
   activated account/token, unavailable here, plus confirmation of provider catalog
   coverage for local aliases. All 19 resolved source coordinates and resolved recipe bytes
   have been inventoried; the bounded failed probe is retained. NuGet/Python audits and
   source CodeQL do not establish a clean Conan advisory verdict.

The PR must remain unmerged while these criteria are unfinished. This record does
not treat a green PR, a report to an upstream project or an available workaround
as completion of either external criterion.
