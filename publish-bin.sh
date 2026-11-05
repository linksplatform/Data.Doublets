#!/usr/bin/env bash
# Legacy binary-branch entry point, also usable outside Travis.
set -euo pipefail
: "${GH_TOKEN:=${GITHUB_TOKEN:-}}"
: "${GH_TOKEN:?A token with contents: write is required}"
: "${GITHUB_REPOSITORY:=${TRAVIS_REPO_SLUG:-}}"
: "${GITHUB_REPOSITORY:?Set owner/repository}"
: "${TARGET_PLATFORM:?Set a target platform}"
: "${LIB_PATH:?Set the binary directory}"
: "${LIB_NAME:?Set the binary name}"
: "${TARGET_LIB_NAME:?Set the installed binary name}"
[[ $TARGET_PLATFORM =~ ^[a-zA-Z0-9_-]+$ && $TARGET_LIB_NAME != */* ]]
[[ -s $LIB_PATH/$LIB_NAME ]]
export GH_TOKEN
gh auth setup-git
source_sha=$(git rev-parse HEAD)
publish_dir=$(mktemp -d)
trap 'rm -rf "$publish_dir"' EXIT
git clone "https://github.com/$GITHUB_REPOSITORY.git" "$publish_dir"
if git -C "$publish_dir" show-ref --verify --quiet refs/remotes/origin/binaries; then
    git -C "$publish_dir" checkout -b binaries origin/binaries
else
    git -C "$publish_dir" checkout --orphan binaries
    git -C "$publish_dir" rm -rf .
fi
mkdir -p "$publish_dir/$TARGET_PLATFORM/native"
cp "$LIB_PATH/$LIB_NAME" "$publish_dir/$TARGET_PLATFORM/native/$TARGET_LIB_NAME"
git -C "$publish_dir" add -- "$TARGET_PLATFORM"
if git -C "$publish_dir" diff --cached --quiet; then exit 0; fi
git -C "$publish_dir" -c user.name=github-actions[bot] -c user.email=41898282+github-actions[bot]@users.noreply.github.com commit -m "Deploy $TARGET_PLATFORM binary for $source_sha"
git -C "$publish_dir" push origin HEAD:binaries
