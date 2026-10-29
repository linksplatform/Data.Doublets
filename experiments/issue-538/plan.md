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

Objective: implement every requirement of issue 538 in PR 539, verify all applicable
validation and publication on the updated main revision, and record evidence in
the issue. Keep incomplete external acceptance explicit. Preserve credential
security, fallback secrets, validated artifacts, automatic versions and required
job expectations. Work only on issue-538-06fae78405da; preserve forward history.

## Investigation

- [x] Read complete issue, comments, PR conversation, inline comments and reviews.
  No comments/reviews or formal sub-issues existed at initial inspection.
  Related upstream management/template issues are references, not required children.
- [x] Read contributing guidelines, current workflow, release code and regressions.
- [x] List recent CI with timestamps/SHAs and download baseline failed main logs.
  Run 40866410787 attempt 2, SHA c184a914504acd820aa6343462be6c869d12a1fd:
  log lines 160–172 show successful exchange followed by both verifykey HTTP 400s.
  Full logs are retained locally in ci-logs/main-40866410787.log (10,124 lines).
- [x] Trace Gallery verification policy and official pinned login source.
  PackageVerify policy may reject push-only keys before package scope evaluation;
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
  malformed response and publication validation failure; never mask build defects.
- [x] Bounded sanitized response diagnostics; redact credentials, omit raw headers,
  exception argv and secret-bearing subprocess output.
- [x] Fresh per-job login and C++ refresh, job-scoped id-token permissions, trusted
  main only; no secrets in cross-job outputs/artifacts; preserve fallback secret.
- [x] Synchronize repository policy checker, regression tests, contributing and
  trusted-publishing docs. Follow existing version rules; no manual version bump.
- [ ] Local complete Python regressions, repository checks, workflow lint/security,
  native formatting, secrets scan and appropriate C#/C++/documentation validation.
  Large logs go to files. Native compiler experiments get finite resource caps;
  hosted CI handles builds that exceed this container's 2.92 GiB shared limit.
- [x] Commit atomic steps after local checks, push only prepared branch; update
  PR title/body with reproduction, tests, requirements/evidence and Fixes #538.
- [x] Inspect every PR diff for regressions and scope; merge latest main into branch
  if needed, check clean status; download/analyze all failing current-SHA CI logs.
- [x] Verify full PR CI: both C# OS legs, GCC/Clang/sanitizers, discovered native
  tests, coverage, benchmark smoke, static analysis, FFI, packages/consumers, API
  docs/PDF/DocFX/Doxygen, quality, links, all security and required aggregate.
- [x] Mark PR 539 ready with gh pr ready after implementation/validation.
- [ ] Updated main run and publication acceptance: use existing matching-SHA run,
  otherwise dispatch official ci.yml on main; never rerun obsolete code or duplicate
  a run. Diagnose downstream failures; settings-only repairs rerun affected jobs.
- [ ] Verify metadata commit, tested artifact paths, GitHub write permissions,
  authentication per package, exact anonymous NuGet downloads/public consumers,
  GitHub Packages mirrors, public release/assets and documentation deployment.
- [ ] Record successful main URL/SHA/job conclusions, exact versions, auth choices,
  release URLs and any remaining limits in issue description. Reopen issue if
  prematurely closed. Main acceptance remains pending until actually verified.

## Local implementation evidence

- Added issue 538 reproductions before implementation: initial run failed 16 tests
  (34 parameterized errors), including the live successful-exchange/400 fixture.
- Corrected suite: 83 regressions pass; repository/changeset checks,
  actionlint 1.7.12 + ShellCheck, zizmor 1.29.0, native formatting and full-history
  Gitleaks pass (3,299 commits scanned, no credential leaks).
- scripts/check-csharp.sh passes restore/build/format, all 193 tests, both examples,
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
  ci-logs/codacy-539.json; fixed unused import/style and documented narrowly scoped
  fixture/argument-list exceptions without disabling any repository checks.

## Main follow-through

- PR CI 40881933352 passed every applicable job, including required. PR 539 was
  marked ready and merged at d56984a3f0021ab332b5ae5d05ae3aa47512c92c.
- Main run 40883578849 passed credentials, both C# legs, all native legs, docs,
  quality, links and every security analysis. Publishing failed before metadata
  and package pushes at the existing GitHub repository-role permission check.
  Log ci-logs/main-publish-123485886053.log lines 451–468 preserves exchange
  success and the rejected write preflight; lines 1547–1548 prove the very same
  job token subsequently pushed gh-pages successfully. The original probe did
  not log HTTP status or body, so its exact response is unavailable.
- Added reproductions for optional/false/true repository-role fields and fatal
  HTTP/malformed responses before the correction. Repository readability must
  not be called proof of installation-token write permissions. Actual metadata,
  mirror and release operations remain strict, as do required-job expectations.
- Documentation deployed at gh-pages a558df87521024c50433f8300ec63659ed6f0880;
  Pages reports the build successful. Package publication is still pending.
- Issue 538 remains open. PR 539 is already merged; GitHub cannot add another
  correction to that merge. Any additional main code correction requires a
  follow-up PR, which conflicts with the requested single-PR constraint.
- Branch run 40886193744 exposed five history-scan false positives introduced
  by the first successful documentation deployment. All five are public
  JavaScript source-map identifiers at one exact historical fingerprint;
  experiments/issue-538/source-map-findings.md records evidence/reproduction.
  The exception covers only that commit/path/rule/line, with full-history
  scanning retained. This correction is also pending main integration.
