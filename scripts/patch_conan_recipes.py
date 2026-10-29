"""Apply bounded Conan 2 compatibility fixes to the pinned upstream recipes.

Header-only dependencies must propagate their headers and link requirements.
This workaround is kept explicit until the upstream recipes fix their metadata.
"""
from pathlib import Path
import re
import sys
import subprocess
import textwrap


def modern_metadata(source):
    # The final assignment in several pinned recipes has no trailing newline.
    return re.sub(r'^[ \t]*self\.cpp_info(?:\.components\[[^\n]+?\])?\.(?:names|filenames)\[[^\n]+(?:\n|$)', '', source, flags=re.MULTILINE)


def dependency_source_compatibility(self):
    from pathlib import Path
    import re
    root = Path(self.source_folder)
    # These dependencies predate CMake's removal of compatibility below 3.10.
    for path in root.rglob('CMakeLists.txt'):
        source = path.read_text()
        def minimum(match):
            version = tuple(int(part) for part in match.group(2).split('.'))
            return match.group(1) + ('3.13' if version < (3, 13) else match.group(2))
        path.write_text(re.sub(r'(cmake_minimum_required\s*\(\s*VERSION\s+)([0-9]+(?:\.[0-9]+)*)', minimum, source, flags=re.IGNORECASE))
    if self.name == 'gtest':
        path = root / 'googletest/include/gtest/gtest-printers.h'
        source = path.read_text()
        for operator, scale in (('*', 'mulfor6'), ('/', 'divfor6')):
            old = f'static_cast<int32_t>(val {operator} {scale} + 0.5)'
            if source.count(old) != 1:
                raise ValueError('Review the pinned GTest precision conversion')
            source = source.replace(old, f'static_cast<FloatType>({old})')
        path.write_text(source)
    if self.name == 'benchmark':
        path = root / 'src/CMakeLists.txt'
        source = path.read_text()
        old = 'target_link_libraries(benchmark  ${BENCHMARK_CXX_LIBRARIES} ${CMAKE_THREAD_LIBS_INIT})'
        if source.count(old) != 1:
            raise ValueError('Review the upstream benchmark thread target')
        source = source.replace(old, 'target_link_libraries(benchmark ${BENCHMARK_CXX_LIBRARIES} Threads::Threads)')
        source, count = re.subn(r'if\(NOT CMAKE_THREAD_LIBS_INIT[^\n]+\n.*?\nendif\(\)', '', source, count=1, flags=re.DOTALL)
        if count != 1:
            raise ValueError('Review the obsolete benchmark pthread workaround')
        path.write_text(source)
    if self.name == 'cpuinfo':
        path = root / 'deps/clog/src/clog.c'
        source = path.read_text()
        # Consume short writes, retry EINTR, and stop on a closed/broken sink.
        helper = '''
#if !defined(_WIN32) && !defined(__hexagon__)
#include <errno.h>
static void clog_write_all(int fd, const char* bytes, size_t size) {
    while (size != 0) {
        ssize_t count = write(fd, bytes, size);
        if (count < 0 && errno == EINTR) continue;
        if (count <= 0) break;
        bytes += count;
        size -= (size_t)count;
    }
}
#endif
'''
        source, count = re.subn(r'\bwrite\((STD(?:ERR|OUT)_FILENO),', r'clog_write_all(\1,', source)
        if count != 5:
            raise ValueError('Review the five upstream clog write calls')
        path.write_text(source.replace('#include <clog.h>', '#include <clog.h>\n' + helper))


def patch(directory):
    for recipe in sorted(directory.glob('recipes/platform.*/all/conanfile.py')):
        source = subprocess.check_output(['git', '-C', str(directory), 'show', 'HEAD:' + recipe.relative_to(directory).as_posix()]).decode()
        source, count = re.subn(r'(class \w+\(ConanFile\):\n)', r'\1    package_type = "header-library"\n', source, count=1)
        if count != 1:
            raise ValueError(f'Unexpected recipe structure: {recipe}')
        source = modern_metadata(source)
        # Internal Conan aliases follow Conan 2 naming guidance. CMake targets,
        # public namespaces, headers and NuGet identities stay unchanged.
        source = re.sub(r'([\"\x27])platform\.([a-z.]+)(?=/|[\"\x27])', lambda match: match[1] + 'platform-' + match[2].replace('.', '-'), source)
        if recipe.parts[-3] == 'platform.data':
            source = source.replace('import os\n', 'import os\nfrom pathlib import Path\n')
            marker = '        get(self, **self.conan_data["sources"][self.version]["source"], strip_root=True)'
            if marker not in source:
                raise ValueError('Unexpected platform.data source method')
            source = source.replace(marker, marker + '\n        for header in Path(self.source_folder).rglob("*.h"):\n            text = header.read_text(encoding="utf-8-sig")\n            header.write_text(text.replace("constexpr auto constants = storage.Constants;", "constexpr auto constants = TStorage::Constants;"))')
        if recipe.parts[-3] == 'platform.memory':
            source = source.replace('import os\n', 'import os\nfrom pathlib import Path\n')
            marker = '        get(self, **self.conan_data["sources"][self.version]["source"], strip_root=True)'
            if source.count(marker) != 1:
                raise ValueError('Unexpected platform.memory source method')
            source = source.replace(marker, marker + '''
        headers = list(Path(self.source_folder).rglob("HeapResizableDirectMemory.h"))
        if len(headers) != 1:
            raise ValueError("Expected one upstream heap memory header")
        header = headers[0]
        text = header.read_text(encoding="utf-8-sig")
        old = "delete static_cast<std::byte*>(Pointer());"
        if text.count(old) != 1:
            raise ValueError("Upstream heap destructor changed; review the allocator workaround")
        header.write_text(text.replace(old, "std::free(Pointer());"))''')
        recipe.write_text(source)
    import inspect
    compatibility = textwrap.dedent(inspect.getsource(dependency_source_compatibility))
    for name in ('benchmark', 'cpuinfo', 'gtest', 'ms-gsl'):
        recipe = directory / 'recipes' / name / 'all/conanfile.py'
        source = subprocess.check_output(['git', '-C', str(directory), 'show', 'HEAD:' + recipe.relative_to(directory).as_posix()]).decode()
        source = modern_metadata(source)
        source, count = re.subn(r'(        get\(self, \*\*self\.conan_data\["sources"\]\[self\.version\],[^)]*strip_root=True\))', r'\1\n        dependency_source_compatibility(self)', source, count=1)
        if count != 1:
            raise ValueError(f'Review the pinned dependency source method: {name}')
        recipe.write_text(source + '\n\n' + compatibility)


if __name__ == '__main__':
    patch(Path(sys.argv[1]))
