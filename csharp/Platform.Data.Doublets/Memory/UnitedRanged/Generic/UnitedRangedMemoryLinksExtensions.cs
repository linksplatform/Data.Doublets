using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.UnitedRanged.Generic
{
    public static class UnitedRangedMemoryLinksExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress AllocateRawLinkSequence<TLinkAddress>(this UnitedRangedMemoryLinks<TLinkAddress> links, long payloadLengthInBytes)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var cells = RawLinkSequenceMethods<TLinkAddress>.ComputeCellsForPayload(payloadLengthInBytes);
            var cellCount = TLinkAddress.CreateTruncating(cells);
            var start = links.AllocateRange(cellCount);
            links.ClearCells(start, cellCount);
            links.RawLinkSequences.WriteDescriptor(start, payloadLengthInBytes);
            return start;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress AllocateRawLinkSequence<TLinkAddress>(this UnitedRangedMemoryLinks<TLinkAddress> links, ReadOnlySpan<byte> payload)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var start = links.AllocateRawLinkSequence(payload.Length);
            links.WriteRawLinkSequence(start, payload);
            return start;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawLinkSequence<TLinkAddress>(this UnitedRangedMemoryLinks<TLinkAddress> links, TLinkAddress start, ReadOnlySpan<byte> payload)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            RawLinkSequenceMethods<TLinkAddress>.ValidatePayloadLength(payload.Length, nameof(payload));
            if (!links.RawLinkSequences.IsRawLinkSequence(start))
            {
                throw new ArgumentException("Address is not a raw link sequence head.", nameof(start));
            }
            var expectedLength = links.RawLinkSequences.GetLengthInBytes(start);
            if (expectedLength != payload.Length)
            {
                throw new ArgumentException("Payload length must match the allocated raw link sequence length.", nameof(payload));
            }
            links.RawLinkSequences.Write(start, payload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadRawLinkSequence<TLinkAddress>(this UnitedRangedMemoryLinks<TLinkAddress> links, TLinkAddress start, Span<byte> destination)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            if (!links.RawLinkSequences.IsRawLinkSequence(start))
            {
                throw new ArgumentException("Address is not a raw link sequence head.", nameof(start));
            }
            links.RawLinkSequences.Read(start, destination);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeallocateRawLinkSequence<TLinkAddress>(this UnitedRangedMemoryLinks<TLinkAddress> links, TLinkAddress start)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            if (!links.RawLinkSequences.IsRawLinkSequence(start))
            {
                return;
            }
            var cells = links.RawLinkSequences.GetCellCount(start);
            links.DeallocateRange(start, TLinkAddress.CreateTruncating(cells));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRawLinkSequence<TLinkAddress>(this UnitedRangedMemoryLinks<TLinkAddress> links, TLinkAddress address)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
            => links.RawLinkSequences.IsRawLinkSequence(address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetRawLinkSequenceLengthInBytes<TLinkAddress>(this UnitedRangedMemoryLinks<TLinkAddress> links, TLinkAddress address)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
            => links.RawLinkSequences.GetLengthInBytes(address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetRawLinkSequenceCellCount<TLinkAddress>(this UnitedRangedMemoryLinks<TLinkAddress> links, TLinkAddress address)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
            => links.RawLinkSequences.GetCellCount(address);
    }
}
