using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Defines the bit string index interface for fast sequence searching.
    /// </para>
    /// <para></para>
    /// </summary>
    public interface IBitStringIndex<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the bit string for the specified link address.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">
        /// <para>The link address.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bit array representing links containing this link address.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        BitArray? GetBitString(TLinkAddress linkAddress);

        /// <summary>
        /// <para>
        /// Sets or updates the bit string for the specified link address.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">
        /// <para>The link address.</para>
        /// <para></para>
        /// </param>
        /// <param name="bitString">
        /// <para>The bit string to associate with the link address.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetBitString(TLinkAddress linkAddress, BitArray bitString);

        /// <summary>
        /// <para>
        /// Updates the bit string for the specified link address by setting the bit at the specified position.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">
        /// <para>The link address.</para>
        /// <para></para>
        /// </param>
        /// <param name="position">
        /// <para>The position of the bit to set.</para>
        /// <para></para>
        /// </param>
        /// <param name="value">
        /// <para>The value to set at the specified position.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateBit(TLinkAddress linkAddress, int position, bool value);

        /// <summary>
        /// <para>
        /// Intersects multiple bit strings to find links that contain all specified fragments.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddresses">
        /// <para>The link addresses to intersect.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The intersected bit array.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        BitArray? IntersectBitStrings(IEnumerable<TLinkAddress> linkAddresses);

        /// <summary>
        /// <para>
        /// Gets the frequency count of the specified link address across all sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">
        /// <para>The link address.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The frequency count.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetFrequency(TLinkAddress linkAddress);

        /// <summary>
        /// <para>
        /// Finds the set of links that contain all specified fragments using bit string intersection.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="fragments">
        /// <para>The fragments to search for.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The enumerable of link addresses that contain all fragments.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerable<TLinkAddress> FindLinksContainingAllFragments(IEnumerable<TLinkAddress> fragments);
    }
}