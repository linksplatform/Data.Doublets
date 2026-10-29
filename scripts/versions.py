"""Changeset-driven release metadata. PRs test the same versions writers publish.

Inspired by the C#/C++ pipeline templates' changeset and guarded-push scripts.
Only release metadata may be committed; a changed release base requires new CI.
"""
import argparse
import json
import os
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
FILES = {
    'csharp': 'csharp/Platform.Data.Doublets/Platform.Data.Doublets.csproj',
    'cpp': 'cpp/Platform.Data.Doublets/Platform.Data.Doublets.TemplateLibrary.nuspec',
}
PATTERNS = {'csharp': r'(?<=<VersionPrefix>)[^<]+', 'cpp': r'(?<=<version>)[^<]+'}
RANK = {'patch': 0, 'minor': 1, 'major': 2}


def git(*args, **kwargs):
    return subprocess.check_output(['git', *args], cwd=ROOT, text=True, **kwargs).strip()


def read_version(language, text):
    match = re.search(PATTERNS[language], text)
    if not match or not re.fullmatch(r'\d+\.\d+\.\d+', match[0]):
        raise ValueError(f'Invalid {language} version')
    return match[0]


def bump(version, kind):
    numbers = list(map(int, version.split('.')))
    index = {'major': 0, 'minor': 1, 'patch': 2}[kind]
    numbers[index] += 1
    numbers[index + 1:] = [0] * (2 - index)
    return '.'.join(map(str, numbers))


def fragments():
    entries = []
    for path in sorted((ROOT / 'changes').glob('*.json')):
        data = json.loads(path.read_text(encoding='utf-8'))
        if not data.get('summary', '').strip() or not data.get('releases'):
            raise ValueError(f'{path}: summary and releases are required')
        if any(language not in FILES or kind not in RANK for language, kind in data['releases'].items()):
            raise ValueError(f'{path}: invalid language or semantic version bump')
        entries.append((path, data))
    return entries


def validate(base):
    from changes import categories
    paths = git('diff', '--no-renames', '--name-only', base, 'HEAD').splitlines()
    changed = categories(paths)
    entries = fragments()
    for language, filename in FILES.items():
        current = (ROOT / filename).read_text(encoding='utf-8')
        previous = git('show', f'{base}:{filename}')
        if read_version(language, current) != read_version(language, previous):
            raise ValueError(f'{filename}: add a changes/*.json fragment instead of manually changing the version')
        if changed[language] and not any(language in data['releases'] for _, data in entries):
            raise ValueError(f'{language} changes require a changes/*.json release fragment')
    print('Changesets and automatic version policy passed')


def prepare():
    entries = fragments()
    notes = []
    for language, filename in FILES.items():
        applicable = [data for _, data in entries if language in data['releases']]
        if not applicable:
            continue
        path = ROOT / filename
        source = path.read_text(encoding='utf-8')
        old = read_version(language, source)
        kind = max((data['releases'][language] for data in applicable), key=RANK.get)
        version = bump(old, kind)
        path.write_text(re.sub(PATTERNS[language], version, source), encoding='utf-8')
        if language == 'cpp':
            cmake = ROOT / 'cpp/CMakeLists.txt'
            cmake.write_text(re.sub(r'(?<=project\(Platform.Data.Doublets VERSION )[\d.]+', version, cmake.read_text()), encoding='utf-8')
        notes.append(f'## {language} {version}\n\n' + '\n'.join('- ' + data['summary'] for data in applicable) + '\n\n')
        print(f'{language}: {old} -> {version} ({kind})')
    if notes:
        changelog = ROOT / 'CHANGELOG.md'
        previous = changelog.read_text(encoding='utf-8') if changelog.exists() else ''
        changelog.write_text(''.join(notes) + previous, encoding='utf-8')
        for path, _ in entries:
            path.unlink()


def commit():
    allowed = set(FILES.values()) | {'cpp/CMakeLists.txt', 'CHANGELOG.md'}
    dirty = git('diff', '--name-only').splitlines() + git('diff', '--cached', '--name-only').splitlines()
    # Include an untracked first changelog, but never arbitrary generated files.
    if (ROOT / 'CHANGELOG.md').exists():
        dirty.append('CHANGELOG.md')
    if any(path not in allowed and not re.fullmatch(r'changes/[^/]+\.json', path) for path in dirty):
        raise ValueError('Refusing to commit files outside the release metadata allowlist')
    if not dirty:
        print('No version changes; repairing publication of the existing version')
        return
    git('config', 'user.name', 'github-actions[bot]')
    git('config', 'user.email', '41898282+github-actions[bot]@users.noreply.github.com')
    git('add', '--', *sorted(set(dirty)))
    if not git('diff', '--cached', '--name-only'):
        return
    git('commit', '-m', 'Release package versions from changesets')
    # A failed mirror/publish can be resumed without bumping again. Never push
    # a source tree different from the validated tree or overwrite another run.
    for attempt in range(1, 4):
        result = subprocess.run(['git', 'push', 'origin', 'HEAD:main'], cwd=ROOT, text=True, capture_output=True)
        if result.returncode == 0:
            os.environ['GITHUB_SHA'] = git('rev-parse', 'HEAD')
            print('Release metadata push confirmed')
            return
        output = result.stdout + result.stderr
        print(output)
        if re.search(r'GH006|GH013|rule violations|protected branch', output, re.I):
            raise RuntimeError('Repository rules reject the release commit. Allow github-actions to push release metadata; do not force-push.')
        if attempt == 3 or not re.search(r'non-fast-forward|fetch first', output):
            raise RuntimeError('Release metadata push failed; no packages were published')
        git('fetch', 'origin', 'main')
        if not git('diff', '--name-only', 'HEAD', 'origin/main'):
            # A queued writer or a rerun may have landed the identical release.
            # Use its confirmed commit for tags, without consuming a second bump.
            git('reset', '--hard', 'origin/main')
            print('Identical release metadata already landed; repairing its publication')
            return
        # Re-read the state after a race. Only rebase over metadata-only updates
        # that leave these planned versions free and the source tree unchanged.
        changed = git('diff', '--name-only', 'HEAD^', 'origin/main').splitlines()
        if changed:
            raise RuntimeError('main advanced after validation; rerun CI on the current merge before publishing')
        git('rebase', 'origin/main')
        print(f'Retrying metadata push after a non-fast-forward race ({attempt}/3)')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('command', choices=['validate', 'prepare', 'commit'])
    parser.add_argument('--base', default='origin/main')
    args = parser.parse_args()
    try:
        if args.command == 'validate':
            validate(args.base)
        elif args.command == 'prepare':
            prepare()
        else:
            commit()
    except (ValueError, RuntimeError, subprocess.CalledProcessError) as error:
        raise SystemExit(f'::error::{error}') from error
