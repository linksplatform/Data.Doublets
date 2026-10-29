"""Apply bounded Conan 2 compatibility fixes to the pinned upstream recipes.

Header-only dependencies must propagate their headers and link requirements.
This workaround is kept explicit until the upstream recipes fix their metadata.
"""
from pathlib import Path
import re
import sys
import subprocess


def patch(directory):
    for recipe in sorted(directory.glob('recipes/platform.*/all/conanfile.py')):
        source = subprocess.check_output(['git', '-C', str(directory), 'show', 'HEAD:' + recipe.relative_to(directory).as_posix()]).decode()
        source, count = re.subn(r'(class \w+\(ConanFile\):\n)', r'\1    package_type = "header-library"\n', source, count=1)
        if count != 1:
            raise ValueError(f'Unexpected recipe structure: {recipe}')
        source = re.sub(r'^\s*self\.cpp_info\.names\[[^\n]+\n', '\n', source, flags=re.MULTILINE)
        if recipe.parts[-3] == 'platform.data':
            source = source.replace('import os\n', 'import os\nfrom pathlib import Path\n')
            marker = '        get(self, **self.conan_data["sources"][self.version]["source"], strip_root=True)'
            if marker not in source:
                raise ValueError('Unexpected platform.data source method')
            source = source.replace(marker, marker + '\n        for header in Path(self.source_folder).rglob("*.h"):\n            text = header.read_text(encoding="utf-8-sig")\n            header.write_text(text.replace("constexpr auto constants = storage.Constants;", "constexpr auto constants = TStorage::Constants;"))')
        recipe.write_text(source)


if __name__ == '__main__':
    patch(Path(sys.argv[1]))
