"""Reject incomplete CodeQL reports and every reported finding, including notes."""
import json
from pathlib import Path
import sys


def check(directory):
    reports = sorted(Path(directory).rglob('*.sarif'))
    if not reports:
        raise ValueError('CodeQL did not produce a SARIF report')
    findings = []
    for path in reports:
        report = json.loads(path.read_text())
        if report.get('version') != '2.1.0' or not report.get('runs'):
            raise ValueError(f'Incomplete SARIF report: {path}')
        for run in report['runs']:
            if not run.get('tool', {}).get('driver', {}).get('name') or 'results' not in run:
                raise ValueError(f'Incomplete SARIF run: {path}')
            for invocation in run.get('invocations', []):
                if invocation.get('executionSuccessful') is False:
                    raise ValueError(f'CodeQL analysis failed: {path}')
                if any(item.get('level') in ('error', 'warning') for item in invocation.get('toolExecutionNotifications', [])):
                    raise ValueError(f'CodeQL analysis reported a diagnostic: {path}')
            for result in run['results']:
                locations = result.get('locations', [])
                location = locations[0].get('physicalLocation', {}) if locations else {}
                uri = location.get('artifactLocation', {}).get('uri', str(path))
                line = location.get('region', {}).get('startLine', 1)
                findings.append(f"{uri}:{line}: {result.get('ruleId', 'unknown')}: {result.get('message', {}).get('text', 'CodeQL finding')}")
    if findings:
        raise ValueError('\n'.join(findings))
    print(f'CodeQL: {len(reports)} complete report(s), no findings')


if __name__ == '__main__':
    try:
        check(sys.argv[1])
    except (ValueError, KeyError, TypeError) as error:
        raise SystemExit(str(error)) from error
