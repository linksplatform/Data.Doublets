using System;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences;
using Platform.Memory;

namespace Platform.Data.Doublets.Examples
{
    /// <summary>
    /// <para>
    /// Example demonstrating how to use the cached sequence walker/reader functionality.
    /// </para>
    /// <para>
    /// The link is two subsequences, we can cache the relationship between each array of subsequence's elements and its address.
    /// So if need to convert the address to array of elements second time we can do it faster, because of cached arrays.
    /// This can have a significant impact on the speed of reading of data from Links storage.
    /// </para>
    /// </summary>
    public class CachedSequenceExample
    {
        /// <summary>
        /// Demonstrates basic usage of the cached sequence walker.
        /// </summary>
        public static void BasicUsage()
        {
            Console.WriteLine("=== Cached Sequence Walker Basic Usage Example ===");
            
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);

            // Create some test data points
            var element1 = links.CreatePoint();
            var element2 = links.CreatePoint();
            var element3 = links.CreatePoint();
            
            Console.WriteLine($"Created elements: {element1}, {element2}, {element3}");

            // Create a sequence: element1 -> (element2 -> element3)
            var innerPair = links.CreateAndUpdate(element2, element3);
            var sequence = links.CreateAndUpdate(element1, innerPair);
            
            Console.WriteLine($"Created sequence: {sequence}");

            // Create cached sequence walker
            using var cachedWalker = new CachedSequenceWalker<ulong>(links, enableFileCache: false);

            // First read - will be cache miss and populate cache
            var result1 = cachedWalker.GetSequenceArray(sequence);
            var stats1 = cachedWalker.GetCacheStatistics();
            
            Console.WriteLine($"First read - Sequence elements: [{string.Join(", ", result1)}]");
            Console.WriteLine($"Cache stats after first read: Hits={stats1.hitCount}, Misses={stats1.missCount}, Ratio={stats1.hitRatio:P2}");

            // Second read - will be cache hit
            var result2 = cachedWalker.GetSequenceArray(sequence);
            var stats2 = cachedWalker.GetCacheStatistics();
            
            Console.WriteLine($"Second read - Sequence elements: [{string.Join(", ", result2)}]");
            Console.WriteLine($"Cache stats after second read: Hits={stats2.hitCount}, Misses={stats2.missCount}, Ratio={stats2.hitRatio:P2}");
            
            Console.WriteLine("✓ Performance improvement through caching demonstrated!");
        }

        /// <summary>
        /// Demonstrates using extension methods for easier access.
        /// </summary>
        public static void ExtensionMethodsUsage()
        {
            Console.WriteLine("\n=== Extension Methods Usage Example ===");
            
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);

            // Create test data
            var element1 = links.CreatePoint();
            var element2 = links.CreatePoint();
            var sequence = links.CreateAndUpdate(element1, element2);
            
            Console.WriteLine($"Created sequence: {sequence}");

            // Use extension method - creates temporary cached walker
            var result = links.GetCachedSequenceArray(sequence);
            
            Console.WriteLine($"Using extension method - Sequence elements: [{string.Join(", ", result)}]");
            Console.WriteLine("✓ Extension methods provide convenient access!");
        }

        /// <summary>
        /// Demonstrates file-based caching functionality.
        /// </summary>
        public static void FileCacheUsage()
        {
            Console.WriteLine("\n=== File Cache Usage Example ===");
            
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);

            // Create test data
            var element1 = links.CreatePoint();
            var element2 = links.CreatePoint();
            var sequence = links.CreateAndUpdate(element1, element2);
            
            Console.WriteLine($"Created sequence: {sequence}");

            // Create cached walker with file cache enabled
            var cacheDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DoubletsSequenceCache");
            using var cachedWalker = new CachedSequenceWalker<ulong>(
                links, 
                enableFileCache: true, 
                cacheDirectory: cacheDirectory, 
                fileCacheDuration: TimeSpan.FromSeconds(10));

            // First read - will populate both memory and file cache
            var result1 = cachedWalker.GetSequenceArray(sequence);
            var stats1 = cachedWalker.GetCacheStatistics();
            
            Console.WriteLine($"First read - Sequence elements: [{string.Join(", ", result1)}]");
            Console.WriteLine($"Cache directory: {cachedWalker.FileCache?.CacheDirectory}");
            Console.WriteLine($"File cache enabled: {cachedWalker.IsFileCacheEnabled}");
            Console.WriteLine($"Cache duration: {cachedWalker.FileCache?.CacheDuration}");
            
            // Clear memory cache but keep file cache
            cachedWalker.MemoryCache.Clear();
            
            // Second read - will hit file cache and populate memory cache again
            var result2 = cachedWalker.GetSequenceArray(sequence);
            var stats2 = cachedWalker.GetCacheStatistics();
            
            Console.WriteLine($"After memory cache clear - Sequence elements: [{string.Join(", ", result2)}]");
            Console.WriteLine($"Cache stats: Hits={stats2.hitCount}, Misses={stats2.missCount}, Ratio={stats2.hitRatio:P2}");
            Console.WriteLine("✓ File caching provides persistence across memory cache clears!");
        }

        /// <summary>
        /// Demonstrates cache invalidation on data modifications.
        /// </summary>
        public static void CacheInvalidationExample()
        {
            Console.WriteLine("\n=== Cache Invalidation Example ===");
            
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);
            using var cachedWalker = new CachedSequenceWalker<ulong>(links, enableFileCache: false);

            // Create test sequence
            var element1 = links.CreatePoint();
            var element2 = links.CreatePoint();
            var sequence = links.CreateAndUpdate(element1, element2);
            
            Console.WriteLine($"Created sequence: {sequence}");

            // Read sequence to populate cache
            var result1 = cachedWalker.GetSequenceArray(sequence);
            var stats1 = cachedWalker.GetCacheStatistics();
            
            Console.WriteLine($"Initial read - Sequence elements: [{string.Join(", ", result1)}]");
            Console.WriteLine($"Cache stats: Hits={stats1.hitCount}, Misses={stats1.missCount}");

            // Update the sequence - this should automatically invalidate cache
            var element3 = links.CreatePoint();
            cachedWalker.Update(sequence, element1, element3);
            
            // Read again - should be cache miss due to automatic invalidation
            var result2 = cachedWalker.GetSequenceArray(sequence);
            var stats2 = cachedWalker.GetCacheStatistics();
            
            Console.WriteLine($"After update - Sequence elements: [{string.Join(", ", result2)}]");
            Console.WriteLine($"Cache stats: Hits={stats2.hitCount}, Misses={stats2.missCount}");
            Console.WriteLine("✓ Cache automatically invalidated on data modification!");
        }

        /// <summary>
        /// Main entry point for the example.
        /// </summary>
        public static void Main()
        {
            try
            {
                BasicUsage();
                ExtensionMethodsUsage();
                FileCacheUsage();
                CacheInvalidationExample();
                
                Console.WriteLine("\n🎉 All examples completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Example failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}