# Issue 541 / PR 542 investigation

## Verdict and limits

The observed main-branch release failure is a **repository authorization blocker**:
GitHub rejects the release metadata push with GH006. It is not a compiler,
NuGet authentication, package-consumer, or test failure. This PR preserves the
fatal guard and does not weaken protection or push to `main`. Release completion
remains unfinished until an administrator authorizes the intended release actor
and a trusted main run publishes and anonymously verifies the resulting versions.
A passing PR does not prove that criterion.

The investigation also reproduces and fixes incomplete SARIF reports passing the
gate, missing embedded-code linter coverage, missing Python quality/security
coverage, two fpdf2 advisories, and deprecated artifact extraction. Full requested
template trees are archived and every tracked file has an explicit comparison.

## Requirements inventory

The complete issue, all issue comments, sub-issue list, PR conversation, inline
comments and reviews were retrieved with pagination. At intake the comment and
sub-issue lists were empty. There are no required sub-issues. The issue's prose
and subsequent user instructions produce these acceptance criteria:

| ID | Requirement | Implementation / verification evidence | State |
| --- | --- | --- | --- |
| R1 | Investigate the cited failed main run and both cited successful automation runs | Complete logs and run/job metadata under `ci-logs/` and `github/`; timeline below | Collected and analyzed |
| R2 | Find false positives, false negatives, warnings and errors throughout CI/CD | Root-cause table; minimal before/after regressions; complete workflow/script review | Code fixes implemented; hosted verification tracked in `VERIFICATION.md` |
| R3 | Compare the **full file tree** of all four named templates, in C#, C++, Python, Rust priority | `research/TEMPLATE-COMPARISON.md`, 620-row hashed manifest, four complete archives; reproducible inventory experiment | Complete inventory and applicable transfer |
| R4 | Reuse applicable template practices across all workflows/helpers | Full comparison plus 17-principle mapping below; Python quality, full actionlint image, standalone ShellCheck, Git initialization and artifact checks added | Implemented, with dependency ecosystem limit stated below |
| R5 | Follow the linked hive-mind CI/CD best practices | Frozen full document and principle-by-principle assessment below | Release authorization remains externally blocked |
| R6 | Fix all occurrences across the codebase | SARIF validation covers every language; Python quality covers all 17 maintained helper modules; raw extraction covers every artifact download; checkout configuration covers every checkout-bearing workflow | Implemented; latest hosted run must verify |
| R7 | Keep all work in existing PR 542 and prepared branch | Forward commits on `issue-541-ef4751af423d`; default branch included in ancestry; no main writes | In progress until final verification |
| R8 | Preserve all related logs/data in the requested repository folder | This folder; raw GitHub JSON, logs, dependency advisories, source snapshots, experiments and evidence indexes | Collected; final CI evidence appended after completion |
| R9 | Deep analysis, online research, timeline, root causes, alternative solutions and plans | This document, full sources under `research/`, references below | Complete analysis; external release recovery plan unfinished |
| R10 | Research existing components/libraries solving similar problems | Component assessment below (actionlint, Ruff, mypy, pip-audit, zipfile, CodeQL, Conan audit) | Complete |
| R11 | Add default-off verbose diagnostics if evidence cannot establish a cause | All observed causes reproduced; existing `CI_VERBOSE=1` native/C# tracing preserved and Python check tracing added; actual logs retained | No unexplained failure is represented as fixed |
| R12 | Report shared upstream defects with reproduction, workaround and code suggestions | Existing action issue supplemented; C# template #81, C++ template #15, Python template #125 and Rust template #223 filed; full report text retained | Reported, applicable downstream repairs implemented |
| R13 | Reproduce bugs before fixing; retain minimal experiments and automated regressions | `tests/reproductions-before.log`, Buffer and missing-tools experiments, baseline Ruff/mypy/audit reports; regression suite | Reproduced and covered |
| R14 | Test all applicable features, preserve existing functionality, respect memory limit | .NET 298 tests and examples/consumer; documentation; helper tests; native compile deliberately stopped near cgroup cap, hosted native matrix required | See verification record; no local native pass claimed |
| R15 | Update title/body, preserve closing keyword, mark ready, ensure clean tree and latest-SHA passing checks | PR description and final verification record; `Fixes #541` remains present | Finalization follows hosted checks |
| R16 | Fully complete every criterion, or identify exact genuine external blockers without claiming completion | GH006 policy evidence and native audit-service limitation below | Release criterion explicitly unfinished; PR must remain unmerged |

## Timeline (UTC, 2026-10-29 unless specified)

Run IDs, exact SHAs and event times are retained in the `github/run-*.json` files;
the runs list is a historical snapshot, not proof of the latest commit.

1. 2026-08-10: fpdf2 publishes GHSA-wqw6-wpr3-45rh and GHSA-rw7v-4hrq-x6r9,
   both fixed in 2.8.8. The repository later retains 2.8.7 without a Python audit.
2. 2026-09-18: the upstream download-artifact Buffer modernization PR #490 begins.
   It remains open; latest released v8.0.2 still contains the obsolete extractor.
3. The cited dependency-graph run `40848691817` succeeds at `c083793`; this
   only updates the graph. It does not prove native tests or release permission.
4. 15:24:46: Dependabot run `40901288554` succeeds at `d56984a`; dependency-update
   automation success is likewise separate from the library release pipeline.
5. 16:43:54: PR #540 run `40914437917` fails full-history Gitleaks on public
   DocFX source-map identifiers. Lines 1980–2018 record exact paths/fingerprints
   and five findings. Current main already contains the narrow, reproduced
   historical false-positive fingerprint exception; this PR does not broaden it.
6. 16:47:42: run `40915053958` is superseded/cancelled. This is not evidence
   that the current commit's native compiler is broken.
7. 16:51:02: PR run `40915593257` succeeds at `461178d` after the targeted repair.
8. 17:03:35–17:18:56: cited main run `40917637619` at `7c6ade6` validates builds,
   consumers, documentation, security and links. OIDC succeeds. Artifact downloads
   at 17:18:21, 17:18:22 and 17:18:31 emit DEP0005. At 17:18:31 the generated
   0.18.4/0.1.1 release metadata push is rejected with GH006. `publish` fails;
   `required` correctly propagates the failure.
9. 18:09:57: issue #541 opens, citing the above main failure.
10. 18:10:27: newer main run `40928285351` at `c1a96a1` starts; the prepared PR
    includes this main merge. It fails at the same release push at 18:19:00.
    This independently confirms the live blocker; lines 18581–18679 repeat the
    three Buffer warnings and rule refusal.
11. 18:11:14: prepared PR run `40928407948` at `9c5093c` succeeds. Publication
    is intentionally inapplicable on PRs; this green result does not contradict
    the main failure. The auto-merge workflow skips a non-Dependabot PR as designed.
12. Local investigation reproduces missing SARIF completeness checks, tool
    omission, lint/type errors, vulnerable fpdf2, and the two-byte Buffer warning.
    GitHub's current open Dependabot alerts #3/#4 confirm the two fpdf2 findings.
13. Implementation commit `1de337ed5955a5036475edc4532a85a8cd3494d3` is pushed;
    hosted run `40931698011` starts at 18:31:59. Later evidence and any additional
    repair commits are recorded separately in `VERIFICATION.md`.

## Root causes, options and solution plans

| Problem | Cause and evidence | Options considered | Chosen implementation / verification |
| --- | --- | --- | --- |
| Main cannot finish releases | `CI-40917637619.log:1562–1570`: GH006, unauthorized push, protected hook refusal; repeated in newer main. `github/main-protection.json` has restrictions with empty users, teams and apps; rulesets list is empty | Administrative authorization for intended actor or an authorized release App; force push/removing protection and automatic release PRs would contradict the requested branch policy and linked practices | Keep fail-once classified refusal and actionable instructions. Finite real bare-remote hook regression proves one attempt, no force/rebase, remote main unchanged. **Administrator recovery remains required**, followed by a real trusted-main publish and anonymous consumer verification |
| DEP0005 on every artifact extraction | Pinned v8.0.2 bundle uses `new Buffer` in unzip-stream's MatcherStream; exact two-byte offline probe produces stack trace. All three production downloads use default extraction | Wait for open upstream #490; pin an unmerged fork; suppress warning; use maintained ZIP extractor | Download raw ZIP with digest check still enabled, validate all member paths/symlinks, extract using stdlib zipfile. All download sites changed. PR quality job performs real round trip under `--throw-deprecation`; unit tests preserve package bytes and reject unsafe/ambiguous archives |
| Clean-looking incomplete SARIF accepted | Gate iterated `run.get('invocations', [])` and checked only explicit false; absence/unknown execution state passed. Empty dict/string results also passed | Trust analyze exit alone; accept absent invocation; require evidence in every run | Require results list, nonempty invocation list, every executionSuccessful exactly true; preserve fatal findings and warning/error tool diagnostics. Original cases fail before and pass after; real CodeQL artifacts tested |
| Workflow lint silently omitted code checks | CI copied only actionlint from the image; no Pyflakes installation and no mandatory native-tool check. Minimal invalid shell passes extracted binary when PATH lacks ShellCheck | Install tools separately everywhere; rely on runner images; execute complete image | Run entire digest-pinned image containing both integrations. Native mode fails if either tool is missing. Standalone maintained .sh files also pass ShellCheck; frozen upstream evidence is explicitly excluded |
| Python source defects escaped mandatory CI | No Ruff/formatter/type gate; baseline finds unused import and three mypy errors | Add another lint framework; use existing Python toolchain | Pin Ruff/mypy/stubs/Pyflakes; lint/format/type all maintained scripts, add pre-commit parity, run all regression tests. Fix max key typing, exception variable reuse, typed PDF WrapMode and unused import; format existing helpers consistently |
| Known vulnerable PDF dependency remained green | Only NuGet tree audited; full source CodeQL does not audit pinned Python packages. `baseline-python-audit.json` finds two advisories in fpdf2 2.8.7; Dependabot alerts agree | Ad hoc advisory ignore; generic source scanner; package-aware dependency audit | Upgrade to 2.8.8; schedule/full CI pip-audit over all three requirements surfaces and transitive resolutions, retain JSON, fail on findings or audit failure. No untrusted XML stress probe is needed to prove a published package advisory |
| Python helpers absent from CodeQL | Language matrix contained C#, C++ and Actions only | Assume C# or Actions analysis covers Python; add separate code scanner | Add Python CodeQL to the fatal-SARIF matrix, covering maintained helpers and historical experiments; exclude only immutable `dev/log/**` source snapshots from Python extraction. Preserve actual traced builds for C#/C++ |
| Repeated initial-branch advice in successful jobs | checkout/recipe git init had no explicit initial branch | Set global git config; ignore broad warning patterns; configure Git before checkout | Workflow-local GIT_CONFIG_* in all checkout-bearing workflows, as templates recommend; no global developer configuration mutation |
| Documentation described only one secret-scan exception | Current main already has a second exact historical DocFX identifier fingerprint | Broaden scanner exclusion; leave misleading documentation | Correct contributor documentation; retain both exact historical exceptions and successful full-history scan |

These changes do not alter library APIs or remove existing compiler, sanitizer,
coverage, package-consumer, authentication fallback, documentation or release
channels. No package-version change is made: repository policy uses existing
pending C#/C++ fragments at release time, and this PR changes tooling only.

### Findings exposed by the first implementation run

Run `40931698011` completed at 18:43:01 with every applicable job successful
except Python CodeQL and the dependent required gate. The analysis action itself
succeeded; the explicit SARIF gate correctly rejected five findings. Exact output
is at `ci-logs/CI-40931698011.log:7930–7935`, and the original report is retained
in `research/first-python-codeql.tar.gz`.

| Finding | Classification and repair |
| --- | --- |
| `py/file-not-closed` in `dev/log/issues/524/.../upstream-scripts/format-csharp-files.py:12` | Obsolete upstream source copied as immutable research evidence, not an executable helper. Preserve its bytes; exclude `dev/log/**` from Python extraction using `.github/codeql/python.yml`. No query/result suppression is added |
| `py/uninitialized-local-variable` in `scripts/smoke_packages.py:90` | The variable was assigned and used under correlated C++ guards; initialize the typed header list explicitly so its validity is evident to both readers and the analyzer |
| `py/multiple-definition` in `experiments/update_ci_workflow.py:79` | Dead assignment overwritten by the next regular-expression search; remove only that assignment |
| `py/repeated-import` in `scripts/patch_conan_recipes.py:42` | The function is injected into standalone Conan recipes via `inspect.getsource`, so its local import is required. Keep the self-contained function and move other imports into the functions that use them |
| `py/unused-import` in `experiments/test_issue_541.py:7` | New regression file's unused import; remove it |

The 96 helper regressions, Ruff, formatting, mypy, workflow checks and package
consumer all pass after these repairs. Final hosted evidence is recorded in
`VERIFICATION.md`; findings remain fatal at every severity.

## Best-practice assessment

The full linked document is frozen as `research/CI-CD-BEST-PRACTICES.md`.

| Principle | Repository implementation and assessment |
| --- | --- |
| 1 Relevant changes | changes.py uses real event base, deletion-aware no-renames diff, distinct language/docs/pipeline booleans; pyproject.toml now routes full pipeline validation. Quality/security/links remain mandatory |
| 2 File limits | check_repository.py applies 1500-line limit to maintained source and validates syntax/configuration; frozen evidence and historical experiments remain separate |
| 3 Formatting | dotnet format whitespace, clang-format-18, Ruff format for maintained Python helpers; local hooks mirror these |
| 4 Static checks | C# analyzers and compiler warnings fatal; native Clang-Tidy/Cppcheck use real compile database; Ruff/mypy, actionlint integrations, standalone ShellCheck, zizmor and four-language CodeQL |
| 5 Fast failure | quality precedes expensive builds; credentials before trusted release builds; tests/builds have hard limits. Branch authorization cannot be proved by a read-only token introspection; actual push remains authoritative |
| 6 Changesets | independent JSON fragments, strongest bump per language, no manual version edits, generated changelog, idempotent preparation |
| 7 Current merge | full history, explicit fresh merge of current main before testing, release writer refuses source advances after validation |
| 8 Hooks | repository, pipeline regressions, native/C# formatting, Python quality, full-history credential gate documented |
| 9 Releases | tested artifacts, NuGet plus GitHub assets and docs, direct allowlisted metadata commit, no human-only release PR. **Blocked by existing branch authorization**; no workaround pretends otherwise |
| 10 Concurrency | per-job keys; cancellable stale PR validation; shared queued noncancellable main writer with queue:max and narrow schema validation; bounded classified push retries |
| 11 Secrets | full-history SHA-verified Gitleaks with redaction and exact historical fingerprints, no new broad allowlists; no token payloads retained |
| 12 Documentation | DocFX warnings-as-errors, Doxygen, Unicode source PDF, generated local/deployment links and maintained root external Markdown links with bounded retries; live link failure remains fatal |
| 13 Native image architectures | No container images are published by this library. Do not add desktop/JS products or emulated architecture builds just to copy template jobs |
| 14 Workflows | full digest-pinned actionlint image, required native integrations, standalone ShellCheck and zizmor; all third-party actions SHA-pinned and runners fixed |
| 15 Dependencies | Locked NuGet all-transitive audit plus all maintained Python requirements and transitive dependency audit on PR/main/schedule. Native Conan provider coverage is **not proven**: its official service requires account activation/token and does not automatically identify locally renamed Platform recipes. The bounded unauthenticated probe and official documentation are retained; no clean native advisory verdict is fabricated. See limit below |
| 16 Publish preflight | Real NuGet OIDC exchange before builds; controlled stored fallback only after rejection; actual push/anonymous indexing validation; push scope explicitly unverified when impossible to prove without publication. Read-only repository API success does not prove main push authorization |
| 17 Changed world | Unknown registry/network responses fail honestly; GH006/013 do not get retried as transient failures; actual job applicability notices, fatal missing artifacts, retained audit/SARIF/link/test evidence and terminal required gate |

## Online research and existing components

Primary sources checked on 2026-10-29 are linked below; frozen API responses and
source snapshots are under `research/` and `github/`.

- [GitHub branch protection](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
  explains why a workflow write permission cannot override the push restriction.
  Preserve protections; an administrator must provision an allowed release actor.
- [actionlint integration documentation](https://github.com/rhysd/actionlint/blob/main/docs/usage.md)
  documents optional native integrations and the image bundling ShellCheck and
  Pyflakes. Reusing that image avoids maintaining a partial local tool stack.
- [download-artifact](https://github.com/actions/download-artifact) documents raw
  download and default digest validation. Existing [issue #484](https://github.com/actions/download-artifact/issues/484)
  and [open PR #490](https://github.com/actions/download-artifact/pull/490) establish
  that this is an upstream extraction-chain defect, still present in the pinned
  latest release. Python zipfile avoids an additional third-party ZIP library.
- [fpdf2 SVG advisory](https://github.com/py-pdf/fpdf2/security/advisories/GHSA-wqw6-wpr3-45rh)
  and [image advisory](https://github.com/py-pdf/fpdf2/security/advisories/GHSA-rw7v-4hrq-x6r9)
  identify the vulnerable/fixed releases. Repository input is trusted source text;
  upgrading avoids retaining a known vulnerable general-purpose renderer anyway.
- [Ruff configuration](https://docs.astral.sh/ruff/configuration/),
  [mypy configuration](https://mypy.readthedocs.io/en/stable/config_file.html), and
  [pip-audit](https://github.com/pypa/pip-audit) provide existing lint, formatting,
  typing and advisory components. They are pinned and audited rather than
  replaced with custom source-style or advisory matching code.
- [Conan audit](https://docs.conan.io/2/security/audit.html) requires activated
  provider credentials. No token is available in this task. Its absence is an
  audit coverage limit, not evidence that native dependencies are vulnerability-free.
  The follow-through plan is to provision the provider, explicitly map original
  upstream coordinates for local aliases, preserve the resolved graph and fail
  on findings/unknown package coverage; that service-backed verdict is unfinished.
- [GitHub concurrency queue announcement](https://github.blog/changelog/2026-05-07-github-actions-concurrency-groups-now-allow-larger-queues/)
  confirms queue:max syntax; the preexisting narrow actionlint exception is
  preserved with its explicit schema validation, rather than disabling lint.

## Upstream reports and external recovery

The action's existing report was supplemented with a precise two-byte
reproduction, pinned revision, workaround and source-level fix suggestions:
https://github.com/actions/download-artifact/issues/484#issuecomment-6505041826.
The Python and Rust templates have the same default-extraction sites, reported
at https://github.com/link-foundation/python-ai-driven-development-pipeline-template/issues/125
and https://github.com/link-foundation/rust-ai-driven-development-pipeline-template/issues/223.
C#/C++ templates have no corresponding artifact download sites at the archived
revisions; no duplicate artifact report was filed there. Their missing full-tree
dependency audit and fatal CodeQL verdict are reported separately at
https://github.com/link-foundation/csharp-ai-driven-development-pipeline-template/issues/81
and https://github.com/link-foundation/cpp-ai-driven-development-pipeline-template/issues/15.
Existing Python #122 and Rust #221 already cover the analyzer-exit-versus-findings
gap; those were checked rather than duplicated. fpdf2 already has fixed upstream
advisories; a duplicate vulnerability issue would add no useful evidence.

Unfinished release recovery: an administrator must authorize metadata updates by
the release actor under the current protection (or configure a dedicated allowed
App/token), rerun CI on the current merge, and verify NuGet/GitHub assets and public
downloads. A dry-run push, successful login, read-only API call, or green PR is
insufficient evidence. The current task explicitly limits writes to its PR branch,
so no branch policy is changed and the PR is left unmerged. Native advisory service
coverage also remains unverified until its credentials/coordinate mapping exist.
