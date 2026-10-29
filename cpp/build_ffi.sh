#!/usr/bin/env bash
set -euo pipefail
if [[ ${CI_VERBOSE:-0} == 1 ]]; then set -x; fi
repo_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
git -C "$repo_root" submodule update --init rust
toolchain=${RUST_FFI_TOOLCHAIN:-nightly-2026-10-01}
rustup toolchain install "$toolchain" --profile minimal
cd "$repo_root/rust/ffi"
CARGO_BUILD_JOBS=${CARGO_BUILD_JOBS:-1} cargo "+$toolchain" build --release
