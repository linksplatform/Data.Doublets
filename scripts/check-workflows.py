"""Keep actionlint's checks while validating its known queue-key false positive.

actionlint 1.7.12 lacks GitHub's concurrency.queue schema (upstream #657).
Validate that field here and exclude only that exact obsolete-schema diagnostic.
Every other syntax, expression and embedded-shell diagnostic remains fatal.
"""
import os
from pathlib import Path
import subprocess
import yaml


def validate_queue(workflow):
    scopes = [workflow, *workflow.get('jobs', {}).values()]
    for scope in scopes:
        concurrency = scope.get('concurrency', {})
        if not isinstance(concurrency, dict) or 'queue' not in concurrency:
            continue
        if concurrency['queue'] not in ('single', 'max'):
            raise ValueError('concurrency.queue must be single or max')
        if concurrency['queue'] == 'max' and concurrency.get('cancel-in-progress', False) is not False:
            raise ValueError('queue: max requires cancel-in-progress: false')


if __name__ == '__main__':
    for path in Path('.github/workflows').glob('*.yml'):
        validate_queue(yaml.safe_load(path.read_text()))
    subprocess.run([os.environ.get('ACTIONLINT', 'actionlint'), '-color', '-ignore', '^unexpected key "queue" for "concurrency" section\\.', *map(str, Path('.github/workflows').glob('*.yml'))], check=True)
