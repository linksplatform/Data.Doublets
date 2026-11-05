# Issue 242 validation evidence

Issue: <https://github.com/linksplatform/Data.Doublets/issues/242>

Pull request: <https://github.com/linksplatform/Data.Doublets/pull/443>

## Findings

The original report is reproducible. On pinned main `68d306f40`, running the
exact snippet with published `Platform.Data.Doublets.Sequences` 0.6.5 throws
`KeyNotFoundException: Link 115 does not exist.` The converter constructs nine
links using raw character codes as endpoints; the formatter previously read
those endpoints as stored links before checking existence.

The PR's original recursion diagnosis was incorrect. The visited set already
prevents cycles, and self references are rendered directly. Omitting
`renderDebug` in recursive calls suppresses debug markers; it does not affect
the visited set. The initial self-reference test also incorrectly required a
`*` marker for a point whose existing notation is `(1:1 1)`.

C++ had no structure-formatting API. The new templates mirror the corrected
C# behavior, including virtual external points and user-supplied element
callbacks. No existing formatter is replaced.

## Verification

- `validation/original-report-before.log`: exact issue snippet using Sequences
  0.6.5 on pinned main; fails with missing link 115.
- `validation/original-report-after.log`: same program with the fixed library;
  expected full debug notation, checked by the program.
- `validation/csharp-before.log`: the focused suite on pinned main; 16 fail,
  7 pass. Failures cover raw/deleted/null endpoints and lost debug markers.
- `validation/csharp-after.log`: all 23 focused cases pass on united and split
  stores. No stress test or unbounded stack-overflow reproduction is used.
- `validation/cpp-before.log`: native regressions fail to compile against
  main because `FormatStructure`/`AppendStructure` do not exist there.
- `validation/cpp-after.log`: united uint64 and split uint32 native checks
  pass with GCC 13, `-Wall -Wextra -Werror`, `-O0 -g0`. Peak compiler RSS is
  approximately 515 MiB, with a 1.8 GiB address-space cap.
- `validation/example-after.log`: the standalone dependency-free example
  asserts and prints the same sequence notation.

The local Clang 21 distribution rejects pre-existing, unused interface methods
with `no member named 'object'`. The focused native verification therefore uses
the configured GCC compiler; CI's pinned Clang 18 jobs cover Clang and sanitizers.
Only supported endpoint values are persisted in tests: query constants are
checked as roots, rather than stored as invalid internal addresses.

Native formatting, C# whitespace formatting, repository validation and all eight
CI routing regressions pass locally. Full suites, both C# operating systems,
GCC/Clang/sanitizers, native static analysis, package consumers, generated docs
and security gates are verified by CI on the pushed commit.

Commands and acceptance mapping are in `experiments/issue-242/PLAN.md`.
Reproduction helpers restore the fixed source in `finally` blocks. The normal
suite tests reconstruct the converter's exact pair layers; the standalone
`OriginalReport` experiment additionally uses the actual published converter.
