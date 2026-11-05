using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Data.Doublets.Memory.United;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.UnitedRanged.Generic
{
    /// <summary>
    /// <para>
    /// Address-sorted doubly-linked list of free ranges of length ≥ 2 cells.
    /// The list head is stored in <see cref="LinksHeader{TLinkAddress}.Reserved8"/>
    /// (re-purposed via <see cref="UnitedRangedMemoryLinks{TLinkAddress}"/>).
    /// </para>
    /// <para>
    /// Each free range is described by its first cell:
    /// </para>
    /// <list type="bullet">
    /// <item><c>Source</c> = <c>FreeRangeMarker</c></item>
    /// <item><c>Target</c> = length of the range in cells (≥ 2)</item>
    /// <item><c>LeftAsSource</c> = previous free range's start address (0 if none)</item>
    /// <item><c>RightAsSource</c> = next free range's start address (0 if none)</item>
    /// </list>
    /// <para>
    /// All other cells of the range are zeroed so they look like uninitialised cells.
    /// </para>
    /// </summary>
    public unsafe class RangedFreeListMethods<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private readonly byte* _links;
        private readonly byte* _header;
        private readonly TLinkAddress _freeRangeMarker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RangedFreeListMethods(byte* links, byte* header, TLinkAddress freeRangeMarker)
        {
            _links = links;
            _header = header;
            _freeRangeMarker = freeRangeMarker;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref LinksHeader<TLinkAddress> GetHeaderReference() => ref AsRef<LinksHeader<TLinkAddress>>(_header);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref RawLink<TLinkAddress> GetLinkReference(TLinkAddress address) => ref AsRef<RawLink<TLinkAddress>>(_links + (RawLink<TLinkAddress>.SizeInBytes * long.CreateTruncating(address)));

        /// <summary>
        /// Returns true if the cell at <paramref name="address"/> is the head of a
        /// multi-cell free range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFreeRangeHead(TLinkAddress address)
        {
            if (address == default)
            {
                return false;
            }
            ref var cell = ref GetLinkReference(address);
            return cell.Source == _freeRangeMarker && cell.Target != default;
        }

        /// <summary>
        /// Returns the length (in cells) of the free range whose head is at
        /// <paramref name="address"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress GetLength(TLinkAddress address)
        {
            ref var cell = ref GetLinkReference(address);
            return cell.Target;
        }

        /// <summary>
        /// Head of the address-sorted free-range list.
        /// </summary>
        public TLinkAddress Head
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetHeaderReference().Reserved8;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => GetHeaderReference().Reserved8 = value;
        }

        /// <summary>
        /// Best-fit search of the free-range list. Returns the smallest range that
        /// can accommodate <paramref name="length"/> cells, or <c>default</c> if none.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress FindBestFit(TLinkAddress length)
        {
            var current = Head;
            var best = default(TLinkAddress);
            var bestLength = default(TLinkAddress);
            while (current != default)
            {
                ref var cell = ref GetLinkReference(current);
                var currentLength = cell.Target;
                if (currentLength >= length)
                {
                    if (best == default || currentLength < bestLength)
                    {
                        best = current;
                        bestLength = currentLength;
                        if (currentLength == length)
                        {
                            break;
                        }
                    }
                }
                current = cell.RightAsSource;
            }
            return best;
        }

        /// <summary>
        /// Inserts a free range covering cells <c>[start .. start + length)</c> into the
        /// address-sorted list and coalesces it with neighbours. The cells inside the
        /// range may contain arbitrary data — <c>Insert</c> zeroes them before linking.
        /// Returns the (possibly coalesced) start address of the inserted range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Insert(TLinkAddress start, TLinkAddress length)
        {
            ClearRange(start, length);

            // Locate the predecessor (largest address < start) and successor.
            var predecessor = default(TLinkAddress);
            var successor = Head;
            while (successor != default && successor < start)
            {
                predecessor = successor;
                successor = GetLinkReference(successor).RightAsSource;
            }

            // Try to coalesce with predecessor.
            if (predecessor != default)
            {
                ref var predCell = ref GetLinkReference(predecessor);
                if (predecessor + predCell.Target == start)
                {
                    predCell.Target = predCell.Target + length;
                    // The old `start` cell is already cleared by ClearRange — no header to wipe.
                    start = predecessor;
                    length = predCell.Target;
                    // Now also try to coalesce with successor.
                    if (successor != default && start + length == successor)
                    {
                        ref var succCell = ref GetLinkReference(successor);
                        predCell.Target = predCell.Target + succCell.Target;
                        // Unlink successor.
                        predCell.RightAsSource = succCell.RightAsSource;
                        if (succCell.RightAsSource != default)
                        {
                            GetLinkReference(succCell.RightAsSource).LeftAsSource = predecessor;
                        }
                        ClearCell(successor);
                    }
                    return start;
                }
            }

            // Try to coalesce with successor only.
            if (successor != default && start + length == successor)
            {
                ref var succCell = ref GetLinkReference(successor);
                var newLength = length + succCell.Target;
                var nextOfSuccessor = succCell.RightAsSource;
                ClearCell(successor);

                ref var newHead = ref GetLinkReference(start);
                newHead.Source = _freeRangeMarker;
                newHead.Target = newLength;
                newHead.LeftAsSource = predecessor;
                newHead.RightAsSource = nextOfSuccessor;
                if (predecessor != default)
                {
                    GetLinkReference(predecessor).RightAsSource = start;
                }
                else
                {
                    Head = start;
                }
                if (nextOfSuccessor != default)
                {
                    GetLinkReference(nextOfSuccessor).LeftAsSource = start;
                }
                return start;
            }

            // No coalescing — just link in.
            ref var head = ref GetLinkReference(start);
            head.Source = _freeRangeMarker;
            head.Target = length;
            head.LeftAsSource = predecessor;
            head.RightAsSource = successor;

            if (predecessor != default)
            {
                GetLinkReference(predecessor).RightAsSource = start;
            }
            else
            {
                Head = start;
            }
            if (successor != default)
            {
                GetLinkReference(successor).LeftAsSource = start;
            }
            return start;
        }

        /// <summary>
        /// Detaches <paramref name="start"/> from the free-range list and clears the
        /// descriptor cell. Returns the length of the range that was removed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Detach(TLinkAddress start)
        {
            ref var cell = ref GetLinkReference(start);
            var length = cell.Target;
            var prev = cell.LeftAsSource;
            var next = cell.RightAsSource;
            if (prev != default)
            {
                GetLinkReference(prev).RightAsSource = next;
            }
            else
            {
                Head = next;
            }
            if (next != default)
            {
                GetLinkReference(next).LeftAsSource = prev;
            }
            ClearCell(start);
            return length;
        }

        /// <summary>
        /// Carves <paramref name="length"/> cells from the head of the range at
        /// <paramref name="start"/>, leaving the remainder as a smaller free range at
        /// <c>start + length</c>. The caller receives the original
        /// <paramref name="start"/> address. The carved cells are zeroed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress CarveFromFront(TLinkAddress start, TLinkAddress length)
        {
            ref var cell = ref GetLinkReference(start);
            var oldLength = cell.Target;
            if (oldLength == length)
            {
                Detach(start);
                return start;
            }
            var prev = cell.LeftAsSource;
            var next = cell.RightAsSource;
            ClearCell(start);
            var newStart = start + length;
            var newLength = oldLength - length;
            ref var newHead = ref GetLinkReference(newStart);
            newHead.Source = _freeRangeMarker;
            newHead.Target = newLength;
            newHead.LeftAsSource = prev;
            newHead.RightAsSource = next;
            if (prev != default)
            {
                GetLinkReference(prev).RightAsSource = newStart;
            }
            else
            {
                Head = newStart;
            }
            if (next != default)
            {
                GetLinkReference(next).LeftAsSource = newStart;
            }
            return start;
        }

        /// <summary>
        /// Carves <paramref name="length"/> cells from the back of the range at
        /// <paramref name="start"/>. Returns the address of the first carved cell.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress CarveFromBack(TLinkAddress start, TLinkAddress length)
        {
            ref var cell = ref GetLinkReference(start);
            var oldLength = cell.Target;
            if (oldLength == length)
            {
                Detach(start);
                return start;
            }
            cell.Target = oldLength - length;
            return start + (oldLength - length);
        }

        /// <summary>
        /// Removes the highest-address free range if it ends exactly at the high-water
        /// mark <paramref name="allocatedLinks"/>. Returns its length or <c>default</c>
        /// if no such range exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress TryDetachTail(TLinkAddress allocatedLinks)
        {
            var current = Head;
            var last = default(TLinkAddress);
            while (current != default)
            {
                last = current;
                current = GetLinkReference(current).RightAsSource;
            }
            if (last == default)
            {
                return default;
            }
            ref var cell = ref GetLinkReference(last);
            if (last + cell.Target - TLinkAddress.One == allocatedLinks)
            {
                var length = cell.Target;
                Detach(last);
                return length;
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearCell(TLinkAddress address)
        {
            ref var cell = ref GetLinkReference(address);
            cell.Source = default;
            cell.Target = default;
            cell.LeftAsSource = default;
            cell.RightAsSource = default;
            cell.SizeAsSource = default;
            cell.LeftAsTarget = default;
            cell.RightAsTarget = default;
            cell.SizeAsTarget = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearRange(TLinkAddress start, TLinkAddress length)
        {
            var startLong = long.CreateTruncating(start);
            var lengthLong = long.CreateTruncating(length);
            var ptr = _links + RawLink<TLinkAddress>.SizeInBytes * startLong;
            new Span<byte>(ptr, checked((int)(lengthLong * RawLink<TLinkAddress>.SizeInBytes))).Clear();
        }
    }
}
