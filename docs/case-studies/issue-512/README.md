# Case Study: Issue #512 — `UnitedRangedMemoryLinks` with Link Ranges

> Source issue: <https://github.com/linksplatform/Data.Doublets/issues/512>
>
> Author: @konard
>
> Branch / PR: [`issue-512-557a0a3ca78d`](https://github.com/linksplatform/Data.Doublets/tree/issue-512-557a0a3ca78d) — PR [#513](https://github.com/linksplatform/Data.Doublets/pull/513)

This directory collects the analysis, design exploration and implementation plan for the new `UnitedRangedMemoryLinks` doublets storage variant. The goal is twofold:

1. Provide an _evolution_ of `UnitedMemoryLinks` that allocates and reclaims **contiguous ranges of links** instead of single links, while preserving the no-fragmentation, uniform-cell invariant that makes united storage so attractive.
2. Allow **raw link sequences** to live inside the same address space as ordinary doublets, by reusing the underlying link cells as payload cells. Those sequences can store raw data blobs, binary files, or any other byte payload whose length is aligned to `TLinkAddress`.

The files in this directory are:

| File | Purpose |
| --- | --- |
| [`requirements.md`](./requirements.md) | Itemised, traceable list of every requirement extracted from the issue text. |
| [`background.md`](./background.md) | Background on `UnitedMemoryLinks`, RawLink/LinksHeader layout, and the constraints imposed by the existing codebase. |
| [`design.md`](./design.md) | Design alternatives (sorted free list, segregated free list, buddy allocator, bitmap, …) and the **chosen design**, including disk layout and invariants. |
| [`related-work.md`](./related-work.md) | External references and prior art used while researching the problem (allocator literature, in-memory tagged-pointer schemes, B-tree page allocators, …). |
| [`solution-plan.md`](./solution-plan.md) | Step-by-step plan that maps every requirement to a concrete code change. |
| [`risks-and-trade-offs.md`](./risks-and-trade-offs.md) | Trade-offs, future work and explicit non-goals. |

## TL;DR

Each cell of the storage still occupies one `RawLink<TLinkAddress>` slot (8 × `TLinkAddress`), so the file format remains uniform and free of internal fragmentation. The improvements are:

* A **range allocator** that tracks free regions as a sorted-by-address, length-keyed doubly-linked list of `RawLink` cells (the same cells reused as range descriptors). Adjacent free regions are eagerly coalesced on deallocation, so the only way fragmentation can grow is when an allocation is _larger than every free region_, in which case the storage is simply extended at the tail.
* A new **`RawLinkSequenceMarker`** constant in `UnitedRangedLinksConstants` — used as the `Source` field of the first cell of a raw link sequence. The second field (`Target`) records the payload length in bytes, from which the number of consumed link cells is derived.
* A new **`UnitedRangedMemoryLinks<TLinkAddress>`** class drop-in compatible with `ILinks<TLinkAddress>` (so the existing tests pass with it as a substitute for `UnitedMemoryLinks`) with range allocation in the implementation and raw-link-sequence convenience operations in extensions.
* `Each` and `Count` include raw link sequence heads by default, while continuation cells and free ranges stay hidden. The `IncludeRawLinkSequences` configuration can exclude sequence heads when a caller wants ordinary doublets only.

For the full rationale, see [`design.md`](./design.md).
