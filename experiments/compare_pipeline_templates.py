"""Archive and inventory every tracked file in the reviewed pipeline templates."""
import hashlib
from pathlib import Path
import subprocess
import tarfile

ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / 'dev/log/issues/524/pulls/526/research'
RESEARCH = Path('/tmp/issue-524-research')

SCRIPT_MAP = {
    'bump-version': ('scripts/versions.py', 'Semantic bump validation and generation'),
    'check-file-size': ('scripts/check_repository.py', '1500-line source limit'),
    'check-pipeline-status': ('scripts/check_required.py', 'Fatal applicable-job and cancellation checks'),
    'check-release-needed': ('scripts/release.py', 'Fail-closed release lookup and idempotent repair'),
    'check-web-archive': ('scripts/check_web_archive.py', 'Archive suggestions never excuse live failures'),
    'create-github-release': ('scripts/release.py', 'Draft then complete public release assets'),
    'detect-code-changes': ('scripts/changes.py', 'Actual event base and deleted-file routing'),
    'merge-changesets': ('scripts/versions.py', 'Largest bump per language; consume fragments once'),
    'nuget-auth': ('scripts/release.py', 'NuGet verification key and actual package-scope probe'),
    'preflight-credentials': ('scripts/release.py', 'PR report; main fails before building'),
    'print-untrusted': ('.github/workflows/ci.yml', 'Untrusted event fields passed through environment, never shell code'),
    'push-nuget-package': ('scripts/release.py', 'Publish tested artifacts and skip duplicates'),
    'recheck-broken-links': ('scripts/recheck_broken_links.py', 'Bounded retries with corrected singleton backoff'),
    'release-naming': ('scripts/release.py', 'Preserve csharp_/cpp_ tags and package identities'),
    'smoke-test-nuget-package': ('scripts/smoke_packages.py', 'Isolated package restore and executable C# consumer'),
    'validate-changeset': ('scripts/versions.py', 'Validate language, summary and bump type'),
    'version-and-commit': ('scripts/versions.py', 'Allowlisted metadata commit and classified bounded push retry'),
    'wait-for-nuget': ('scripts/release.py', 'Bounded anonymous registry download after push'),
    'check-changelog-fragment': ('scripts/versions.py', 'Require independent fragments for code changes'),
    'check-nuget-package': ('scripts/smoke_packages.py', 'NuGet restore in a clean isolated package cache'),
    'check-consumers': ('scripts/smoke_packages.py', 'Restore NuGet; compare every native header and ZIP byte'),
    'check-version-modification': ('scripts/versions.py', 'Reject manual package-version edits in PRs'),
    'collect-changelog': ('scripts/versions.py', 'Generate per-language changelog from consumed changesets'),
    'create-changelog-fragment': ('changes/524-ci-reliability.json', 'Document independent fragment schema in CONTRIBUTING'),
    'get-bump-type': ('scripts/versions.py', 'Choose highest semantic bump'),
    'get-version': ('scripts/release.py', 'Read authoritative XML package metadata'),
    'lint-cpp': ('scripts/check-cpp.py', 'clang-format, compilation-database clang-tidy and cppcheck'),
    'pack-nuget': ('scripts/release.py', 'Complete native NuGet layout and package metadata'),
    'package-release': ('scripts/release.py', 'Package all headers and license in release ZIP'),
    'publish-release': ('scripts/release.py', 'Independent registry and public release repair'),
    'read-job-cancel-in-progress': ('.github/workflows/ci.yml', 'Explicit per-job cancellation and shared queued writer'),
    'simulate-fresh-merge': ('scripts/fresh-merge.sh', 'Fetch and validate current base merge'),
    'check-required-docs': ('scripts/check_docs.py', 'Required C#/C++ API pages, PDF and local links'),
    'requirements-ci': ('scripts/requirements-ci.txt', 'Pin Python helper dependencies'),
    'run-with-budget-warning': ('.github/workflows/ci.yml', 'Adapt budgets as job/test hard limits and retained logs'),
}


def decision(language, name):
    path = Path(name)
    if name.startswith(('docs/case-studies/', 'ci-logs/')) or name in ('docs/issue-69-analysis.md', 'docs/roadmap/preview-regeneration.md', 'docs/roadmap/toc.yml'):
        return 'Reference evidence', 'dev/log/issues/524/pulls/526', 'Historical template incidents retained in full archive; no production behavior to transplant'
    if name.startswith('experiments/') or name.startswith('scripts/fixtures/'):
        return 'Adapted regression', 'experiments/', 'Preserve reusable minimal regressions for this repository rather than template demonstrations'
    if name.startswith(('scripts/tests/', 'tests/')) or '.test.' in name:
        return 'Adapted regression', 'experiments/test_pipeline_policies.py; experiments/test_link_retries.py; csharp/Platform.Data.Doublets.Tests; cpp/Platform.Data.Doublets.Tests', 'Use existing language test frameworks plus targeted pipeline regressions'
    if name.startswith('scripts/'):
        stem = path.stem.replace('_', '-')
        if stem in SCRIPT_MAP:
            target, reason = SCRIPT_MAP[stem]
            return 'Adapted', target, reason
        if 'workflow-policy' in stem or 'workflow-' in stem:
            return 'Adapted regression', 'experiments/test_pipeline_policies.py; scripts/check_repository.py; scripts/check-workflows.py', 'Validate maintained workflow pins, permissions, routing and cancellation'
        if stem in ('check-vcpkg-port', 'render-vcpkg-port'):
            return 'Not applicable', 'scripts/smoke_packages.py', 'No existing vcpkg distribution channel; preserve and validate the existing NuGet and header ZIP channels'
        if stem == 'check-dependency-review-support':
            return 'Adapted', '.github/workflows/security.yml', 'Scheduled full transitive audit works without optional dependency-review entitlement'
        if stem in ('cpp-project', 'rename-project'):
            return 'Not applicable', 'cpp/CMakeLists.txt', 'Template bootstrap/rename utility; existing published package names must remain stable'
        raise ValueError(f'Unreviewed script: {name}')
    direct = {
        '.editorconfig': ('.editorconfig', 'Repository-wide whitespace with established C# style'),
        '.clang-format': ('.clang-format', 'Allman C++ formatting; preserve amalgamated header include order'),
        '.clang-tidy': ('.clang-tidy', 'Defect checks with actual dependency includes; warnings fatal'),
        '.github/dependabot.yml': ('.github/dependabot.yml', 'Pinned tooling updates with grouped schedules and cooldown'),
        '.github/zizmor.yml': ('.github/workflows/AutoMerge.yml', 'Narrow inline suppression for metadata-only trusted Dependabot trigger'),
        '.gitignore': ('.gitignore', 'Ignore generated build/site/package outputs; retain issue evidence'),
        '.lycheeignore': ('.lycheeignore', 'Exclude local example hosts; validate own deployment URLs against generated artifacts'),
        'lychee.toml': ('.github/workflows/links.yml', 'Explicit timeouts, retry limits and request concurrency'),
        '.pre-commit-config.yaml': ('.pre-commit-config.yaml', 'Local formatting, regressions, size and secret checks'),
        '.secretlintrc.json': ('.gitleaksignore; scripts/check-secrets.sh', 'Use full-history Gitleaks with exact revoked historical fingerprint'),
        'CHANGELOG.md': ('scripts/versions.py', 'Generate changes from fragments rather than copy template history'),
        'CONTRIBUTING.md': ('CONTRIBUTING.md', 'Document supported local checks, credentials, release and security workflow'),
        'Directory.Build.props': ('Directory.Build.props', 'Apply analyzers and warnings policy to every C# project'),
        'docfx.json': ('docfx.json', 'Pinned modern DocFX plus PDF and C++ API documentation'),
        'Doxyfile': ('cpp/Doxyfile', 'Generate native API docs and fail malformed references'),
        'CMakeLists.txt': ('cpp/CMakeLists.txt', 'Conditional tests/benchmarks, required dependencies, strict compiler options and CTest'),
        'CMakePresets.json': ('cpp/ci-build.sh', 'Use one shared fail-fast configure/build/test entrypoint with Conan toolchain'),
        'gcovr.cfg': ('.github/workflows/ci.yml', 'Native coverage reporting alongside C# XPlat coverage'),
    }
    if name in direct:
        target, reason = direct[name]
        return 'Adapted', target, reason
    if name.startswith('.github/workflows/'):
        target = {'docs.yml': '.github/workflows/ci.yml', 'links.yml': '.github/workflows/links.yml', 'release.yml': '.github/workflows/ci.yml', 'security.yml': '.github/workflows/security.yml', 'workflows.yml': '.github/workflows/ci.yml'}[path.name]
        return 'Adapted', target, 'Consolidate related gates; pin actions/runners; preserve tested artifact publication'
    if name.startswith(('.changeset/', 'changelog.d/')):
        return 'Adapted', 'changes/524-ci-reliability.json; scripts/versions.py', 'One language-aware changeset store; no Node runtime needed for C# releases'
    if name.startswith('packaging/nuget/'):
        return 'Adapted', 'scripts/release.py; cpp/Platform.Data.Doublets/Platform.Data.Doublets.TemplateLibrary.nuspec', 'Preserve native package ID/layout and validate complete contents'
    if name.startswith(('packaging/vcpkg/', 'test_package/')) or name in ('vcpkg.json', 'conanfile.py'):
        return 'Not applicable', 'cpp/conanfile.txt; scripts/smoke_packages.py', 'Project consumes Conan dependencies; its existing release channels are NuGet and header ZIP, not a new Conan/vcpkg registry'
    if name.startswith('cmake/'):
        return 'Adapted', 'cpp/CMakeLists.txt; cpp/ci-build.sh; scripts/smoke_packages.py', 'Keep dependency/build options and verify shipped headers through the existing consumer path; template install/pkg-config products are new channels'
    if name.startswith(('src/', 'include/', 'benchmarks/', 'examples/')) or name == 'MyPackage.sln':
        return 'Adapted', 'csharp/; cpp/; examples/ExternalReferences/', 'Use real Doublets library, tests, benchmarks and examples in place of template Calculator/demo code'
    if name.startswith('docs/') or name == 'README.md':
        return 'Adapted', 'README.md; CONTRIBUTING.md; docs/; dev/log/issues/524/pulls/526/ANALYSIS.md', 'Document this repository layout, validation, distribution and investigation; no template-specific roadmap copied'
    if name == 'LICENSE':
        return 'Preserved', 'LICENSE', 'Keep existing Unlicense; include it in native packages and archives'
    raise ValueError(f'Unreviewed file: {language}/{name}')


def main():
    rows = []
    for language in ('csharp', 'cpp'):
        root = RESEARCH / language
        revision = subprocess.check_output(['git', '-C', str(root), 'rev-parse', 'HEAD'], text=True).strip()
        files = subprocess.check_output(['git', '-C', str(root), 'ls-files', '-z']).decode().strip('\0').split('\0')
        with tarfile.open(OUTPUT / f'{language}-template-full-tree.tar.gz', 'w:gz') as archive:
            for name in files:
                archive.add(root / name, arcname=name, recursive=False)
        for name in files:
            data = (root / name).read_bytes()
            status, target, reason = decision(language, name)
            rows.append((language, name, revision, hashlib.sha256(data).hexdigest(), len(data), status, target, reason))
    (OUTPUT / 'template-file-manifest.tsv').write_text('template\tfile\trevision\tsha256\tbytes\tdecision\trepository equivalent\treason\n' + '\n'.join('\t'.join(map(str, row)) for row in rows) + '\n')
    text = '# Complete template file comparison\n\nAll tracked files at the archived revisions are inventoried below, including scripts, tests, fixtures and historical evidence. Exact hashes and byte sizes are in `template-file-manifest.tsv`; full source is in the two `*-template-full-tree.tar.gz` archives.\n\n'
    for language in ('csharp', 'cpp'):
        selected = [row for row in rows if row[0] == language]
        text += f'## {language}: {len(selected)} files\n\n| Template file | Decision | Repository equivalent / reason |\n| --- | --- | --- |\n'
        for row in selected:
            text += f'| `{row[1]}` | {row[5]} | `{row[6]}` — {row[7]} |\n'
        text += '\n'
    (OUTPUT / 'TEMPLATE-COMPARISON.md').write_text(text)
    print(f'Compared and archived all {len(rows)} tracked template files')


if __name__ == '__main__':
    main()
