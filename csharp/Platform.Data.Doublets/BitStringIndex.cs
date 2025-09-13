using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents a bit string index implementation for fast sequence searching.
    /// </para>
    /// <para></para>
    /// </summary>
    public class BitStringIndex<TLinkAddress> : IBitStringIndex<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private readonly Dictionary<TLinkAddress, BitArray> _bitStrings;
        private readonly object _lock = new();
        private int _maxCapacity;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="BitStringIndex{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="initialCapacity">
        /// <para>The initial capacity for bit strings.</para>
        /// <para></para>
        /// </param>
        public BitStringIndex(int initialCapacity = 1024)
        {
            _bitStrings = new Dictionary<TLinkAddress, BitArray>();
            _maxCapacity = initialCapacity;
        }

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
        public BitArray? GetBitString(TLinkAddress linkAddress)
        {
            lock (_lock)
            {
                return _bitStrings.TryGetValue(linkAddress, out var bitString) ? bitString : null;
            }
        }

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
        public void SetBitString(TLinkAddress linkAddress, BitArray bitString)
        {
            lock (_lock)
            {
                _bitStrings[linkAddress] = bitString;
                if (bitString.Length > _maxCapacity)
                {
                    _maxCapacity = bitString.Length;
                    ResizeAllBitStrings(_maxCapacity);
                }
            }
        }

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
        public void UpdateBit(TLinkAddress linkAddress, int position, bool value)
        {
            lock (_lock)
            {
                if (!_bitStrings.TryGetValue(linkAddress, out var bitString))
                {
                    var newSize = Math.Max(position + 1, _maxCapacity);
                    bitString = new BitArray(newSize);
                    _bitStrings[linkAddress] = bitString;
                    if (newSize > _maxCapacity)
                    {
                        _maxCapacity = newSize;
                        ResizeAllBitStrings(_maxCapacity);
                    }
                }
                else if (position >= bitString.Length)
                {
                    var newSize = Math.Max(position + 1, _maxCapacity);
                    var newBitString = new BitArray(newSize);
                    for (int i = 0; i < bitString.Length; i++)
                    {
                        newBitString[i] = bitString[i];
                    }
                    bitString = newBitString;
                    _bitStrings[linkAddress] = bitString;
                    if (newSize > _maxCapacity)
                    {
                        _maxCapacity = newSize;
                        ResizeAllBitStrings(_maxCapacity);
                    }
                }
                bitString[position] = value;
            }
        }

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
        public BitArray? IntersectBitStrings(IEnumerable<TLinkAddress> linkAddresses)
        {
            lock (_lock)
            {
                BitArray? result = null;
                foreach (var linkAddress in linkAddresses)
                {
                    var bitString = GetBitString(linkAddress);
                    if (bitString == null)
                    {
                        // If any fragment has no bitstring, return empty result
                        return new BitArray(_maxCapacity);
                    }
                    
                    if (result == null)
                    {
                        result = new BitArray(bitString);
                    }
                    else
                    {
                        result = result.And(bitString);
                    }
                }
                return result;
            }
        }

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
        public int GetFrequency(TLinkAddress linkAddress)
        {
            var bitString = GetBitString(linkAddress);
            if (bitString == null) return 0;
            
            int count = 0;
            for (int i = 0; i < bitString.Length; i++)
            {
                if (bitString[i]) count++;
            }
            return count;
        }

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
        public IEnumerable<TLinkAddress> FindLinksContainingAllFragments(IEnumerable<TLinkAddress> fragments)
        {
            var result = IntersectBitStrings(fragments);
            if (result == null) yield break;
            
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i])
                {
                    yield return TLinkAddress.CreateTruncating(i);
                }
            }
        }

        private void ResizeAllBitStrings(int newSize)
        {
            var keys = _bitStrings.Keys.ToList();
            foreach (var key in keys)
            {
                var oldBitString = _bitStrings[key];
                if (oldBitString.Length < newSize)
                {
                    var newBitString = new BitArray(newSize);
                    for (int i = 0; i < oldBitString.Length; i++)
                    {
                        newBitString[i] = oldBitString[i];
                    }
                    _bitStrings[key] = newBitString;
                }
            }
        }
    }
}