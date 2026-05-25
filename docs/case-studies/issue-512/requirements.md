# Requirements (Issue #512)

The requirements below are extracted verbatim from the issue body, then re-expressed as
acceptance criteria. Identifiers (`R1`, `R2`, …) are referenced from
[`solution-plan.md`](./solution-plan.md) so every change in the PR maps back to one of
them.

## R1. New folder `UnitedRanged` next to `UnitedMemoryLinks`

> "We add new UnitedRanged folder, and do not break any other existing feature."

* **Acceptance:** new directory `csharp/Platform.Data.Doublets/Memory/UnitedRanged/` exists
  with the new types. Existing `Memory/United/` files are **unchanged in behaviour**.

## R2. New class `UnitedRangedMemoryLinks`

> "add fully supported in all places UnitedRangedMemoryLinks, that can be used as
> substitution of UnitedMemoryLinks."

* **Acceptance:**
  * implements `ILinks<TLinkAddress>`,
  * exposes the same set of constructors as `UnitedMemoryLinks`
    (`(string)`, `(string, long)`, `(IResizableDirectMemory)`, `(IResizableDirectMemory, long)`,
    `(IResizableDirectMemory, long, UnitedRangedLinksConstants<TLinkAddress>, IndexTreeType)`),
    plus overloads that configure whether raw link sequence heads are visible in
    `Each`/`Count`,
  * existing tests (`ResizableDirectMemoryLinksTests`, `ILinksBasicTests`,
    `GenericLinksTests`, `GarbageCollectionTests`) succeed when the type is plugged in
    instead of `UnitedMemoryLinks` for storage operations covered by `ILinks<>`.

## R3. Range allocation/deallocation in multiples of the cell size

> "We need elegant solution, that will allow us to allocate/deallocate ranges that are
> multiple of single link size, so the memory management is still uniform without
> possibility of any fragmentation"

* **Acceptance:**
  * `AllocateRange(TLinkAddress length)` returns the start address of a contiguous block of
    `length` cells, or grows the file by one cell at a time when no suitable free range
    exists (cf. R7).
  * `DeallocateRange(TLinkAddress start, TLinkAddress length)` returns cells from a
    previously allocated range or raw link sequence to the free list and **coalesces**
    with adjacent free regions.
  * Every range described by the allocator has a length that is a positive integer
    multiple of `RawLink<TLinkAddress>.SizeInBytes`. No partial cells are ever
    produced.

## R4. Range allocation should be faster than allocating one-by-one

> "we should also be to allocate/deallocate ranges of links (that should be faster than
> allocating one by one)"

* **Acceptance:** a unit test compares `AllocateRange(N)` against `N` individual
  `Create()` calls and verifies that range allocation advances the high-water mark in
  one operation instead of one operation per cell.

## R5. Raw link sequence allocation

> "and also allocating raw binary ranges. And use some constant in LinksContants as a
> marker of such raw binary links"

* **Acceptance:**
  * a new constant `RawLinkSequenceMarker` is exposed via
    `UnitedRangedLinksConstants<TLinkAddress>`
    (a subclass of `LinksConstants<TLinkAddress>` so we don't break the upstream
    contract),
  * extension method `AllocateRawLinkSequence(long sizeInBytes)` validates that the
    byte size is a non-negative multiple of `sizeof(TLinkAddress)` and returns the
    start cell address of the sequence,
  * the first cell of the sequence carries:
    * `Source = RawLinkSequenceMarker`,
    * `Target = lengthInBytes`,
  * extension methods `IsRawLinkSequence(start)` and `IsRawLinkSequence(linkFromEach)`
    identify sequence heads.

## R6. Binary tree fields are part of the payload

> "in binary range the fields we usually used for indexing trees should be supported as
> just continuation of binary data"

* **Acceptance:** the entire `RawLink` struct fields beyond `Source`/`Target` of the
  **first** cell (`LeftAsSource`, `RightAsSource`, `SizeAsSource`, `LeftAsTarget`,
  `RightAsTarget`, `SizeAsTarget`) are addressable and writable as continuation of the
  payload via `WriteRawLinkSequence`/`ReadRawLinkSequence`.
  Trees are **not attached** to the cells that belong to a raw link sequence, so the
  indexing fields can be freely used as data bytes.

## R7. No fragmentation

> "if the size of requested range is greater than any free range, we should just append
> it to the end of the data store."

* **Acceptance:**
  * On allocation, the allocator scans the free list and uses **first-fit by smallest
    range that satisfies the request** ("best-fit"). If none qualifies, it grows
    `AllocatedLinks` at the tail.
  * On deallocation, neighbours are coalesced.
  * A property-based test allocates and deallocates a deterministic random sequence and
    asserts that, after every operation, the free list contains no two adjacent free
    regions.

## R8. Prefer filling empty/unused space first

> "we should prefer filling the empty / unused space, to pack up everything nicely."

* **Acceptance:** for any allocation request that fits in any existing free range, no
  new cells are appended at the tail; this is covered by
  `AllocateRange_PrefersExistingFreeRange`.

## R9. Treat marker'd cells as binary, not as references

> "that should be treated not as references to links, but binary data itself"

* **Acceptance:**
  * `Each` and `Count` include raw link sequence heads by default so universal
    `ILinks<>` consumers can discover them.
  * `IncludeRawLinkSequences = false` excludes raw link sequence heads from
    `Each`/`Count` for callers that want ordinary doublets only.
  * Continuation cells inside a raw link sequence are always skipped.
  * Tree-method invariants are preserved by never inserting raw link sequence cells in
    the source/target trees; ranged iteration scans visible cells directly instead.

## R10. Backwards compatibility

> "do not break any other existing feature"

* **Acceptance:** the original `UnitedMemoryLinks` class is untouched; the existing test
  suite continues to pass; the new class is additive.

## R11. Documentation & case study

> "We need to collect data related about the issue to this repository, make sure we
> compile that data to `./docs/case-studies/issue-{id}` folder, and use it to do deep
> case study analysis"

* **Acceptance:** the present folder (`docs/case-studies/issue-512`) contains the
  background, requirements, design and solution plan.

## R12. Single pull request

> "Please plan and execute everything in a single pull request"

* **Acceptance:** all work lands in PR #513 against branch `issue-512-557a0a3ca78d`.
