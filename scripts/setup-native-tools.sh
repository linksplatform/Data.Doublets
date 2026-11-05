#!/usr/bin/env bash
# Official releases for tools whose Ubuntu packages trail upstream.
set -euo pipefail
install_dir=${NATIVE_TOOLS_DIR:-$HOME/.local/native-tools}
scratch=$(mktemp -d)
trap 'rm -rf "$scratch"' EXIT
mkdir -p "$install_dir/bin"
case ${1:-} in
    doxygen)
        curl --fail --silent --show-error --location \
            https://github.com/doxygen/doxygen/releases/download/Release_1_18_0/doxygen-1.18.0.linux.bin.tar.gz -o "$scratch/tool.tar.gz"
        echo "14fa81bdc34171edb5f1f02b1d60e74802f0439b77fa44e592565d517d72df90  $scratch/tool.tar.gz" | sha256sum --check
        tar -xzf "$scratch/tool.tar.gz" -C "$scratch"
        install -m 755 "$scratch/doxygen-1.18.0/bin/doxygen" "$install_dir/bin/doxygen"
        "$install_dir/bin/doxygen" --version
        ;;
    cppcheck)
        curl --fail --silent --show-error --location \
            https://github.com/cppcheck-opensource/cppcheck/archive/refs/tags/2.22.0.tar.gz -o "$scratch/tool.tar.gz"
        echo "d74945deb2d50393430e07596b766f8a779512c7f60dac2a30ea64e059ece57b  $scratch/tool.tar.gz" | sha256sum --check
        tar -xzf "$scratch/tool.tar.gz" -C "$scratch"
        make -C "$scratch/cppcheck-2.22.0" -j 1 MATCHCOMPILER=yes \
            CXXFLAGS='-O2 -DNDEBUG' FILESDIR="$install_dir/share/cppcheck" PREFIX="$install_dir" install
        "$install_dir/bin/cppcheck" --version
        ;;
    *) echo 'Usage: setup-native-tools.sh {doxygen|cppcheck}' >&2; exit 2 ;;
esac
if [[ -n ${GITHUB_PATH:-} ]]; then echo "$install_dir/bin" >> "$GITHUB_PATH"; fi
