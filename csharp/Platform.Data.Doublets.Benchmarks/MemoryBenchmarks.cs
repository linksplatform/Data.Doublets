using System.IO;
using BenchmarkDotNet.Attributes;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
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
        private static UnitedMemoryLinks<uint> _unitedMemory;
        private static ILinks<uint> _unitedMemoryLinks;
        private static FFI.UnitedMemoryLinks<uint> _ffiUnitedMemory;
        private static ILinks<uint> _ffiUnitedMemoryLinks;

        [GlobalSetup]
        public static void Setup()
        {
            var dataMemory = new HeapResizableDirectMemory();
            var indexMemory = new HeapResizableDirectMemory();
            _splitMemory = new SplitMemoryLinks<uint>(dataMemory, indexMemory);
            _splitMemoryLinks = _splitMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();

            var memory = new HeapResizableDirectMemory();
            _unitedMemory = new UnitedMemoryLinks<uint>(memory);
            _unitedMemoryLinks = _unitedMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();

            File.Delete("db.links");
            _ffiUnitedMemory = new FFI.UnitedMemoryLinks<uint>("db.links");
            _ffiUnitedMemoryLinks = _ffiUnitedMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();
        }

        [GlobalCleanup]
        public static void Cleanup()
        {
            _splitMemory.Dispose();
            _unitedMemory.Dispose();
             _ffiUnitedMemory.Dispose();
            File.Delete("db.links");
        }

        [Benchmark]
        public void Split()
        {
            _splitMemoryLinks.TestMultipleRandomCreationsAndDeletions(1000);
        }

        [Benchmark]
        public void United()
        {
            _unitedMemoryLinks.TestMultipleRandomCreationsAndDeletions(1000);
        }

        [Benchmark]
        public void FfiUnited()
        {
            _ffiUnitedMemoryLinks.TestMultipleRandomCreationsAndDeletions(1000);
        }
    }
}
