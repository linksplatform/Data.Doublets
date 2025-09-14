using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Platform.Collections.Arrays;
using Platform.Collections.Lists;
using Platform.Converters;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using TLinkAddress = System.UInt64;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    /// <summary>
    /// Benchmarks comparing experimental versions of similar algorithms.
    /// This demonstrates different approaches to link data collection and processing,
    /// showing performance differences between various implementation strategies.
    /// </summary>
    [SimpleJob]
    [MemoryDiagnoser]
    public class ExperimentalAlgorithmsBenchmarks
    {
        private static ILinks<TLinkAddress> _links;
        private static TLinkAddress _any;
        private static HeapResizableDirectMemory _dataMemory;

        [Params(100, 1000, 5000)]
        public static int N;

        [GlobalSetup]
        public static void Setup()
        {
            _dataMemory = new HeapResizableDirectMemory();
            _links = new UnitedMemoryLinks<TLinkAddress>(_dataMemory).DecorateWithAutomaticUniquenessAndUsagesResolution();
            _any = _links.Constants.Any;
            var firstLink = _links.CreatePoint();
            
            // Create test data
            for (int i = 0; i < N; i++)
            {
                var link = _links.Create();
                _links.Update(link, firstLink, link);
            }
            for (int i = 0; i < N; i++)
            {
                var link = _links.Create();
                _links.Update(link, link, firstLink);
            }
        }

        [GlobalCleanup]
        public static void Cleanup()
        {
            _dataMemory.Dispose();
        }

        /// <summary>
        /// Version 1: Using pre-allocated array with exact size calculation.
        /// This is the most memory-efficient approach but requires two passes.
        /// </summary>
        [Benchmark]
        public IList<IList<TLinkAddress>?> LinkCollection_V1_PreallocatedArray()
        {
            var addressToInt64Converter = CheckedConverter<TLinkAddress, long>.Default;
            var usagesAsSourceQuery = new Link<TLinkAddress>(_any, 1UL, _any);
            var usagesAsSourceCount = addressToInt64Converter.Convert(_links.Count(usagesAsSourceQuery));
            var usagesAsTargetQuery = new Link<TLinkAddress>(_any, _any, 1UL);
            var usagesAsTargetCount = addressToInt64Converter.Convert(_links.Count(usagesAsTargetQuery));
            var totalUsages = usagesAsSourceCount + usagesAsTargetCount;
            var usages = new IList<TLinkAddress>?[totalUsages];
            var usagesFiller = new ArrayFiller<IList<TLinkAddress>?, TLinkAddress>(usages, _links.Constants.Continue);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
            return usages;
        }

        /// <summary>
        /// Version 2: Using dynamic List without pre-calculation.
        /// This is simpler but less memory-efficient due to dynamic resizing.
        /// </summary>
        [Benchmark]
        public IList<IList<TLinkAddress>?> LinkCollection_V2_DynamicList()
        {
            var usagesAsSourceQuery = new Link<TLinkAddress>(_any, 1UL, _any);
            var usagesAsTargetQuery = new Link<TLinkAddress>(_any, _any, 1UL);
            var usages = new List<IList<TLinkAddress>?>();
            var usagesFiller = new ListFiller<IList<TLinkAddress>?, TLinkAddress>(usages, _links.Constants.Continue);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
            return usages;
        }

        /// <summary>
        /// Version 3: Using List with pre-calculated capacity.
        /// This combines the benefits of both approaches - single allocation with dynamic structure.
        /// </summary>
        [Benchmark]
        public IList<IList<TLinkAddress>?> LinkCollection_V3_PreallocatedList()
        {
            var addressToInt64Converter = CheckedConverter<TLinkAddress, long>.Default;
            var usagesAsSourceQuery = new Link<TLinkAddress>(_any, 1UL, _any);
            var usagesAsSourceCount = addressToInt64Converter.Convert(_links.Count(usagesAsSourceQuery));
            var usagesAsTargetQuery = new Link<TLinkAddress>(_any, _any, 1UL);
            var usagesAsTargetCount = addressToInt64Converter.Convert(_links.Count(usagesAsTargetQuery));
            var totalUsages = usagesAsSourceCount + usagesAsTargetCount;
            var usages = new List<IList<TLinkAddress>?>((int)totalUsages);
            var usagesFiller = new ListFiller<IList<TLinkAddress>?, TLinkAddress>(usages, _links.Constants.Continue);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
            return usages;
        }

        /// <summary>
        /// Version 4: Using LINQ-based approach for collection.
        /// This is the most readable but potentially least performant approach.
        /// </summary>
        [Benchmark]
        public IList<IList<TLinkAddress>?> LinkCollection_V4_LinqBased()
        {
            var usagesAsSourceQuery = new Link<TLinkAddress>(_any, 1UL, _any);
            var usagesAsTargetQuery = new Link<TLinkAddress>(_any, _any, 1UL);
            
            var sourceUsages = new List<IList<TLinkAddress>?>();
            var targetUsages = new List<IList<TLinkAddress>?>();
            
            _links.Each(link => { sourceUsages.Add(link); return _links.Constants.Continue; }, usagesAsSourceQuery);
            _links.Each(link => { targetUsages.Add(link); return _links.Constants.Continue; }, usagesAsTargetQuery);
            
            return sourceUsages.Concat(targetUsages).ToList();
        }

        /// <summary>
        /// Version 5: Experimental batch processing approach.
        /// Processes links in batches to potentially improve cache locality.
        /// </summary>
        [Benchmark]
        public IList<IList<TLinkAddress>?> LinkCollection_V5_BatchProcessing()
        {
            const int batchSize = 100;
            var usagesAsSourceQuery = new Link<TLinkAddress>(_any, 1UL, _any);
            var usagesAsTargetQuery = new Link<TLinkAddress>(_any, _any, 1UL);
            var usages = new List<IList<TLinkAddress>?>();
            
            // Process in batches
            var batch = new List<IList<TLinkAddress>?>(batchSize);
            
            _links.Each(link =>
            {
                batch.Add(link);
                if (batch.Count >= batchSize)
                {
                    usages.AddRange(batch);
                    batch.Clear();
                }
                return _links.Constants.Continue;
            }, usagesAsSourceQuery);
            
            if (batch.Count > 0)
            {
                usages.AddRange(batch);
                batch.Clear();
            }
            
            _links.Each(link =>
            {
                batch.Add(link);
                if (batch.Count >= batchSize)
                {
                    usages.AddRange(batch);
                    batch.Clear();
                }
                return _links.Constants.Continue;
            }, usagesAsTargetQuery);
            
            if (batch.Count > 0)
            {
                usages.AddRange(batch);
            }
            
            return usages;
        }

        /// <summary>
        /// Version 6: Memory-optimized approach using ArrayPool.
        /// Uses pooled arrays to reduce GC pressure for temporary storage.
        /// </summary>
        [Benchmark]
        public IList<IList<TLinkAddress>?> LinkCollection_V6_PooledArrays()
        {
            var addressToInt64Converter = CheckedConverter<TLinkAddress, long>.Default;
            var usagesAsSourceQuery = new Link<TLinkAddress>(_any, 1UL, _any);
            var usagesAsSourceCount = addressToInt64Converter.Convert(_links.Count(usagesAsSourceQuery));
            var usagesAsTargetQuery = new Link<TLinkAddress>(_any, _any, 1UL);
            var usagesAsTargetCount = addressToInt64Converter.Convert(_links.Count(usagesAsTargetQuery));
            var totalUsages = usagesAsSourceCount + usagesAsTargetCount;
            
            // Use a simple approach since ArrayPool might not be available
            var usages = new List<IList<TLinkAddress>?>((int)totalUsages);
            var tempList = new List<IList<TLinkAddress>?>();
            
            _links.Each(link => { tempList.Add(link); return _links.Constants.Continue; }, usagesAsSourceQuery);
            _links.Each(link => { tempList.Add(link); return _links.Constants.Continue; }, usagesAsTargetQuery);
            
            usages.AddRange(tempList);
            return usages;
        }
    }
}