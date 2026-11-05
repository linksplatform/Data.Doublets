# Complete template file comparison

All tracked files at the archived revisions are inventoried below, including scripts, tests, fixtures and historical evidence. Exact hashes and byte sizes are in `template-file-manifest.tsv`; full source is in the two `*-template-full-tree.tar.gz` archives.

## csharp: 81 files

| Template file | Decision | Repository equivalent / reason |
| --- | --- | --- |
| `.changeset/README.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py` — One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/complete-ci-hardening.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py` — One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/config.json` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py` — One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/fix-changeset-script-injection.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py` — One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/harden-pipeline-gates.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py` — One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/pin-ubuntu-runners.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py` — One language-aware changeset store; no Node runtime needed for C# releases |
| `.editorconfig` | Adapted | `.editorconfig` — Repository-wide whitespace with established C# style |
| `.github/workflows/docs.yml` | Adapted | `.github/workflows/ci.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/links.yml` | Adapted | `.github/workflows/links.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/release.yml` | Adapted | `.github/workflows/ci.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/security.yml` | Adapted | `.github/workflows/security.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/workflows.yml` | Adapted | `.github/workflows/ci.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/zizmor.yml` | Adapted | `.github/workflows/AutoMerge.yml` — Narrow inline suppression for metadata-only trusted Dependabot trigger |
| `.gitignore` | Adapted | `.gitignore` — Ignore generated build/site/package outputs; retain issue evidence |
| `.lycheeignore` | Adapted | `.lycheeignore` — Exclude local example hosts; validate own deployment URLs against generated artifacts |
| `.pre-commit-config.yaml` | Adapted | `.pre-commit-config.yaml` — Local formatting, regressions, size and secret checks |
| `CHANGELOG.md` | Adapted | `scripts/versions.py` — Generate changes from fragments rather than copy template history |
| `CONTRIBUTING.md` | Adapted | `CONTRIBUTING.md` — Document supported local checks, credentials, release and security workflow |
| `Directory.Build.props` | Adapted | `Directory.Build.props` — Apply analyzers and warnings policy to every C# project |
| `LICENSE` | Preserved | `LICENSE` — Keep existing Unlicense; include it in native packages and archives |
| `MyPackage.sln` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `README.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md` — Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `ci-logs/links-32335007232.log` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docfx.json` | Adapted | `docfx.json` — Pinned modern DocFX plus PDF and C++ API documentation |
| `docs/index.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md` — Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/issue-69-analysis.md` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/roadmap/preview-regeneration.md` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/roadmap/toc.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/toc.yml` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md` — Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `examples/BasicUsage.cs` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/BasicUsage.csproj` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `experiments/issue-69/reproduce-transient-links.mjs` | Adapted regression | `experiments/` — Preserve reusable minimal regressions for this repository rather than template demonstrations |
| `experiments/reproduce-bug.sh` | Adapted regression | `experiments/` — Preserve reusable minimal regressions for this repository rather than template demonstrations |
| `lychee.toml` | Adapted | `.github/workflows/links.yml` — Explicit timeouts, retry limits and request concurrency |
| `scripts/bump-version.mjs` | Adapted | `scripts/versions.py` — Semantic bump validation and generation |
| `scripts/check-file-size.mjs` | Adapted | `scripts/check_repository.py` — 1500-line source limit |
| `scripts/check-file-size.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/check-pipeline-status.sh` | Adapted | `scripts/check_required.py` — Fatal applicable-job and cancellation checks |
| `scripts/check-release-needed.mjs` | Adapted | `scripts/release.py` — Fail-closed release lookup and idempotent repair |
| `scripts/check-release-needed.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/check-web-archive.mjs` | Adapted | `scripts/check_web_archive.py` — Archive suggestions never excuse live failures |
| `scripts/check-web-archive.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/create-github-release.mjs` | Adapted | `scripts/release.py` — Draft then complete public release assets |
| `scripts/create-github-release.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/detect-code-changes.mjs` | Adapted | `scripts/changes.py` — Actual event base and deleted-file routing |
| `scripts/detect-code-changes.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/docs-workflow-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/fixtures/lychee-report.md` | Adapted regression | `experiments/` — Preserve reusable minimal regressions for this repository rather than template demonstrations |
| `scripts/links-workflow-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/merge-changesets.mjs` | Adapted | `scripts/versions.py` — Largest bump per language; consume fragments once |
| `scripts/nuget-auth.mjs` | Adapted | `scripts/release.py` — NuGet verification key and actual package-scope probe |
| `scripts/nuget-auth.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/preflight-credentials.mjs` | Adapted | `scripts/release.py` — PR report; main fails before building |
| `scripts/preflight-credentials.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/print-untrusted.mjs` | Adapted | `.github/workflows/ci.yml` — Untrusted event fields passed through environment, never shell code |
| `scripts/print-untrusted.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/push-nuget-package.mjs` | Adapted | `scripts/release.py` — Publish tested artifacts and skip duplicates |
| `scripts/push-nuget-package.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/recheck-broken-links.mjs` | Adapted | `scripts/recheck_broken_links.py` — Bounded retries with corrected singleton backoff |
| `scripts/recheck-broken-links.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/release-naming.mjs` | Adapted | `scripts/release.py` — Preserve csharp_/cpp_ tags and package identities |
| `scripts/release-naming.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/release-workflow-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/run-with-budget-warning.sh` | Adapted | `.github/workflows/ci.yml` — Adapt budgets as job/test hard limits and retained logs |
| `scripts/security-workflow-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/smoke-test-nuget-package.mjs` | Adapted | `scripts/smoke_packages.py` — Isolated package restore and executable C# consumer |
| `scripts/smoke-test-nuget-package.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/validate-changeset.mjs` | Adapted | `scripts/versions.py` — Validate language, summary and bump type |
| `scripts/version-and-commit.mjs` | Adapted | `scripts/versions.py` — Allowlisted metadata commit and classified bounded push retry |
| `scripts/version-and-commit.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/wait-for-nuget.mjs` | Adapted | `scripts/release.py` — Bounded anonymous registry download after push |
| `scripts/wait-for-nuget.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/workflow-git-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/workflow-injection-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/workflow-runner-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `src/MyPackage/Calculator.cs` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `src/MyPackage/MyPackage.csproj` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `src/MyPackage/PackageInfo.cs` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `tests/MyPackage.Tests/CalculatorTests.cs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `tests/MyPackage.Tests/MyPackage.Tests.csproj` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `tests/MyPackage.Tests/PackageInfoTests.cs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |

## cpp: 216 files

| Template file | Decision | Repository equivalent / reason |
| --- | --- | --- |
| `.clang-format` | Adapted | `.clang-format` — Allman C++ formatting; preserve amalgamated header include order |
| `.clang-tidy` | Adapted | `.clang-tidy` — Defect checks with actual dependency includes; warnings fatal |
| `.editorconfig` | Adapted | `.editorconfig` — Repository-wide whitespace with established C# style |
| `.github/dependabot.yml` | Adapted | `.github/dependabot.yml` — Pinned tooling updates with grouped schedules and cooldown |
| `.github/workflows/docs.yml` | Adapted | `.github/workflows/ci.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/links.yml` | Adapted | `.github/workflows/links.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/release.yml` | Adapted | `.github/workflows/ci.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/security.yml` | Adapted | `.github/workflows/security.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/workflows.yml` | Adapted | `.github/workflows/ci.yml` — Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/zizmor.yml` | Adapted | `.github/workflows/AutoMerge.yml` — Narrow inline suppression for metadata-only trusted Dependabot trigger |
| `.gitignore` | Adapted | `.gitignore` — Ignore generated build/site/package outputs; retain issue evidence |
| `.lycheeignore` | Adapted | `.lycheeignore` — Exclude local example hosts; validate own deployment URLs against generated artifacts |
| `.pre-commit-config.yaml` | Adapted | `.pre-commit-config.yaml` — Local formatting, regressions, size and secret checks |
| `.secretlintrc.json` | Adapted | `.gitleaksignore; scripts/check-secrets.sh` — Use full-history Gitleaks with exact revoked historical fingerprint |
| `CHANGELOG.md` | Adapted | `scripts/versions.py` — Generate changes from fragments rather than copy template history |
| `CMakeLists.txt` | Adapted | `cpp/CMakeLists.txt` — Conditional tests/benchmarks, required dependencies, strict compiler options and CTest |
| `CMakePresets.json` | Adapted | `cpp/ci-build.sh` — Use one shared fail-fast configure/build/test entrypoint with Conan toolchain |
| `CONTRIBUTING.md` | Adapted | `CONTRIBUTING.md` — Document supported local checks, credentials, release and security workflow |
| `Doxyfile` | Adapted | `cpp/Doxyfile` — Generate native API docs and fail malformed references |
| `LICENSE` | Preserved | `LICENSE` — Keep existing Unlicense; include it in native packages and archives |
| `README.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md` — Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `benchmarks/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `benchmarks/my_package_benchmarks.cpp` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `changelog.d/README.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py` — One language-aware changeset store; no Node runtime needed for C# releases |
| `cmake/Dependencies.cmake` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py` — Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/Documentation.cmake` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py` — Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/PackageConfig.cmake.in` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py` — Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/PackageInstall.cmake` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py` — Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/ProjectOptions.cmake` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py` — Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/package.pc.in` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py` — Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `conanfile.py` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py` — Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `docs/case-studies/issue-1/CICD-COMPARISON.md` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/ONLINE-RESEARCH.md` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/README.md` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/README.md` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/issue-1.json` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-cpp-CMakeLists.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-cpp-conanfile.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-cpp-install-local-conan-dependencies.sh` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-cpp-test.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-deploy-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Disposables-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Documentation-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-.github-scripts-check-cpp-docs.py` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-codeql.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-CMakeLists.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-Doxyfile` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-Platform.Interfaces.TemplateLibrary.nuspec` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-conanfile.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-docs.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-test.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-deploy-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-readme-badges.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Numbers-cpp-CMakeLists.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Numbers-cpp-test.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Numbers-deploy-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Numbers-rust.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Scripts-MultiProjectRepository-publish-cpp-release.sh` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Scripts-MultiProjectRepository-push-cpp-nuget.sh` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Scripts-MultiProjectRepository-read_cpp_package_info.sh` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Settings-.clang-format` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-cpp-test.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-deploy-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-get-cpp-conan-package-info.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-get-cpp-package-info.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-publish-to-conan.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/conan-center-index-platform.interfaces-conandata.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/conan-center-index-platform.interfaces-conanfile.py` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/conan-center-index-platform.interfaces-test_package-CMakeLists.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/conan-center-index-platform.interfaces-test_package-conanfile.py` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/linksplatform-Files-TemplateLibrary.targets` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-repos.json` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/conan-center-index-platform-prs.json` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/conancenter-upstream-platform-recipes.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/cpp-repo-probe.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/cpp-repos.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/hive-mind-CI-CD-BEST-PRACTICES.md.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/interfaces-deploy-cpp-failed-run-36213155523.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/interfaces-recent-runs.json` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/interfaces-releases.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-Files-TemplateLibrary.targets.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-conan-center-index-platform.interfaces-conandata.yml.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-conan-center-index-platform.interfaces-conanfile.py.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-conan-recipes.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-release-tags-sample.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-repo-list.json` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/non-cpp-repo-probe.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/non-cpp-repos.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/nuget-templatelibrary-packages.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-cmake.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-codeql.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-conan.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-docs-quality.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-github-releases.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-other-package-managers.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-runner-images.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-templates-release.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-vcpkg.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/template-workflow-structure.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/vcpkg-linksplatform-presence.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/csharp.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/go.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/java.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/js.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/php.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/python.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/rust.txt` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/docs.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/go/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/go/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/java/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/example-app.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/docs.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/docs.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/desktop-release.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-7/README.md` | Reference evidence | `dev/log/issues/524/pulls/526` — Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/ci-cd.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md` — Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/distribution.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md` — Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/mainpage.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md` — Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/multi-language.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md` — Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `examples/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/basic_usage.cpp` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/add_subdirectory/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/cpm/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/fetch_content/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/find_package/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/main.cpp` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/pkg_config/build.sh` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `experiments/multi_language_layout.py` | Adapted regression | `experiments/` — Preserve reusable minimal regressions for this repository rather than template demonstrations |
| `gcovr.cfg` | Adapted | `.github/workflows/ci.yml` — Native coverage reporting alongside C# XPlat coverage |
| `include/my_package/my_package.hpp` | Adapted | `csharp/; cpp/; examples/ExternalReferences/` — Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `packaging/nuget/package.nuspec.in` | Adapted | `scripts/release.py; cpp/Platform.Data.Doublets/Platform.Data.Doublets.TemplateLibrary.nuspec` — Preserve native package ID/layout and validate complete contents |
| `packaging/nuget/package.targets.in` | Adapted | `scripts/release.py; cpp/Platform.Data.Doublets/Platform.Data.Doublets.TemplateLibrary.nuspec` — Preserve native package ID/layout and validate complete contents |
| `packaging/vcpkg/portfile.cmake.in` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py` — Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `packaging/vcpkg/usage.in` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py` — Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `packaging/vcpkg/vcpkg.json.in` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py` — Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `scripts/bump_version.py` | Adapted | `scripts/versions.py` — Semantic bump validation and generation |
| `scripts/check-dependency-review-support.sh` | Adapted | `.github/workflows/security.yml` — Scheduled full transitive audit works without optional dependency-review entitlement |
| `scripts/check-nuget-package.sh` | Adapted | `scripts/smoke_packages.py` — NuGet restore in a clean isolated package cache |
| `scripts/check-pipeline-status.sh` | Adapted | `scripts/check_required.py` — Fatal applicable-job and cancellation checks |
| `scripts/check-required-docs.sh` | Adapted | `scripts/check_docs.py` — Required C#/C++ API pages, PDF and local links |
| `scripts/check-vcpkg-port.sh` | Not applicable | `scripts/smoke_packages.py` — No existing vcpkg distribution channel; preserve and validate the existing NuGet and header ZIP channels |
| `scripts/check_changelog_fragment.py` | Adapted | `scripts/versions.py` — Require independent fragments for code changes |
| `scripts/check_consumers.py` | Adapted | `scripts/smoke_packages.py` — Restore NuGet; compare every native header and ZIP byte |
| `scripts/check_file_size.py` | Adapted | `scripts/check_repository.py` — 1500-line source limit |
| `scripts/check_release_needed.py` | Adapted | `scripts/release.py` — Fail-closed release lookup and idempotent repair |
| `scripts/check_version_modification.py` | Adapted | `scripts/versions.py` — Reject manual package-version edits in PRs |
| `scripts/check_web_archive.py` | Adapted | `scripts/check_web_archive.py` — Archive suggestions never excuse live failures |
| `scripts/collect_changelog.py` | Adapted | `scripts/versions.py` — Generate per-language changelog from consumed changesets |
| `scripts/cpp_project.py` | Not applicable | `cpp/CMakeLists.txt` — Template bootstrap/rename utility; existing published package names must remain stable |
| `scripts/create_changelog_fragment.py` | Adapted | `changes/524-ci-reliability.json` — Document independent fragment schema in CONTRIBUTING |
| `scripts/create_github_release.py` | Adapted | `scripts/release.py` — Draft then complete public release assets |
| `scripts/detect_code_changes.py` | Adapted | `scripts/changes.py` — Actual event base and deleted-file routing |
| `scripts/get_bump_type.py` | Adapted | `scripts/versions.py` — Choose highest semantic bump |
| `scripts/get_version.py` | Adapted | `scripts/release.py` — Read authoritative XML package metadata |
| `scripts/lint_cpp.py` | Adapted | `scripts/check-cpp.py` — clang-format, compilation-database clang-tidy and cppcheck |
| `scripts/pack_nuget.py` | Adapted | `scripts/release.py` — Complete native NuGet layout and package metadata |
| `scripts/package_release.py` | Adapted | `scripts/release.py` — Package all headers and license in release ZIP |
| `scripts/preflight-credentials.sh` | Adapted | `scripts/release.py` — PR report; main fails before building |
| `scripts/publish-release.sh` | Adapted | `scripts/release.py` — Independent registry and public release repair |
| `scripts/read-job-cancel-in-progress.sh` | Adapted | `.github/workflows/ci.yml` — Explicit per-job cancellation and shared queued writer |
| `scripts/recheck_broken_links.py` | Adapted | `scripts/recheck_broken_links.py` — Bounded retries with corrected singleton backoff |
| `scripts/rename_project.py` | Not applicable | `cpp/CMakeLists.txt` — Template bootstrap/rename utility; existing published package names must remain stable |
| `scripts/render_vcpkg_port.py` | Not applicable | `scripts/smoke_packages.py` — No existing vcpkg distribution channel; preserve and validate the existing NuGet and header ZIP channels |
| `scripts/requirements-ci.txt` | Adapted | `scripts/requirements-ci.txt` — Pin Python helper dependencies |
| `scripts/run-with-budget-warning.sh` | Adapted | `.github/workflows/ci.yml` — Adapt budgets as job/test hard limits and retained logs |
| `scripts/simulate-fresh-merge.sh` | Adapted | `scripts/fresh-merge.sh` — Fetch and validate current base merge |
| `scripts/tests/__init__.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/conftest.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_changelog.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_consumers.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_dependency_review_support.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_pipeline_status.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_required_docs.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_web_archive.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_cpp_project.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_lint_cpp.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_packaging.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_pr_checks.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_preflight_credentials.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_publish_release.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_recheck_broken_links.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_release.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_rename_project.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_run_with_budget_warning.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_simulate_fresh_merge.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_versioning.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_workflows.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/version_and_commit.py` | Adapted | `scripts/versions.py` — Allowlisted metadata commit and classified bounded push retry |
| `test_package/CMakeLists.txt` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py` — Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `test_package/conanfile.py` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py` — Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `test_package/test_package.cpp` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py` — Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `tests/CMakeLists.txt` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `tests/my_package_tests.cpp` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests` — Use existing language test frameworks plus targeted pipeline regressions |
| `vcpkg.json` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py` — Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |

