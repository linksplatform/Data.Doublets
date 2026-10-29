"""Native checks adapted from the C++ template's scripts/lint_cpp.py."""
import argparse
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]


def run(*args):
    subprocess.run(args, cwd=ROOT, check=True)


def sources():
    names = subprocess.check_output(['git', 'ls-files', '--cached', '--others', '--exclude-standard', '-z', 'cpp', 'c'], cwd=ROOT).decode().split('\0')
    return [name for name in names if name and Path(name).suffix in ('.c', '.h', '.cpp', '.hpp') and (ROOT / name).is_file()]


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build-dir', type=Path)
    args = parser.parse_args()
    run('clang-format-18', '--dry-run', '--Werror', *sources())
    if args.build_dir:
        database = args.build_dir.resolve() / 'compile_commands.json'
        units = sorted({entry['file'] for entry in json.loads(database.read_text())})
        if not units:
            raise SystemExit('No native translation units found for static analysis')
        run('clang-tidy-18', '-p', str(args.build_dir.resolve()), '--quiet', *units)
        run('cppcheck', '--library=googletest', '--enable=warning,performance,portability', '--error-exitcode=1', '--inline-suppr', '--std=c++20', '--check-level=exhaustive', '--suppress=missingIncludeSystem', '--quiet', '--project=' + str(database))
