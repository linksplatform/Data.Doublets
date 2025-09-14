using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences
{
    /// <summary>
    /// <para>
    /// Provides extension methods for cached sequence operations on ILinks.
    /// </para>
    /// <para>
    /// These extensions make it easy to use cached sequence reading functionality with any ILinks implementation.
    /// </para>
    /// </summary>
    public static class CachedSequenceExtensions
    {
        /// <summary>
        /// Creates a cached sequence walker decorator for the given links storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
        /// <param name="links">The links storage to decorate.</param>
        /// <param name="enableFileCache">Whether to enable file-based caching.</param>
        /// <param name="cacheDirectory">The directory for file cache. If null, uses temp directory.</param>
        /// <param name="fileCacheDuration">The duration to keep file cache. Default is 1 second.</param>
        /// <returns>A cached sequence walker decorator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CachedSequenceWalker<TLinkAddress> WithSequenceCache<TLinkAddress>(
            this ILinks<TLinkAddress> links,
            bool enableFileCache = true,
            string? cacheDirectory = null,
            TimeSpan? fileCacheDuration = null)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            return new CachedSequenceWalker<TLinkAddress>(links, enableFileCache, cacheDirectory, fileCacheDuration);
        }

        /// <summary>
        /// Gets a sequence as an array of elements using cached sequence walker.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <param name="sequenceAddress">The sequence address.</param>
        /// <returns>The sequence elements array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress[] GetCachedSequenceArray<TLinkAddress>(
            this ILinks<TLinkAddress> links, 
            TLinkAddress sequenceAddress)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links is CachedSequenceWalker<TLinkAddress> cachedWalker)
            {
                return cachedWalker.GetSequenceArray(sequenceAddress);
            }

            // If not already a cached walker, create a temporary one
            using var walker = links.WithSequenceCache();
            return walker.GetSequenceArray(sequenceAddress);
        }

        /// <summary>
        /// Gets a sequence as an array of elements asynchronously using cached sequence walker.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <param name="sequenceAddress">The sequence address.</param>
        /// <returns>A task that represents the asynchronous operation containing the sequence elements array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TLinkAddress[]> GetCachedSequenceArrayAsync<TLinkAddress>(
            this ILinks<TLinkAddress> links, 
            TLinkAddress sequenceAddress)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links is CachedSequenceWalker<TLinkAddress> cachedWalker)
            {
                return await cachedWalker.GetSequenceArrayAsync(sequenceAddress).ConfigureAwait(false);
            }

            // If not already a cached walker, create a temporary one
            using var walker = links.WithSequenceCache();
            return await walker.GetSequenceArrayAsync(sequenceAddress).ConfigureAwait(false);
        }

        /// <summary>
        /// Tries to get a cached sequence address for the given elements.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <param name="elements">The sequence elements.</param>
        /// <param name="address">The found address, if any.</param>
        /// <returns>True if a cached address was found, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetCachedSequenceAddress<TLinkAddress>(
            this ILinks<TLinkAddress> links,
            TLinkAddress[] elements,
            out TLinkAddress address)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            address = default;
            
            if (links is CachedSequenceWalker<TLinkAddress> cachedWalker)
            {
                return cachedWalker.TryGetCachedSequenceAddress(elements, out address);
            }

            // If not a cached walker, we can't get cached address
            return false;
        }

        /// <summary>
        /// Invalidates cached data for a sequence.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <param name="sequenceAddress">The sequence address to invalidate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvalidateCachedSequence<TLinkAddress>(
            this ILinks<TLinkAddress> links,
            TLinkAddress sequenceAddress)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links is CachedSequenceWalker<TLinkAddress> cachedWalker)
            {
                cachedWalker.InvalidateSequence(sequenceAddress);
            }
        }

        /// <summary>
        /// Clears all cached sequence data.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
        /// <param name="links">The links storage.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearSequenceCache<TLinkAddress>(this ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links is CachedSequenceWalker<TLinkAddress> cachedWalker)
            {
                cachedWalker.ClearCache();
            }
        }

        /// <summary>
        /// Gets cache statistics for sequence operations.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <returns>A tuple containing hit count, miss count, and hit ratio, or null if not a cached walker.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (long hitCount, long missCount, double hitRatio)? GetSequenceCacheStatistics<TLinkAddress>(
            this ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links is CachedSequenceWalker<TLinkAddress> cachedWalker)
            {
                return cachedWalker.GetCacheStatistics();
            }

            return null;
        }
    }
}