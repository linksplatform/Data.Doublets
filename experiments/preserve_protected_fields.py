"""One-time migration: retain protected field ABI while checking initialized use.

The original protected names remain fields (nullable during init/disposal).
Private accessors in the base class check the initialization invariant.
"""
from pathlib import Path
import re

for path in Path('csharp/Platform.Data.Doublets/Memory').glob('*/Generic/*MemoryLinksBase.cs'):
    source = path.read_text()
    pattern = r'private (?P<type>[\w<>]+)\? (?P<back>_\w+);\s+protected (?P=type) (?P<name>\w+)\s*\{\s*get => (?P=back) \?\? throw new System.ObjectDisposedException\(GetType\(\).Name, "Store indices are not initialized or have been disposed\."\);\s*set => (?P=back) = value;\s*\}'
    matches = list(re.finditer(pattern, source))
    for match in reversed(matches):
        name, kind, backing = match['name'], match['type'], match['back']
        replacement = f'protected {kind}? {name};\n        private {kind} Required{name} => {name} ?? throw new ObjectDisposedException(GetType().Name);'
        source = source[:match.start()] + replacement + source[match.end():]
        # Method calls require initialization; assignments preserve old fields.
        source = re.sub(rf'\b{name}\.', f'Required{name}.', source)
        source = re.sub(rf'\b{backing}\b', name, source)
    path.write_text(source)
