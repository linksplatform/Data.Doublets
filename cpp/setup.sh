#!/usr/bin/env bash
set -euo pipefail
cpp_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
python3 -m pip install -r "$cpp_root/../scripts/requirements-ci.txt"
bash "$cpp_root/../scripts/setup-cmake.sh"
export PATH="${CMAKE_INSTALL_DIR:-$HOME/.local/cmake-4.4.5}/bin:$PATH"
python3 "$cpp_root/../scripts/conan_profile.py"
bash "$cpp_root/install-local-conan-dependencies.sh"
