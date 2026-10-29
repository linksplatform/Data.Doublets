# Contributing

Use a pull request against `main`. Install the .NET SDK selected by `global.json`,
Python 3.12+, CMake, a C++20 compiler, clang-format/clang-tidy 18, Cppcheck and
ShellCheck. Install Python tools with
`python3 -m pip install -r scripts/requirements-ci.txt -r scripts/requirements-quality.txt`.

Run these checks before pushing:

```sh
bash scripts/check-python.sh
python3 scripts/check_repository.py
python3 scripts/check-workflows.py --container
git ls-files -z '*.sh' ':(exclude)dev/log/**' | xargs -0 shellcheck
bash scripts/check-csharp.sh
bash cpp/setup.sh
bash cpp/run_tests.sh
bash cpp/run_benchmarks.sh --benchmark_filter=BM_JustCall --benchmark_min_time=0.01
python3 scripts/check-cpp.py --build-dir cpp/build
bash scripts/check-secrets.sh
```

The workflow linter runs the digest-pinned actionlint container, including
ShellCheck and Pyflakes. Native execution requires all three tools on PATH and
fails if an embedded-code checker is missing. Archived upstream scripts under
`dev/log/` are frozen evidence, excluded from the standalone ShellCheck pass.
Python helpers under `scripts/` must pass Ruff lint/format and mypy; historical
experiment scripts remain runnable regression evidence. Install pre-commit
and run `pre-commit install` to enable the local formatting, regression, size,
and credential checks. All compiler and analyzer warnings fail validation.
Tests have bounded timeouts; native builds use one worker by default. The GCC
CI job records gcovr line/branch coverage and fails if instrumentation is missing.
`CI_VERBOSE=1` enables build tracing. Never enable shell tracing around tokens.

## Versions and releases

Add an independent JSON file under `changes/` for C# or C++ code changes:

```json
{"summary":"Describe the user-visible fix","releases":{"csharp":"patch","cpp":"patch"}}
```

Choose `patch`, `minor`, or `major` for each affected language. Do not manually
edit package versions. CI validates the actual merge with current `main`, applies
the largest requested bump per language, and tests the resulting packages in
isolated consumers. Documentation and investigation-only changes need no bump.

Pull requests build packages and documentation without publishing credentials.
After merge, the credentials job exchanges GitHub OIDC through the official,
SHA-pinned `NuGet/login` action before expensive builds. Configure the actual NuGet
policy creator's profile in the non-secret `NUGET_USER` Actions variable, and an
active publishing policy covering both package IDs. Missing configuration or a
rejected policy reports trusted-publishing unavailability and can use only the
provided `secrets.NUGET_TOKEN` fallback. A working OIDC path never tests that token.
Pull requests need no publishing credentials. Follow the
[trusted publishing setup and recovery guide](docs/trusted-publishing.md).

The publishing job authenticates again before committing version metadata. Its
40-minute limit is below NuGet's one-hour key lifetime, and C++ obtains another
fresh key immediately before its push. Keys stay in step outputs/environment
within their own job; never put them in secrets, artifacts, cross-job outputs or
diagnostic logs. Package scope is reported as
unverified where push-only credentials cannot prove it without uploading. Actual
pushes and public downloads enforce publication success. Only HTTP 401/403 push
rejections can retry once with the provided fallback and identical artifacts and
versions; unrelated failures remain fatal. Retain the configured fallback secret.

Writers share one noncancellable queue. Only generated release metadata may be
committed directly to main. Repository rules must permit that bot commit; a rule
rejection fails once with instructions, and never triggers a forced push. Every
published package comes from successful validation, and an existing version can
be retried to repair missing release assets. Registry indexing is checked
anonymously for up to fifteen minutes. GitHub Packages mirror failures remain
visible while public release assets can still be repaired.

If publication reports `GH006` or `GH013`, a repository administrator must allow
the release actor to update `main` under the existing branch policy (or provision
a specifically authorized release GitHub App and its token). A workflow's
`contents: write` does not override branch protection. Keep required reviews for
ordinary changes; do not disable protection, force-push, or introduce automatic
release pull requests to bypass the rejection. See the evidence and unresolved
release criterion in `dev/log/issues/541/pulls/542/ANALYSIS.md`.

Artifact downloads retain SHA-256 verification and use `skip-decompress: true`
with `scripts/extract_artifact.py` to avoid the action's deprecated Buffer-based
ZIP extractor. A real upload/download round trip runs on every pull request.

## Documentation

```sh
dotnet tool restore
python3 scripts/source_pdf.py
dotnet docfx docfx.json --warningsAsErrors
doxygen cpp/Doxyfile
python3 scripts/check_docs.py
```

Install Doxygen and DejaVu fonts for these commands. The documentation artifact
contains both language APIs and the Unicode C# source PDF. CI checks generated
local links and maps this repository's deployment URLs to generated files, so
new documentation can pass before publication. It also checks maintained root
Markdown links; broken external links remain
failures even when an archive copy exists.

## Dependency and security maintenance

The pinned Conan recipe snapshot needs explicit Conan 2 header-library metadata
and a Clang-compatible constant-expression workaround in Platform.Data. These
bounded patches live in `scripts/patch_conan_recipes.py`; they also repair the
upstream heap allocator destructor and dependency build diagnostics. Explicit
Conan profiles select the compiler from `CC`/`CXX`; set `CPP_CONAN_PROFILE` to an
existing reviewed profile for another platform. Internal Conan aliases use
hyphens; public CMake targets and headers retain their original names.
Upstream reports and
reproduction evidence are under `dev/log/issues/524/pulls/526/upstream/`.

The scheduled security workflow audits transitive NuGet and Python dependencies, runs
full-repository CodeQL with a fatal gate on every SARIF finding, and scans
complete Git history for credentials. `.gitleaksignore` contains
one exact historical fingerprint: the removed key was independently verified
revoked (HTTP 401). It does not exempt current files or future credentials.
Install `scripts/requirements-audit.txt` and run
`python3 -m pip_audit -r scripts/requirements-ci.txt -r scripts/requirements-quality.txt -r scripts/requirements-audit.txt`
to reproduce the Python audit. Audit JSON and CodeQL SARIF are retained in CI.
CodeQL covers C#, C++, Python and Actions; empty results count as success only
when the report includes a successful analysis invocation.

Native temporary files live in atomically created private directories. Optional
Rust FFI development remains in the `rust` submodule and is separate from the
header-only C++ build; the public FFI declarations compile as C11 and C++20.
