#!/usr/bin/env bash
# Use the official current CMake release; the PyPI redistribution trails it.
set -euo pipefail
version=4.4.5
if [[ $(cmake --version 2>/dev/null | head -1) == "cmake version $version" ]]; then exit 0; fi
strip=1
case "$(uname -s)-$(uname -m)" in
    Linux-x86_64)
        platform=linux-x86_64
        checksum=ed85135c5a5135b45e50423cb406dea04ca3338da0b8ef3c9ede963559680be0 ;;
    Linux-aarch64)
        platform=linux-aarch64
        checksum=301d5063ab259577c1b57ce5490b1aab9ea5c56dc4c822f73499553dab94925f ;;
    Darwin-*)
        platform=macos-universal
        checksum=679e77a96b9ef9484cc6c1445255fc4f84206b48a65420efbedf16bdab55fbff
        strip=3 ;;
    *) echo "Install CMake $version for this platform before native validation." >&2; exit 1 ;;
esac
scratch=$(mktemp -d)
trap 'rm -rf "$scratch"' EXIT
curl --fail --location --silent --show-error \
    "https://github.com/Kitware/CMake/releases/download/v$version/cmake-$version-$platform.tar.gz" \
    --output "$scratch/cmake.tar.gz"
if command -v sha256sum >/dev/null; then
    printf '%s  %s\n' "$checksum" "$scratch/cmake.tar.gz" | sha256sum --check --status
else
    printf '%s  %s\n' "$checksum" "$scratch/cmake.tar.gz" | shasum -a 256 --check --status
fi
install_dir=${CMAKE_INSTALL_DIR:-$HOME/.local/cmake-4.4.5}
mkdir -p "$install_dir"
tar -xzf "$scratch/cmake.tar.gz" --strip-components="$strip" -C "$install_dir"
if [[ -n ${GITHUB_PATH:-} ]]; then printf '%s\n' "$install_dir/bin" >> "$GITHUB_PATH"; fi
printf 'CMake installed: %s/bin\n' "$install_dir"
