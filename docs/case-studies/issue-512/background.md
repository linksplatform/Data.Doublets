# Background — How `UnitedMemoryLinks` Works Today

This is a short tour of the parts of the existing implementation that the
`UnitedRangedMemoryLinks` design needs to interact with. Line numbers refer to the
state of the repository at the time of writing.

## File layout

A united-memory database is a single mapped file that begins with a `LinksHeader` and
then continues with a sequence of equally sized `RawLink` cells:

```
+-------------------+-------------------+-------------------+-----+
|     Header        |   Cell #1         |   Cell #2         |  …  |
| (LinkSizeInBytes) | (LinkSizeInBytes) | (LinkSizeInBytes) |     |
+-------------------+-------------------+-------------------+-----+
```

The header overlays the very first cell, so cell #0 never carries real data
(`csharp/Platform.Data.Doublets/Memory/United/Generic/UnitedMemoryLinksBase.cs:184`).

`LinkSizeInBytes` is `8 * sizeof(TLinkAddress)` — that is, eight `TLinkAddress` words:

```csharp
public struct RawLink<TLinkAddress>
{
    public TLinkAddress Source;        // word 0
    public TLinkAddress Target;        // word 1
    public TLinkAddress LeftAsSource;  // word 2
    public TLinkAddress RightAsSource; // word 3
    public TLinkAddress SizeAsSource;  // word 4
    public TLinkAddress LeftAsTarget;  // word 5
    public TLinkAddress RightAsTarget; // word 6
    public TLinkAddress SizeAsTarget;  // word 7
}
```

The header is exactly the same size and is laid out as:

```csharp
public struct LinksHeader<TLinkAddress>
{
    public TLinkAddress AllocatedLinks; // word 0 — high-water mark
    public TLinkAddress ReservedLinks;  // word 1 — capacity in cells
    public TLinkAddress FreeLinks;      // word 2 — size of the unused list
    public TLinkAddress FirstFreeLink;  // word 3 — head of the unused list
    public TLinkAddress RootAsSource;   // word 4 — root of the by-source tree
    public TLinkAddress RootAsTarget;   // word 5 — root of the by-target tree
    public TLinkAddress LastFreeLink;   // word 6 — tail of the unused list
    public TLinkAddress Reserved8;      // word 7 — currently unused
}
```

The matching `Reserved8` word is what `UnitedRangedMemoryLinks` will use for the
**free-range list head**.

## Lifecycle of a single link

* `Create` (`UnitedMemoryLinksBase.cs:509-535`) takes the next unused cell from the
  unused list (`UnusedLinksListMethods`), or appends a cell at the tail and grows the
  underlying memory by `_memoryReservationStep` bytes if the reserved capacity is
  exhausted.
* `Delete` (`UnitedMemoryLinksBase.cs:548-574`) either attaches the cell to the front
  of the unused list, or — if it is the very last allocated cell — shrinks
  `AllocatedLinks`, then keeps popping from the unused list while its tail is the new
  high-water mark.
* `Update` (`UnitedMemoryLinksBase.cs:472-503`) detaches the link from the
  source/target trees, mutates the cell, and re-attaches.

The "unused list" is an _absolute circular doubly-linked list_
(`UnusedLinksListMethods.cs`). Critically, it stores the previous/next pointers in
the `Source`/`Target` slots of the cell it links — so a cell on the free list can be
detected by the predicate

```csharp
link.SizeAsSource == default && link.Source != default
```

(`UnitedMemoryLinksBase.cs:686-697`).

## Implications for the new design

1. **Cell #0 is the header.** The reserved word `Reserved8` is _the_ obvious place to
   store an extra root pointer — for the free-range list — without breaking any code
   that does not look at it. The header will be repurposed as
   `LinksRangedHeader<TLinkAddress>` (a `LayoutKind.Explicit` struct with the same
   fields plus a typed alias for `Reserved8`) so the binary representation stays
   identical to `LinksHeader`. This means a database written by `UnitedMemoryLinks` can
   be opened by `UnitedRangedMemoryLinks` and vice-versa, as long as no binary blobs
   are present.

2. **A free single cell remains a free single cell.** The original unused-links list is
   _preserved_; the new "free range" list only tracks runs of two or more contiguous
   free cells. When a range deallocation produces a run of length 1, it is pushed back
   onto the original unused-links list.

3. **Source-or-Target equal to `RawMarker`** marks a binary blob. The marker value is
   chosen so that:
   * it is outside `InternalReferencesRange` (so it cannot accidentally appear as a
     valid link reference);
   * it is _stable_ across versions of `LinksConstants` — we anchor it to one position
     above the existing `Error` constant inside the reserved tail of the references
     range, which `LinksConstants` already keeps for housekeeping (`Continue`, `Break`,
     `Skip`, `Any`, `Itself`, `Error`).

4. **Tree methods are unchanged.** The new class only intercepts `Create`, `Update`,
   `Delete`, `Each` and `Count` to (a) skip cells that belong to a binary blob and
   (b) ignore the free-range descriptor cells. All the tree methods receive the same
   pointers as before and operate without modification.
