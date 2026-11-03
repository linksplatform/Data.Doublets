#!/usr/bin/env bash
set -euo pipefail
if [[ ${CI_VERBOSE:-0} == 1 ]]; then set -x; fi
repo_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
cd "$repo_root"
ruff check scripts
ruff format --check scripts
mypy scripts
python3 -m unittest discover -s experiments -p 'test_*.py' -v
