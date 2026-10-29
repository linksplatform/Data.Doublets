<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# Native package license regression", "*"]
  }
}
-->

# Native package license regression

Issue: <https://github.com/linksplatform/Data.Doublets/issues/538>
PR: <https://github.com/linksplatform/Data.Doublets/pull/543>

## Reproduction and root cause

Main run 40939659906, attempt 2, tested source
`5846b3767e37b711a7119e439275dcb9a848b681`. Its metadata commit advanced main
to `d5c403d91d7d54d81cdde798587bdef13db8ea8a`. The downloaded publishing log
for job 123707734595 records fresh OIDC exchange success at line 573 and
NuGet's HTTP 400 license rejection at line 594. C# 0.18.4 was published;
C++ 0.1.1 reached GitHub Packages but not NuGet.org.

`scripts/release.py pack-cpp` serializes the nuspec directly into a ZIP.
It does not perform the compatibility normalization an official NuGet pack
command would provide. The source declared the modern SPDX expression but
omitted the legacy URL that NuGet.org requires alongside it.

Before the fix, run:

```sh
python3 -m unittest experiments.test_cpp_package_metadata -v
```

The generated nupkg regression failed with
`None != 'https://licenses.nuget.org/LGPL-3.0-only'`. This inspects the emitted
archive rather than merely checking source text. The local before/after logs
are retained under `ci-logs/`.

## Repair and release plan

The source nuspec now supplies the required compatibility URL while retaining
`<license type="expression">LGPL-3.0-only</license>` and the bundled license.
Keeping the existing native ZIP layout avoids changing packaging tools and
consumer behavior for this metadata correction. The normal package smoke
check now rejects missing or mismatched embedded license metadata before
restore/compilation; negative fixtures also cover removed SPDX metadata and
changed license content. The exact live HTTP 400 diagnostic is covered by
the existing regression that forbids credential fallback for package defects.

`changes/538-native-license.json` requests a C++ patch. Normal automatic
version preparation will select C++ 0.1.2; 0.1.1 already exists on the immutable
GitHub mirror and must not be overwritten. C# remains at 0.18.4. The modern
license, OIDC-first policy, bounded authentication retries, existing fallback
secret, required-job expectations and package consumer execution are retained.

Comparing the original C# release with an unchanged-version local rebuild
also exposed an idempotence defect: the rebuilt nuspec, assembly and package
relationship entry differ, but publication unconditionally replaced the public
release's original package and symbols. Reruns now preserve existing nonempty
public assets, upload only missing or empty assets, and keep native ZIPs on the
matching C++ release. Draft repair and independent mirror failure reporting
remain enforced. Six regression tests cover preservation, partial repair,
draft replacement, malformed metadata, anonymous asset loss and empty assets.
`verify-public-consumers.sh` downloads exact anonymous NuGet packages and the
public native release ZIP, then restores and executes both normal consumers;
it accepts the released versions explicitly without editing source versions.

## Verification

Local verification passes 105 Python regressions, Ruff, formatting, mypy,
repository and version policy, digest-pinned workflow lint with ShellCheck
and Pyflakes, standalone shell lint, native formatting, report-only preflight,
and a complete Gitleaks history scan (3,316 commits, no leaks).

The container has a shared 2.92 GiB memory limit. The full native test
translation unit previously exceeded the local budget, so it is not repeated
here. The hosted GCC, Clang and sanitizer legs must verify all native tests,
coverage, bounded benchmark smoke, static analysis, FFI and package consumers.
Local C# validation passes all 298 tests, all three examples, packaging and
the isolated consumer with SDK 8.0.427. Full PR and main validation, public
packages/mirrors/releases and deployed documentation remain pending until
their exact run evidence is recorded in the issue description.
