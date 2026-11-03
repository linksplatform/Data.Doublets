"""Recheck actual native translation units with another compiler, without linking."""
import argparse
import json
from pathlib import Path
import shlex
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('build_dir', type=Path)
parser.add_argument('--compiler', default='clang++-18')
args = parser.parse_args()
for entry in json.loads((args.build_dir / 'compile_commands.json').read_text()):
    command = shlex.split(entry['command'])
    filtered = [args.compiler, '-fsyntax-only', '-ferror-limit=0'] if 'clang' in args.compiler else [args.compiler, '-fsyntax-only']
    index = 1
    while index < len(command):
        if command[index] == '-o':
            index += 2
            continue
        if command[index] not in ('-c',):
            filtered.append(command[index])
        index += 1
    subprocess.run(filtered, cwd=entry['directory'], check=True)
