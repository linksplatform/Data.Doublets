using System.Collections.Generic;
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
    [SimpleJob]
    [MemoryDiagnoser]
    public class CountBenchmarks
    {
        private static ILinks<TLinkAddress> _links;
        private static TLinkAddress _any;
        private static CheckedConverter<TLinkAddress, long> _addressToInt64Converter = CheckedConverter<TLinkAddress, long>.Default;
        private static HeapResizableDirectMemory _dataMemory;

        [Params(10, 100, 1000, 10000, 100000)]
        public static ulong N;

        [GlobalSetup]
        public static void Setup()
        {
            _dataMemory = new HeapResizableDirectMemory();
            _links = new UnitedMemoryLinks<TLinkAddress>(_dataMemory).DecorateWithAutomaticUniquenessAndUsagesResolution();
            _any = _links.Constants.Any;
            var firstLink = _links.CreatePoint();
            for (ulong i = 0; i < N; i++)
            {
                var link = _links.Create();
                _links.Update(link, firstLink, link);
            }
            for (ulong i = 0; i < N; i++)
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

        [Benchmark]
        public IList<IList<TLinkAddress>?> Array()
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

        [Benchmark]
        public IList<IList<TLinkAddress>?> List()
        {
            var usagesAsSourceQuery = new Link<TLinkAddress>(_any, 1UL, _any);
            var usagesAsTargetQuery = new Link<TLinkAddress>(_any, _any, 1UL);
            var usages = new List<IList<TLinkAddress>?>();
            var usagesFiller = new ListFiller<IList<TLinkAddress>?, TLinkAddress>(usages, _links.Constants.Continue);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
            return usages;
        }

        [Benchmark]
        public IList<IList<TLinkAddress>?> ListWithCapacity()
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
    }
}
