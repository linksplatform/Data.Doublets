# Complete four-template comparison

Every tracked file, including fixtures and historical incident data, has a decision below. The TSV gives exact revisions, SHA-256 hashes and byte counts; full-tree archives retain every file. Priority is C#, C++, Python, Rust. This is a transfer of applicable practices to the real library, not a new desktop/JS/PyPI/crates.io product.

## csharp: 81 files

| File | Decision | Equivalent and rationale |
| --- | --- | --- |
| `.changeset/README.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py`: One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/complete-ci-hardening.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py`: One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/config.json` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py`: One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/fix-changeset-script-injection.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py`: One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/harden-pipeline-gates.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py`: One language-aware changeset store; no Node runtime needed for C# releases |
| `.changeset/pin-ubuntu-runners.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py`: One language-aware changeset store; no Node runtime needed for C# releases |
| `.editorconfig` | Adapted | `.editorconfig`: Repository-wide whitespace with established C# style |
| `.github/workflows/docs.yml` | Adapted | `.github/workflows/ci.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/links.yml` | Adapted | `.github/workflows/links.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/release.yml` | Adapted | `.github/workflows/ci.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/security.yml` | Adapted | `.github/workflows/security.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/workflows.yml` | Adapted | `.github/workflows/ci.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/zizmor.yml` | Adapted | `.github/workflows/AutoMerge.yml`: Narrow inline suppression for metadata-only trusted Dependabot trigger |
| `.gitignore` | Adapted | `.gitignore`: Ignore generated build/site/package outputs; retain issue evidence |
| `.lycheeignore` | Adapted | `.lycheeignore`: Exclude local example hosts; validate own deployment URLs against generated artifacts |
| `.pre-commit-config.yaml` | Adapted | `.pre-commit-config.yaml`: Local formatting, regressions, size and secret checks |
| `CHANGELOG.md` | Adapted | `scripts/versions.py`: Generate changes from fragments rather than copy template history |
| `CONTRIBUTING.md` | Adapted | `CONTRIBUTING.md`: Document supported local checks, credentials, release and security workflow |
| `Directory.Build.props` | Adapted | `Directory.Build.props`: Apply analyzers and warnings policy to every C# project |
| `LICENSE` | Preserved | `LICENSE`: Keep existing Unlicense; include it in native packages and archives |
| `MyPackage.sln` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `README.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `ci-logs/links-32335007232.log` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docfx.json` | Adapted | `docfx.json`: Pinned modern DocFX plus PDF and C++ API documentation |
| `docs/index.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/issue-69-analysis.md` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/roadmap/preview-regeneration.md` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/roadmap/toc.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/toc.yml` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `examples/BasicUsage.cs` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/BasicUsage.csproj` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `experiments/issue-69/reproduce-transient-links.mjs` | Adapted regression | `experiments/`: Preserve reusable minimal regressions for this repository rather than template demonstrations |
| `experiments/reproduce-bug.sh` | Adapted regression | `experiments/`: Preserve reusable minimal regressions for this repository rather than template demonstrations |
| `lychee.toml` | Adapted | `.github/workflows/links.yml`: Explicit timeouts, retry limits and request concurrency |
| `scripts/bump-version.mjs` | Adapted | `scripts/versions.py`: Semantic bump validation and generation |
| `scripts/check-file-size.mjs` | Adapted | `scripts/check_repository.py`: 1500-line source limit |
| `scripts/check-file-size.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/check-pipeline-status.sh` | Adapted | `scripts/check_required.py`: Fatal applicable-job and cancellation checks |
| `scripts/check-release-needed.mjs` | Adapted | `scripts/release.py`: Fail-closed release lookup and idempotent repair |
| `scripts/check-release-needed.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/check-web-archive.mjs` | Adapted | `scripts/check_web_archive.py`: Archive suggestions never excuse live failures |
| `scripts/check-web-archive.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/create-github-release.mjs` | Adapted | `scripts/release.py`: Draft then complete public release assets |
| `scripts/create-github-release.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/detect-code-changes.mjs` | Adapted | `scripts/changes.py`: Actual event base and deleted-file routing |
| `scripts/detect-code-changes.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/docs-workflow-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/fixtures/lychee-report.md` | Adapted regression | `experiments/`: Preserve reusable minimal regressions for this repository rather than template demonstrations |
| `scripts/links-workflow-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/merge-changesets.mjs` | Adapted | `scripts/versions.py`: Largest bump per language; consume fragments once |
| `scripts/nuget-auth.mjs` | Adapted | `scripts/release.py`: NuGet verification key and actual package-scope probe |
| `scripts/nuget-auth.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/preflight-credentials.mjs` | Adapted | `scripts/release.py`: PR report; main fails before building |
| `scripts/preflight-credentials.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/print-untrusted.mjs` | Adapted | `.github/workflows/ci.yml`: Untrusted event fields passed through environment, never shell code |
| `scripts/print-untrusted.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/push-nuget-package.mjs` | Adapted | `scripts/release.py`: Publish tested artifacts and skip duplicates |
| `scripts/push-nuget-package.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/recheck-broken-links.mjs` | Adapted | `scripts/recheck_broken_links.py`: Bounded retries with corrected singleton backoff |
| `scripts/recheck-broken-links.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/release-naming.mjs` | Adapted | `scripts/release.py`: Preserve csharp_/cpp_ tags and package identities |
| `scripts/release-naming.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/release-workflow-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/run-with-budget-warning.sh` | Adapted | `.github/workflows/ci.yml`: Adapt budgets as job/test hard limits and retained logs |
| `scripts/security-workflow-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/smoke-test-nuget-package.mjs` | Adapted | `scripts/smoke_packages.py`: Isolated package restore and executable C# consumer |
| `scripts/smoke-test-nuget-package.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/validate-changeset.mjs` | Adapted | `scripts/versions.py`: Validate language, summary and bump type |
| `scripts/version-and-commit.mjs` | Adapted | `scripts/versions.py`: Allowlisted metadata commit and classified bounded push retry |
| `scripts/version-and-commit.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/wait-for-nuget.mjs` | Adapted | `scripts/release.py`: Bounded anonymous registry download after push |
| `scripts/wait-for-nuget.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/workflow-git-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/workflow-injection-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/workflow-runner-policy.test.mjs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `src/MyPackage/Calculator.cs` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `src/MyPackage/MyPackage.csproj` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `src/MyPackage/PackageInfo.cs` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `tests/MyPackage.Tests/CalculatorTests.cs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `tests/MyPackage.Tests/MyPackage.Tests.csproj` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `tests/MyPackage.Tests/PackageInfoTests.cs` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |

## cpp: 216 files

| File | Decision | Equivalent and rationale |
| --- | --- | --- |
| `.clang-format` | Adapted | `.clang-format`: Allman C++ formatting; preserve amalgamated header include order |
| `.clang-tidy` | Adapted | `.clang-tidy`: Defect checks with actual dependency includes; warnings fatal |
| `.editorconfig` | Adapted | `.editorconfig`: Repository-wide whitespace with established C# style |
| `.github/dependabot.yml` | Adapted | `.github/dependabot.yml`: Pinned tooling updates with grouped schedules and cooldown |
| `.github/workflows/docs.yml` | Adapted | `.github/workflows/ci.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/links.yml` | Adapted | `.github/workflows/links.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/release.yml` | Adapted | `.github/workflows/ci.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/security.yml` | Adapted | `.github/workflows/security.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/workflows/workflows.yml` | Adapted | `.github/workflows/ci.yml`: Consolidate related gates; pin actions/runners; preserve tested artifact publication |
| `.github/zizmor.yml` | Adapted | `.github/workflows/AutoMerge.yml`: Narrow inline suppression for metadata-only trusted Dependabot trigger |
| `.gitignore` | Adapted | `.gitignore`: Ignore generated build/site/package outputs; retain issue evidence |
| `.lycheeignore` | Adapted | `.lycheeignore`: Exclude local example hosts; validate own deployment URLs against generated artifacts |
| `.pre-commit-config.yaml` | Adapted | `.pre-commit-config.yaml`: Local formatting, regressions, size and secret checks |
| `.secretlintrc.json` | Adapted | `.gitleaksignore; scripts/check-secrets.sh`: Use full-history Gitleaks with exact revoked historical fingerprint |
| `CHANGELOG.md` | Adapted | `scripts/versions.py`: Generate changes from fragments rather than copy template history |
| `CMakeLists.txt` | Adapted | `cpp/CMakeLists.txt`: Conditional tests/benchmarks, required dependencies, strict compiler options and CTest |
| `CMakePresets.json` | Adapted | `cpp/ci-build.sh`: Use one shared fail-fast configure/build/test entrypoint with Conan toolchain |
| `CONTRIBUTING.md` | Adapted | `CONTRIBUTING.md`: Document supported local checks, credentials, release and security workflow |
| `Doxyfile` | Adapted | `cpp/Doxyfile`: Generate native API docs and fail malformed references |
| `LICENSE` | Preserved | `LICENSE`: Keep existing Unlicense; include it in native packages and archives |
| `README.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `benchmarks/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `benchmarks/my_package_benchmarks.cpp` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `changelog.d/README.md` | Adapted | `changes/524-ci-reliability.json; scripts/versions.py`: One language-aware changeset store; no Node runtime needed for C# releases |
| `cmake/Dependencies.cmake` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py`: Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/Documentation.cmake` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py`: Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/PackageConfig.cmake.in` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py`: Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/PackageInstall.cmake` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py`: Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/ProjectOptions.cmake` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py`: Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `cmake/package.pc.in` | Adapted | `cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py`: Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels |
| `conanfile.py` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py`: Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `docs/case-studies/issue-1/CICD-COMPARISON.md` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/ONLINE-RESEARCH.md` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/README.md` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/README.md` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/issue-1.json` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-cpp-CMakeLists.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-cpp-conanfile.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-cpp-install-local-conan-dependencies.sh` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-cpp-test.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Data.Doublets-deploy-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Disposables-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Documentation-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-.github-scripts-check-cpp-docs.py` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-codeql.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-CMakeLists.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-Doxyfile` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-Platform.Interfaces.TemplateLibrary.nuspec` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-conanfile.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-docs.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-cpp-test.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-deploy-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Interfaces-readme-badges.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Numbers-cpp-CMakeLists.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Numbers-cpp-test.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Numbers-deploy-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Numbers-rust.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Scripts-MultiProjectRepository-publish-cpp-release.sh` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Scripts-MultiProjectRepository-push-cpp-nuget.sh` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Scripts-MultiProjectRepository-read_cpp_package_info.sh` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Settings-.clang-format` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-cpp-test.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-deploy-cpp.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-get-cpp-conan-package-info.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-get-cpp-package-info.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/Workflows-publish-to-conan.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/conan-center-index-platform.interfaces-conandata.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/conan-center-index-platform.interfaces-conanfile.py` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/conan-center-index-platform.interfaces-test_package-CMakeLists.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/conan-center-index-platform.interfaces-test_package-conanfile.py` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-cpp-workflows/linksplatform-Files-TemplateLibrary.targets` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/linksplatform-repos.json` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/conan-center-index-platform-prs.json` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/conancenter-upstream-platform-recipes.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/cpp-repo-probe.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/cpp-repos.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/hive-mind-CI-CD-BEST-PRACTICES.md.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/interfaces-deploy-cpp-failed-run-36213155523.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/interfaces-recent-runs.json` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/interfaces-releases.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-Files-TemplateLibrary.targets.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-conan-center-index-platform.interfaces-conandata.yml.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-conan-center-index-platform.interfaces-conanfile.py.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-conan-recipes.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-release-tags-sample.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/linksplatform-repo-list.json` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/non-cpp-repo-probe.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/non-cpp-repos.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/nuget-templatelibrary-packages.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-cmake.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-codeql.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-conan.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-docs-quality.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-github-releases.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-other-package-managers.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-runner-images.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-templates-release.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/online-vcpkg.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/template-workflow-structure.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/raw/vcpkg-linksplatform-presence.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/csharp.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/go.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/java.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/js.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/php.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/python.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-trees/rust.txt` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/docs.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/csharp/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/go/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/go/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/java/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/example-app.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/js/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/docs.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/php/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/docs.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/python/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/desktop-release.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/links.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/release.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/security.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-1/data/template-workflows/rust/workflows.yml` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/case-studies/issue-7/README.md` | Reference evidence | `dev/log/issues/524/pulls/526`: Historical template incidents retained in full archive; no production behavior to transplant |
| `docs/ci-cd.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/distribution.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/mainpage.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/multi-language.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `examples/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/basic_usage.cpp` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/add_subdirectory/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/cpm/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/fetch_content/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/find_package/CMakeLists.txt` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/main.cpp` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `examples/consumers/pkg_config/build.sh` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `experiments/multi_language_layout.py` | Adapted regression | `experiments/`: Preserve reusable minimal regressions for this repository rather than template demonstrations |
| `gcovr.cfg` | Adapted | `.github/workflows/ci.yml`: Native coverage reporting alongside C# XPlat coverage |
| `include/my_package/my_package.hpp` | Adapted | `csharp/; cpp/; examples/ExternalReferences/`: Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code |
| `packaging/nuget/package.nuspec.in` | Adapted | `scripts/release.py; cpp/Platform.Data.Doublets/Platform.Data.Doublets.TemplateLibrary.nuspec`: Preserve native package ID/layout and validate complete contents |
| `packaging/nuget/package.targets.in` | Adapted | `scripts/release.py; cpp/Platform.Data.Doublets/Platform.Data.Doublets.TemplateLibrary.nuspec`: Preserve native package ID/layout and validate complete contents |
| `packaging/vcpkg/portfile.cmake.in` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py`: Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `packaging/vcpkg/usage.in` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py`: Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `packaging/vcpkg/vcpkg.json.in` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py`: Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `scripts/bump_version.py` | Adapted | `scripts/versions.py`: Semantic bump validation and generation |
| `scripts/check-dependency-review-support.sh` | Adapted | `.github/workflows/security.yml`: Scheduled full transitive audit works without optional dependency-review entitlement |
| `scripts/check-nuget-package.sh` | Adapted | `scripts/smoke_packages.py`: NuGet restore in a clean isolated package cache |
| `scripts/check-pipeline-status.sh` | Adapted | `scripts/check_required.py`: Fatal applicable-job and cancellation checks |
| `scripts/check-required-docs.sh` | Adapted | `scripts/check_docs.py`: Required C#/C++ API pages, PDF and local links |
| `scripts/check-vcpkg-port.sh` | Not applicable | `scripts/smoke_packages.py`: No existing vcpkg distribution channel; preserve and validate the existing NuGet and header ZIP channels |
| `scripts/check_changelog_fragment.py` | Adapted | `scripts/versions.py`: Require independent fragments for code changes |
| `scripts/check_consumers.py` | Adapted | `scripts/smoke_packages.py`: Restore NuGet; compare every native header and ZIP byte |
| `scripts/check_file_size.py` | Adapted | `scripts/check_repository.py`: 1500-line source limit |
| `scripts/check_release_needed.py` | Adapted | `scripts/release.py`: Fail-closed release lookup and idempotent repair |
| `scripts/check_version_modification.py` | Adapted | `scripts/versions.py`: Reject manual package-version edits in PRs |
| `scripts/check_web_archive.py` | Adapted | `scripts/check_web_archive.py`: Archive suggestions never excuse live failures |
| `scripts/collect_changelog.py` | Adapted | `scripts/versions.py`: Generate per-language changelog from consumed changesets |
| `scripts/cpp_project.py` | Not applicable | `cpp/CMakeLists.txt`: Template bootstrap/rename utility; existing published package names must remain stable |
| `scripts/create_changelog_fragment.py` | Adapted | `changes/524-ci-reliability.json`: Document independent fragment schema in CONTRIBUTING |
| `scripts/create_github_release.py` | Adapted | `scripts/release.py`: Draft then complete public release assets |
| `scripts/detect_code_changes.py` | Adapted | `scripts/changes.py`: Actual event base and deleted-file routing |
| `scripts/get_bump_type.py` | Adapted | `scripts/versions.py`: Choose highest semantic bump |
| `scripts/get_version.py` | Adapted | `scripts/release.py`: Read authoritative XML package metadata |
| `scripts/lint_cpp.py` | Adapted | `scripts/check-cpp.py`: clang-format, compilation-database clang-tidy and cppcheck |
| `scripts/pack_nuget.py` | Adapted | `scripts/release.py`: Complete native NuGet layout and package metadata |
| `scripts/package_release.py` | Adapted | `scripts/release.py`: Package all headers and license in release ZIP |
| `scripts/preflight-credentials.sh` | Adapted | `scripts/release.py`: PR report; main fails before building |
| `scripts/publish-release.sh` | Adapted | `scripts/release.py`: Independent registry and public release repair |
| `scripts/read-job-cancel-in-progress.sh` | Adapted | `.github/workflows/ci.yml`: Explicit per-job cancellation and shared queued writer |
| `scripts/recheck_broken_links.py` | Adapted | `scripts/recheck_broken_links.py`: Bounded retries with corrected singleton backoff |
| `scripts/rename_project.py` | Not applicable | `cpp/CMakeLists.txt`: Template bootstrap/rename utility; existing published package names must remain stable |
| `scripts/render_vcpkg_port.py` | Not applicable | `scripts/smoke_packages.py`: No existing vcpkg distribution channel; preserve and validate the existing NuGet and header ZIP channels |
| `scripts/requirements-ci.txt` | Adapted | `scripts/requirements-ci.txt`: Pin Python helper dependencies |
| `scripts/run-with-budget-warning.sh` | Adapted | `.github/workflows/ci.yml`: Adapt budgets as job/test hard limits and retained logs |
| `scripts/simulate-fresh-merge.sh` | Adapted | `scripts/fresh-merge.sh`: Fetch and validate current base merge |
| `scripts/tests/__init__.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/conftest.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_changelog.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_consumers.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_dependency_review_support.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_pipeline_status.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_required_docs.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_check_web_archive.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_cpp_project.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_lint_cpp.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_packaging.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_pr_checks.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_preflight_credentials.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_publish_release.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_recheck_broken_links.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_release.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_rename_project.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_run_with_budget_warning.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_simulate_fresh_merge.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_versioning.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/tests/test_workflows.py` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `scripts/version_and_commit.py` | Adapted | `scripts/versions.py`: Allowlisted metadata commit and classified bounded push retry |
| `test_package/CMakeLists.txt` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py`: Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `test_package/conanfile.py` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py`: Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `test_package/test_package.cpp` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py`: Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |
| `tests/CMakeLists.txt` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `tests/my_package_tests.cpp` | Adapted regression | `experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests`: Use existing language test frameworks plus targeted pipeline regressions |
| `vcpkg.json` | Not applicable | `cpp/conanfile.txt; scripts/smoke_packages.py`: Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry |

## python: 121 files

| File | Decision | Equivalent and rationale |
| --- | --- | --- |
| `.github/actionlint.yaml` | Adapted | `check-workflows.py`: Narrow known queue schema exception only; validate queue values and all other diagnostics |
| `.github/codeql/codeql-config.yml` | Adapted | `security.yml; check_sarif.py`: Analyze full C#/C++/Python/Actions source; fail on every finding and incomplete invocation |
| `.github/workflows/docs.yml` | Adapted | `.github/workflows/ci.yml; security.yml; links.yml`: Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers |
| `.github/workflows/links.yml` | Adapted | `.github/workflows/ci.yml; security.yml; links.yml`: Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers |
| `.github/workflows/release.yml` | Adapted | `.github/workflows/ci.yml; security.yml; links.yml`: Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers |
| `.github/workflows/security.yml` | Adapted | `.github/workflows/ci.yml; security.yml; links.yml`: Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers |
| `.github/workflows/workflows.yml` | Adapted | `.github/workflows/ci.yml; security.yml; links.yml`: Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers |
| `.github/zizmor.yml` | Adapted | `.github/workflows/AutoMerge.yml`: Narrow inline suppression for metadata-only trusted Dependabot trigger |
| `.gitignore` | Adapted | `.gitignore`: Ignore generated build/site/package outputs; retain issue evidence |
| `.lycheeignore` | Adapted | `.lycheeignore`: Exclude local example hosts; validate own deployment URLs against generated artifacts |
| `.pre-commit-config.yaml` | Adapted | `.pre-commit-config.yaml`: Local formatting, regressions, size and secret checks |
| `.ruff.toml` | Adapted | `pyproject.toml; check-python.sh`: Lint, format and type-check maintained Python helpers; no new Python distribution |
| `.secretlintrc.json` | Adapted | `.gitleaksignore; scripts/check-secrets.sh`: Use full-history Gitleaks with exact revoked historical fingerprint |
| `CHANGELOG.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `CONTRIBUTING.md` | Adapted | `CONTRIBUTING.md`: Document supported local checks, credentials, release and security workflow |
| `LICENSE` | Preserved | `LICENSE`: Keep existing Unlicense; include it in native packages and archives |
| `README.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `changelog.d/20251218_133759_drakonard_issue_1_3b50e2f12be6.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260509_204000_issue_6_release_metadata.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260515_000000_issue_8_docs_pages.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260529_222900_issue_9_preview_regeneration.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260604_170000_issue_16_release_notes_limit.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260609_000000_issue_18_manual_release_skip.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260614_000000_issue_20_published_package_smoke_test.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260614_issue_21_python_monorepo_releases.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260628_issue_24_file_size_warnings.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260703_issue_26_docs_pages_opt_in.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260703_issue_27_codecov_token_gate.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260703_issue_28_git_default_branch_env.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260722_issue_32_node24_actions.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260722_issue_33_release_permissions.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260722_issue_34_codecov_v7_input.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260722_issue_35_ci_release_guards.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260727_issue_40_change_detection.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260801_issue_42_workflow_hardening.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260802_issue_44_mypy_python_39.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260802_issue_45_changelog_check.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260809_issue_48_security_scanning.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260809_issue_49_broken_links.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260809_issue_50_pipeline_timeout_status.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260812_issue_54_archived_broken_links.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260812_issue_55_multi_arch_docker.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260816_issue_58_dependency_audit.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260820_issue_60_step_execution_budgets.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260828_issue_62_workflow_lint.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260903_issue_64_workflow_audit.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260909_issue_66_pipeline_hardening.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260915_issue_83_pipeline_false_negatives.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20260927_issue_85_86_release_changelog_artifact.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20261005_issue_91_ci_links.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20261007_issue_100_fragment_replacements.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20261007_issue_93_changelog_validation.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20261007_issue_98_ci_release_recovery.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20261010_issue_102_zizmor_reproduction.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/20261024_pipeline_safeguards.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/README.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/fragment_template.md.j2` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `docs/api.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/conf.py` | Adapted | `docfx.json; cpp/Doxyfile; requirements-ci.txt`: Use actual DocFX/Doxygen/PDF documentation and audit its dependencies, not a second Sphinx site |
| `docs/index.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/preview-regeneration.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/requirements.txt` | Adapted | `docfx.json; cpp/Doxyfile; requirements-ci.txt`: Use actual DocFX/Doxygen/PDF documentation and audit its dependencies, not a second Sphinx site |
| `examples/basic_usage.py` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `experiments/issue_98_analysis.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/reproduce_actionlint_queue.py` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/reproduce_partial_release.py` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/reproduce_zizmor_scope.py` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `lychee.toml` | Adapted | `.github/workflows/links.yml`: Explicit timeouts, retry limits and request concurrency |
| `pyproject.toml` | Adapted | `pyproject.toml; check-python.sh`: Lint, format and type-check maintained Python helpers; no new Python distribution |
| `scripts/audit_dependencies.py` | Adapted | `security.yml; requirements-*.txt`: Resolve and audit every maintained requirements surface, including audit tooling, with pip-audit; retain JSON |
| `scripts/bump_version.py` | Adapted | `scripts/versions.py`: Semantic bump validation and generation |
| `scripts/check-dependency-graph.sh` | Adapted | `security.yml`: Use full transitive NuGet/Python audits without treating optional dependency-review entitlement as proof of coverage |
| `scripts/check-pipeline-status.sh` | Adapted | `scripts/check_required.py`: Fatal applicable-job and cancellation checks |
| `scripts/check-required-docs.sh` | Adapted | `scripts/check_docs.py`: Required C#/C++ API pages, PDF and local links |
| `scripts/check_file_size.py` | Adapted | `check_repository.py`: Fatal 1500-line source limit |
| `scripts/check_release.py` | Adapted | `release.py`: Unknown registry/release responses fail closed; repair existing artifacts |
| `scripts/check_web_archive.py` | Adapted | `scripts/check_web_archive.py`: Archive suggestions never excuse live failures |
| `scripts/create_github_release.py` | Adapted | `scripts/release.py`: Draft then complete public release assets |
| `scripts/create_manual_changeset.py` | Adapted | `versions.py; changes/`: Independent validated language-aware fragments; no Python distribution to release |
| `scripts/detect_code_changes.py` | Adapted | `changes.py`: Actual push/PR base and deletions; route Python configuration into full validation |
| `scripts/format_release_notes.py` | Adapted | `release.py; versions.py`: Generated language changelog; no template-specific PyPI release notes |
| `scripts/preflight-credentials.sh` | Adapted | `scripts/release.py`: PR report; main fails before building |
| `scripts/publish_to_pypi.py` | Adapted | `release.py`: No PyPI package in this repository; preserve NuGet/header ZIP identities |
| `scripts/read-job-cancel-in-progress.sh` | Adapted | `.github/workflows/ci.yml`: Explicit per-job cancellation and shared queued writer |
| `scripts/read_manifest.py` | Adapted | `versions.py; release.py`: Read authoritative csproj/nuspec versions; pyproject is tooling-only |
| `scripts/recheck_broken_links.py` | Adapted | `scripts/recheck_broken_links.py`: Bounded retries with corrected singleton backoff |
| `scripts/release_naming.py` | Adapted | `scripts/release.py`: Preserve csharp_/cpp_ tags and package identities |
| `scripts/run-with-budget-warning.sh` | Adapted | `.github/workflows/ci.yml`: Adapt budgets as job/test hard limits and retained logs |
| `scripts/simulate-fresh-merge.sh` | Adapted | `scripts/fresh-merge.sh`: Fetch and validate current base merge |
| `scripts/smoke_test_published_package.py` | Adapted | `smoke_packages.py; release.py`: Clean package consumers plus anonymous bounded public download checks |
| `scripts/validate_changeset.py` | Adapted | `scripts/versions.py`: Validate language, summary and bump type |
| `scripts/version_and_commit.py` | Adapted | `scripts/versions.py`: Allowlisted metadata commit and classified bounded push retry |
| `src/my_package/__init__.py` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `src/my_package/py.typed` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `tests/__init__.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_audit_dependencies.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_check_dependency_graph.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_check_file_size.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_check_pipeline_status.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_check_release.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_check_required_docs.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_check_web_archive.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_create_github_release.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_detect_code_changes.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_issue_118_workflows.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_issue_91.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_issue_98.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_my_package.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_preflight_credentials.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_preview_regeneration_docs.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_project_metadata.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_read_manifest.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_recheck_broken_links.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_release_workflow.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_run_with_budget_warning.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_simulate_fresh_merge.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_smoke_test_published_package.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_validate_changeset.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_version_and_commit.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/test_workflows.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/workflow_helpers.py` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |

## rust: 202 files

| File | Decision | Equivalent and rationale |
| --- | --- | --- |
| `.github/actions/setup-buildx-resilient/action.yml` | Not applicable | `No published container image`: Buildx mirror resilience is specific to template desktop/container builds |
| `.github/codeql/codeql-config.yml` | Adapted | `security.yml; check_sarif.py`: Analyze full C#/C++/Python/Actions source; fail on every finding and incomplete invocation |
| `.github/workflows/desktop-release.yml` | Not applicable | `Library distribution`: No desktop/container/JavaScript product is published here |
| `.github/workflows/javascript-first-example.yml` | Not applicable | `Library distribution`: No desktop/container/JavaScript product is published here |
| `.github/workflows/javascript-first-gate.yml` | Not applicable | `Library distribution`: No desktop/container/JavaScript product is published here |
| `.github/workflows/links.yml` | Adapted | `.github/workflows/ci.yml; security.yml; links.yml`: Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers |
| `.github/workflows/release.yml` | Adapted | `.github/workflows/ci.yml; security.yml; links.yml`: Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers |
| `.github/workflows/security.yml` | Adapted | `.github/workflows/ci.yml; security.yml; links.yml`: Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers |
| `.github/workflows/workflows.yml` | Adapted | `.github/workflows/ci.yml; security.yml; links.yml`: Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers |
| `.github/zizmor.yml` | Adapted | `.github/workflows/AutoMerge.yml`: Narrow inline suppression for metadata-only trusted Dependabot trigger |
| `.gitignore` | Adapted | `.gitignore`: Ignore generated build/site/package outputs; retain issue evidence |
| `.lycheeignore` | Adapted | `.lycheeignore`: Exclude local example hosts; validate own deployment URLs against generated artifacts |
| `.pre-commit-config.yaml` | Adapted | `.pre-commit-config.yaml`: Local formatting, regressions, size and secret checks |
| `.secretlintrc.json` | Adapted | `.gitleaksignore; scripts/check-secrets.sh`: Use full-history Gitleaks with exact revoked historical fingerprint |
| `AGENTS.md` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `CHANGELOG.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `CONTRIBUTING.md` | Adapted | `CONTRIBUTING.md`: Document supported local checks, credentials, release and security workflow |
| `Cargo.lock` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `Cargo.toml` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `LICENSE` | Preserved | `LICENSE`: Keep existing Unlicense; include it in native packages and archives |
| `README.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `changelog.d/20261026_120000_pipeline_safety.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `changelog.d/README.md` | Adapted | `changes/; versions.py`: Use independent JSON release fragments for the actual C#/C++ package versions |
| `docs/case-studies/issue-11/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-11/analysis-crates-io.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-11/analysis-set-output.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-11/analysis-workflow-dispatch.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-11/online-research.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-17/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/ci-logs/ci-run-20885464993.log.gz` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/pr-114-data/issue-113-details.txt` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/pr-114-data/pr-commits.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/pr-114-data/pr-conversation-comments.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/pr-114-data/pr-details.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/pr-114-data/pr-diff.patch` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/pr-114-data/pr-review-comments.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/pr-114-data/pr-reviews.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/pr-114-data/solution-draft-log-1.txt.gz` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-19/pr-114-data/solution-draft-log-2.txt.gz` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-21/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-21/browser-commander-issue-27.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-21/browser-commander-issue-29.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-21/browser-commander-issue-31.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-21/browser-commander-issue-33.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-21/browser-commander-rust.yml` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-25/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-29/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-32/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-34/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/downstream-meta-after-run-24985948212.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/downstream-meta-after-run-24985948212.log.gz` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/downstream-meta-before-run-24983875003.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/downstream-meta-before-run-24983875003.log.gz` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/downstream-meta-ontology-issue-3.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/downstream-meta-ontology-pr-4.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/issue-38-comments.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/issue-38.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/js-template-issue-search.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/main-run-24465255225.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/main-run-24465255225.log.gz` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/main-runs.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/pr-39-conversation-comments.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/pr-39-review-comments.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/pr-39-reviews.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/pr-39.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/pr-branch-runs.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/pr-run-25212295127.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/pr-run-25212295127.log.gz` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/raw-data/rust-template-issue-search.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/template-data/js-template-ci-tree.txt` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/template-data/js-template-links.yml` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/template-data/js-template-release.yml` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/template-data/rust-template-ci-tree.txt` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/template-data/rust-template-release-after.yml` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-38/template-data/rust-template-release-before.yml` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-52/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-52/raw-data/issue-52-comments.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-52/raw-data/issue-52.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-52/raw-data/js-issue-62.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-52/raw-data/vk-bot-desktop-issue-51.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-52/raw-data/vk-bot-desktop-pr-52.json` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/case-studies/issue-69/README.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/ci-cd/troubleshooting.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/download/index.html` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/issue-188-analysis.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/issue-194-analysis.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/issue-215-analysis.md` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `docs/javascript-first.md` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `docs/screenshots/desktop-download-page.png` | Adapted | `README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md`: Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied |
| `examples/basic_usage.rs` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/.dockerignore` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/Cargo.lock` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/Cargo.toml` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/Dockerfile` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/check-parity.mjs` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/fixtures.json` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/generate.mjs` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/ir.json` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/native.mjs` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/native.test.mjs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `examples/javascript-first/parity.json` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `examples/javascript-first/src/lib.rs` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `experiments/issue-139-multi-language-detect-code-changes.sh` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/issue-188-regressions.py` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/issue-190-rename-regressions.py` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/issue-192-release-io-regressions.py` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/issue-193-zizmor-inputs.py` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/issue-215-regressions.py` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/test-changelog-parsing.rs` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/test-crates-io-check.rs` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/test-detect-code-changes.sh` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/test-issue141-manifest-printf-quoting.sh` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/test-issue143-throttled-crates-io-probe.rs` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/test-issue69-buildx-mirror-fallback.sh` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/test-version-check-dependencies.sh` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `experiments/test-version-check.sh` | Reference evidence | `Full source archive`: Historical incidents retained and compared as context; no template project history copied into production |
| `lychee.toml` | Adapted | `.github/workflows/links.yml`: Explicit timeouts, retry limits and request concurrency |
| `scripts/bump-version.rs` | Adapted | `scripts/versions.py`: Semantic bump validation and generation |
| `scripts/changelog-files.rs` | Adapted | `versions.py`: Validate independent JSON fragments and consume once |
| `scripts/check-cargo-lock.rs` | Adapted | `rust submodule`: External Rust package owns its lockfile; no root Cargo workspace |
| `scripts/check-cargo-warnings.sh` | Adapted | `native/C# warnings-as-errors`: Transfer fail-on-warning policy; external Rust package owns cargo checks |
| `scripts/check-changelog-fragment.rs` | Adapted | `scripts/versions.py`: Require independent fragments for code changes |
| `scripts/check-crate-size.rs` | Adapted | `rust submodule`: No crates.io publication from this repository |
| `scripts/check-dependency-graph.sh` | Adapted | `security.yml`: Full supported dependency audits do not rely on optional dependency-review entitlement |
| `scripts/check-file-size.rs` | Adapted | `scripts/check_repository.py`: 1500-line source limit |
| `scripts/check-pipeline-status.sh` | Adapted | `scripts/check_required.py`: Fatal applicable-job and cancellation checks |
| `scripts/check-release-needed.rs` | Adapted | `scripts/release.py`: Fail-closed release lookup and idempotent repair |
| `scripts/check-required-docs.sh` | Adapted | `scripts/check_docs.py`: Required C#/C++ API pages, PDF and local links |
| `scripts/check-version-modification.rs` | Adapted | `scripts/versions.py`: Reject manual package-version edits in PRs |
| `scripts/check-web-archive.mjs` | Adapted | `scripts/check_web_archive.py`: Archive suggestions never excuse live failures |
| `scripts/check-web-archive.test.mjs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `scripts/check-workflow-tools.rs` | Adapted | `check-workflows.py`: Require ShellCheck and Pyflakes or run the complete pinned container |
| `scripts/collect-changelog.rs` | Adapted | `scripts/versions.py`: Generate per-language changelog from consumed changesets |
| `scripts/crates-publish-preflight.mjs` | Adapted | `release.py`: Use actual NuGet OIDC exchange, explicit auth source and scope limits |
| `scripts/crates-publish-preflight.test.mjs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `scripts/create-changelog-fragment.rs` | Adapted | `changes/524-ci-reliability.json`: Document independent fragment schema in CONTRIBUTING |
| `scripts/create-github-release.rs` | Adapted | `scripts/release.py`: Draft then complete public release assets |
| `scripts/desktop-release-resolve.sh` | Not applicable | `No desktop application`: Not applicable: library has no desktop release resolver |
| `scripts/detect-code-changes.rs` | Adapted | `scripts/changes.py`: Actual event base and deleted-file routing |
| `scripts/fixtures/lychee-report.md` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `scripts/get-bump-type.rs` | Adapted | `scripts/versions.py`: Choose highest semantic bump |
| `scripts/get-version.rs` | Adapted | `scripts/release.py`: Read authoritative XML package metadata |
| `scripts/git-config.rs` | Adapted | `workflow env GIT_CONFIG_*`: Set initial branch before checkout and recipe git init; avoid global mutation |
| `scripts/github-output.rs` | Adapted | `changes.py`: Write trusted boolean outputs through GITHUB_OUTPUT |
| `scripts/install-rust-script.sh` | Adapted | `scripts/*.py`: Use existing Python helper runtime; do not add Rust script compilation for CI helpers |
| `scripts/javascript-first.test.mjs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `scripts/package-desktop.sh` | Not applicable | `No desktop application`: Not applicable: library has no desktop binaries |
| `scripts/preflight-credentials.sh` | Adapted | `scripts/release.py`: PR report; main fails before building |
| `scripts/publish-crate.rs` | Adapted | `release.py`: No crates.io package here; validated NuGet pushes remain fatal |
| `scripts/read-job-cancel-in-progress.sh` | Adapted | `.github/workflows/ci.yml`: Explicit per-job cancellation and shared queued writer |
| `scripts/recheck-broken-links.mjs` | Adapted | `scripts/recheck_broken_links.py`: Bounded retries with corrected singleton backoff |
| `scripts/recheck-broken-links.test.mjs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `scripts/release-index.rs` | Adapted | `release.py`: Retain existing GitHub release asset channels; no desktop release index product |
| `scripts/release-naming.rs` | Adapted | `scripts/release.py`: Preserve csharp_/cpp_ tags and package identities |
| `scripts/run-javascript-first.sh` | Not applicable | `No JS implementation`: Not applicable: template demo language parity is a separate product |
| `scripts/run-with-budget-warning.sh` | Adapted | `.github/workflows/ci.yml`: Adapt budgets as job/test hard limits and retained logs |
| `scripts/rust-paths.rs` | Adapted | `changes.py`: Explicit native/C#/tooling roots; external submodule pin is not a root workspace |
| `scripts/simulate-fresh-merge.sh` | Adapted | `scripts/fresh-merge.sh`: Fetch and validate current base merge |
| `scripts/smoke-test-published-crate.rs` | Adapted | `smoke_packages.py`: Apply isolated consumer checks to actual published languages |
| `scripts/test-scripts.sh` | Adapted | `check-python.sh; experiments/`: Execute helper regressions as a mandatory gate |
| `scripts/version-and-commit.rs` | Adapted | `scripts/versions.py`: Allowlisted metadata commit and classified bounded push retry |
| `scripts/version-and-commit/crates-io.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `scripts/version-and-commit/tests.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `scripts/wait-for-crate.rs` | Adapted | `release.py`: Bounded anonymous NuGet download with diagnostic final failure |
| `src/lib.rs` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `src/main.rs` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `src/sum.rs` | Project-specific | `csharp/; cpp/; examples/; rust submodule`: Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases |
| `tests/integration/mod.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/integration/sum.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/changelog_parsing.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/desktop_release_resolve.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_119.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_127.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_135.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_141.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_143.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_147.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_149.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_150.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_156.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_157.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_160.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_161.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_163.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_168.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_171.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_172.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_188.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/issue_193.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/mod.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/release_naming_tests.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/version_and_commit_behind_check.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/version_and_commit_tag_order.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/workflow_desktop_release.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/workflow_release.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/workflow_release_gates.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/workflow_security.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/ci-cd/workspace_manifest_resolution.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/mod.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |
| `tests/unit/sum.rs` | Adapted regression | `experiments/test_*.py`: Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers |

