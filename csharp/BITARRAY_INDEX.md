# Optional BitArray links index

`BitArrayLinksIndex<TLinkAddress>` is an additional in-memory index decorator.
It leaves the store's tree indexes in place, so callers can compare their results
or switch read routing without changing how data is stored.

```csharp
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

using var store = new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory());
var links = new BitArrayLinksIndex<ulong>(store);
var id = links.CreateAndUpdate<ulong>(11, 22);
var any = links.Constants.Any;
var bySource = new ulong[] { any, 11, any };

var indexedCount = links.Count(bySource); // 1
var treeCount = store.Count(bySource);   // same data, existing tree index

links.ReadMode = BitArrayIndexReadMode.WhenSelective;
links.Update<ulong>(id, 33, 44);
links.ReadMode = BitArrayIndexReadMode.Never; // writes still maintain both indexes
links.ReadMode = BitArrayIndexReadMode.Always;

// Native stores require populated values to be reset before deletion.
// Use this existing helper, or the appropriate deletion resolver decorator.
Platform.Data.Doublets.ILinksExtensions.Delete(links, id, null);
```

The wrapper does not own or dispose the underlying store. It builds an index of
existing links when constructed. Place it outside the write resolvers you want
to observe; all subsequent writes must pass through the same index wrapper.

## Read routing and exact scope

The BitArrays accelerate these three-part restrictions:

| Restriction | Indexed membership |
| --- | --- |
| `(Any, source, Any)` with a non-null, non-Any source | Links with that source |
| `(Any, Any, target)` with a non-null, non-Any target | Links with that target |

`Always` selects the bitmap for every supported query. `Never` forwards all
reads. `WhenSelective` uses the bitmap when the number of matches is at most
half the indexed link count; otherwise it forwards to the tree. This is a
simple deterministic routing policy, not a measured claim that one index is
always faster.

Every other restriction is passed through unchanged, including exact link IDs,
two-part endpoint-OR queries, fully specified source/target pairs, zero/null
endpoint searches, empty restrictions and unsupported shapes. This matters:
native stores have their own matching, duplicate, and callback-return semantics
for these forms. The decorator does not silently reinterpret those semantics.

`Count` uses the bitmap's maintained member count. `Each` visits matching links
and honors `Constants.Break`; its visit order is unspecified and can differ
from the tree. Other handler results are treated as continuation, as in native
endpoint traversal. A null read handler is forwarded to the underlying store.

## Writes and recovery

Create, update, and delete forward the original arguments and callback images,
including the callback's flow return. The method returns the underlying store's
return value, not an assumed link ID. Null write handlers remain null.

After every successful outermost write, the decorator rebuilds the index from
all final stored rows. Existing resolvers can create or modify other links
without forwarding their callbacks, so callback images or unchanged row counts
cannot prove which memberships changed. The full rebuild covers these hidden
changes as well as multi-row callbacks and interrupted batches. Reads inside a
write callback use the underlying store; nested writes defer rebuilding until
the outermost write finishes.

If a storage call or user callback throws after a partial change, the original
exception propagates and the index is marked invalid. A later eligible indexed
read rebuilds it. This provides index consistency, **not transaction rollback**.

`ClearIndex()` releases the bitmap pages and invalidates the index. It does not delete or
hide stored links: a later eligible read rebuilds it. `RebuildIndex()` also lets
the caller explicitly resynchronize after a write through another store
reference. Direct concurrent access to the underlying store is not protected
by the decorator's lock and must be coordinated by the caller.

Calls through the same wrapper are serialized by a reentrant lock. Writing,
clearing, or rebuilding during indexed `Each` is rejected before changing
storage; collect IDs first, then mutate them. A callback should not wait for a
different thread to acquire the same wrapper lock. Existing native deletion,
uniqueness, validity and persistence requirements continue to apply.

## Memory and cost

Link addresses are dictionary keys; they are never converted to bitmap offsets
or `int`. Dense integer slots map rows to 256-bit `BitArray` pages.
Only occupied pages are allocated for each source and target, and empty pages
are released. Sparse 64-bit IDs or large external endpoints therefore cannot
allocate a bitmap proportional to the address value. Every successful outer
write rebuilds the dense slot mapping. The collections can retain their
high-water capacity after clearing; their old row references and pages are
released.

Indexed counts are constant-time after membership lookup. Enumeration scans
the occupied pages, plus the matching rows. Each outer write scans the entire
store to rebuild membership, so write cost is linear in the total stored rows
and includes fresh bitmap allocation. This is an optional memory/performance
tradeoff and does not remove the native tree index.

## Verification

```sh
dotnet restore csharp/Platform.Data.Doublets.sln
dotnet build csharp/Platform.Data.Doublets.sln --no-restore
dotnet test csharp/Platform.Data.Doublets.sln --no-restore
```

`BitArrayLinksIndexTests` includes differential query/CRUD checks, seeded write
sequences, all three read modes, existing data, callback flow and exceptions,
batch interruption, nested writes, null handlers, cleared indexes, 600-row page
reuse, small address types and sparse large IDs. Routing tests also verify that
eligible reads actually avoid calling the decorated store.
