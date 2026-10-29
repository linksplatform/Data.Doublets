"""Fast, dependency-free CI checks also run by the local pre-commit hook."""
import ast
import json
from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]


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
    if errors:
        raise SystemExit('\n'.join(errors))
    print('Repository configuration, source sizes, scripts and action pins passed')


if __name__ == '__main__':
    main()
