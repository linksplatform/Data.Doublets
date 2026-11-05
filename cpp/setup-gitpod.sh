#!/usr/bin/env bash
set -euo pipefail
cpp_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
bash "$cpp_root/setup.sh"
