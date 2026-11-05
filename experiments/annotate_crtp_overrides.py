"""Annotate only cppcheck's recorded intentional CRTP member customizations."""
from pathlib import Path
import re

root = Path(__file__).resolve().parents[1]
log = root / 'dev/log/issues/524/pulls/526/local/cpp-static-iteration3.log'
locations = {}
for line in log.read_text().splitlines():
    match = re.match(r'(.+\.h):(\d+):\d+: warning:.*\[duplInheritedMember\]', line)
    if match:
        path = Path(match[1])
        if 'TreeMethods' not in path.name:
            raise ValueError(path)
        locations.setdefault(path, set()).add(int(match[2]))
for path, numbers in locations.items():
    lines = path.read_text(encoding='utf-8-sig').splitlines()
    for number in sorted(numbers, reverse=True):
        indent = lines[number - 1][:-len(lines[number - 1].lstrip())]
        lines[number - 1:number - 1] = [indent + '// Intentional CRTP customization of the base tree algorithm.', indent + '// cppcheck-suppress duplInheritedMember']
    path.write_text('\n'.join(lines) + '\n')
print(f'Annotated {sum(map(len, locations.values()))} intentional CRTP customizations')
