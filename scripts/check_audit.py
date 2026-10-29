"""Fail NuGet audit on any vulnerability or an incomplete audit response."""
import json
from pathlib import Path
import sys

report = json.loads(Path(sys.argv[1]).read_text())
assert report.get('version') == 1 and report.get('projects'), 'NuGet audit response is incomplete'
errors = []
for project in report['projects']:
    # With --vulnerable, a clean project is represented by its path only.
    # Restore has already completed with NuGetAuditMode=all and -warnaserror.
    assert project.get('path'), 'NuGet audit project path is missing'
    for framework in project.get('frameworks', []):
        for package in framework.get('topLevelPackages', []) + framework.get('transitivePackages', []):
            for vulnerability in package.get('vulnerabilities', []):
                errors.append(f"{project['path']}: {package['id']} {package['resolvedVersion']} {vulnerability['severity']} {vulnerability['advisoryurl']}")
assert not report.get('problems'), 'NuGet audit reported errors: ' + str(report.get('problems'))
assert not errors, '\n'.join(errors)
print('NuGet audit completed without vulnerabilities')
