#!/usr/bin/env bash
# Run bounded experimental parity checks against the same pinned Rust source.
set -euo pipefail
repo_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
cd "$repo_root"
export CARGO_BUILD_JOBS=1
export CARGO_PROFILE_DEV_DEBUG=0
export CARGO_PROFILE_TEST_DEBUG=0
export CARGO_TARGET_DIR="${CARGO_TARGET_DIR:-$repo_root/artifacts/rust-target}"
for manifest in experiments/issue-359/RustParity/Cargo.toml experiments/issue-519/Cargo.toml; do
    cargo fmt --manifest-path "$manifest" --check
    cargo clippy --manifest-path "$manifest" --locked --all-targets -- -D warnings
    cargo test --manifest-path "$manifest" --locked
    cargo run --manifest-path "$manifest" --locked
    cargo audit --file "$(dirname "$manifest")/Cargo.lock" --deny warnings
done
