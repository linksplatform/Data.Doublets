using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Memory;
using Platform.Singletons;
using Platform.Data.Doublets.Memory.United;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Exceptions;
using Platform.Delegates;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.UnitedRanged.Generic
{
    /// <summary>
    /// <para>
    /// A drop-in substitute for <see cref="UnitedMemoryLinks{TLinkAddress}"/> that
    /// additionally tracks unused space as a list of <em>ranges</em> of cells (not
    /// only one-cell at a time) and supports raw binary payloads stored inside the
    /// same address space.
    /// </para>
    /// <para>
    /// Single-cell <see cref="Create"/>/<see cref="Delete"/> semantics are unchanged
    /// for callers, but the implementation will prefer to fill an existing free
    /// range before extending the underlying memory. <see cref="AllocateRange"/> /
    /// <see cref="DeallocateRange"/> expose contiguous multi-cell allocations
    /// (best-fit + coalescing). <see cref="AllocateRawBinary"/> stores a blob whose
    /// payload reuses the tree-index fields of the spanned cells as opaque bytes.
    /// </para>
    /// </summary>
    public unsafe class UnitedRangedMemoryLinks<TLinkAddress> : UnitedMemoryLinks<TLinkAddress>
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private byte* _rangedLinks;
        private RangedFreeListMethods<TLinkAddress>? _freeRanges;
        private RawBinaryMethods<TLinkAddress>? _rawBinary;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(string address) : this(address, DefaultLinksSizeStep) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(string address, long memoryReservationStep) : this(new FileMappedResizableDirectMemory(address, memoryReservationStep), memoryReservationStep) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(IResizableDirectMemory memory) : this(memory, DefaultLinksSizeStep) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep) : this(memory, memoryReservationStep, Default<UnitedRangedLinksConstants<TLinkAddress>>.Instance, IndexTreeType.Default) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep, UnitedRangedLinksConstants<TLinkAddress> constants, IndexTreeType indexTreeType)
            : base(memory, memoryReservationStep, constants, indexTreeType)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetPointers(IResizableDirectMemory memory)
        {
            base.SetPointers(memory);
            _rangedLinks = (byte*)memory.Pointer;
            var rangedConstants = (UnitedRangedLinksConstants<TLinkAddress>)Constants;
            _freeRanges = new RangedFreeListMethods<TLinkAddress>(_rangedLinks, _rangedLinks, rangedConstants.FreeRangeMarker);
            _rawBinary = new RawBinaryMethods<TLinkAddress>(_rangedLinks, rangedConstants.RawMarker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ResetPointers()
        {
            base.ResetPointers();
            _rangedLinks = null;
            _freeRanges = null;
            _rawBinary = null;
        }

        // -------------------------------------------------------------------------
        // ILinks<TLinkAddress> overrides
        // -------------------------------------------------------------------------

        /// <summary>
        /// Returns the number of regular doublets (excludes single-cell unused
        /// links, multi-cell free ranges and raw binary blobs).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Count(IList<TLinkAddress>? restriction)
        {
            if (restriction!.Count == 0)
            {
                return CountRegularLinks();
            }
            return base.Count(restriction);
        }

        /// <summary>
        /// Iterates over regular doublets only; skips free-range and raw-binary
        /// cells entirely.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
        {
            if (restriction!.Count == 0)
            {
                var @break = Constants.Break;
                var allocated = GetHeaderReference().AllocatedLinks;
                var link = TLinkAddress.One;
                while (link <= allocated)
                {
                    if (_freeRanges!.IsFreeRangeHead(link))
                    {
                        link = link + _freeRanges.GetLength(link);
                        continue;
                    }
                    if (_rawBinary!.IsRawBinary(link))
                    {
                        link = link + TLinkAddress.CreateTruncating(_rawBinary.GetCellCount(link));
                        continue;
                    }
                    if (Exists(link) && handler!(GetLinkStruct(link)) == @break)
                    {
                        return @break;
                    }
                    link = link + TLinkAddress.One;
                }
                return @break;
            }
            return base.Each(restriction, handler);
        }

        /// <summary>
        /// Creates a single doublet. Prefers the single-cell unused list, then a
        /// carved cell from the smallest free range whose length is &gt;= 3
        /// (carving from a 2-cell range would leave a 1-cell remainder that
        /// cannot be tracked as a range; we leave such ranges intact so that
        /// <see cref="AllocateRange"/> may still use them).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            ref var header = ref GetHeaderReference();
            if (header.FirstFreeLink == Constants.Null)
            {
                var three = TLinkAddress.One + TLinkAddress.One + TLinkAddress.One;
                var range = _freeRanges!.FindBestFit(three);
                if (range != default)
                {
                    var newLink = _freeRanges.CarveFromFront(range, TLinkAddress.One);
                    return handler != null
                        ? handler(null, new Link<TLinkAddress>(newLink, Constants.Null, Constants.Null))
                        : Constants.Continue;
                }
            }
            return base.Create(substitution, handler);
        }

        /// <summary>
        /// Deletes a single doublet. Behaviour matches the base class for
        /// non-tail links; for tail links the trimming loop additionally retires
        /// trailing single-cell unused links and trailing free ranges, but never
        /// confuses a free-range head or a blob head with a single-cell unused
        /// link.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            ref var header = ref GetHeaderReference();
            var link = restriction![Constants.IndexPart];
            var before = GetLinkStruct(link);
            if (link < header.AllocatedLinks)
            {
                UnusedLinksListMethods.AttachAsFirst(link);
                return handler != null ? handler(before, null) : Constants.Continue;
            }
            if (link == header.AllocatedLinks)
            {
                header.AllocatedLinks = header.AllocatedLinks - TLinkAddress.One;
                _memory.UsedCapacity -= LinkSizeInBytes;
                TrimTail();
                return handler != null ? handler(before, null) : Constants.Continue;
            }
            return Constants.Continue;
        }

        // -------------------------------------------------------------------------
        // Public range / raw-binary API
        // -------------------------------------------------------------------------

        /// <summary>
        /// Allocates <paramref name="length"/> contiguous cells and returns the
        /// address of the first cell. The cells are uninitialised — the caller
        /// is expected to immediately write a meaningful payload (or pass the
        /// result to <see cref="WriteRawBinary"/>).
        /// </summary>
        public TLinkAddress AllocateRange(TLinkAddress length)
        {
            if (length == default)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            // Try best-fit on the multi-cell free-range list.
            var existing = _freeRanges!.FindBestFit(length);
            if (existing != default)
            {
                var existingLength = _freeRanges.GetLength(existing);
                if (existingLength == length)
                {
                    _freeRanges.Detach(existing);
                    return existing;
                }
                var remainder = existingLength - length;
                if (remainder == TLinkAddress.One)
                {
                    _freeRanges.Detach(existing);
                    // 1-cell remainder cannot be tracked as a range — push to the
                    // single-cell unused list so it is still reachable by Create().
                    UnusedLinksListMethods.AttachAsFirst(existing + length);
                    return existing;
                }
                return _freeRanges.CarveFromFront(existing, length);
            }
            // For length == 1, also try the single-cell unused list before bumping
            // the high-water mark.
            if (length == TLinkAddress.One)
            {
                var freeLink = GetHeaderReference().FirstFreeLink;
                if (freeLink != Constants.Null)
                {
                    UnusedLinksListMethods.Detach(freeLink);
                    return freeLink;
                }
            }
            // No fit anywhere — bump AllocatedLinks (extending memory if needed).
            return BumpAllocatedLinks(length);
        }

        /// <summary>
        /// Returns a multi-cell range to the allocator. <paramref name="start"/>
        /// must be the first cell previously returned by
        /// <see cref="AllocateRange"/> (or the head of a blob being released),
        /// and <paramref name="length"/> must match the original allocation.
        /// </summary>
        public void DeallocateRange(TLinkAddress start, TLinkAddress length)
        {
            if (length == default)
            {
                return;
            }
            ref var header = ref GetHeaderReference();
            // Tail-only fast path: nothing to insert, just shrink.
            if (start + length - TLinkAddress.One == header.AllocatedLinks)
            {
                ClearCells(start, length);
                header.AllocatedLinks = header.AllocatedLinks - length;
                _memory.UsedCapacity -= long.CreateTruncating(length) * LinkSizeInBytes;
                TrimTail();
                return;
            }
            // 1-cell mid-range deallocation: go on the single-cell unused list.
            if (length == TLinkAddress.One)
            {
                ClearCells(start, length);
                UnusedLinksListMethods.AttachAsFirst(start);
                return;
            }
            // 2+ cells: register as a multi-cell free range (coalesces with neighbours).
            _freeRanges!.Insert(start, length);
            TrimTail();
        }

        /// <summary>
        /// Allocates space for a raw binary blob of <paramref name="byteLength"/>
        /// bytes and returns the head cell address. <paramref name="byteLength"/>
        /// must be a non-negative multiple of <c>sizeof(TLinkAddress)</c>.
        /// The blob is left uninitialised until <see cref="WriteRawBinary"/> is
        /// called.
        /// </summary>
        public TLinkAddress AllocateRawBinary(long byteLength)
        {
            var cells = RawBinaryMethods<TLinkAddress>.ComputeCellsForBlob(byteLength);
            var start = AllocateRange(TLinkAddress.CreateTruncating(cells));
            // Clear so that IsRawBinary / IsFreeRangeHead probes on uninitialised
            // cells behave predictably until the payload is actually written.
            ClearCells(start, TLinkAddress.CreateTruncating(cells));
            // Stamp the descriptor (Source = RawMarker, Target = byteLength).
            _rawBinary!.Write(start, ReadOnlySpan<byte>.Empty);
            // Write() with an empty payload sets the descriptor's Target to 0, so
            // overwrite it now that we know the real length.
            var rangedConstants = (UnitedRangedLinksConstants<TLinkAddress>)Constants;
            ref var head = ref AsRef<RawLink<TLinkAddress>>(_rangedLinks + (RawLink<TLinkAddress>.SizeInBytes * long.CreateTruncating(start)));
            head.Source = rangedConstants.RawMarker;
            head.Target = TLinkAddress.CreateTruncating(byteLength);
            return start;
        }

        /// <summary>
        /// Writes <paramref name="payload"/> into the blob whose head is at
        /// <paramref name="start"/>. The blob must have been allocated with
        /// <see cref="AllocateRawBinary"/> using the same byte length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRawBinary(TLinkAddress start, ReadOnlySpan<byte> payload) => _rawBinary!.Write(start, payload);

        /// <summary>
        /// Copies the payload of the blob at <paramref name="start"/> into
        /// <paramref name="destination"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadRawBinary(TLinkAddress start, Span<byte> destination) => _rawBinary!.Read(start, destination);

        /// <summary>
        /// Releases the storage of the blob at <paramref name="start"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeallocateRawBinary(TLinkAddress start)
        {
            var cells = _rawBinary!.GetCellCount(start);
            DeallocateRange(start, TLinkAddress.CreateTruncating(cells));
        }

        /// <summary>True if the cell at <paramref name="address"/> is a raw binary head.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRawBinary(TLinkAddress address) => _rawBinary!.IsRawBinary(address);

        /// <summary>Returns the byte length of the blob at <paramref name="address"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetRawBinaryLengthInBytes(TLinkAddress address) => _rawBinary!.GetLengthInBytes(address);

        // -------------------------------------------------------------------------
        // Internals
        // -------------------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress BumpAllocatedLinks(TLinkAddress length)
        {
            ref var header = ref GetHeaderReference();
            var maximumPossibleInnerReference = Constants.InternalReferencesRange.Maximum;
            var newAllocated = header.AllocatedLinks + length;
            if (newAllocated > maximumPossibleInnerReference)
            {
                throw new LinksLimitReachedException<TLinkAddress>(maximumPossibleInnerReference);
            }
            // Ensure capacity: keep one cell of headroom so that base.Create() can
            // also extend by one without re-entering this path mid-call.
            while (newAllocated >= header.ReservedLinks - TLinkAddress.One)
            {
                _memory.ReservedCapacity += _memoryReservationStep;
                SetPointers(_memory);
                header = ref GetHeaderReference();
                header.ReservedLinks = TLinkAddress.CreateTruncating((_memory.ReservedCapacity - LinkHeaderSizeInBytes) / LinkSizeInBytes);
            }
            var start = header.AllocatedLinks + TLinkAddress.One;
            header.AllocatedLinks = newAllocated;
            _memory.UsedCapacity += long.CreateTruncating(length) * LinkSizeInBytes;
            return start;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TrimTail()
        {
            ref var header = ref GetHeaderReference();
            while (header.AllocatedLinks > default(TLinkAddress))
            {
                var tail = header.AllocatedLinks;
                if (IsSingleCellUnused(tail))
                {
                    UnusedLinksListMethods.Detach(tail);
                    header.AllocatedLinks = header.AllocatedLinks - TLinkAddress.One;
                    _memory.UsedCapacity -= LinkSizeInBytes;
                    continue;
                }
                var detachedLength = _freeRanges!.TryDetachTail(tail);
                if (detachedLength != default)
                {
                    header.AllocatedLinks = header.AllocatedLinks - detachedLength;
                    _memory.UsedCapacity -= long.CreateTruncating(detachedLength) * LinkSizeInBytes;
                    continue;
                }
                break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSingleCellUnused(TLinkAddress link)
        {
            ref var header = ref GetHeaderReference();
            if (header.FirstFreeLink == link)
            {
                return true;
            }
            ref var cell = ref AsRef<RawLink<TLinkAddress>>(_rangedLinks + (RawLink<TLinkAddress>.SizeInBytes * long.CreateTruncating(link)));
            if (cell.SizeAsSource != default)
            {
                return false;
            }
            if (cell.Source == default)
            {
                return false;
            }
            var rangedConstants = (UnitedRangedLinksConstants<TLinkAddress>)Constants;
            if (cell.Source == rangedConstants.FreeRangeMarker || cell.Source == rangedConstants.RawMarker)
            {
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearCells(TLinkAddress start, TLinkAddress length)
        {
            var startLong = long.CreateTruncating(start);
            var lengthLong = long.CreateTruncating(length);
            var ptr = _rangedLinks + (RawLink<TLinkAddress>.SizeInBytes * startLong);
            new Span<byte>(ptr, checked((int)(lengthLong * RawLink<TLinkAddress>.SizeInBytes))).Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress CountRegularLinks()
        {
            var count = default(TLinkAddress);
            var allocated = GetHeaderReference().AllocatedLinks;
            var link = TLinkAddress.One;
            while (link <= allocated)
            {
                if (_freeRanges!.IsFreeRangeHead(link))
                {
                    link = link + _freeRanges.GetLength(link);
                    continue;
                }
                if (_rawBinary!.IsRawBinary(link))
                {
                    link = link + TLinkAddress.CreateTruncating(_rawBinary.GetCellCount(link));
                    continue;
                }
                if (Exists(link))
                {
                    count = count + TLinkAddress.One;
                }
                link = link + TLinkAddress.One;
            }
            return count;
        }
    }
}
