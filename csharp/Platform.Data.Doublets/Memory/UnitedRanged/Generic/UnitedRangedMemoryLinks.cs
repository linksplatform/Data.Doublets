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
    /// only one-cell at a time) and supports raw link sequences stored inside the
    /// same address space. Raw link sequences can be used as byte payloads for raw
    /// data, binary files, and similar use cases.
    /// </para>
    /// <para>
    /// Single-cell <see cref="Create"/>/<see cref="Delete"/> semantics are unchanged
    /// for callers, but the implementation will prefer to fill an existing free
    /// range before extending the underlying memory. <see cref="AllocateRange"/> /
    /// <see cref="DeallocateRange"/> expose contiguous multi-cell allocations
    /// (best-fit + coalescing). Convenience operations for raw link sequence payloads
    /// are provided as extension methods over this range allocator.
    /// </para>
    /// </summary>
    public unsafe class UnitedRangedMemoryLinks<TLinkAddress> : UnitedMemoryLinks<TLinkAddress>
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private byte* _rangedLinks;
        private RangedFreeListMethods<TLinkAddress>? _freeRanges;
        private RawLinkSequenceMethods<TLinkAddress>? _rawLinkSequences;
        private bool _includeRawLinkSequences = true;

        /// <summary>
        /// Controls whether raw link sequence heads are returned by <see cref="Each"/>
        /// and included by <see cref="Count"/>. Continuation cells are never returned.
        /// </summary>
        public bool IncludeRawLinkSequences
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _includeRawLinkSequences;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _includeRawLinkSequences = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(string address) : this(address, DefaultLinksSizeStep) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(string address, long memoryReservationStep) : this(new FileMappedResizableDirectMemory(address, memoryReservationStep), memoryReservationStep) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(string address, bool includeRawLinkSequences) : this(address, DefaultLinksSizeStep, includeRawLinkSequences) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(string address, long memoryReservationStep, bool includeRawLinkSequences) : this(new FileMappedResizableDirectMemory(address, memoryReservationStep), memoryReservationStep, includeRawLinkSequences) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(IResizableDirectMemory memory) : this(memory, DefaultLinksSizeStep) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep) : this(memory, memoryReservationStep, Default<UnitedRangedLinksConstants<TLinkAddress>>.Instance, IndexTreeType.Default, includeRawLinkSequences: true) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep, bool includeRawLinkSequences) : this(memory, memoryReservationStep, Default<UnitedRangedLinksConstants<TLinkAddress>>.Instance, IndexTreeType.Default, includeRawLinkSequences) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep, UnitedRangedLinksConstants<TLinkAddress> constants, IndexTreeType indexTreeType)
            : this(memory, memoryReservationStep, constants, indexTreeType, includeRawLinkSequences: true)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep, UnitedRangedLinksConstants<TLinkAddress> constants, IndexTreeType indexTreeType, bool includeRawLinkSequences)
            : base(memory, memoryReservationStep, constants, indexTreeType)
        {
            IncludeRawLinkSequences = includeRawLinkSequences;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetPointers(IResizableDirectMemory memory)
        {
            base.SetPointers(memory);
            _rangedLinks = (byte*)memory.Pointer;
            var rangedConstants = (UnitedRangedLinksConstants<TLinkAddress>)Constants;
            _freeRanges = new RangedFreeListMethods<TLinkAddress>(_rangedLinks, _rangedLinks, rangedConstants.FreeRangeMarker);
            _rawLinkSequences = new RawLinkSequenceMethods<TLinkAddress>(_rangedLinks, rangedConstants.RawLinkSequenceMarker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ResetPointers()
        {
            base.ResetPointers();
            _rangedLinks = null;
            _freeRanges = null;
            _rawLinkSequences = null;
        }

        // -------------------------------------------------------------------------
        // ILinks<TLinkAddress> overrides
        // -------------------------------------------------------------------------

        /// <summary>
        /// Returns the number of visible records. Free ranges and raw link sequence
        /// continuation cells are always hidden; raw link sequence heads are included
        /// when <see cref="IncludeRawLinkSequences"/> is enabled.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Count(IList<TLinkAddress>? restriction)
        {
            restriction ??= Array.Empty<TLinkAddress>();
            if (restriction.Count > 3)
            {
                throw new NotSupportedException("Другие размеры и способы ограничений не поддерживаются.");
            }
            var constants = Constants;
            var any = constants.Any;
            var count = default(TLinkAddress);
            if (restriction.Count == 2 && restriction[constants.IndexPart] == any)
            {
                var value = restriction[1];
                if (value == any)
                {
                    return CountVisibleLinks();
                }
                ForEachVisibleLink(link =>
                {
                    if (link.Source == value)
                    {
                        count = count + TLinkAddress.One;
                    }
                    if (link.Target == value)
                    {
                        count = count + TLinkAddress.One;
                    }
                    return constants.Continue;
                });
                return count;
            }
            ForEachVisibleLink(link =>
            {
                if (MatchesRestriction(link, restriction))
                {
                    count = count + TLinkAddress.One;
                }
                return constants.Continue;
            });
            return count;
        }

        /// <summary>
        /// Iterates over visible records. Free ranges and raw link sequence
        /// continuation cells are always hidden; raw link sequence heads are included
        /// when <see cref="IncludeRawLinkSequences"/> is enabled.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
        {
            restriction ??= Array.Empty<TLinkAddress>();
            if (restriction.Count > 3)
            {
                throw new NotSupportedException("Другие размеры и способы ограничений не поддерживаются.");
            }
            var constants = Constants;
            var @break = constants.Break;
            var @continue = constants.Continue;
            var any = constants.Any;
            if (restriction.Count == 2 && restriction[constants.IndexPart] == any)
            {
                var value = restriction[1];
                if (value == any)
                {
                    return EachMatchingLink(handler, link => true, returnBreakOnCompletion: true);
                }
                if (ForEachVisibleLink(link =>
                {
                    if (link.Source != value)
                    {
                        return @continue;
                    }
                    if (handler != null && handler(link) == @break)
                    {
                        return @break;
                    }
                    return @continue;
                }) == @break)
                {
                    return @break;
                }
                return ForEachVisibleLink(link =>
                {
                    if (link.Target != value)
                    {
                        return @continue;
                    }
                    if (handler != null && handler(link) == @break)
                    {
                        return @break;
                    }
                    return @continue;
                });
            }
            return EachMatchingLink(handler, link => MatchesRestriction(link, restriction), IsWholeStoreScan(restriction));

            TLinkAddress EachMatchingLink(ReadHandler<TLinkAddress>? visibleHandler, Func<Link<TLinkAddress>, bool> predicate, bool returnBreakOnCompletion)
            {
                if (ForEachVisibleLink(link =>
                {
                    if (!predicate(link))
                    {
                        return @continue;
                    }
                    if (visibleHandler != null && visibleHandler(link) == @break)
                    {
                        return @break;
                    }
                    return @continue;
                }) == @break || returnBreakOnCompletion)
                {
                    return @break;
                }
                return @continue;
            }
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
        /// confuses a free-range head or a raw link sequence head with a
        /// single-cell unused link.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            ref var header = ref GetHeaderReference();
            var link = restriction![Constants.IndexPart];
            var before = GetLinkStruct(link);
            if (_rawLinkSequences!.IsRawLinkSequence(link))
            {
                var cells = _rawLinkSequences.GetCellCount(link);
                DeallocateRange(link, TLinkAddress.CreateTruncating(cells));
                return handler != null ? handler(before, null) : Constants.Continue;
            }
            if (_freeRanges!.IsFreeRangeHead(link))
            {
                return Constants.Continue;
            }
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

        /// <summary>
        /// Protects ranged metadata cells from being treated as normal doublets by
        /// generic update helpers. Reset updates are accepted as no-ops so the
        /// existing delete extension can still deallocate a raw link sequence through
        /// the universal <see cref="ILinks{TLinkAddress}"/> surface.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            var link = restriction![Constants.IndexPart];
            if (_rawLinkSequences!.IsRawLinkSequence(link) || _freeRanges!.IsFreeRangeHead(link))
            {
                if (IsResetSubstitution(substitution))
                {
                    return Constants.Continue;
                }
                throw new InvalidOperationException("Ranged metadata cells cannot be updated as regular doublets.");
            }
            return base.Update(restriction, substitution, handler);
        }

        // -------------------------------------------------------------------------
        // Public range API
        // -------------------------------------------------------------------------

        /// <summary>
        /// Allocates <paramref name="length"/> contiguous cells and returns the
        /// address of the first cell. The cells are uninitialised — the caller
        /// is expected to immediately write a meaningful payload (or pass the
        /// result to a raw link sequence extension method).
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
        /// <see cref="AllocateRange"/> (or the head of a raw link sequence being
        /// released), and <paramref name="length"/> must match the original
        /// allocation.
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
            if (cell.Source == rangedConstants.FreeRangeMarker || cell.Source == rangedConstants.RawLinkSequenceMarker)
            {
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ClearCells(TLinkAddress start, TLinkAddress length)
        {
            var startLong = long.CreateTruncating(start);
            var lengthLong = long.CreateTruncating(length);
            var ptr = _rangedLinks + (RawLink<TLinkAddress>.SizeInBytes * startLong);
            new Span<byte>(ptr, checked((int)(lengthLong * RawLink<TLinkAddress>.SizeInBytes))).Clear();
        }

        internal RawLinkSequenceMethods<TLinkAddress> RawLinkSequences
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rawLinkSequences!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress CountVisibleLinks()
        {
            var count = default(TLinkAddress);
            ForEachVisibleLink(_ =>
            {
                count = count + TLinkAddress.One;
                return Constants.Continue;
            });
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress ForEachVisibleLink(Func<Link<TLinkAddress>, TLinkAddress> action)
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
                if (_rawLinkSequences!.IsRawLinkSequence(link))
                {
                    if (IncludeRawLinkSequences && action(new Link<TLinkAddress>(link, GetLinkReference(link).Source, GetLinkReference(link).Target)) == @break)
                    {
                        return @break;
                    }
                    link = link + TLinkAddress.CreateTruncating(_rawLinkSequences.GetCellCount(link));
                    continue;
                }
                if (Exists(link))
                {
                    if (action(new Link<TLinkAddress>(link, GetLinkReference(link).Source, GetLinkReference(link).Target)) == @break)
                    {
                        return @break;
                    }
                }
                link = link + TLinkAddress.One;
            }
            return Constants.Continue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesRestriction(Link<TLinkAddress> link, IList<TLinkAddress> restriction)
        {
            var constants = Constants;
            var any = constants.Any;
            return restriction.Count switch
            {
                0 => true,
                1 => restriction[constants.IndexPart] == any || link.Index == restriction[constants.IndexPart],
                2 => MatchesIndex(link, restriction[constants.IndexPart], any)
                    && (restriction[1] == any || link.Source == restriction[1] || link.Target == restriction[1]),
                3 => MatchesIndex(link, restriction[constants.IndexPart], any)
                    && (restriction[constants.SourcePart] == any || link.Source == restriction[constants.SourcePart])
                    && (restriction[constants.TargetPart] == any || link.Target == restriction[constants.TargetPart]),
                _ => false
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool MatchesIndex(Link<TLinkAddress> link, TLinkAddress index, TLinkAddress any) => index == any || link.Index == index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsWholeStoreScan(IList<TLinkAddress> restriction)
        {
            var constants = Constants;
            var any = constants.Any;
            return restriction.Count switch
            {
                0 => true,
                1 => restriction[constants.IndexPart] == any,
                2 => restriction[constants.IndexPart] == any && restriction[1] == any,
                3 => restriction[constants.IndexPart] == any
                    && restriction[constants.SourcePart] == any
                    && restriction[constants.TargetPart] == any,
                _ => false
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsResetSubstitution(IList<TLinkAddress>? substitution)
        {
            if (substitution == null || substitution.Count < 3)
            {
                return false;
            }
            return substitution[Constants.SourcePart] == Constants.Null && substitution[Constants.TargetPart] == Constants.Null;
        }
    }
}
