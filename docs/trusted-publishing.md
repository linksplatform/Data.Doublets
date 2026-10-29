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
No stored NuGet API key is selected, including when configuration is incomplete.
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

The `credentials` job checks username configuration and runs `NuGet/login` with
job-level `id-token: write`. A username by itself never passes authentication.
A rejected exchange stops before C#/C++/documentation builds and reports the
username, workflow/environment, policy activation, owner and scope to check.
There is no credential passed from this job to another job.

`preflight` receives `NUGET_TOKEN` **only** from that job's successful login output.
It anonymously reads each package's public version index and sends the original
temporary key to NuGet's `/api/v2/verifykey/{id}/{version}` endpoint. This endpoint
accepts push credentials and checks package ownership and the credential's scope.
It does not create a derived symbol-verification key, whose owner/package scope
can differ from the original credential. HTTP errors and malformed responses
fail closed; a 403 is not silently ignored as an OIDC compatibility exception.

An absent package index is reported as **scope unverified**: the first push must
enforce ownership and scope. Successful verification against an existing version
does not prove that a future package will pass every NuGet validation. Success
still requires the push, anonymous exact-version download and public assets.

The `publish` job downloads tested artifacts, performs its own login and verifies
both scopes and GitHub write permission before preparing/committing release
metadata. C# consumes that fresh key; C++ logs in again immediately before its
push, even if C# failed and C++ can independently repair its publication. Metadata
failure prevents either package push. NuGet temporary keys last one hour; the
entire publishing job is limited to 40 minutes, leaving at least 20 minutes even
for the first credential. Increasing that job limit requires revisiting refresh
timing. The official action masks OIDC/API tokens; shell tracing around tokens is
forbidden even when `CI_VERBOSE=1`.

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

Only after verified OIDC publication, audit other consumers of obsolete
`NUGET_TOKEN`/`NUGET_API_KEY` secrets and remove unused secrets through the
management tools. Check organization-wide consumers before deleting a shared
secret; this migration never deletes one as a side effect of a PR test.

## Regression checks and references

Run `python3 -m unittest discover -s experiments -p 'test_*.py' -v`,
`python3 scripts/check_repository.py` and `python3 scripts/check-workflows.py`.
`experiments/test_trusted_publishing.py` covers configuration failures, absent
exchange output, original-key scope checks, HTTP rejection, malformed indexes,
first-publication limits, fresh keys, metadata ordering and PR credential isolation.
Existing publication regressions cover bounded indexing and release repair.

- [Official NuGet trusted publishing setup and key lifetime](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
- [Reviewed NuGet/login v1.2.0 source](https://github.com/NuGet/login/blob/8d196754b4036150537f80ac539e15c2f1028841/src/index.ts)
- [NuGet direct verification and scope evaluation](https://github.com/NuGet/NuGetGallery/blob/c25bab040e19d24593fc0c298a0cedb51bf57dcb/src/NuGetGallery/Controllers/ApiController.cs#L352)
- [Derived verification-key scope construction](https://github.com/NuGet/NuGetGallery/blob/c25bab040e19d24593fc0c298a0cedb51bf57dcb/src/NuGetGallery.Services/Authentication/CredentialBuilder.cs#L79)
