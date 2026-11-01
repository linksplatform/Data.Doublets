<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# MergeUsages endpoint regression", "*"]
  }
}
-->

# MergeUsages endpoint regression

Issue: <https://github.com/linksplatform/Data.Doublets/issues/515>

Pull request: <https://github.com/linksplatform/Data.Doublets/pull/546>

Three points followed by `(4: 1 3)` and `(5: 3 1)`, then `MergeUsages(1, 2)`,
must give `(4: 2 3)` and `(5: 3 2)`. The issue reported `(4: 3 0)` and
`(5: 2 0)` for C# 0.18.1.

## Root cause

A two-value link is `(index, source)` with a default target in both ports.
In C#, `new Link<T>(a, b)` binds to `Link(params T[])`, and no
`(source, target)` constructor exists. In C++, `LinkType` is
`std::vector<address>`, and the original `(a, b)` initializers were comma
expressions. Each substitution therefore lost its target and took its source
from the wrong half. The usage-as-target branch also kept the target, which is
the address being merged away, when it should have kept the source.

## Requirement inventory

- Rewrite only the source endpoint: both ports pass the index, the new
  source and the old target. Verified by the C# and C++ tests.
- Rewrite only the target endpoint: both ports pass the index, the old
  source and the new target. Verified by the C# and C++ tests.
- `MergeAndDelete` inherits the fix: covered with and without handlers in
  both test suites.
- Grep other two-value constructions: only the two `MergeUsages` sites per
  port. The `check_repository.py` guard now rejects new ones.

The C# substitutions were already corrected on `main` by 77e2b6934 (released
in C# 0.18.5 for issue #519), but nothing pinned the issue's results. This
pull request adds that coverage. The C++ port still had both defects. Its
`MergeUsages` template didn't even compile when instantiated, so no native
caller could have used it.

Fixing the native port exposed two more defects in the same path:

- The handler-taking native `Delete(storage, link, handler)` called itself.
  GCC rejects this with `-Werror=infinite-recursion` once it is instantiated.
  It now calls the store's `Delete` directly.
- Native stores invoke handlers unchecked. A null or broken handler raised
  `std::bad_function_call`. `MergeUsages` and `MergeAndDelete` now route the
  handler through `WriteHandlerState::Handle`, which skips null and broken
  handlers.

A link that uses the merged address on both ends, `(6: 1 1)`, becomes
`(6: 2 2)`. Its handler sees one update per endpoint.

## Guard against new two-value constructions

`scripts/check_repository.py` rejects two-value link constructions in
`csharp/Platform.Data.Doublets/` and `cpp/Platform.Data.Doublets/`. In C#
this means `new Link<...>(a, b)`. In C++ it means a `LinkType` initializer
with two top-level values, including a parenthesised comma expression.
Tests may still build two-value either-endpoint queries. The guard runs in CI
and pre-commit. `experiments/test_issue_515.py` checks that the guard flags
both original sites in each port: C# before 77e2b6934 and C++ before this
pull request.

## Commands

```sh
dotnet test csharp/Platform.Data.Doublets.Tests -c Release \
  --filter FullyQualifiedName~MergeUsagesTests
python3 -m unittest experiments/test_issue_515.py
```

The two native cases are part of the normal CMake/CTest suite
(`bash cpp/setup.sh`, then `bash cpp/run_tests.sh`). `focused-native.cpp` is a
smaller translation unit containing just these cases. It can reuse the include
flags and libraries in the generated `cpp/build/compile_commands.json` and the
test target's `link.txt`.

No comments, required sub-issues or review feedback were present during
implementation.
