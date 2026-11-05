#!/usr/bin/env bash
set -euo pipefail
if [[ ${GITHUB_EVENT_NAME:-} == pull_request ]]; then
    BASE_REF=${GITHUB_BASE_REF:-main} bash scripts/fresh-merge.sh
fi
python3 scripts/versions.py prepare
