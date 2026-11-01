<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# Issue 515 verification", "*"]
  }
}
-->

# Issue 515 verification

The requirement inventory, root cause and repeatable regression commands are in
[the experiment record](../../../../../../experiments/issue-515/README.md).

## Before and after

- [C# before](validation/csharp-before.txt): with the two original
  substitutions temporarily restored, all 7 new test cases fail. The handler
  records `(4 1 3) -> (4 3 0)` and `(5 3 1) -> (5 2 0)`, which are exactly
  the issue's results.
- [C# after](validation/csharp-after.txt): the new `MergeUsagesTests` and the
  existing `UniquenessResultTests` pass with warnings as errors (32 tests).
- [C++ before](validation/cpp-before.txt): compiling the new regression cases
  against the original header fails in `MergeUsages`
  (`ILinksExtensions.h:844`, `:853` and `:858`). The template couldn't be
  instantiated at all.
- [C++ focused after](validation/cpp-focused-after.txt): both native cases pass
  with GCC, `-Wall -Wextra -Werror`. They cover united `uint64` and split
  `uint32` stores, `MergeUsages` and `MergeAndDelete`, each with and without a
  handler.
- [C++ sanitizers](validation/cpp-focused-sanitizers.txt): the same cases pass
  under AddressSanitizer and UndefinedBehaviorSanitizer.
- [Checks](validation/checks.txt): Ruff, formatting, mypy, the repository
  check and its guard tests, clang-format 18 and `dotnet format` all pass.

The full native suite, Clang-Tidy, Cppcheck and coverage run in hosted CI,
together with the full C# suite and packaging. They are not repeated locally.

## Scope and release

`changes/515-merge-usages-endpoints.json` requests a patch release for both
languages. The C++ library changes. For C#, only tests change, but the
release check requires a fragment for any change under `csharp/`.
