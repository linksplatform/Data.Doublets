using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Converters
{
    /// <summary>
    /// <para>
    /// Provides extension methods for ILinks to work with byte array converters.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class ILinksConverterExtensions
    {
        /// <summary>
        /// <para>
        /// Converts a byte array to a doublet sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The type of link address.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="bytes">
        /// <para>The byte array to convert.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link address of the created sequence.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress CreateSequenceFromByteArray<TLinkAddress>(this ILinks<TLinkAddress> links, byte[] bytes)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var converter = new ByteArrayToSequenceConverter<TLinkAddress>(links);
            return converter.Convert(bytes);
        }

        /// <summary>
        /// <para>
        /// Converts a doublet sequence back to a byte array.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The type of link address.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="sequence">
        /// <para>The link address of the sequence to convert.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The reconstructed byte array.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ConvertSequenceToByteArray<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress sequence)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var converter = new SequenceToByteArrayConverter<TLinkAddress>(links);
            return converter.Convert(sequence);
        }
    }
}