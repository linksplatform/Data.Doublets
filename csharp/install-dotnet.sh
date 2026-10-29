#!/usr/bin/env bash
# Install the SDK selected by global.json without changing system apt sources.
set -euo pipefail
repo_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
version=$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["sdk"]["version"])' "$repo_root/global.json")
install_dir=${DOTNET_INSTALL_DIR:-$HOME/.dotnet}
installer=$(mktemp)
trap 'rm -f "$installer"' EXIT
curl --fail --silent --show-error --location https://dot.net/v1/dotnet-install.sh --output "$installer"
bash "$installer" --version "$version" --install-dir "$install_dir"
echo "Installed .NET $version in $install_dir; add that directory to PATH."
