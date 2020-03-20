using BenchmarkDotNet.Attributes;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.ResizableDirectMemory.Generic;
using Platform.Data.Doublets.Tests;
using Platform.Memory;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    [SimpleJob]
    [MemoryDiagnoser]
    public class MemoryBenchmarks
    {
        private static SplitMemoryLinks<uint> _splitMemory;
        private static ILinks<uint> _splitMemoryLinks;
        private static ResizableDirectMemoryLinks<uint> _simpleMemory;
        private static ILinks<uint> _simpleMemoryLinks;

        [GlobalSetup]
        public static void Setup()
        {
            var dataMemory = new HeapResizableDirectMemory();
            var indexMemory = new HeapResizableDirectMemory();
            _splitMemory = new SplitMemoryLinks<uint>(dataMemory, indexMemory);
            _splitMemoryLinks = _splitMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();

            var memory = new HeapResizableDirectMemory();
            _simpleMemory = new ResizableDirectMemoryLinks<uint>(memory);
            _simpleMemoryLinks = _simpleMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();
        }

        [GlobalCleanup]
        public static void Cleanup()
        {
            _splitMemory.Dispose();
            _simpleMemory.Dispose();
        }

        [Benchmark]
        public void Split()
        {
            _splitMemoryLinks.TestMultipleRandomCreationsAndDeletions(1000);
        }

        [Benchmark]
        public void Simple()
        {
            _simpleMemoryLinks.TestMultipleRandomCreationsAndDeletions(1000);
        }
    }
}