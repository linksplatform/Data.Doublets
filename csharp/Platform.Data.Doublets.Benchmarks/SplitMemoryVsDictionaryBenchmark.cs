using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Memory;
using Platform.Data.Doublets.Tests;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    [SimpleJob]
    [MemoryDiagnoser]
    public class SplitMemoryVsDictionaryBenchmark
    {
        private static ILinks<uint> _splitMemoryLinks;
        private static Dictionary<uint, uint> _dictionary;
        private static SplitMemoryLinks<uint> _splitMemoryLinksRaw;
        private static HeapResizableDirectMemory _dataMemory;
        private static HeapResizableDirectMemory _indexMemory;

        [Params(100, 1000)]
        public static int N;

        [GlobalSetup]
        public static void Setup()
        {
            // Setup Split Memory - following the test pattern
            _dataMemory = new HeapResizableDirectMemory();
            _indexMemory = new HeapResizableDirectMemory();
            _splitMemoryLinksRaw = new SplitMemoryLinks<uint>(_dataMemory, _indexMemory);
            _splitMemoryLinks = _splitMemoryLinksRaw.DecorateWithAutomaticUniquenessAndUsagesResolution();
            
            // Setup Dictionary
            _dictionary = new Dictionary<uint, uint>();
        }

        [GlobalCleanup]
        public static void Cleanup()
        {
            _splitMemoryLinksRaw?.Dispose();
            _dataMemory?.Dispose();
            _indexMemory?.Dispose();
        }

        [IterationSetup]
        public static void IterationSetup()
        {
            // Clear both structures before each iteration
            _splitMemoryLinksRaw?.Dispose();
            _dataMemory?.Dispose();
            _indexMemory?.Dispose();
            
            _dataMemory = new HeapResizableDirectMemory();
            _indexMemory = new HeapResizableDirectMemory();
            _splitMemoryLinksRaw = new SplitMemoryLinks<uint>(_dataMemory, _indexMemory);
            _splitMemoryLinks = _splitMemoryLinksRaw.DecorateWithAutomaticUniquenessAndUsagesResolution();
            
            _dictionary.Clear();
        }

        /// <summary>
        /// Tests multiple creations and deletions using the established test pattern for SplitMemoryLinks
        /// vs simple dictionary operations for the same workload
        /// </summary>
        [Benchmark]
        public void SplitMemory_MultipleOperations()
        {
            _splitMemoryLinks.TestMultipleRandomCreationsAndDeletions(N);
        }

        /// <summary>
        /// Equivalent operations on Dictionary: create N key-value pairs and then delete them
        /// </summary>
        [Benchmark]
        public void Dictionary_MultipleOperations()
        {
            var random = new System.Random(1); // Use same seed as the test
            var keys = new List<uint>();

            // Create N key-value pairs
            for (var i = 0; i < N; i++)
            {
                var key = (uint)random.Next(1, int.MaxValue);
                var value = (uint)random.Next(1, int.MaxValue);
                if (!_dictionary.ContainsKey(key))
                {
                    _dictionary[key] = value;
                    keys.Add(key);
                }
            }

            // Delete all created pairs
            foreach (var key in keys)
            {
                _dictionary.Remove(key);
            }
        }

        /// <summary>
        /// Simple insert benchmark - create N links vs N dictionary entries
        /// </summary>
        [Benchmark]
        public void SplitMemory_SimpleInsert()
        {
            for (var i = 0; i < N; i++)
            {
                var linkAddress = _splitMemoryLinks.Create();
                _splitMemoryLinks.Update(linkAddress, linkAddress, linkAddress);
            }
        }

        /// <summary>
        /// Simple insert benchmark - create N dictionary entries
        /// </summary>
        [Benchmark]
        public void Dictionary_SimpleInsert()
        {
            for (var i = 0; i < N; i++)
            {
                var key = (uint)i + 1; // Start from 1 to avoid zero
                var value = key * 2;
                _dictionary[key] = value;
            }
        }

        /// <summary>
        /// Count operation comparison
        /// </summary>
        [Benchmark]
        public void SplitMemory_Count()
        {
            // Create some links first
            for (var i = 0; i < N; i++)
            {
                _splitMemoryLinks.Create();
            }
            
            // Count operation
            var count = _splitMemoryLinks.Count();
        }

        /// <summary>
        /// Count operation for dictionary
        /// </summary>
        [Benchmark]
        public void Dictionary_Count()
        {
            // Create some entries first
            for (var i = 0; i < N; i++)
            {
                var key = (uint)i + 1;
                var value = key * 2;
                _dictionary[key] = value;
            }
            
            // Count operation
            var count = _dictionary.Count;
        }
    }
}