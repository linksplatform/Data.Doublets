"""Write an explicit native Conan 2 profile from compiler predefined macros.

Unlike `conan profile detect`, this does not guess the compiler or ABI. Set CXX
and CC to select the toolchain, or CPP_CONAN_PROFILE to use a reviewed profile.
"""

import os
from pathlib import Path
import platform
import shlex
import subprocess


def profile():
    cxx = shlex.split(os.environ.get("CXX", "c++"))
    definitions = subprocess.check_output([*cxx, "-dM", "-E", "-x", "c++", "-"], input=b"").decode()
    macros = dict(
        line.removeprefix("#define ").split(" ", 1)
        for line in definitions.splitlines()
        if line.startswith("#define ") and " " in line.removeprefix("#define ")
    )
    system = platform.system()
    if "__clang__" in macros:
        compiler = "apple-clang" if system == "Darwin" else "clang"
        version = macros["__clang_major__"]
    elif "__GNUC__" in macros:
        compiler, version = "gcc", macros["__GNUC__"]
    else:
        raise ValueError("Set CPP_CONAN_PROFILE for this unsupported compiler")
    architectures = {"x86_64": "x86_64", "amd64": "x86_64", "aarch64": "armv8", "arm64": "armv8"}
    arch = architectures.get(platform.machine().lower())
    if system not in ("Linux", "Darwin") or arch is None:
        raise ValueError("Set CPP_CONAN_PROFILE for this unsupported native platform")
    return "\n".join(
        (
            "[settings]",
            f"os={'Macos' if system == 'Darwin' else system}",
            f"arch={arch}",
            f"compiler={compiler}",
            f"compiler.version={version}",
            f"compiler.libcxx={'libc++' if system == 'Darwin' else 'libstdc++11'}",
            "compiler.cppstd=20",
            "build_type=Release",
            "",
        )
    )


if __name__ == "__main__":
    custom = os.environ.get("CPP_CONAN_PROFILE")
    if custom:
        if not Path(custom).is_file():
            raise ValueError("CPP_CONAN_PROFILE must name an existing reviewed profile")
        print(f"Using explicit Conan profile: {custom}")
    else:
        home = Path(subprocess.check_output(["conan", "config", "home"]).decode().strip())
        destination = home / "profiles/default"
        destination.parent.mkdir(parents=True, exist_ok=True)
        text = profile()
        destination.write_text(text)
        print(text)
