# Finding a link with an external target

Run the reproduction for [issue #522](https://github.com/linksplatform/Data.Doublets/issues/522)
against the library in this checkout:

```sh
dotnet run --project examples/ExternalReferences -c Release
```

Requires .NET 8. With only a newer runtime installed, set `DOTNET_ROLL_FORWARD=Major`.
The example uses small memory reservations and runs the two stores sequentially.

It creates point 1, then link 2 with source 1 and external target
18446744073709551596 (`new Hybrid<ulong>(20, isExternal: true).Value`).
Both stores must print:

```text
united: created 2 = (1, 18446744073709551596); Each -> [2], Count -> 1, SearchOrDefault -> 2
split: created 2 = (1, 18446744073709551596); Each -> [2], Count -> 1, SearchOrDefault -> 2
```

Before the fix, split printed `Each -> [], Count -> 0, SearchOrDefault -> 0`,
and this example failed. Direct reads still showed the correct endpoints.

The united store keeps data and index metadata in one memory region. Its
store-wide source and target trees index every link. The split store separates
data and metadata into two memory regions. It chooses internal or external
indexes using the configured external-reference range. By default, an internal
source uses a linked list, while an external target uses a store-wide target tree.

Exact mixed-reference queries incorrectly searched the external **source** tree,
where source 1 has no entry. They now search the external **target** tree, which
contains link 2. Both `Each` and `Count` use this correction. `SearchOrDefault`
and `GetOrCreate` benefit because they call `Each`.

[Rust PR #75](https://github.com/linksplatform/doublets-rs/pull/75) supplied the
requested reference for checking index consistency. Rust classifies references
by whether the referenced ID exists, so that fix moves entries when an ID is
created. C# uses a configured address range; the repair here changes the query's
choice of index without moving entries or changing stored data.

The regression suite tests all four integer address widths, both tree
implementations, and source lists/trees. It also compares exact and wildcard
queries with a full scan through updates, deletion, and ID reuse, including all
four internal/external endpoint combinations and absent pairs:

```sh
dotnet test csharp/Platform.Data.Doublets.Tests -c Release \
  --filter FullyQualifiedName~SplitMemoryExternalReferencesTests
```
