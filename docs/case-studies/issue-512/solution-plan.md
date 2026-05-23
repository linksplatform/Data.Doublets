# Solution Plan

The plan below maps each requirement to a concrete change and lists the order of
implementation. Every checkbox corresponds to one logical commit; the commits land on
branch `issue-512-557a0a3ca78d` (PR #513).

## Step 1 — Header & constants scaffolding (`R5`, `R10`)

* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/LinksRangedHeader.cs` — a
  byte-compatible alias of `LinksHeader` with a typed `FreeRangesHead` slot that
  overlays the existing `Reserved8` word.
* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/UnitedRangedLinksConstants.cs`
  — `LinksConstants<TLinkAddress>` subclass that exposes a `RawMarker` constant.

## Step 2 — Range allocator (`R3`, `R7`, `R8`)

* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/Generic/RangedFreeListMethods.cs`
  — the in-cell, address-and-size-sorted doubly-linked free-range allocator.
* The allocator exposes:
  * `Allocate(length) → start`
  * `Deallocate(start, length)`
  * `AlreadyFreeRange(start) → bool`

## Step 3 — Raw binary blobs (`R5`, `R6`, `R9`)

* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/Generic/RawBinaryMethods.cs`
  — encodes/decodes blobs over the allocator. Exposes
  `AllocateRawBinary(byteLength)`, `WriteRawBinary(start, span)`,
  `ReadRawBinary(start, span)`, `IsRawBinary(start)`, `GetRawBinaryLengthInBytes(start)`,
  `DeallocateRawBinary(start)`.

## Step 4 — `UnitedRangedMemoryLinks` (`R1`, `R2`)

* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/Generic/UnitedRangedMemoryLinksBase.cs`
  — subclass of `UnitedMemoryLinksBase` that overrides `Create`, `Delete`, `Each`,
  `Count`, `Exists`, `IsUnusedLink` so that blob and free-range cells are correctly
  ignored.
* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/Generic/UnitedRangedMemoryLinks.cs`
  — concrete class mirroring the constructor surface of `UnitedMemoryLinks`.

## Step 5 — Tests (`R2`, `R3`, `R4`, `R5`, `R6`, `R7`, `R8`, `R9`)

* Add `csharp/Platform.Data.Doublets.Tests/UnitedRangedMemoryLinksTests.cs` containing:
  * `BasicMemoryOperations_Substitution` — equivalent to
    `ResizableDirectMemoryLinksTests.BasicHeapMemoryTest` but using the new class.
  * `AllocateRange_ReturnsContiguousBlock`.
  * `AllocateRange_FasterThanIndividualCreates` — counts memory-resize events.
  * `AllocateRange_PrefersExistingFreeRange`.
  * `DeallocateRange_CoalescesNeighbours`.
  * `DeallocateRange_TrimsTail`.
  * `RawBinary_Roundtrip_SingleCell`.
  * `RawBinary_Roundtrip_MultiCell`.
  * `RawBinary_DoesNotAppearInEach`.
  * `Each_SkipsFreeRangesAndBlobs`.
  * `NoFragmentation_ChaosTest` — deterministic random allocations/deallocations.

## Step 6 — Documentation (`R11`)

* Populate the `docs/case-studies/issue-512` folder (this directory).
* Reference the case study from the PR description.

## Step 7 — Final review (`R12`)

* Verify the full build / test pass.
* Ensure PR description summarises the design and points to the case study.
* Mark PR #513 ready for review.
