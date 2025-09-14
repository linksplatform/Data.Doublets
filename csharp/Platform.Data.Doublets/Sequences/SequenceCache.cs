using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences
{
    /// <summary>
    /// <para>
    /// Represents an in-memory cache for sequence address-to-array mappings.
    /// </para>
    /// <para>
    /// The link is two subsequences, we can cache the relationship between each array of subsequence's elements and its address.
    /// So if need to convert the address to array of elements second time we can do it faster, because of cached arrays.
    /// </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class SequenceCache<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly ConcurrentDictionary<TLinkAddress, TLinkAddress[]> _addressToSequenceCache;
        private readonly ConcurrentDictionary<string, TLinkAddress> _sequenceHashToAddressCache;
        private readonly object _lock = new();
        private long _hitCount;
        private long _missCount;

        /// <summary>
        /// Gets the number of cache hits.
        /// </summary>
        public long HitCount => _hitCount;

        /// <summary>
        /// Gets the number of cache misses.
        /// </summary>
        public long MissCount => _missCount;

        /// <summary>
        /// Gets the cache hit ratio.
        /// </summary>
        public double HitRatio => _hitCount + _missCount > 0 ? (double)_hitCount / (_hitCount + _missCount) : 0;

        /// <summary>
        /// Gets the current size of the cache.
        /// </summary>
        public int Size => _addressToSequenceCache.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceCache{TLinkAddress}"/> class.
        /// </summary>
        public SequenceCache()
        {
            _addressToSequenceCache = new ConcurrentDictionary<TLinkAddress, TLinkAddress[]>();
            _sequenceHashToAddressCache = new ConcurrentDictionary<string, TLinkAddress>();
        }

        /// <summary>
        /// Tries to get a cached sequence by its address.
        /// </summary>
        /// <param name="address">The sequence address.</param>
        /// <param name="sequence">The cached sequence if found.</param>
        /// <returns>True if the sequence was found in cache, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSequence(TLinkAddress address, out TLinkAddress[]? sequence)
        {
            if (_addressToSequenceCache.TryGetValue(address, out var cachedSequence))
            {
                sequence = cachedSequence;
                Interlocked.Increment(ref _hitCount);
                return true;
            }

            sequence = null;
            Interlocked.Increment(ref _missCount);
            return false;
        }

        /// <summary>
        /// Tries to get a cached address by sequence elements hash.
        /// </summary>
        /// <param name="sequence">The sequence elements.</param>
        /// <param name="address">The cached address if found.</param>
        /// <returns>True if the address was found in cache, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetAddress(TLinkAddress[] sequence, out TLinkAddress address)
        {
            var hash = ComputeSequenceHash(sequence);
            if (_sequenceHashToAddressCache.TryGetValue(hash, out address))
            {
                Interlocked.Increment(ref _hitCount);
                return true;
            }

            address = default;
            Interlocked.Increment(ref _missCount);
            return false;
        }

        /// <summary>
        /// Caches a sequence and its address.
        /// </summary>
        /// <param name="address">The sequence address.</param>
        /// <param name="sequence">The sequence elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CacheSequence(TLinkAddress address, TLinkAddress[] sequence)
        {
            if (sequence == null || sequence.Length == 0)
            {
                return;
            }

            var sequenceCopy = new TLinkAddress[sequence.Length];
            Array.Copy(sequence, sequenceCopy, sequence.Length);
            
            var hash = ComputeSequenceHash(sequenceCopy);
            
            _addressToSequenceCache.TryAdd(address, sequenceCopy);
            _sequenceHashToAddressCache.TryAdd(hash, address);
        }

        /// <summary>
        /// Removes a sequence from cache.
        /// </summary>
        /// <param name="address">The sequence address to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSequence(TLinkAddress address)
        {
            if (_addressToSequenceCache.TryRemove(address, out var sequence))
            {
                var hash = ComputeSequenceHash(sequence);
                _sequenceHashToAddressCache.TryRemove(hash, out _);
            }
        }

        /// <summary>
        /// Clears all cached sequences.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _addressToSequenceCache.Clear();
            _sequenceHashToAddressCache.Clear();
            
            lock (_lock)
            {
                _hitCount = 0;
                _missCount = 0;
            }
        }

        /// <summary>
        /// Computes a hash for a sequence of elements.
        /// </summary>
        /// <param name="sequence">The sequence elements.</param>
        /// <returns>The hash string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ComputeSequenceHash(TLinkAddress[] sequence)
        {
            var hash = new HashCode();
            foreach (var element in sequence)
            {
                hash.Add(element);
            }
            return hash.ToHashCode().ToString();
        }
    }
}