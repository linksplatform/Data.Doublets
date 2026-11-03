#!/usr/bin/env bash
set -euo pipefail
if [[ ${CI_VERBOSE:-0} == 1 ]]; then set -x; fi
repo_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
cd "$repo_root"
# Separate compilations preserve each executable's entry point and references.
# Recompile even cached projects so the CodeQL tracer observes every compiler.
while IFS= read -r -d '' project; do
    dotnet build "$project" -c Release --no-incremental -m:1 -warnaserror -p:UseSharedCompilation=false
done < <(git ls-files -z '*.csproj')
