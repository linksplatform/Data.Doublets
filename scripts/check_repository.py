"""Fast CI checks also run by the local pre-commit hook."""
import ast
import json
from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET
import yaml

ROOT = Path(__file__).resolve().parents[1]


def trusted_publishing_errors(workflow):
    """Keep publishing credentials confined to trusted jobs and step outputs."""
    errors = []
    source = json.dumps(workflow)
    if re.search(r"secrets\s*(?:\.\s*NUGET_(?:TOKEN|API_KEY)|\[\s*['\"]NUGET_(?:TOKEN|API_KEY))", source):
        errors.append('NuGet trusted publishing must not fall back to a stored API key')
    for name, job in workflow.get('jobs', {}).items():
        login_steps = {step['id'] for step in job.get('steps', []) if step.get('uses', '').startswith('NuGet/login@') and step.get('id')}
        if name in ('credentials', 'publish'):
            if job.get('permissions', {}).get('id-token') != 'write' or not login_steps:
                errors.append(f'{name}: actual NuGet/login exchange and id-token: write are required')
            condition = job.get('if', '')
            if "github.ref == 'refs/heads/main'" not in condition or "github.event_name != 'pull_request'" not in condition:
                errors.append(f'{name}: OIDC must be restricted to trusted main execution')
        elif job.get('permissions', {}).get('id-token'):
            errors.append(f'{name}: NuGet OIDC permissions belong only in credentials/publish')
        if 'NUGET_API_KEY' in json.dumps(job.get('outputs', {})):
            errors.append(f'{name}: temporary NuGet keys must not cross job boundaries')
        for step in job.get('steps', []):
            if step.get('uses', '').startswith('NuGet/login@') and step.get('with', {}).get('user') != '${{ vars.NUGET_USER }}':
                errors.append(f'{name}: login must use the configured NuGet profile vars.NUGET_USER')
            token = step.get('env', {}).get('NUGET_TOKEN')
            if token is not None and token not in {f'${{{{ steps.{login}.outputs.NUGET_API_KEY }}}}' for login in login_steps}:
                errors.append(f'{name}: NUGET_TOKEN must come from this job\'s NuGet/login output')
    return errors


def main():
    errors = []
    paths = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
    # Include new scripts in local validation before their first commit.
    paths += [str(p.relative_to(ROOT)) for folder in ('scripts', '.github', 'csharp', 'cpp') for p in (ROOT / folder).rglob('*') if p.is_file() and not any(x in p.parts for x in ('bin', 'obj', 'build')) and p.suffix in ('.py', '.sh', '.yml')]
    for name in sorted(set(paths)):
        path = ROOT / name
        if not path.is_file() or name.startswith(('dev/log/', 'experiments/')):
            continue
        if path.suffix not in ('.py', '.sh', '.cs', '.h', '.cpp', '.yml', '.json', '.csproj', '.nuspec'):
            continue
        source = path.read_text(encoding='utf-8-sig')
        if len(source.splitlines()) > 1500:
            errors.append(f'{name}: exceeds 1500 lines; split this source file')
        if path.suffix == '.py':
            ast.parse(source, filename=name)
        elif path.suffix == '.json':
            json.loads(source)
        elif path.suffix in ('.csproj', '.nuspec'):
            ET.fromstring(source)
        elif path.suffix == '.sh':
            subprocess.run(['bash', '-n', str(path)], check=True)
        if name.startswith('.github/workflows/'):
            for action in re.findall(r'uses:\s*([^\s#]+)', source):
                if not action.startswith('./') and not re.fullmatch(r'[^@]+@[0-9a-f]{40}', action):
                    errors.append(f'{name}: action must be pinned to reviewed commit: {action}')
            if name == '.github/workflows/ci.yml':
                errors.extend(trusted_publishing_errors(yaml.safe_load(source)))
    if errors:
        raise SystemExit('\n'.join(errors))
    print('Repository configuration, source sizes, scripts and action pins passed')


if __name__ == '__main__':
    main()
