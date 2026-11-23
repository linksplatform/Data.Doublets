using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Converters;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Converters
{
    /// <summary>
    /// <para>
    /// Represents a converter that converts doublet sequences back to byte arrays.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The type of link address.</para>
    /// <para></para>
    /// </typeparam>
    public class SequenceToByteArrayConverter<TLinkAddress> : IConverter<TLinkAddress, byte[]>
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly ILinks<TLinkAddress> _links;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="SequenceToByteArrayConverter{TLinkAddress}"/> class.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">
        /// <para>The links storage to use for reading sequences.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceToByteArrayConverter(ILinks<TLinkAddress> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
        }

        /// <summary>
        /// <para>
        /// Converts a doublet sequence back to a byte array.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="sequence">
        /// <para>The link address of the sequence to convert.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The reconstructed byte array.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Convert(TLinkAddress sequence)
        {
            if (EqualityComparer<TLinkAddress>.Default.Equals(sequence, _links.Constants.Null))
            {
                return Array.Empty<byte>();
            }

            if (!_links.Exists(sequence))
            {
                throw new ArgumentException($"Sequence with address {sequence} does not exist.", nameof(sequence));
            }

            var bytes = new List<byte>();
            var visited = new HashSet<TLinkAddress>();
            
            WalkSequence(sequence, bytes, visited);
            
            return bytes.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WalkSequence(TLinkAddress current, List<byte> bytes, HashSet<TLinkAddress> visited)
        {
            if (EqualityComparer<TLinkAddress>.Default.Equals(current, _links.Constants.Null) || 
                !_links.Exists(current) ||
                !visited.Add(current))
            {
                return;
            }

            var link = _links.GetLink(current);
            var source = _links.GetSource(link);
            var target = _links.GetTarget(link);

            // If source and target are the same (point link), it represents a single byte value
            if (EqualityComparer<TLinkAddress>.Default.Equals(source, target))
            {
                var byteValue = ulong.CreateTruncating(source);
                if (byteValue <= byte.MaxValue)
                {
                    bytes.Add((byte)byteValue);
                }
                return;
            }

            // First, recursively walk the source (which contains the previous sequence or byte)
            WalkSequence(source, bytes, visited);

            // Then, handle the target
            if (_links.Exists(target))
            {
                var targetLink = _links.GetLink(target);
                var targetSource = _links.GetSource(targetLink);
                var targetTarget = _links.GetTarget(targetLink);
                
                if (EqualityComparer<TLinkAddress>.Default.Equals(targetSource, targetTarget))
                {
                    // Target is a point link representing a byte value
                    var byteValue = ulong.CreateTruncating(targetSource);
                    if (byteValue <= byte.MaxValue)
                    {
                        bytes.Add((byte)byteValue);
                    }
                }
                else
                {
                    // Target is a sequence, recursively walk it
                    WalkSequence(target, bytes, visited);
                }
            }
            else
            {
                // Target is a direct byte value
                var byteValue = ulong.CreateTruncating(target);
                if (byteValue <= byte.MaxValue)
                {
                    bytes.Add((byte)byteValue);
                }
            }
        }
    }
}