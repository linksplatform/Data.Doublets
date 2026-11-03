#!/usr/bin/env bash
set -euo pipefail
if [[ -n ${BASE_REF:-} ]]; then
    git check-ref-format "refs/heads/$BASE_REF"
    git fetch origin "refs/heads/$BASE_REF:refs/remotes/origin/$BASE_REF"
    git -c user.name='github-actions[bot]' \
        -c user.email='41898282+github-actions[bot]@users.noreply.github.com' \
        merge --no-edit "refs/remotes/origin/$BASE_REF"
fi
