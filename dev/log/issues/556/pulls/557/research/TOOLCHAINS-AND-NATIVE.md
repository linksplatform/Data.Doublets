# Toolchains, images and native dependency inventory

Registry/release queries: 2026-11-05 and 2026-11-06 UTC. Recheck CI logs for installed compiler versions, rather than infer them from the host container. This complements every NuGet, PyPI, Cargo and Action row in DEPENDENCIES.md.

| Dependency | Before | Latest stable / selected | Primary registry / evidence |
|---|---|---|---|
| .NET SDK | 8.0.427 | 10.0.403 | [Microsoft release index](https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json), dotnet-releases.json; .NET 11 is preview |
| .NET target/runtime | net8.0 | net10.0 / 10.0.14 | Same Microsoft release metadata; all projects, docfx and package consumer updated |
| CPython | workflow 3.12, Ruff/mypy 3.10 | 3.15.2 | [Python downloads](https://www.python.org/downloads/), setup-python registry python-versions.json |
| Rust | implicit 1.99.0 on host / edition 2021 | 1.99.1 / edition 2024 | [Stable channel](https://static.rust-lang.org/dist/channel-rust-stable.toml), rust-latest.txt |
| rustup | runner-provided | 1.29.3 | [Official stable manager metadata](https://static.rust-lang.org/rustup/release-stable.toml), rustup-stable.toml; explicit toolchain installation avoids deprecated automatic behavior |
| Rust submodule / doublets Git dependencies | 645dd2b8e5c9bff1e38335da42e3c008b5d2769e | 18d6a81da69ee661c6fa7563abca4c8b20c3404d | [upstream](https://github.com/linksplatform/doublets-rs/commit/18d6a81da69ee661c6fa7563abca4c8b20c3404d), same revision in both probes and gitlink |
| cargo-audit | absent | 0.22.2 | crates.io sparse index cargo-audit-index.jsonl; CI audits both locks with warnings fatal |
| CMake | minimum 3.13 / older recipe tools | 4.4.5 | [Kitware release](https://github.com/Kitware/CMake/releases/tag/v4.4.5), SHA-verified installer, Conan platform_tool_requires |
| GCC | runner default | 16.2 | [GCC releases](https://gcc.gnu.org/releases.html); Ubuntu 26 package g++-16, recorded CI --version |
| Clang / clang-tidy | Ubuntu default / 18 | 23.1.5 | [LLVM release](https://github.com/llvm/llvm-project/releases/tag/llvmorg-23.1.5), signed apt.llvm.org 23 packages; PyPI clang-tidy trails upstream and is not used |
| clang-format | Ubuntu default | 23.1.5 | PyPI exact wheel; registry table and lock |
| Doxygen | Ubuntu package | 1.18.0 | doxygen-release.json; official binary SHA-256 pinned |
| Cppcheck | Ubuntu package | 2.22.0 | cppcheck-release.json; official source SHA-256 pinned, one-worker build |
| ShellCheck | actionlint image bundled | 0.11.0 (current image bundle) | [upstream](https://github.com/koalaman/shellcheck/releases), pinned actionlint image; no separate obsolete host pin |
| actionlint / image | rhysd 1.7.12 / b1934ee5… | maintained kjanat fork 1.17.0 / d484cccadf56b6a356466ade9af4c8e05c5dce1badaf0a9ecd0d7639d68d9c92 | [release](https://github.com/kjanat/actionlint/releases/tag/v1.17.0), verified source archive rebuilt with patched Go; native queue/self-reference support replaces our compatibility handling |
| Go (workflow checker builder) | released checker embeds 1.27.0 | 1.27.3-alpine / dbaa054703263d06c8b49576667418473b6231f8c0fa69dee103207fdb8e3654 | [official registry](https://go.dev/dl/?mode=json), go-releases.json; unchanged checker source rebuilt to clear 14 runtime advisories |
| Python checker base image | released image bundles Python 3.14 / Pyflakes 3.4 | 3.15.2-alpine / d499023466546cd88a358f80b46fba4016f32597bc4482bb1c47b483a07d457d | python-workflow-image.txt; same Python, pip 26.3 and Pyflakes 4.1.0 as repository constraints |
| OSV Scanner | absent | 2.7.0 | [release](https://github.com/google/osv-scanner/releases/tag/v2.7.0), official binary SHA-256 pinned; native Conan v2 lock parsing and full workflow image audit |
| Gitleaks / pre-commit hook | 8.30.1 | 8.30.1 | github-gitleaks-gitleaks.json; consistent hook and CI downloader |
| zizmor | 1.29.0 | 1.32.0 | github-zizmorcore-zizmor.json; new self-repository check adopted |
| lychee | 0.23.0 | 0.24.2 | github-lycheeverse-lychee.json; action remains latest 2.9.0 |
| Linux runner | ubuntu-24.04 | ubuntu-26.04 | [GA announcement](https://github.blog/changelog/2026-09-17-ubuntu-26-generally-available-and-latest-migration/), all workflows consistent |
| Windows runner | windows-2025 | windows-2025 | Current Windows Server generation; exact .NET SDK installed independently of Visual Studio bundles |
| Gitpod image | gitpod/workspace-dotnet mutable tag | 2026-02-12-23-06-03@sha256:253772ffe6d042a39b7802e3101ace08b6b21914e8116d9bf6d9f84332b78e11 | Docker Hub tags snapshot gitpod-tags.json: newest publisher tag; newer bundled runtime cannot be selected because publisher has not released a newer image. Native setup explicitly installs current CMake/Conan. |

The C++20 language minimum is the existing public header contract, rather than a compiler version. All compilers are updated independently. C++26 remains an unfinished standard; raising the public standard requirement is not necessary for these dependency migrations. C11 is likewise the existing FFI declaration baseline.

## Conan packages (including transitive)

All Platform recipe versions below are the newest versions in the current recipe registry HEAD, 435ad91f44d2fd40c4e4be366163f97333013d65. C# and C++ packages are separately versioned artifacts; NuGet releases are not C++ source releases. Local transformations preserve source archive identities and checksum verification. `cpp/conan.lock` records every selected recipe revision.

| Package | Before | Latest registry / selected | Evidence |
|---|---|---|---|
| gtest | 1.13.0 | 1.18.0 | conan-gtest.yml; current Conan Center recipe |
| benchmark | 1.6.0 | 1.10.0 | conan-benchmark.yml |
| ms-gsl | 4.0.0 | 5.0.1 | conan-ms-gsl.yml; current recipe exported with obsolete metadata removed |
| mio | cci.20201220 | cci.20230303 | conan-mio.yml |
| cpuinfo | cci.20220228 | cci.20260612 | conan-cpuinfo.yml; current recipe not yet published to remote, exported from immutable Center revision |
| platform-interfaces | 0.3.41 | 0.3.41 | pinned registry recipes/platform.interfaces/config.yml |
| platform-delegates | 0.3.7 | 0.3.7 | recipes/platform.delegates/config.yml |
| platform-exceptions | 0.3.2 | 0.3.2 | recipes/platform.exceptions/config.yml |
| platform-converters | 0.2.0 | 0.2.0 | recipes/platform.converters/config.yml |
| platform-hashing | 0.5.6 | 0.5.6 | recipes/platform.hashing/config.yml |
| platform-ranges | 0.2.0 | 0.2.0 | recipes/platform.ranges/config.yml |
| platform-random | 0.2.0 | 0.2.0 | recipes/platform.random/config.yml |
| platform-equality | 0.0.1 | 0.0.1 | recipes/platform.equality/config.yml |
| platform-collections | 0.2.1 | 0.2.1 | recipes/platform.collections/config.yml |
| platform-setters | 0.1.0 | 0.1.0 | recipes/platform.setters/config.yml |
| platform-data | 0.1.1 | 0.1.1 | recipes/platform.data/config.yml |
| platform-numbers | 0.1.0 | 0.1.0 | recipes/platform.numbers/config.yml |
| platform-memory | 0.1.0 | 0.1.0 | recipes/platform.memory/config.yml |
| platform-collections-methods | 0.3.0 | 0.3.0 | recipes/platform.collections.methods/config.yml |
| cmake (build requirement) | recipe-selected older distribution | 4.4.5#platform | Actual graph native-graph-after.json, cpp/conan.lock; uses installed checksum-verified official release |

OS tools supplied by the hosted image (git, bash, curl, tar, make, fontconfig, fonts-dejavu-core, OpenSSL and platform SDKs) follow the current Ubuntu/Windows package repositories; they are not vendored language dependencies. The moved runner image updates their platform together; compiler/analyzer versions that trail upstream are explicitly installed above.
