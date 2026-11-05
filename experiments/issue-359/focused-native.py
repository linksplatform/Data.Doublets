"""Compile only the positional tests using CMake's real dependency flags."""
import json
from pathlib import Path
import resource
import shlex
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
build = ROOT / "cpp/build"
entry = json.loads((build / "compile_commands.json").read_text())[0]
command = shlex.split(entry["command"])
output = command.index("-o")
del command[output : output + 2]
command.remove("-c")
command.remove(entry["file"])
link = shlex.split((build / "CMakeFiles/Platform.Data.Doublets.Tests.dir/link.txt").read_text())
libraries = [argument for argument in link if argument.endswith(".a") or argument.startswith("-l")]
executable = build / "positional-tests"
# Bound the compiler independently of the container's shared memory limit.
resource.setrlimit(resource.RLIMIT_AS, (1900 * 1024 * 1024, 1900 * 1024 * 1024))
subprocess.run(
    [*command, str(ROOT / "experiments/issue-359/focused-native.cpp"), *libraries, "-pthread", "-o", str(executable)],
    cwd=build,
    check=True,
)
print(f"Compiler peak RSS: {resource.getrusage(resource.RUSAGE_CHILDREN).ru_maxrss} KiB", flush=True)
result = subprocess.run([str(executable), *sys.argv[1:]], cwd=ROOT, check=False)
raise SystemExit(result.returncode)
