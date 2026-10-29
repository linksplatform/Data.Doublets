<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": [
      "# Issue 535 work plan",
      "*"
    ]
  },
  "MD013": {
    "tables": false
  }
}
-->

# Issue 535 work plan

Objective: implement and verify all Data.Doublets trusted-publishing requirements
in PR 536; leave external acceptance criteria explicitly unfinished until verified.

1. Read issue 535 and all comments; read PR conversation, inline comments and
   reviews; read linked template issue 80 and management issues 72 and 29.
2. Preserve relevant failed CI logs, verify timestamps and commit IDs, and trace
   the rejected credential to its actual failure. Review official NuGet/login
   source, pin its v1 commit, and inspect NuGet verification endpoint compatibility.
3. Add failing regressions for missing/invalid usernames, absent/rejected exchange,
   ephemeral credentials, package scope reporting, PR isolation, and fresh login
   before metadata and each package push.
4. Implement job-level OIDC, actionable setup checks, fresh per-job/per-package
   authentication, no implicit legacy fallback, and honest verification evidence.
5. Update setup/contribution documentation, repository policy validation, release
   changeset, and requirement/evidence inventory. Preserve recovery, anonymous
   downloads, both consumer smoke tests, and truthful publication failure.
6. Run local regressions and repository/workflow checks; run available language
   checks sequentially within the memory limit and use CI for remaining platforms.
7. Review the full diff, merge current main if needed, commit atomic changes, push
   only issue-535-70622edb4ab7, update PR 536, monitor current-SHA CI and inspect
   every failed check's saved logs; mark the PR ready after implementation.
8. Through the management tools, verify actual NuGet profile/owner, narrow policy
   for linksplatform/Data.Doublets/ci.yml (no environment), and NUGET_USER variable.
   Run the new workflow revision after merge; verify exact public versions and
   consumers. Audit obsolete secrets only after verified OIDC release.

## Initial evidence and boundaries

- Issue and PR discussions read; PR has no comments or reviews at investigation.
- Related cross-repository issues are upstream tracking work, not implemented
  by merely linking them. Their tool capabilities must not be claimed available.
- CI 40848685729, commit c083793a, credentials log lines 1214–1217: HTTP 403
  for both legacy-key verification calls. Expiry versus scope is undetermined.
- Repository Actions variables are empty. Publishing jobs use no environment.
- No package-registry-manager, gh-manager or browser-commander executable/plugin
  is available. Available browser reaches NuGet sign-in, not an authenticated
  policy page. Actual profile and saved policy cannot currently be verified.
- Publishing acceptance needs the reviewed workflow on main. This task must not
  merge its own PR or report a PR-only test as a successful public OIDC release.

## Requirement / evidence inventory

| Requirement | Implementation / verification |
| --- | --- |
| Job-level OIDC on trusted main only | `ci.yml` credentials/publish permissions and explicit non-PR main conditions; workflow regressions |
| Official action and actual username | Reviewed `NuGet/login` v1.2.0 commit `8d196754b4036150537f80ac539e15c2f1028841`; all logins use `vars.NUGET_USER`; profile validation rejects missing/email values |
| Actual pre-build exchange | Credentials runs official login before preflight; missing output fails; no cross-job key output |
| Honest scope verification | `verify_nuget` uses the original temporary credential on direct `verifykey`; source confirmed at NuGetGallery `c25bab040e19d24593fc0c298a0cedb51bf57dcb`; first publication is explicitly unverified |
| Fresh publishing keys | Independent login before metadata/C#; another before C++; job timeout 40 minutes versus one-hour lifetime |
| No automatic legacy fallback | Removed every secret reference from CI; repository checker and regressions reject its reintroduction |
| Authentication before metadata | Download artifacts, login, check both scopes and GitHub permissions, then prepare/commit; failed metadata blocks package pushes |
| Preserve recovery and truthful success | Existing `publish`, anonymous indexing, asset repair, isolated C#/native smoke code retained; existing publication regressions pass |
| Release trigger | `changes/535-trusted-publishing.json` requests patch releases for both package IDs |
| NuGet policy/owner/profile and GitHub variable | **Unfinished external setup**: manager tools unavailable, unauthenticated NuGet browser, no verified actual profile; repository variables empty |
| Successful keyless release and exact public versions | **Unfinished external acceptance**: requires saved policy/variable, reviewed workflow on main, successful run, public-feed consumers |
| Idempotent setup and targeted rerun | Procedure documented; **live verification unfinished** with manager prerequisites unavailable; no unrelated CI restarted |
| Obsolete-secret cleanup | Intentionally not performed before verified OIDC publication and consumer audit |

## Validation checkpoints

- New regressions were run before implementation with the old workflow/preflight:
  failures included missing username still accepting a stored key, legacy symbol
  verification, secret fallback and missing job-level OIDC.
- Python regressions: 64 pass, including 17 trusted-publishing tests.
- Repository policy, changeset policy, actionlint/ShellCheck, native formatting,
  `git diff --check` and zizmor 1.29.0: pass.
- Gitleaks 8.30.1: 3,289 commits / 332 MB scanned, no leaks.
- Local .NET SDK 8.0.427: warning-free build, all 193 tests, both examples,
  packaging and isolated C# consumer pass. Measured peak child RSS 472,700 KiB;
  GC heap capped at 768 MiB and shared compilation disabled.
- Native dependency setup passed. Project compile reached the intentional
  1.25 GiB address-space limit (`virtual memory exhausted: Cannot allocate
  memory`, peak child RSS 1,219,612 KiB). No repeated memory-heavy local build;
  native tests/benchmark/static analysis/consumer validation require the hosted
  runner. Native formatting passed. Logs remain in local ignored `ci-logs/`.
- No formal child issues are registered under #535 (`sub_issues` API returns []).
  All three linked upstream issue bodies and their comments were read.
- Current-SHA CI logs and Codacy API responses are saved in `ci-logs/`.
  Codacy on `30f0228` reported 22 new findings: dummy test credentials,
  subprocess calls, and Markdown headings/table width. These now follow the
  repository's scoped Bandit annotations and per-file Markdown configuration.
  Its potential complexity finding in `release.py:89` is addressed by extracting
  NuGet/GitHub verification helpers. All 64 pipeline regressions pass after the
  refactor; no new Bandit findings remain locally, and Markdown checks pass.
