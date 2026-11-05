"""Reproduce the full four-template inventory; requires the frozen clones in /tmp."""

import hashlib
import importlib.util
from pathlib import Path
import subprocess
import tarfile

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "dev/log/issues/541/pulls/542/research"
CLONES = Path("/tmp/issue-541-templates")
spec = importlib.util.spec_from_file_location(
    "prior_comparison", ROOT / "experiments/compare_pipeline_templates.py"
)
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)

PYTHON = {
    "audit-dependencies": (
        "security.yml; requirements-*.txt",
        "Resolve and audit every maintained requirements surface, including audit tooling, with pip-audit; retain JSON",
    ),
    "check-dependency-graph": (
        "security.yml",
        "Use full transitive NuGet/Python audits without treating optional dependency-review entitlement as proof of coverage",
    ),
    "check-file-size": ("check_repository.py", "Fatal 1500-line source limit"),
    "check-release": (
        "release.py",
        "Unknown registry/release responses fail closed; repair existing artifacts",
    ),
    "create-manual-changeset": (
        "versions.py; changes/",
        "Independent validated language-aware fragments; no Python distribution to release",
    ),
    "detect-code-changes": (
        "changes.py",
        "Actual push/PR base and deletions; route Python configuration into full validation",
    ),
    "format-release-notes": (
        "release.py; versions.py",
        "Generated language changelog; no template-specific PyPI release notes",
    ),
    "publish-to-pypi": (
        "release.py",
        "No PyPI package in this repository; preserve NuGet/header ZIP identities",
    ),
    "read-manifest": (
        "versions.py; release.py",
        "Read authoritative csproj/nuspec versions; pyproject is tooling-only",
    ),
    "smoke-test-published-package": (
        "smoke_packages.py; release.py",
        "Clean package consumers plus anonymous bounded public download checks",
    ),
}

RUST = {
    "check-workflow-tools": (
        "check-workflows.py",
        "Require ShellCheck and Pyflakes or run the complete pinned container",
    ),
    "check-dependency-graph": (
        "security.yml",
        "Full supported dependency audits do not rely on optional dependency-review entitlement",
    ),
    "git-config": (
        "workflow env GIT_CONFIG_*",
        "Set initial branch before checkout and recipe git init; avoid global mutation",
    ),
    "github-output": ("changes.py", "Write trusted boolean outputs through GITHUB_OUTPUT"),
    "changelog-files": ("versions.py", "Validate independent JSON fragments and consume once"),
    "release-index": (
        "release.py",
        "Retain existing GitHub release asset channels; no desktop release index product",
    ),
    "check-cargo-lock": (
        "rust submodule",
        "External Rust package owns its lockfile; no root Cargo workspace",
    ),
    "check-cargo-warnings": (
        "native/C# warnings-as-errors",
        "Transfer fail-on-warning policy; external Rust package owns cargo checks",
    ),
    "check-crate-size": ("rust submodule", "No crates.io publication from this repository"),
    "crates-publish-preflight": (
        "release.py",
        "Use actual NuGet OIDC exchange, explicit auth source and scope limits",
    ),
    "publish-crate": (
        "release.py",
        "No crates.io package here; validated NuGet pushes remain fatal",
    ),
    "wait-for-crate": (
        "release.py",
        "Bounded anonymous NuGet download with diagnostic final failure",
    ),
    "smoke-test-published-crate": (
        "smoke_packages.py",
        "Apply isolated consumer checks to actual published languages",
    ),
    "rust-paths": (
        "changes.py",
        "Explicit native/C#/tooling roots; external submodule pin is not a root workspace",
    ),
    "install-rust-script": (
        "scripts/*.py",
        "Use existing Python helper runtime; do not add Rust script compilation for CI helpers",
    ),
    "test-scripts": (
        "check-python.sh; experiments/",
        "Execute helper regressions as a mandatory gate",
    ),
    "package-desktop": (
        "No desktop application",
        "Not applicable: library has no desktop binaries",
    ),
    "desktop-release-resolve": (
        "No desktop application",
        "Not applicable: library has no desktop release resolver",
    ),
    "run-javascript-first": (
        "No JS implementation",
        "Not applicable: template demo language parity is a separate product",
    ),
    "javascript-first.test": ("No JS implementation", "Not applicable: no JS parity demo to test"),
}


def decision(language, name):
    if language in ("csharp", "cpp"):
        return prior.decision(language, name)
    path = Path(name)
    stem = path.stem.replace("_", "-")
    if name.startswith(("docs/case-studies/", "experiments/")) or name in (
        "docs/issue-188-analysis.md",
        "docs/issue-194-analysis.md",
        "docs/issue-215-analysis.md",
    ):
        return (
            "Reference evidence",
            "Full source archive",
            "Historical incidents retained and compared as context; no template project history copied into production",
        )
    if (
        name.startswith(("tests/", "scripts/fixtures/"))
        or ".test." in name
        or name.startswith("scripts/version-and-commit/")
    ):
        return (
            "Adapted regression",
            "experiments/test_*.py",
            "Use bounded regressions against the real routing, release race, auth, link, SARIF and artifact helpers",
        )
    if name.startswith("scripts/"):
        table = PYTHON if language == "python" else RUST
        if stem in table:
            target, reason = table[stem]
            return (
                "Adapted" if not reason.startswith("Not applicable") else "Not applicable",
                target,
                reason,
            )
        if stem in prior.SCRIPT_MAP:
            target, reason = prior.SCRIPT_MAP[stem]
            return ("Adapted", target, reason)
        raise ValueError(f"Unreviewed script {language}/{name}")
    if name.startswith("changelog.d/") or name == "CHANGELOG.md":
        return (
            "Adapted",
            "changes/; versions.py",
            "Use independent JSON release fragments for the actual C#/C++ package versions",
        )
    if name.startswith(".github/workflows/"):
        if path.name in (
            "desktop-release.yml",
            "javascript-first-example.yml",
            "javascript-first-gate.yml",
        ):
            return (
                "Not applicable",
                "Library distribution",
                "No desktop/container/JavaScript product is published here",
            )
        return (
            "Adapted",
            ".github/workflows/ci.yml; security.yml; links.yml",
            "Reuse relevant gates with SHA pins, fixed runners, hard timeouts, explicit fatal aggregation, trusted releases and queued writers",
        )
    if name.startswith(".github/actions/"):
        return (
            "Not applicable",
            "No published container image",
            "Buildx mirror resilience is specific to template desktop/container builds",
        )
    if name == ".github/codeql/codeql-config.yml":
        return (
            "Adapted",
            "security.yml; check_sarif.py",
            "Analyze full C#/C++/Python/Actions source; fail on every finding and incomplete invocation",
        )
    if name == ".github/actionlint.yaml":
        return (
            "Adapted",
            "check-workflows.py",
            "Narrow known queue schema exception only; validate queue values and all other diagnostics",
        )
    if name in (".ruff.toml", "pyproject.toml"):
        return (
            "Adapted",
            "pyproject.toml; check-python.sh",
            "Lint, format and type-check maintained Python helpers; no new Python distribution",
        )
    if name in ("Cargo.toml", "Cargo.lock", "AGENTS.md") or name.startswith(
        ("src/", "examples/", "tests/")
    ):
        return (
            "Project-specific",
            "csharp/; cpp/; examples/; rust submodule",
            "Retain real library/test layout and external Rust ownership; do not transplant template demo APIs, agent directives or crate releases",
        )
    if name == "docs/requirements.txt" or name == "docs/conf.py":
        return (
            "Adapted",
            "docfx.json; cpp/Doxyfile; requirements-ci.txt",
            "Use actual DocFX/Doxygen/PDF documentation and audit its dependencies, not a second Sphinx site",
        )
    return prior.decision(language, name)


def main():
    rows = []
    for language in ("csharp", "cpp", "python", "rust"):
        root = CLONES / language
        revision = subprocess.check_output(
            ["git", "-C", str(root), "rev-parse", "HEAD"], text=True
        ).strip()
        names = (
            subprocess.check_output(["git", "-C", str(root), "ls-files", "-z"])
            .decode()
            .strip("\0")
            .split("\0")
        )
        with tarfile.open(OUT / f"{language}-template-full-tree.tar.gz", "w:gz") as archive:
            for name in names:
                archive.add(root / name, arcname=name, recursive=False)
        for name in names:
            data = (root / name).read_bytes()
            status, target, reason = decision(language, name)
            rows.append(
                (
                    language,
                    name,
                    revision,
                    hashlib.sha256(data).hexdigest(),
                    len(data),
                    status,
                    target,
                    reason,
                )
            )
    (OUT / "template-file-manifest.tsv").write_text(
        "template\tfile\trevision\tsha256\tbytes\tdecision\trepository equivalent\treason\n"
        + "\n".join("\t".join(map(str, row)) for row in rows)
        + "\n"
    )
    text = "# Complete four-template comparison\n\nEvery tracked file, including fixtures and historical incident data, has a decision below. The TSV gives exact revisions, SHA-256 hashes and byte counts; full-tree archives retain every file. Priority is C#, C++, Python, Rust. This is a transfer of applicable practices to the real library, not a new desktop/JS/PyPI/crates.io product.\n\n"
    for language in ("csharp", "cpp", "python", "rust"):
        selected = [row for row in rows if row[0] == language]
        text += f"## {language}: {len(selected)} files\n\n| File | Decision | Equivalent and rationale |\n| --- | --- | --- |\n"
        for row in selected:
            text += f"| `{row[1]}` | {row[5]} | `{row[6]}`: {row[7]} |\n"
        text += "\n"
    (OUT / "TEMPLATE-COMPARISON.md").write_text(text)
    print(f"Inventoried and archived {len(rows)} files without unreviewed fallbacks")


if __name__ == "__main__":
    main()
