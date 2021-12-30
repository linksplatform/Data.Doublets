using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using TLink = System.UInt64;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    [SimpleJob]
    [MemoryDiagnoser]
    public class LinkStructBenchmarks
    {
        [GlobalSetup]
        public static void Setup()
        {
        }

        [GlobalCleanup]
        public static void Cleanup() { }

        [Benchmark]
        public IList<TLink> Struct()
        {
            return new Link<TLink>(1UL, 1UL, 1UL);
        }

        [Benchmark]
        public IList<TLink> Array()
        {
            return new TLink[] {1UL, 1UL, 1UL};
        }

        [Benchmark]
        public IList<TLink> List()
        {
            return new List<TLink>{1UL, 1UL, 1UL};
        }
    }
}
