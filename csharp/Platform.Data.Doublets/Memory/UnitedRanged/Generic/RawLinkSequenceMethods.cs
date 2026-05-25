using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Data.Doublets.Memory.United;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.UnitedRanged.Generic
{
    /// <summary>
    /// Encodes and decodes raw link sequences that live inside the link cell address
    /// space. A sequence can be used as an opaque byte payload, but its storage remains
    /// a contiguous range of regular <see cref="RawLink{TLinkAddress}"/> cells.
    /// </summary>
    public unsafe class RawLinkSequenceMethods<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private const long HeaderWordsReserved = 2;
        private static readonly long WordSizeInBytes = System.Runtime.CompilerServices.Unsafe.SizeOf<TLinkAddress>();
        private static readonly long CellSizeInBytes = RawLink<TLinkAddress>.SizeInBytes;
        private static readonly long PayloadBytesInHeaderCell = RawLink<TLinkAddress>.SizeInBytes - HeaderWordsReserved * System.Runtime.CompilerServices.Unsafe.SizeOf<TLinkAddress>();
        private static readonly long PayloadBytesInContinuationCell = RawLink<TLinkAddress>.SizeInBytes;

        private readonly byte* _links;
        private readonly TLinkAddress _sequenceMarker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawLinkSequenceMethods(byte* links, TLinkAddress sequenceMarker)
        {
            _links = links;
            _sequenceMarker = sequenceMarker;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref RawLink<TLinkAddress> GetLinkReference(TLinkAddress address) => ref AsRef<RawLink<TLinkAddress>>(_links + CellSizeInBytes * long.CreateTruncating(address));

        /// <summary>
        /// Number of cells required to hold <paramref name="payloadLengthInBytes"/>
        /// bytes. The length must be a non-negative multiple of
        /// <c>sizeof(TLinkAddress)</c>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ComputeCellsForPayload(long payloadLengthInBytes)
        {
            ValidatePayloadLength(payloadLengthInBytes, nameof(payloadLengthInBytes));
            if (payloadLengthInBytes <= PayloadBytesInHeaderCell)
            {
                return 1;
            }
            var overflow = payloadLengthInBytes - PayloadBytesInHeaderCell;
            return 1 + (overflow + PayloadBytesInContinuationCell - 1) / PayloadBytesInContinuationCell;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidatePayloadLength(long payloadLengthInBytes, string argumentName)
        {
            if (payloadLengthInBytes < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
            if ((payloadLengthInBytes % WordSizeInBytes) != 0)
            {
                throw new ArgumentException("Raw link sequence length must be a multiple of sizeof(TLinkAddress).", argumentName);
            }
        }

        /// <summary>
        /// Returns true if the cell at <paramref name="address"/> is the head of a raw
        /// link sequence.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRawLinkSequence(TLinkAddress address)
        {
            if (address == default)
            {
                return false;
            }
            return GetLinkReference(address).Source == _sequenceMarker;
        }

        /// <summary>
        /// Returns the sequence's payload length in bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetLengthInBytes(TLinkAddress address) => long.CreateTruncating(GetLinkReference(address).Target);

        /// <summary>
        /// Returns the number of cells the sequence at <paramref name="address"/> occupies.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetCellCount(TLinkAddress address) => ComputeCellsForPayload(GetLengthInBytes(address));

        /// <summary>
        /// Writes only the marker and length descriptor into the sequence head.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDescriptor(TLinkAddress start, long payloadLengthInBytes)
        {
            ValidatePayloadLength(payloadLengthInBytes, nameof(payloadLengthInBytes));
            ref var head = ref GetLinkReference(start);
            head.Source = _sequenceMarker;
            head.Target = TLinkAddress.CreateTruncating(payloadLengthInBytes);
        }

        /// <summary>
        /// Writes the descriptor and payload into a previously allocated range starting
        /// at <paramref name="start"/>.
        /// </summary>
        public void Write(TLinkAddress start, ReadOnlySpan<byte> payload)
        {
            ValidatePayloadLength(payload.Length, nameof(payload));
            ref var head = ref GetLinkReference(start);
            head.Source = _sequenceMarker;
            head.Target = TLinkAddress.CreateTruncating(payload.Length);

            var headPtr = (byte*)AsPointer(ref head) + (HeaderWordsReserved * WordSizeInBytes);
            var firstChunk = (int)Math.Min(payload.Length, PayloadBytesInHeaderCell);
            if (firstChunk > 0)
            {
                payload.Slice(0, firstChunk).CopyTo(new Span<byte>(headPtr, firstChunk));
            }
            if (firstChunk < PayloadBytesInHeaderCell)
            {
                new Span<byte>(headPtr + firstChunk, (int)(PayloadBytesInHeaderCell - firstChunk)).Clear();
            }

            var remaining = payload.Length - firstChunk;
            var offset = firstChunk;
            var continuationIndex = long.CreateTruncating(start) + 1;
            while (remaining > 0)
            {
                var chunk = (int)Math.Min(remaining, PayloadBytesInContinuationCell);
                var dst = _links + (CellSizeInBytes * continuationIndex);
                payload.Slice(offset, chunk).CopyTo(new Span<byte>(dst, chunk));
                if (chunk < PayloadBytesInContinuationCell)
                {
                    new Span<byte>(dst + chunk, (int)(PayloadBytesInContinuationCell - chunk)).Clear();
                }
                offset += chunk;
                remaining -= chunk;
                continuationIndex++;
            }
        }

        /// <summary>
        /// Reads the payload of the sequence at <paramref name="start"/> into
        /// <paramref name="destination"/>.
        /// </summary>
        public void Read(TLinkAddress start, Span<byte> destination)
        {
            ref var head = ref GetLinkReference(start);
            var byteLength = long.CreateTruncating(head.Target);
            if (destination.Length < byteLength)
            {
                throw new ArgumentException("Destination buffer is too small.", nameof(destination));
            }
            var headPtr = (byte*)AsPointer(ref head) + (HeaderWordsReserved * WordSizeInBytes);
            var firstChunk = (int)Math.Min(byteLength, PayloadBytesInHeaderCell);
            if (firstChunk > 0)
            {
                new ReadOnlySpan<byte>(headPtr, firstChunk).CopyTo(destination.Slice(0, firstChunk));
            }
            var remaining = byteLength - firstChunk;
            var offset = firstChunk;
            var continuationIndex = long.CreateTruncating(start) + 1;
            while (remaining > 0)
            {
                var chunk = (int)Math.Min(remaining, PayloadBytesInContinuationCell);
                var src = _links + (CellSizeInBytes * continuationIndex);
                new ReadOnlySpan<byte>(src, chunk).CopyTo(destination.Slice(offset, chunk));
                offset += chunk;
                remaining -= chunk;
                continuationIndex++;
            }
        }
    }
}
