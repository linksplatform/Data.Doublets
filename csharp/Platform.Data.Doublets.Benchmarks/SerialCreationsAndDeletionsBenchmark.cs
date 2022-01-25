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
    public class SerialCreationsAndDeletionsBenchmark
    {
        private static SplitMemoryLinks<uint> _splitMemory;
        private static UnitedMemoryLinks<uint> _unitedMemory;
        private static FFI.UnitedMemoryLinks<uint> _ffiUnitedMemory;

        [GlobalSetup]
        public static void Setup()
        {
            var dataMemory = new HeapResizableDirectMemory();
            var indexMemory = new HeapResizableDirectMemory();
            _splitMemory = new SplitMemoryLinks<uint>(dataMemory, indexMemory);

            var memory = new HeapResizableDirectMemory();
            _unitedMemory = new UnitedMemoryLinks<uint>(memory);

            File.Delete("db.links");
            _ffiUnitedMemory = new FFI.UnitedMemoryLinks<uint>("db.links");
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
            _splitMemory.TestMultipleCreationsAndDeletions(10000);
        }

        [Benchmark]
        public void United()
        {
            _unitedMemory.TestMultipleCreationsAndDeletions(10000);
        }

        [Benchmark]
        public void FfiUnited()
        {
            _ffiUnitedMemory.TestMultipleCreationsAndDeletions(10000);
        }
    }
}
