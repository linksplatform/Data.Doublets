using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Platform.Collections.Lists;
using Platform.Data.Doublets.Decorators;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences
{
    /// <summary>
    /// <para>
    /// Represents a cached sequence walker/reader that optimizes sequence operations through caching.
    /// </para>
    /// <para>
    /// The link is two subsequences, we can cache the relationship between each array of subsequence's elements and its address.
    /// So if need to convert the address to array of elements second time we can do it faster, because of cached arrays.
    /// This can have a significant impact on the speed of reading of data from Links storage.
    /// </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class CachedSequenceWalker<TLinkAddress> : LinksDisposableDecoratorBase<TLinkAddress>
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly SequenceCache<TLinkAddress> _memoryCache;
        private readonly SequenceFileCache<TLinkAddress>? _fileCache;
        private readonly bool _enableFileCache;

        /// <summary>
        /// Gets the in-memory cache instance.
        /// </summary>
        public SequenceCache<TLinkAddress> MemoryCache => _memoryCache;

        /// <summary>
        /// Gets the file cache instance, if enabled.
        /// </summary>
        public SequenceFileCache<TLinkAddress>? FileCache => _fileCache;

        /// <summary>
        /// Gets a value indicating whether file caching is enabled.
        /// </summary>
        public bool IsFileCacheEnabled => _enableFileCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedSequenceWalker{TLinkAddress}"/> class.
        /// </summary>
        /// <param name="links">The underlying links storage.</param>
        /// <param name="enableFileCache">Whether to enable file-based caching.</param>
        /// <param name="cacheDirectory">The directory for file cache. If null, uses temp directory.</param>
        /// <param name="fileCacheDuration">The duration to keep file cache. Default is 1 second.</param>
        public CachedSequenceWalker(
            ILinks<TLinkAddress> links,
            bool enableFileCache = true,
            string? cacheDirectory = null,
            TimeSpan? fileCacheDuration = null) : base(links)
        {
            _memoryCache = new SequenceCache<TLinkAddress>();
            _enableFileCache = enableFileCache;
            
            if (_enableFileCache)
            {
                _fileCache = new SequenceFileCache<TLinkAddress>(cacheDirectory, fileCacheDuration);
            }
        }

        /// <summary>
        /// Gets a sequence as an array of elements, using cache when possible.
        /// </summary>
        /// <param name="sequence">The sequence address.</param>
        /// <returns>The sequence elements array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress[] GetSequenceArray(TLinkAddress sequence)
        {
            // Try memory cache first
            if (_memoryCache.TryGetSequence(sequence, out var cachedSequence) && cachedSequence != null)
            {
                return cachedSequence;
            }

            // Try file cache if enabled
            if (_enableFileCache && _fileCache != null && 
                _fileCache.TryGetSequence(sequence, out var fileCachedSequence) && fileCachedSequence != null)
            {
                // Store in memory cache for faster future access
                _memoryCache.CacheSequence(sequence, fileCachedSequence);
                return fileCachedSequence;
            }

            // Cache miss - need to walk the sequence
            var elements = WalkSequenceToArray(sequence);
            
            // Cache the result
            CacheSequenceElements(sequence, elements);
            
            return elements;
        }

        /// <summary>
        /// Gets a sequence as an array of elements asynchronously, using cache when possible.
        /// </summary>
        /// <param name="sequence">The sequence address.</param>
        /// <returns>A task that represents the asynchronous operation containing the sequence elements array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TLinkAddress[]> GetSequenceArrayAsync(TLinkAddress sequence)
        {
            // Try memory cache first
            if (_memoryCache.TryGetSequence(sequence, out var cachedSequence) && cachedSequence != null)
            {
                return cachedSequence;
            }

            // Try file cache if enabled
            if (_enableFileCache && _fileCache != null && 
                _fileCache.TryGetSequence(sequence, out var fileCachedSequence) && fileCachedSequence != null)
            {
                // Store in memory cache for faster future access
                _memoryCache.CacheSequence(sequence, fileCachedSequence);
                return fileCachedSequence;
            }

            // Cache miss - need to walk the sequence
            var elements = WalkSequenceToArray(sequence);
            
            // Cache the result asynchronously
            await CacheSequenceElementsAsync(sequence, elements).ConfigureAwait(false);
            
            return elements;
        }

        /// <summary>
        /// Tries to find an existing sequence address for the given elements, using cache when possible.
        /// </summary>
        /// <param name="elements">The sequence elements.</param>
        /// <param name="address">The found address, if any.</param>
        /// <returns>True if a cached address was found, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCachedSequenceAddress(TLinkAddress[] elements, out TLinkAddress address)
        {
            return _memoryCache.TryGetAddress(elements, out address);
        }

        /// <summary>
        /// Walks a sequence and converts it to an array of elements.
        /// </summary>
        /// <param name="sequence">The sequence address.</param>
        /// <returns>The sequence elements array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress[] WalkSequenceToArray(TLinkAddress sequence)
        {
            var elements = new List<TLinkAddress>();
            
            // Walk the sequence using the standard doublets pattern
            // A sequence is typically represented as nested pairs: (element1, (element2, (element3, ...)))
            WalkSequenceRecursive(sequence, elements);
            
            return elements.ToArray();
        }

        /// <summary>
        /// Recursively walks a sequence structure and collects elements.
        /// </summary>
        /// <param name="current">The current link to examine.</param>
        /// <param name="elements">The list to collect elements into.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WalkSequenceRecursive(TLinkAddress current, IList<TLinkAddress> elements)
        {
            if (!_links.Exists(current))
            {
                return;
            }

            var link = _links.GetLink(current);
            var source = _links.GetSource(link);
            var target = _links.GetTarget(link);

            // Check if this is a point (self-referencing link)
            if (source == current && target == current)
            {
                elements.Add(current);
                return;
            }

            // If source is different from current, it's likely an element
            if (source != current)
            {
                // Check if source is a point or should be treated as an element
                var sourceLink = _links.GetLink(source);
                var sourceSource = _links.GetSource(sourceLink);
                var sourceTarget = _links.GetTarget(sourceLink);
                
                if (sourceSource == source && sourceTarget == source)
                {
                    // Source is a point, add it as element
                    elements.Add(source);
                }
                else
                {
                    // Source might be a nested sequence, walk it recursively
                    WalkSequenceRecursive(source, elements);
                }
            }

            // Process target similarly
            if (target != current)
            {
                var targetLink = _links.GetLink(target);
                var targetSource = _links.GetSource(targetLink);
                var targetTarget = _links.GetTarget(targetLink);
                
                if (targetSource == target && targetTarget == target)
                {
                    // Target is a point, add it as element
                    elements.Add(target);
                }
                else
                {
                    // Target might be a nested sequence, walk it recursively
                    WalkSequenceRecursive(target, elements);
                }
            }
        }

        /// <summary>
        /// Caches sequence elements in both memory and file cache.
        /// </summary>
        /// <param name="address">The sequence address.</param>
        /// <param name="elements">The sequence elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CacheSequenceElements(TLinkAddress address, TLinkAddress[] elements)
        {
            if (elements.Length == 0)
            {
                return;
            }

            // Cache in memory
            _memoryCache.CacheSequence(address, elements);
            
            // Cache in file if enabled
            if (_enableFileCache && _fileCache != null)
            {
                _fileCache.CacheSequence(address, elements);
            }
        }

        /// <summary>
        /// Asynchronously caches sequence elements in both memory and file cache.
        /// </summary>
        /// <param name="address">The sequence address.</param>
        /// <param name="elements">The sequence elements.</param>
        /// <returns>A task representing the asynchronous caching operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task CacheSequenceElementsAsync(TLinkAddress address, TLinkAddress[] elements)
        {
            if (elements.Length == 0)
            {
                return;
            }

            // Cache in memory
            _memoryCache.CacheSequence(address, elements);
            
            // Cache in file if enabled
            if (_enableFileCache && _fileCache != null)
            {
                await _fileCache.CacheSequenceAsync(address, elements).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Invalidates cached data for a sequence.
        /// </summary>
        /// <param name="address">The sequence address to invalidate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvalidateSequence(TLinkAddress address)
        {
            _memoryCache.RemoveSequence(address);
            
            if (_enableFileCache && _fileCache != null)
            {
                _fileCache.RemoveSequence(address);
            }
        }

        /// <summary>
        /// Clears all cached data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearCache()
        {
            _memoryCache.Clear();
            
            if (_enableFileCache && _fileCache != null)
            {
                _fileCache.Clear();
            }
        }

        /// <summary>
        /// Gets cache statistics.
        /// </summary>
        /// <returns>A tuple containing hit count, miss count, and hit ratio.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (long hitCount, long missCount, double hitRatio) GetCacheStatistics()
        {
            return (_memoryCache.HitCount, _memoryCache.MissCount, _memoryCache.HitRatio);
        }

        // Override decorator methods to invalidate cache when data changes

        /// <summary>
        /// Updates a link and invalidates related cache entries.
        /// </summary>
        public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            var result = base.Update(restriction, substitution, handler);
            
            // Invalidate cache for updated links
            if (restriction != null && restriction.Count > 0)
            {
                InvalidateSequence(restriction[0]);
            }
            
            return result;
        }

        /// <summary>
        /// Deletes a link and invalidates related cache entries.
        /// </summary>
        public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            // Invalidate cache for deleted links
            if (restriction != null && restriction.Count > 0)
            {
                InvalidateSequence(restriction[0]);
            }
            
            return base.Delete(restriction, handler);
        }

        /// <summary>
        /// Disposes the cached sequence walker and its resources.
        /// </summary>
        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed && manual)
            {
                _fileCache?.Dispose();
            }
            base.Dispose(manual, wasDisposed);
        }
    }
}