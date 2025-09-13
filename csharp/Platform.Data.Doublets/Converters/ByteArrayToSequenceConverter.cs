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
    /// Represents a converter that converts byte arrays to doublet sequences.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The type of link address.</para>
    /// <para></para>
    /// </typeparam>
    public class ByteArrayToSequenceConverter<TLinkAddress> : IConverter<byte[], TLinkAddress>
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly ILinks<TLinkAddress> _links;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="ByteArrayToSequenceConverter{TLinkAddress}"/> class.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">
        /// <para>The links storage to use for creating sequences.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteArrayToSequenceConverter(ILinks<TLinkAddress> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
        }

        /// <summary>
        /// <para>
        /// Converts a byte array to a doublet sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">
        /// <para>The byte array to convert.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link address of the created sequence.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Convert(byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Length == 0)
            {
                return _links.Constants.Null;
            }

            if (source.Length == 1)
            {
                // For single byte, create a point link with the byte value
                var byteValue = TLinkAddress.CreateTruncating(source[0]);
                return _links.GetOrCreate(byteValue, byteValue);
            }

            // For multiple bytes, create a chain of links representing the sequence
            var firstByteValue = TLinkAddress.CreateTruncating(source[0]);
            var firstByteLink = _links.GetOrCreate(firstByteValue, firstByteValue);
            
            var currentSequence = firstByteLink;
            
            for (int i = 1; i < source.Length; i++)
            {
                var byteValue = TLinkAddress.CreateTruncating(source[i]);
                var byteLink = _links.GetOrCreate(byteValue, byteValue);
                
                // Create a new link that connects the current sequence with the next byte
                currentSequence = _links.GetOrCreate(currentSequence, byteLink);
            }
            
            return currentSequence;
        }
    }
}