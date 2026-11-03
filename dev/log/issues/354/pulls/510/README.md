<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# Issue 354 verification", "*"]
  }
}
-->

# Issue 354 verification

The root cause, the rejected alternatives, the known limitation and the
requirement inventory are in
[the experiment record](../../../../../../experiments/issue-354/README.md).

## Before and after

- [C# before, `main`](validation/csharp-before-main.txt): with the original
  loop temporarily restored, 12 of the 14 new test cases fail. Only the two
  empty store cases pass.
- [C# before, issue proposal](validation/csharp-before-issue-proposal.txt):
  with the loop suggested in the issue, 8 of the 14 cases fail: links between
  points, freed addresses, the cascade decorator and the usages validator.
  Only points, a link to higher addresses and the empty store pass.
- [C# after](validation/csharp-after.txt): all 14 cases pass.
- [C++ before](validation/cpp-before.txt): with the original native loop
  temporarily restored, both new native tests fail. After `DeleteAll`, a
  stale index entry makes `GetOrCreate` return a deleted link instead of a
  new one. In another case the reused store reports 6 links, and 8 links with
  source 1 where 2 are expected. The united run ends in a segmentation fault.
- [C++ before, `CreateAndUpdate`](validation/cpp-before-create-and-update.txt):
  with the new `DeleteAll` but the original handler-less `CreateAndUpdate`,
  every `DeleteAll` assertion passes. Only the `GetOrCreate` result is wrong:
  it creates link 3 but returns `Continue`.
- [C++ after](validation/cpp-after.txt): the full native suite passes, 54
  tests including both new ones (GCC, Release, warnings as errors).
- [Probe](validation/probe.txt): every loop variant, decorator and fill
  scenario, on united and split stores.
- [Checks](validation/checks.txt): `dotnet format whitespace`, the
  repository check and the native clang-format 18 check pass. The changeset
  check (`scripts/versions.py validate`) passes too.

Clang-Tidy, Cppcheck, coverage, the full C# suite and packaging run in hosted
CI. They are not repeated locally.

## Scope and release

`changes/354-delete-all.json` requests a patch release for both languages,
because both libraries change. The C++ part also covers the
`CreateAndUpdate` fix.
