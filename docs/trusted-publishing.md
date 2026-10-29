<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": [
      "# NuGet trusted publishing",
      "*"
    ]
  }
}
-->

# NuGet trusted publishing

Both `Platform.Data.Doublets` (C#) and
`Platform.Data.Doublets.TemplateLibrary` (C++) publish through GitHub Actions OIDC.
Trusted publishing is always attempted first. The existing provided
`secrets.NUGET_TOKEN` is considered only when that path is unavailable or fails.
PR validation, including fork PRs, builds and consumes local packages without
publishing credentials. Only non-PR execution on `refs/heads/main` can authenticate.

## Configure and verify the account

Use package-registry-manager for NuGet policy management and gh-manager for
generic GitHub settings. Their implementation prerequisites are tracked in
[package-registry-manager #72](https://github.com/link-foundation/package-registry-manager/issues/72)
and [gh-manager #29](https://github.com/link-foundation/gh-manager/issues/29).
Those reports do not establish that setup is implemented or a policy exists.
The related template migration is
[template #80](https://github.com/link-foundation/csharp-ai-driven-development-pipeline-template/issues/80).

1. Authenticate to the NuGet account that creates the policy. Read its actual
   profile name from NuGet; do not infer it from the GitHub username or use an
   email address. For organization-owned policies, login uses the **creator's**
   profile name, while the policy owner is the package-owning organization.
2. Prefer official CLI or supported API operations after checking capabilities.
   `dotnet nuget trust` manages package-signature trust, not publishing policies.
   Current official policy setup uses NuGet's authenticated
   [Trusted Publishing page](https://www.nuget.org/account/TrustedPublishing).
   When policy management is web-only, use browser-commander's authenticated
   profile through package-registry-manager. A sign-in page or a submitted form
   is not proof of a saved policy.
3. Inspect existing policies and reuse an equivalent one. The required identity
   is owner `linksplatform`, repository `Data.Doublets`, workflow **`ci.yml`**
   (filename only). Leave the environment empty: these publishing jobs use none.
   Do not create a GitHub environment or a separate GitHub publisher registration.
4. Select the NuGet user/organization that owns **both** package IDs. Limit allowed
   operations to the required package push/new-version operations. Prefer exact
   package selection where supported; if only a single glob is supported, use
   `Platform.Data.Doublets*`, the narrow shared prefix covering these two IDs,
   after auditing other matching packages. Do not use the unrestricted `*` glob.
5. Re-read saved state: identity, creator, owner, scope and activation status must
   match. An inactive policy cannot authenticate. A temporarily active policy may
   need a successful publication within its activation window; report this state
   explicitly rather than claiming permanent activation.
6. Through gh-manager, ensure the non-secret Actions variable `NUGET_USER` at a
   scope visible to both jobs. Read effective organization/repository precedence,
   preserve existing visibility restrictions, skip an equal value and re-read
   after writing. Do not widen global workflow permissions or rotate a legacy key
   to repair missing OIDC setup.

## What the gate verifies

The `credentials` job checks the configured profile and tries the SHA-pinned
`NuGet/login` with job-level `id-token: write`. Missing/invalid profiles are
reported as trusted-publishing unavailability; failed configuration or exchange
can consider the provided fallback. A username alone never proves authentication.
Only those OIDC attempt steps tolerate failure; the final preflight is mandatory.
No credential passes from this job to any other job.

Every release caller passes the temporary `NUGET_TOKEN`, `NUGET_LOGIN_OUTCOME`
and the separate `NUGET_FALLBACK_TOKEN` to an explicit selector. Logs identify
`auth=trusted-publishing|token-fallback|none`, the failed step and a sanitized
reason. A successful exchange selects only the temporary key, even when an expired
fallback exists. Never select credentials with an empty-output expression.

The former `/api/v2/verifykey/{id}/{version}` probe is incompatible with push-only
credentials under Gallery's `RequirePackageVerifyScopePolicy`: HTTP 400 can occur
before owner/package scope evaluation. The baseline discarded the response body,
so its precise live rejection remains unconfirmed. All preflights now report
package scope as **unverified/unsupported** instead of calling this symbols
verification endpoint. Anonymous indexes must still be well formed/reachable;
first publication explicitly reports that no public version exists. An index is
not authentication or authorization evidence.

For a provided fallback, the push-compatible
`POST /api/v2/package/create-verification-key/{id}/{version}` authenticates the
original key for push actions, even before first publication. Its returned key is
discarded: Gallery replaces the subject with the requested ID, so this is **never
package-scope proof**, and no derived key is sent to `verifykey`. A malformed,
unsupported or unavailable authentication response fails explicitly without being
called an ownership failure. Real publication enforces original package/owner
permissions. HTTP error bodies are retained with bounded reads and sanitized,
flattened diagnostics; credentials, raw headers and exception argv stay out of logs.

The `publish` job downloads tested artifacts, obtains a fresh key and checks
GitHub repository access before preparing/committing release metadata. The
read-only repository API's user-role `permissions.push` field cannot prove an
Actions installation token's endpoint permissions. Write permission remains
explicitly unverified until the actual metadata, mirror and release operations
succeed; none may ignore failures. HTTP failures preserve bounded sanitized
diagnostics. The job still requires `contents: write` and `packages: write`.
C# uses that login; C++ logs in again immediately before publication, even when
C# fails and C++ can independently repair its publication. Missing configuration
also permits fallback here. Metadata failure prevents either package push.
NuGet temporary keys last one hour and the publishing job is bounded to 40
minutes. Never move keys into artifacts, persistent secrets or cross-job
outputs. Shell tracing around credentials remains forbidden.

A real NuGet push rejection with HTTP **401/403** can retry **once** with the
provided token. First inspect the anonymous exact-version download: an already
public version is reported, a 404 permits retry of the same artifact, and an
indeterminate result stops the retry. `--skip-duplicate` repairs partial
package/symbol publication without rebuilding or changing versions. Authentication
and both failed push outcomes remain visible. HTTP 400 package validation,
transport/5xx failures, incompatible verification and build/test/doc defects
cannot switch credentials. If both authentication methods fail, publication fails.
Public download, mirror and release-asset failures also remain fatal.

## Run and recover

After the code is reviewed and merged and settings are verified, execute CI on
the **current main revision**. A rerun of an older SHA cannot load this workflow
change. For settings-only repairs, restart only the affected failed CI after
verified changes. Repeated setup with identical saved settings must create no
duplicate policies and trigger no redundant workflow runs. Do not restart
unrelated failures.

Monitor exchange, per-package scope checks, package pushes, anonymous indexing
checks and consumer smoke checks. Record the run ID, attempt, actual SHA, exact
C#/C++ versions and final conclusion. The existing smoke jobs restore packages
into isolated C# and native C++ consumers. After publication, confirm both exact
versions from the public NuGet feed, then install/consume the public artifacts;
cached or local artifacts are not evidence of public availability.

Already-published versions remain recoverable with `--skip-duplicate` and no
second version bump. Anonymous package indexing waits are bounded to fifteen
minutes. Mirror failures remain failures while public GitHub assets can still be
repaired. Do not claim release success from exchange success, username presence,
an upstream issue, a package listing or a requested workflow dispatch.

Retain the intentionally configured fallback after OIDC succeeds. Never delete
it automatically, mint a replacement long-lived token, widen package permissions
or weaken NuGet policy settings to make a release pass. The verified policy uses
owner `LinksPlatform`, creator `konard`, owner/repository IDs `15116341`/`199508305`,
workflow `ci.yml`, no environment and exactly the two package IDs with push-only
new-version permission. Preserve this setup and `NUGET_USER=konard`.

## Regression checks and references

Run `python3 -m unittest discover -s experiments -p 'test_*.py' -v`,
`python3 scripts/check_repository.py` and `python3 scripts/check-workflows.py`.
`experiments/test_trusted_publishing.py` and `experiments/test_nuget_fallback.py`
cover the live 400-after-exchange regression at all preflight callers, the
authentication decision table, original/derived scope limits, HTTP diagnostics,
first publication, expired keys and refresh, same-artifact retry, metadata ordering
and credential-free PR validation.
Existing publication regressions cover bounded indexing and release repair.

- [Official NuGet trusted publishing setup and key lifetime](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
- [Reviewed NuGet/login v1.2.0 source](https://github.com/NuGet/login/blob/8d196754b4036150537f80ac539e15c2f1028841/src/index.ts)
- [NuGet direct verification and scope evaluation](https://github.com/NuGet/NuGetGallery/blob/c25bab040e19d24593fc0c298a0cedb51bf57dcb/src/NuGetGallery/Controllers/ApiController.cs#L352)
- [Derived verification-key scope construction](https://github.com/NuGet/NuGetGallery/blob/c25bab040e19d24593fc0c298a0cedb51bf57dcb/src/NuGetGallery.Services/Authentication/CredentialBuilder.cs#L79)
- [GitHub repository read API and its token requirements](https://docs.github.com/en/rest/repos/repos#get-a-repository)
