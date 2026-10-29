#!/usr/bin/env bash
set -euo pipefail
cpp_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
python3 -m pip install -r "$cpp_root/../scripts/requirements-ci.txt"
conan profile detect --force
bash "$cpp_root/install-local-conan-dependencies.sh"
