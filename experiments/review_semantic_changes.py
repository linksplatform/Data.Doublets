"""Normalize C-family formatting to make the final PR self-review tractable."""
from pathlib import Path
import difflib
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
changes = subprocess.check_output(['git', 'diff', '--name-status', '-M20%', 'origin/main...HEAD']).decode().splitlines()
output = []
for change in changes:
    status, *paths = change.split('\t')
    before, after = paths[0], paths[-1]
    if not after.startswith(('csharp/', 'cpp/', 'c/')) or Path(after).suffix not in ('.cs', '.cpp', '.h'):
        continue
    old = subprocess.check_output(['git', 'show', 'origin/main:' + before]) if status[0] != 'A' else b''
    new = (ROOT / after).read_bytes() if status[0] != 'D' else b''
    suffix = Path(after).suffix
    style = '{BasedOnStyle: Microsoft, Language: CSharp}' if suffix == '.cs' else 'file'
    def normalized(source):
        # Retain quoted strings while excluding comments from this review view.
        pattern = r'("(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|//[^\n]*|/\*.*?\*/)'
        source = re.sub(pattern, lambda match: match[0] if match[0].startswith(('"', "'")) else '', source.decode('utf-8-sig'), flags=re.DOTALL).encode()
        text = subprocess.check_output(['clang-format-18', '--assume-filename=' + after, '--style=' + style], input=source).decode('utf-8-sig')
        if suffix == '.cs':
            text = '\n'.join(line for line in text.splitlines() if not line.strip().startswith('///')) + '\n'
        return [line for line in text.splitlines(keepends=True) if line.strip()]
    diff = list(difflib.unified_diff(normalized(old), normalized(new), fromfile=before, tofile=after, n=3))
    output.extend(diff)
(ROOT / 'dev/log/issues/524/pulls/526/local/final-code-review.diff').write_text(''.join(output))
print(f'{len(output)} normalized diff lines')
