using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Platform.Collections.Arrays;
using Platform.Collections.Lists;
using Platform.Converters;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using TLink = System.UInt64;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    [SimpleJob]
    [MemoryDiagnoser]
    public class CountBenchmarks
    {
        private static ILinks<TLink> _links;
        private static TLink _any;
        private static CheckedConverter<TLink, long> _addressToInt64Converter = CheckedConverter<TLink, long>.Default;

        [Params(10, 100, 1000, 10000, 100000)]
        public static ulong N;

        [GlobalSetup]
        public static void Setup()
        {
            var dataMemory = new HeapResizableDirectMemory();
            _links = new UnitedMemoryLinks<TLink>(dataMemory).DecorateWithAutomaticUniquenessAndUsagesResolution();
            _any = _links.Constants.Any;
            var firstLink = _links.CreatePoint();
            for (ulong i = 0; i < N; i++)
            {
                var link = links.Create();
                return links.Update(link, firstLink, link);
            }
            for (ulong i = 0; i < N; i++)
            {
                var link = _links.CreatePoint();
                _links.Update(new LinkAddress<TLink>(link), new List<TLink>{ link, link, firstLink });
            }
        }

        [GlobalCleanup]
        public static void Cleanup() { }

        [Benchmark]
        public IList<IList<TLink>> Array()
        {
            var addressToInt64Converter = CheckedConverter<TLink, long>.Default;
            var usagesAsSourceQuery = new Link<TLink>(_any, 1UL, _any);
            var usagesAsSourceCount = addressToInt64Converter.Convert(_links.Count(usagesAsSourceQuery));
            var usagesAsTargetQuery = new Link<TLink>(_any, _any, 1UL);
            var usagesAsTargetCount = addressToInt64Converter.Convert(_links.Count(usagesAsTargetQuery));
            var totalUsages = usagesAsSourceCount + usagesAsTargetCount;
            var usages = new IList<TLink>[totalUsages];
            var usagesFiller = new ArrayFiller<IList<TLink>, TLink>(usages, _links.Constants.Continue);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
            return usages;
        }

        [Benchmark]
        public IList<IList<TLink>> List()
        {
            var usagesAsSourceQuery = new Link<TLink>(_any, 1UL, _any);
            var usagesAsTargetQuery = new Link<TLink>(_any, _any, 1UL);
            var usages = new List<IList<TLink>>();
            var usagesFiller = new ListFiller<IList<TLink>, TLink>(usages, _links.Constants.Continue);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
            return usages;
        }

        [Benchmark]
        public IList<IList<TLink>> ListWithCapacity()
        {
            var addressToInt64Converter = CheckedConverter<TLink, long>.Default;
            var usagesAsSourceQuery = new Link<TLink>(_any, 1UL, _any);
            var usagesAsSourceCount = addressToInt64Converter.Convert(_links.Count(usagesAsSourceQuery));
            var usagesAsTargetQuery = new Link<TLink>(_any, _any, 1UL);
            var usagesAsTargetCount = addressToInt64Converter.Convert(_links.Count(usagesAsTargetQuery));
            var totalUsages = usagesAsSourceCount + usagesAsTargetCount;
            var usages = new List<IList<TLink>>((int)totalUsages);
            var usagesFiller = new ListFiller<IList<TLink>, TLink>(usages, _links.Constants.Continue);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
            _links.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
            return usages;
        }
    }
}
