"""Apply bounded Conan 2 compatibility fixes to the pinned upstream recipes.

Header-only dependencies must propagate their headers and link requirements.
This workaround is kept explicit until the upstream recipes fix their metadata.
"""

from pathlib import Path
import sys
import subprocess


def modern_metadata(source):
    import re

    # The final assignment in several pinned recipes has no trailing newline.
    return re.sub(
        r"^[ \t]*self\.cpp_info(?:\.components\[[^\n]+?\])?\.(?:names|filenames)\[[^\n]+(?:\n|$)",
        "",
        source,
        flags=re.MULTILINE,
    )


def package_id_metadata(source, name):
    import re

    if name == "cpuinfo":
        # Date-based upstream versions are not semantic versions. Consumers must
        # include the complete version in their ABI identity, including when the
        # relationship between the two package types is not known.
        source, count = re.subn(
            r"(class \w+\(ConanFile\):\n)",
            r'\1    package_id_non_embed_mode = "full_mode"\n    package_id_unknown_mode = "full_mode"\n',
            source,
            count=1,
        )
        if count != 1:
            raise ValueError("Review the pinned cpuinfo recipe structure")
    return source


def patch(directory):
    import re

    for recipe in sorted(directory.glob("recipes/platform.*/all/conanfile.py")):
        source = subprocess.check_output(
            [
                "git",
                "-C",
                str(directory),
                "show",
                "HEAD:" + recipe.relative_to(directory).as_posix(),
            ]
        ).decode()
        source, count = re.subn(
            r"(class \w+\(ConanFile\):\n)",
            r'\1    package_type = "header-library"\n',
            source,
            count=1,
        )
        if count != 1:
            raise ValueError(f"Unexpected recipe structure: {recipe}")
        source = modern_metadata(source)
        source = (
            source.replace("ms-gsl/4.0.0", "ms-gsl/5.0.1")
            .replace("mio/cci.20201220", "mio/cci.20230303")
            .replace("cpuinfo/cci.20220228", "cpuinfo/cci.20260612")
        )
        # Internal Conan aliases follow Conan 2 naming guidance. CMake targets,
        # public namespaces, headers and NuGet identities stay unchanged.
        source = re.sub(
            r"([\"\x27])platform\.([a-z.]+)(?=/|[\"\x27])",
            lambda match: match[1] + "platform-" + match[2].replace(".", "-"),
            source,
        )
        if recipe.parts[-3] == "platform.data":
            source = source.replace("import os\n", "import os\nfrom pathlib import Path\n")
            marker = '        get(self, **self.conan_data["sources"][self.version]["source"], strip_root=True)'
            if marker not in source:
                raise ValueError("Unexpected platform.data source method")
            source = source.replace(
                marker,
                marker
                + '\n        for header in Path(self.source_folder).rglob("*.h"):\n            text = header.read_text(encoding="utf-8-sig")\n            header.write_text(text.replace("constexpr auto constants = storage.Constants;", "constexpr auto constants = TStorage::Constants;"))',
            )
        if recipe.parts[-3] == "platform.memory":
            source = source.replace("import os\n", "import os\nfrom pathlib import Path\n")
            marker = '        get(self, **self.conan_data["sources"][self.version]["source"], strip_root=True)'
            if source.count(marker) != 1:
                raise ValueError("Unexpected platform.memory source method")
            source = source.replace(
                marker,
                marker
                + """
        headers = list(Path(self.source_folder).rglob("HeapResizableDirectMemory.h"))
        if len(headers) != 1:
            raise ValueError("Expected one upstream heap memory header")
        header = headers[0]
        text = header.read_text(encoding="utf-8-sig")
        old = "delete static_cast<std::byte*>(Pointer());"
        if text.count(old) != 1:
            raise ValueError("Upstream heap destructor changed; review the allocator workaround")
        header.write_text(text.replace(old, "std::free(Pointer());"))""",
            )
        recipe.write_text(source)


if __name__ == "__main__":
    patch(Path(sys.argv[1]))
