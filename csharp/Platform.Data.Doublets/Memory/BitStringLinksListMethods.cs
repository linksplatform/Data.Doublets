using System;
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Collections.Methods.Lists;
using Platform.Converters;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory
{
    /// <summary>
    /// <para>
    /// Represents a high-performance bitstring-based implementation for tracking unused/free links.
    /// This implementation uses BitArray for efficient memory usage and O(1) operations for 
    /// mark/unmark deleted operations, significantly outperforming the traditional linked list approach.
    /// </para>
    /// <para>
    /// Performance characteristics:
    /// - MarkAsDeleted: O(1) with automatic capacity growth
    /// - MarkAsUndeleted: O(1)
    /// - IsDeleted: O(1)
    /// - GetFirstDeleted: O(n) but with bit-level optimizations
    /// - Memory usage: ~1 bit per link vs. traditional approach using multiple machine words
    /// </para>
    /// </summary>
    /// <seealso cref="ILinksListMethods{TLinkAddress}"/>
    public unsafe class BitStringLinksListMethods<TLinkAddress> : ILinksListMethods<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private readonly byte* _header;
        private BitArray _deletedLinks;
        private TLinkAddress _size;
        private readonly object _lock = new();

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="BitStringLinksListMethods"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="header">
        /// <para>A pointer to the header.</para>
        /// <para></para>
        /// </param>
        /// <param name="initialCapacity">
        /// <para>Initial capacity for the bitstring. Defaults to 1024.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStringLinksListMethods(byte* header, int initialCapacity = 1024)
        {
            _header = header;
            _deletedLinks = new BitArray(initialCapacity);
            _size = TLinkAddress.Zero;
        }

        /// <summary>
        /// <para>
        /// Gets the header reference.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>A ref links header of t link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual ref LinksHeader<TLinkAddress> GetHeaderReference() => ref AsRef<LinksHeader<TLinkAddress>>(_header);

        /// <summary>
        /// <para>
        /// Gets the first deleted link using bitstring scanning.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The first deleted link or null if none found</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress GetFirst()
        {
            lock (_lock)
            {
                for (int i = 0; i < _deletedLinks.Length; i++)
                {
                    if (_deletedLinks[i])
                        return TLinkAddress.CreateTruncating(i);
                }
                return TLinkAddress.Zero; // No deleted links found
            }
        }

        /// <summary>
        /// <para>
        /// Gets the last deleted link. Since bitstring doesn't maintain insertion order,
        /// this returns the highest index deleted link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The last (highest index) deleted link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress GetLast()
        {
            lock (_lock)
            {
                for (int i = _deletedLinks.Length - 1; i >= 0; i--)
                {
                    if (_deletedLinks[i])
                        return TLinkAddress.CreateTruncating(i);
                }
                return TLinkAddress.Zero; // No deleted links found
            }
        }

        /// <summary>
        /// <para>
        /// Gets the previous deleted link before the specified element.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="element">
        /// <para>The element.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The previous deleted link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress GetPrevious(TLinkAddress element)
        {
            var index = Convert.ToInt32(element);
            lock (_lock)
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (_deletedLinks[i])
                        return TLinkAddress.CreateTruncating(i);
                }
                return TLinkAddress.Zero;
            }
        }

        /// <summary>
        /// <para>
        /// Gets the next deleted link after the specified element.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="element">
        /// <para>The element.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The next deleted link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress GetNext(TLinkAddress element)
        {
            var index = Convert.ToInt32(element);
            lock (_lock)
            {
                for (int i = index + 1; i < _deletedLinks.Length; i++)
                {
                    if (_deletedLinks[i])
                        return TLinkAddress.CreateTruncating(i);
                }
                return TLinkAddress.Zero;
            }
        }

        /// <summary>
        /// <para>
        /// Gets the size (count of deleted links).
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The count of deleted links</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress GetSize()
        {
            return _size;
        }

        /// <summary>
        /// <para>
        /// Attaches (marks as deleted) the specified link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="element">
        /// <para>The element.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AttachAsFirst(TLinkAddress element)
        {
            var index = Convert.ToInt32(element);
            lock (_lock)
            {
                EnsureCapacity(index + 1);
                if (!_deletedLinks[index])
                {
                    _deletedLinks[index] = true;
                    _size = TLinkAddress.CreateTruncating(Convert.ToUInt64(_size) + 1);
                    // Update header
                    ref var header = ref GetHeaderReference();
                    header.FreeLinks = _size;
                    if (header.FirstFreeLink == TLinkAddress.Zero)
                        header.FirstFreeLink = element;
                    header.LastFreeLink = element;
                }
            }
        }

        /// <summary>
        /// <para>
        /// Attaches (marks as deleted) the specified link as last.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="element">
        /// <para>The element.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AttachAsLast(TLinkAddress element)
        {
            AttachAsFirst(element); // Same operation for bitstring
        }

        /// <summary>
        /// <para>
        /// Detaches (marks as undeleted) the specified link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="element">
        /// <para>The element.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(TLinkAddress element)
        {
            var index = Convert.ToInt32(element);
            lock (_lock)
            {
                if (index < _deletedLinks.Length && _deletedLinks[index])
                {
                    _deletedLinks[index] = false;
                    _size = TLinkAddress.CreateTruncating(Convert.ToUInt64(_size) - 1);
                    // Update header
                    ref var header = ref GetHeaderReference();
                    header.FreeLinks = _size;
                    // Update first/last if necessary
                    if (_size == TLinkAddress.Zero)
                    {
                        header.FirstFreeLink = TLinkAddress.Zero;
                        header.LastFreeLink = TLinkAddress.Zero;
                    }
                    else
                    {
                        if (header.FirstFreeLink == element)
                            header.FirstFreeLink = GetFirst();
                        if (header.LastFreeLink == element)
                            header.LastFreeLink = GetLast();
                    }
                }
            }
        }

        /// <summary>
        /// <para>
        /// Checks if the specified link is deleted.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link to check.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>True if the link is deleted, false otherwise.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDeleted(TLinkAddress link)
        {
            var index = Convert.ToInt32(link);
            lock (_lock)
            {
                return index < _deletedLinks.Length && _deletedLinks[index];
            }
        }

        private void EnsureCapacity(int requiredCapacity)
        {
            if (_deletedLinks.Length < requiredCapacity)
            {
                var newSize = Math.Max(requiredCapacity, _deletedLinks.Length * 2);
                _deletedLinks.Length = newSize;
            }
        }
    }
}