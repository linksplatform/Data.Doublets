#!/usr/bin/env bash
# Verify exact public packages after main publication using the normal consumers.
# Requires the configured native build's compile_commands.json/link.txt and SDK.
# Usage: bash experiments/issue-538/verify-public-consumers.sh 0.1.2 0.18.4
set -euo pipefail
repo_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$repo_root"
native_version=${1:?Provide the published C++ version}
managed_version=${2:?Provide the published C# version}
for version in "$native_version" "$managed_version"; do
    if [[ ! $version =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
        echo "Expected an exact stable package version" >&2
        exit 1
    fi
done
mkdir -p artifacts/packages
for package in "Platform.Data.Doublets.TemplateLibrary:$native_version" "Platform.Data.Doublets:$managed_version"; do
    package_id=${package%:*}
    package_version=${package#*:}
    lower_id=${package_id,,}
    url="https://api.nuget.org/v3-flatcontainer/$lower_id/$package_version/$lower_id.$package_version.nupkg"
    # Deliberately no authentication or local build: test anonymous public bytes.
    curl --fail --silent --show-error --location "$url" \
        --output "artifacts/packages/$package_id.$package_version.nupkg"
    echo "Downloaded $url"
done
curl --fail --silent --show-error --location \
    "https://github.com/linksplatform/Data.Doublets/releases/download/cpp_$native_version/platform.data.doublets_$native_version.zip" \
    --output "artifacts/packages/platform.data.doublets_$native_version.zip"
PYTHONPATH=scripts python3 - "$native_version" "$managed_version" <<'PY'
import sys
import smoke_packages

source_info = smoke_packages.info
versions = {'cpp': sys.argv[1], 'csharp': sys.argv[2]}


def public_info(language):
    return {**source_info(language), 'version': versions[language]}


smoke_packages.info = public_info
for language in ('csharp', 'cpp'):
    smoke_packages.smoke(language)
PY
