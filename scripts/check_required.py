"""Prevent skipped jobs from turning missing validation into a green check."""
import json
import os


def validate(jobs, main=False):
    failed = [name for name, job in jobs.items() if job['result'] not in ('success', 'skipped')]
    if failed:
        raise ValueError('Validation did not pass: ' + ', '.join(failed))
    expected = {'changes', 'quality', 'security', 'links'}
    outputs = jobs['changes']['outputs']
    pipeline = outputs.get('pipeline') == 'true'
    for language in ('csharp', 'cpp'):
        if pipeline or outputs.get(language) == 'true':
            expected.add(language)
    if pipeline or outputs.get('csharp') == 'true' or outputs.get('cpp') == 'true' or outputs.get('docs') == 'true':
        expected.add('docs')
    if main:
        if pipeline or outputs.get('csharp') == 'true' or outputs.get('cpp') == 'true':
            expected.add('credentials')
        if expected.intersection({'csharp', 'cpp', 'docs'}):
            expected.add('publish')
    missing = sorted(name for name in expected if jobs[name]['result'] != 'success')
    if missing:
        raise ValueError('Required validation was skipped: ' + ', '.join(missing))
    for name, job in jobs.items():
        if job['result'] == 'skipped':
            print(f'::notice::{name} skipped: event/change prerequisites did not require this job')


if __name__ == '__main__':
    validate(json.loads(os.environ['RESULTS']), os.environ.get('GITHUB_REF') == 'refs/heads/main')
