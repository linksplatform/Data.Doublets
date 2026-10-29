<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": [
      "# Issue 538 work plan and acceptance evidence",
      "*"
    ]
  }
}
-->

# Issue 538 work plan and acceptance evidence

Objective: implement every requirement of issue 538 in PR 540, verify all
applicable
validation and publication on the updated main revision, and record evidence in
the issue. Keep incomplete external acceptance explicit. Preserve credential
security, fallback secrets, validated artifacts, automatic versions and required
job expectations. Work only on issue-538-104e1d46cf25; preserve forward history.

## Investigation

- [x] Read complete issue, comments, PR conversation, inline comments and
  reviews.
  No comments/reviews or formal sub-issues existed at initial inspection.
  Related upstream management/template issues are references, not required
  children.
- [x] Read contributing guidelines, current workflow, release code and
  regressions.
- [x] List recent CI with timestamps/SHAs and download baseline failed main
  logs.
  Run 40866410787 attempt 2, SHA c184a914504acd820aa6343462be6c869d12a1fd:
  log lines 160–172 show successful exchange followed by both verifykey HTTP
  400s.
  Full logs are retained locally in ci-logs/main-40866410787.log (10,124 lines).
- [x] Trace Gallery verification policy and official pinned login source.
  PackageVerify policy may reject push-only keys before package scope
  evaluation;
  the discarded baseline response does not establish its exact live cause.
- [x] Add failing minimal regression fixtures before implementing corrections.

## Implementation and verification map

- [x] Compatible preflight in credentials-only, pre-metadata and both language
  callers; explicit unsupported package-scope proof, actual authentication,
  first-publication limitations and no derived-key subject overclaim.
- [x] OIDC preferred even with valid/expired fallback; invalid/missing username,
  configuration or exchange failure can consider only provided NUGET_TOKEN.
- [x] Bounded controlled fallback on actual registry 401/403, same artifacts and
  version, anonymous exact-version check before retry; both failures diagnosed.
- [x] Distinguish unsupported verification, authentication, transient/transport,
  malformed response and publication validation failure; never mask build
  defects.
- [x] Bounded sanitized response diagnostics; redact credentials, omit raw
  headers,
  exception argv and secret-bearing subprocess output.
- [x] Fresh per-job login and C++ refresh, job-scoped id-token permissions,
  trusted
  main only; no secrets in cross-job outputs/artifacts; preserve fallback
  secret.
- [x] Synchronize repository policy checker, regression tests, contributing and
  trusted-publishing docs. Follow existing version rules; no manual version
  bump.
- [ ] Local complete Python regressions, repository checks, workflow
  lint/security,
  native formatting, secrets scan and appropriate C#/C++/documentation
  validation.
  Large logs go to files. Native compiler experiments get finite resource caps;
  hosted CI handles builds that exceed this container's 2.92 GiB shared limit.
- [x] Commit atomic steps after local checks, push only prepared branch; update
  PR title/body with reproduction, tests, requirements/evidence and Fixes #538.
- [ ] Inspect every PR diff for regressions and scope; merge latest main into
  branch
  if needed, check clean status; download/analyze all failing current-SHA CI
  logs.
- [ ] Verify full PR CI: both C# OS legs, GCC/Clang/sanitizers, discovered
  native
  tests, coverage, benchmark smoke, static analysis, FFI, packages/consumers,
  API
  docs/PDF/DocFX/Doxygen, quality, links, all security and required aggregate.
- [ ] Mark PR 540 ready with gh pr ready after implementation/validation.
- [ ] Updated main run and publication acceptance: use existing matching-SHA
  run,
  otherwise dispatch official ci.yml on main; never rerun obsolete code or
  duplicate
  a run. Diagnose downstream failures; settings-only repairs rerun affected
  jobs.
- [ ] Verify metadata commit, tested artifact paths, GitHub write permissions,
  authentication per package, exact anonymous NuGet downloads/public consumers,
  GitHub Packages mirrors, public release/assets and documentation deployment.
- [ ] Record successful main URL/SHA/job conclusions, exact versions, auth
  choices,
  release URLs and any remaining limits in issue description. Reopen issue if
  prematurely closed. Main acceptance remains pending until actually verified.

## Local implementation evidence

- Added issue 538 reproductions before implementation: initial run failed 16
tests
  (34 parameterized errors), including the live successful-exchange/400 fixture.
- Corrected suite: 83 regressions pass; repository/changeset checks,
  actionlint 1.7.12 + ShellCheck, zizmor 1.29.0, native formatting and
  full-history
  Gitleaks pass (3,299 commits scanned, no credential leaks).
- scripts/check-csharp.sh passes restore/build/format, all 193 tests, both
examples,
  .nupkg/.snupkg creation and isolated consumer. Child peak RSS 558,932 KiB.
- Pipeline-only changes do not require a new changeset. Existing changesets
  (including 535) remain for normal automatic version preparation; no versions
  were manually edited.
- New releases remain drafts if NuGet authentication/public download fails;
  mirror failures still allow public asset repair after NuGet succeeds.
- Issue step 7 explicitly requires merging the reviewed correction before live
  main verification. Only PR merge after successful required checks is in scope;
  no direct source push to main or branch-protection bypass is authorized.

- First hosted run 40881539681 passes quality and links; full matrix underway.
  Codacy reported 14 new findings: unused test import, mock credential literals,
  explicit subprocess review and Markdown format. Preserved its API response in
  ci-logs/codacy-539.json; fixed unused import/style and documented narrowly
  scoped
  fixture/argument-list exceptions without disabling any repository checks.

## PR 540 continuation checklist (2026-10-29)

- [x] Read current issue, all comments, sub-issues (none), PR 540 conversation,
  inline comments and reviews (none), and prior PR 539 discussion/evidence.
- [x] Verify prepared branch and current main SHA; main remains d56984a3f.
- [x] Preserve failing main publication and subsequent audit logs under
  ci-logs/.
- [x] Reproduce repository-role permission bug before applying prepared fix.
- [x] Reproduce exact historical DocFX scan findings before applying exception.
- [x] Bring both reviewed corrections into this PR with forward-moving commits.
- [x] Run all Python regressions, repository/version checks, workflow
  lint/security,
  and full-history credential scan; run feasible local builds sequentially.
- [x] Push only prepared branch, review complete diff, update PR
  title/description.
- [ ] Verify current-SHA complete PR CI and Codacy; mark PR 540 ready.
- [ ] Merge PR through normal protected-branch checks, per issue step 7.
- [ ] Follow the single matching updated-main CI run, diagnose downstream
  failures,
  and retain explicit unfinished evidence for any external blocker.
- [ ] Verify publication auth, metadata commit, both exact NuGet downloads and
  consumers, mirrors, public release assets, and served documentation.
- [ ] Record final main SHA, run, all job conclusions and publication evidence
  in
  issue description; do not claim completion if any required criterion is unmet.

Existing merged authentication implementation and its acceptance mapping above
remain applicable. PR 540 is the explicitly authorized continuation; the former
single-PR authorization obstacle recorded on PR 539 is resolved by this task.
No manual version edits or fallback-secret changes are required.

PR 540 reproduction evidence: `ci-logs/github-preflight-before.log` records five
assertion failures before the correction, across missing/empty/false/true
repository
role fields and HTTP diagnostic retention. The initial environment also lacked
PyYAML; dependency installation is tracked separately from these real failures.
`ci-logs/source-map-before.log` reproduces five findings with Gitleaks 8.30.1 at
exact historical commit a558df87521024c50433f8300ec63659ed6f0880.
The source changes reuse prepared corrections 10c484074 and c2245b095; this task
supplies the follow-up PR authorization missing from the previous session.

Current continuation validation: all 85 regressions, repository/version policy,
actionlint/ShellCheck, zizmor and native formatting pass. Full-history Gitleaks
scans 3,307 commits / 377.72 MB with no findings. Local C# validation passes all
193 tests, both examples, both package types and its isolated packaged consumer.
Codacy reported only two Markdown line-length findings; both are corrected
without suppressing any rule. Native validation uses one worker and a finite
per-process memory cap; hosted CI supplies any validation exceeding this
container's 2.92 GiB shared memory limit.
