#!/usr/bin/env bash
set -euo pipefail
: "${GH_TOKEN:?Publishing documentation needs contents: write}"
: "${GITHUB_REPOSITORY:?Repository is required}"
[[ -s _site/csharp/Platform.Data.Doublets.pdf && -s _site/csharp/index.html && -s _site/cpp/index.html ]]
# Credentials stay in gh's helper, outside URLs and shell tracing.
gh auth setup-git
publish_dir=$(mktemp -d)
trap 'rm -rf "$publish_dir"' EXIT
git -C "$publish_dir" init --quiet
git -C "$publish_dir" remote add origin "https://github.com/$GITHUB_REPOSITORY.git"
for attempt in 1 2 3; do
    base=''
    if git -C "$publish_dir" ls-remote --exit-code --heads origin gh-pages > "$publish_dir/remote-ref"; then
        git -C "$publish_dir" fetch --no-tags origin refs/heads/gh-pages
        base=$(git -C "$publish_dir" rev-parse FETCH_HEAD)
        git -C "$publish_dir" checkout --force -B gh-pages "$base"
    else
        status=$?
        if [[ $status != 2 || $attempt != 1 ]]; then
            echo 'Cannot determine the documentation branch state; refusing to publish.' >&2
            exit 1
        fi
        git -C "$publish_dir" checkout --orphan gh-pages
    fi
    # Replace only our outputs. Preserve documentation from other languages.
    rm -rf "$publish_dir/csharp" "$publish_dir/cpp"
    cp -R _site/csharp "$publish_dir/csharp"
    cp -R _site/cpp "$publish_dir/cpp"
    touch "$publish_dir/.nojekyll"
    git -C "$publish_dir" add csharp cpp .nojekyll
    if git -C "$publish_dir" diff --cached --quiet; then exit 0; fi
    git -C "$publish_dir" -c user.name=github-actions[bot] -c user.email=41898282+github-actions[bot]@users.noreply.github.com commit -m "Publish C#/C++ APIs and source PDF"
    if git -C "$publish_dir" push origin HEAD:gh-pages > "$publish_dir/push.log" 2>&1; then
        cat "$publish_dir/push.log"
        exit 0
    fi
    cat "$publish_dir/push.log" >&2
    if rg -qi 'GH006|GH013|protected branch|rule violations|permission.*denied|authentication failed' "$publish_dir/push.log"; then
        echo 'Documentation publication rejected: configure gh-pages write access/rules for the Actions token.' >&2
        exit 1
    fi
    remote=$(git -C "$publish_dir" ls-remote --exit-code --heads origin gh-pages)
    if [[ ${remote%%[[:space:]]*} == "$base" ]]; then
        echo 'Documentation push failed without a concurrent branch update; refusing to retry.' >&2
        exit 1
    fi
    echo "Concurrent gh-pages update; reapplying generated pages (attempt $attempt/3)."
done
echo 'Documentation publication could not converge after three attempts.' >&2
exit 1
