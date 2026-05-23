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
    /// Encodes and decodes raw binary blobs that live inside the link cell address
    /// space. A blob spans one or more consecutive cells.
    /// </para>
    /// <para>
    /// The first cell stores a small descriptor:
    /// </para>
    /// <list type="bullet">
    /// <item><c>Source</c> = <c>RawMarker</c></item>
    /// <item><c>Target</c> = blob length in bytes (must be a multiple of
    /// <c>sizeof(TLinkAddress)</c>)</item>
    /// </list>
    /// <para>
    /// The remaining six <c>TLinkAddress</c> words of the header cell carry the first
    /// chunk of payload. Each subsequent cell stores eight more words of payload. There
    /// are no continuation markers; iteration is driven by the head cell's
    /// <c>Target</c>, and intermediate cell indices are not valid link handles.
    /// </para>
    /// </summary>
    public unsafe class RawBinaryMethods<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        public static readonly long WordSizeInBytes = System.Runtime.CompilerServices.Unsafe.SizeOf<TLinkAddress>();
        public static readonly long CellSizeInBytes = RawLink<TLinkAddress>.SizeInBytes;
        public static readonly long WordsPerCell = CellSizeInBytes / WordSizeInBytes;
        public static readonly long HeaderWordsReserved = 2;
        public static readonly long PayloadBytesInHeaderCell = (WordsPerCell - HeaderWordsReserved) * WordSizeInBytes;
        public static readonly long PayloadBytesInContinuationCell = CellSizeInBytes;

        private readonly byte* _links;
        private readonly TLinkAddress _rawMarker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawBinaryMethods(byte* links, TLinkAddress rawMarker)
        {
            _links = links;
            _rawMarker = rawMarker;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref RawLink<TLinkAddress> GetLinkReference(TLinkAddress address) => ref AsRef<RawLink<TLinkAddress>>(_links + CellSizeInBytes * long.CreateTruncating(address));

        /// <summary>
        /// Number of cells required to hold a blob of <paramref name="byteLength"/>
        /// bytes. <paramref name="byteLength"/> must be a non-negative multiple of
        /// <see cref="WordSizeInBytes"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ComputeCellsForBlob(long byteLength)
        {
            if (byteLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteLength));
            }
            if ((byteLength % WordSizeInBytes) != 0)
            {
                throw new ArgumentException("Blob length must be a multiple of sizeof(TLinkAddress).", nameof(byteLength));
            }
            if (byteLength <= PayloadBytesInHeaderCell)
            {
                return 1;
            }
            var overflow = byteLength - PayloadBytesInHeaderCell;
            return 1 + (overflow + PayloadBytesInContinuationCell - 1) / PayloadBytesInContinuationCell;
        }

        /// <summary>
        /// Returns true if the cell at <paramref name="address"/> is the head of a raw
        /// binary blob.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRawBinary(TLinkAddress address)
        {
            if (address == default)
            {
                return false;
            }
            return GetLinkReference(address).Source == _rawMarker;
        }

        /// <summary>
        /// Returns the blob's length in bytes (the value stored in the head cell's
        /// <c>Target</c> field).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetLengthInBytes(TLinkAddress address) => long.CreateTruncating(GetLinkReference(address).Target);

        /// <summary>
        /// Returns the number of cells the blob at <paramref name="address"/> occupies.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetCellCount(TLinkAddress address) => ComputeCellsForBlob(GetLengthInBytes(address));

        /// <summary>
        /// Writes the blob descriptor and payload into a previously-allocated range
        /// starting at <paramref name="start"/>. The destination range must be large
        /// enough to fit <c>ComputeCellsForBlob(payload.Length)</c> cells.
        /// </summary>
        public void Write(TLinkAddress start, ReadOnlySpan<byte> payload)
        {
            if ((payload.Length % WordSizeInBytes) != 0)
            {
                throw new ArgumentException("Blob length must be a multiple of sizeof(TLinkAddress).", nameof(payload));
            }
            ref var head = ref GetLinkReference(start);
            head.Source = _rawMarker;
            head.Target = TLinkAddress.CreateTruncating(payload.Length);

            // Copy first chunk into the header cell, after the 2 reserved descriptor words.
            var headPtr = (byte*)AsPointer(ref head) + (HeaderWordsReserved * WordSizeInBytes);
            var firstChunk = (int)Math.Min(payload.Length, PayloadBytesInHeaderCell);
            if (firstChunk > 0)
            {
                payload.Slice(0, firstChunk).CopyTo(new Span<byte>(headPtr, firstChunk));
            }
            // Zero the unused tail of the header cell's payload area.
            if (firstChunk < PayloadBytesInHeaderCell)
            {
                new Span<byte>(headPtr + firstChunk, (int)(PayloadBytesInHeaderCell - firstChunk)).Clear();
            }
            // Copy remaining chunks into continuation cells.
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
        /// Reads the payload of the blob at <paramref name="start"/> into
        /// <paramref name="destination"/>. <paramref name="destination"/> must be at
        /// least as long as the blob.
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

        /// <summary>
        /// Zeroes the entire blob range so that it looks like a fresh, uninitialised
        /// span of cells ready to be returned to the allocator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(TLinkAddress start)
        {
            var cells = GetCellCount(start);
            var dst = _links + CellSizeInBytes * long.CreateTruncating(start);
            new Span<byte>(dst, checked((int)(cells * CellSizeInBytes))).Clear();
        }
    }
}
