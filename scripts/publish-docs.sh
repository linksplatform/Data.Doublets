#!/usr/bin/env bash
set -euo pipefail
: "${GH_TOKEN:?Publishing documentation needs contents: write}"
: "${GITHUB_REPOSITORY:?Repository is required}"
[[ -s _site/csharp/Platform.Data.Doublets.pdf && -s _site/csharp/index.html && -s _site/cpp/index.html ]]
# Credentials stay in gh's helper, outside URLs and shell tracing.
gh auth setup-git
publish_dir=$(mktemp -d)
trap 'rm -rf "$publish_dir"' EXIT
git clone --branch gh-pages --single-branch "https://github.com/$GITHUB_REPOSITORY.git" "$publish_dir"
rm -rf "$publish_dir/csharp" "$publish_dir/cpp"
cp -R _site/csharp "$publish_dir/csharp"
cp -R _site/cpp "$publish_dir/cpp"
touch "$publish_dir/.nojekyll"
git -C "$publish_dir" add csharp cpp .nojekyll
if git -C "$publish_dir" diff --cached --quiet; then exit 0; fi
git -C "$publish_dir" -c user.name=github-actions[bot] -c user.email=41898282+github-actions[bot]@users.noreply.github.com commit -m "Publish C#/C++ APIs and source PDF"
git -C "$publish_dir" push origin HEAD:gh-pages
