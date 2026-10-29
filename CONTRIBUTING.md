# Contributing

Use a pull request against `main`. Install the .NET SDK selected by `global.json`,
Python 3.12+, CMake, a C++20 compiler, clang-format/clang-tidy 18, Cppcheck and
ShellCheck. Install Python tools with `python3 -m pip install -r scripts/requirements-ci.txt -r scripts/requirements-quality.txt`.

Run these checks before pushing:

```sh
python3 -m unittest discover -s experiments -p 'test_*.py' -v
python3 scripts/check_repository.py
python3 scripts/check-workflows.py
bash scripts/check-csharp.sh
bash cpp/setup.sh
bash cpp/run_tests.sh
bash cpp/run_benchmarks.sh --benchmark_filter=BM_JustCall --benchmark_min_time=0.01
python3 scripts/check-cpp.py --build-dir cpp/build
bash scripts/check-secrets.sh
```

The workflow linter needs actionlint 1.7.12 and ShellCheck on PATH. CI obtains
its pinned actionlint binary from the official container. Install pre-commit
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
After merge, the release job first verifies NuGet key validity and package scope.
Set the repository secret `NUGET_TOKEN` to a valid push key for both
`Platform.Data.Doublets` and `Platform.Data.Doublets.TemplateLibrary`. The old
main run returned HTTP 403: an owner must rotate an expired/revoked/scoped key.
A missing or unusable key explicitly fails main before expensive builds; it does
not fail pull requests. NuGet trusted publishing is the preferred future
alternative when the repository owner configures its trust policy.

Writers share one noncancellable queue. Only generated release metadata may be
committed directly to main. Repository rules must permit that bot commit; a rule
rejection fails once with instructions, and never triggers a forced push. Every
published package comes from successful validation, and an existing version can
be retried to repair missing release assets. Registry indexing is checked
anonymously for up to fifteen minutes. GitHub Packages mirror failures remain
visible while public release assets can still be repaired.

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
bounded patches live in `scripts/patch_conan_recipes.py`; upstream reports and
reproduction evidence are under `dev/log/issues/524/pulls/526/upstream/`.

The scheduled security workflow audits transitive NuGet dependencies, runs
CodeQL, and scans complete Git history for credentials. `.gitleaksignore` contains
one exact historical fingerprint: the removed key was independently verified
revoked (HTTP 401). It does not exempt current files or future credentials.

Native temporary files live in atomically created private directories. Optional
Rust FFI development remains in the `rust` submodule and is separate from the
header-only C++ build; the public FFI declarations compile as C11 and C++20.
