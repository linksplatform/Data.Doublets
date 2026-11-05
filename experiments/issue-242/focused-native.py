"""Compile only formatter regressions using the configured CMake dependency paths."""
import json
from pathlib import Path
import resource
import shlex
from shutil import which
import subprocess  # nosec B404
import sys

ROOT = Path(__file__).resolve().parents[2]
build = ROOT / "cpp/build"
entry = json.loads((build / "compile_commands.json").read_text())[0]
command = shlex.split(entry["command"])
output = command.index("-o")
del command[output : output + 2]
command.remove("-c")
command.remove(entry["file"])
git = which("git")
if git is None:
    raise SystemExit("git is required")
command = [argument for argument in command if not argument.startswith("-O")]
command += ["-O0", "-g0"]
link = shlex.split((build / "CMakeFiles/Platform.Data.Doublets.Tests.dir/link.txt").read_text())
libraries = [argument for argument in link if argument.endswith(".a") or argument.startswith("-l")]
executable = build / "format-structure-tests"
resource.setrlimit(resource.RLIMIT_AS, (1800 * 1024 * 1024, 1800 * 1024 * 1024))
header = ROOT / "cpp/Platform.Data.Doublets/Platform.Data.Doublets.h"
saved = header.read_bytes()
before = "--before" in sys.argv[1:]
try:
    if before:
        # Fixed revision/path and resolved executable; no shell.
        header.write_bytes(subprocess.check_output(  # nosec B603
            [git, "show", "68d306f40:cpp/Platform.Data.Doublets/Platform.Data.Doublets.h"], cwd=ROOT))
    # CMake paths from this checkout and explicit compiler arguments; no shell.
    result = subprocess.run(  # nosec B603
        [*command, str(ROOT / "experiments/issue-242/focused-native.cpp"), *libraries,
         "-pthread", "-o", str(executable)], cwd=build, check=False)
finally:
    header.write_bytes(saved)
print(f"Compiler peak RSS: {resource.getrusage(resource.RUSAGE_CHILDREN).ru_maxrss} KiB", flush=True)
if result.returncode:
    raise SystemExit(result.returncode)
if before:
    raise SystemExit("Unexpected success: the baseline should lack FormatStructure")
# The executable was just built above; no shell or user-controlled command.
raise SystemExit(subprocess.run([str(executable)], cwd=ROOT, check=False).returncode)  # nosec B603
