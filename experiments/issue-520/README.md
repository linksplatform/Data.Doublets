# Either-endpoint query regression

Issue: <https://github.com/linksplatform/Data.Doublets/issues/520>

Pull request: <https://github.com/linksplatform/Data.Doublets/pull/545>

Two points followed by `Update(2, 1, 2)` leave records `(1: 1 1)` and
`(2: 1 2)`. The original `[Any, 1]` query counted three matches and visited
`[1, 2, 1]`. The corrected result is count two and visits `[1, 2]`.

Both memory layouts added source and target usages without removing their
intersection. The target pass now skips records already covered by the source
pass. Counting uses the same predicate with constant additional storage. It
retains every distinct raw record with equal endpoints; an exact-pair lookup
cannot count that intersection reliably because it returns at most one record.
Null endpoints are unindexed, so `[Any, Null]` filters the full record scan.

## Automated coverage

| Requirement | Regression evidence |
| --- | --- |
| Original count and visit order | Separate C# Count/Each reproductions; C++ original reproduction |
| United and split, plain and decorated | All four combinations in both languages |
| Supported C# index implementations | United default/AVL; split default/size-balanced/recursionless, with both source-index modes |
| Endpoint union and distinct records | Scan oracle, internal/external/null/missing endpoints, raw duplicate pairs |
| Address widths | C# byte, ushort, uint, ulong |
| Callback flow | Early Break in source and target passes; Continue after completion |
| Existing query forms | Wildcard and concrete-index assertions; complete existing suites |
| Deletion callers | Snapshot deletion checks that each address still exists before deleting it |
| Index maintenance | Update, deletion and free-slot reuse before repeating the scan oracle |

The C# regression class has 80 cases. Run it with:

```sh
dotnet test csharp/Platform.Data.Doublets.Tests -c Release \
  --filter FullyQualifiedName~EitherEndpointQueryTests
```

The four C++ cases are included in the normal CMake/CTest suite. After installing
the pinned dependencies with `bash cpp/setup.sh`, run `bash cpp/run_tests.sh`.
`focused-native.cpp` is a smaller translation unit containing just these cases;
it can reuse the include flags and libraries in the generated
`cpp/build/compile_commands.json` and test target's `link.txt`.

## Diagnostic trace

```sh
dotnet run --project experiments/issue-520/Trace/Trace.csproj
```

This finite byte-addressed split-store trace prints counts and visited records
before and after creation, update, reset, deletion and free-slot reuse. It
includes external references and a newly created record with null endpoints.
Normal library operations emit no additional diagnostic output.

No required sub-issues or additional issue/PR feedback were present during
implementation. Related issue #360 describes a separate API redesign; the Rust
counterpart, linksplatform/doublets-rs#63, is already closed.
