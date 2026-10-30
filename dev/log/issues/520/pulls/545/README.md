# Issue 520 verification

The requirement inventory, root cause and repeatable regression commands are in
[the experiment record](../../../../../../experiments/issue-520/README.md).

## Before and after

- [C# before](validation/csharp-before.txt): the initial 64 regression cases all
  fail against the unchanged library, including count three and visits
  `[1, 2, 1]` for the original two-point reproduction.
- [C++ original reproduction before](validation/cpp-original-before.txt): the
  captured original assertions show the same count and visit failures. This is
  an excerpt of the original reproduction, not a completed native suite.
- [C# after](validation/csharp-after.txt): warning-free Release build, formatting,
  all 403 tests (80 new cases), examples, NuGet packaging and an isolated consumer
  pass. No tests are skipped.
- [C++ focused after](validation/cpp-focused-after.txt): all four new cases pass
  with GCC, covering united/split and plain/automatic stores.
- [C++ full suite after](validation/cpp-full-after.txt): all 50 tests pass with
  Clang 18, using the generated Conan include and link flags, `-O0 -g0` and one
  compiler process. [Measured compilation](validation/cpp-clang-resources.txt)
  peaks at 1,036,240 KiB RSS under a 1,800,000 KiB virtual-memory limit.
- [GCC resource boundary](validation/cpp-gcc-memory-limit.txt): the full test
  translation unit exceeded that local virtual-memory limit. It was stopped
  without an OOM kill and replaced by the successful Clang build. The full GCC
  and sanitizer configurations are also required by hosted CI.
- The [native benchmark smoke](validation/cpp-benchmark.txt) and
  [packaged native consumer](validation/cpp-package-consumer.txt) pass. Debug
  benchmark output is smoke evidence, not a performance comparison.
- The finite [split byte-address trace](validation/split-byte-trace.txt) shows
  counts and visits across index updates, null records and free-slot reuse.
- [Native static analysis](validation/cpp-analysis.txt) passes clang-format,
  Clang-Tidy 18 and Cppcheck with the real compilation database.
- [Pipeline checks](validation/pipeline-regressions.txt) pass Ruff, formatting,
  mypy and all 105 regression tests. The logged rejected temporary Git push is
  an intentional concurrency regression test. Repository configuration,
  workflow lint, ShellCheck and full-history credential scanning also pass.
- Documentation builds with [zero DocFX warnings/errors](validation/docs-docfx.txt),
  a [294-page source PDF](validation/docs-pdf.txt), warning-free Doxygen output,
  and [549 validated HTML pages and local links](validation/docs-links.txt).

The maintained regression tests supply assertions; the trace supplies additional
diagnostic output. Build outputs and temporary dependency installations remain
outside the committed sources.

## Scope and release

Both maintained C# and C++ implementations use the same union predicate. Count
retains distinct raw records even when their endpoint pairs are equal. Other
restriction shapes remain covered by the existing suites. The C++ split full
scan now returns Continue after normal completion, which is needed by wildcard
and null-endpoint enumeration; early Break remains unchanged.

The independent changeset requests patch releases for both packages. No package
version is manually changed. Issue #520 has no required sub-issues, comments or
additional PR feedback at the final local review. Related #360 is a separate API
redesign; linksplatform/doublets-rs#63 is an already resolved counterpart.

Hosted checks and final commit verification are recorded on
[PR #545](https://github.com/linksplatform/Data.Doublets/pull/545).
