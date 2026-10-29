"""Compile a bounded upstream memory reproduction using resolved Conan headers."""
import argparse
import json
from pathlib import Path
import shlex
import subprocess

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('build_dir', type=Path)
parser.add_argument('--source', type=Path, default=root / 'experiments/heap_memory_sanitizer.cpp')
args = parser.parse_args()
entry = json.loads((args.build_dir / 'compile_commands.json').read_text())[0]
arguments = shlex.split(entry['command'])
includes = []
i = 1
while i < len(arguments):
    argument = arguments[i]
    if argument in ('-I', '-isystem'):
        includes.extend(arguments[i:i + 2])
        i += 2
    elif argument.startswith('-I'):
        includes.append(argument)
        i += 1
    else:
        i += 1
binary = '/tmp/issue-524-' + args.source.stem
libraries = [arg for arg in shlex.split((args.build_dir / 'CMakeFiles/Platform.Data.Doublets.Tests.dir/link.txt').read_text())
             if (arg.endswith('.a') or arg.startswith('-l') or arg.startswith('-Wl,')) and 'gtest' not in arg]
subprocess.run(['clang++-18', '-std=c++20', '-O0', '-g0', '-fsanitize=address,undefined', *includes,
                str(args.source.resolve()), *libraries, '-o', binary], check=True)
raise SystemExit(subprocess.run([binary]).returncode)
