/*
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
        private const int N = 1000;
        private static SplitMemoryLinks<uint> _splitMemory;
        private static ILinks<uint> _splitMemoryLinks;
        private static UnitedMemoryLinks<uint> _unitedMemory;
        private static ILinks<uint> _unitedMemoryLinks;
        private static Ffi.Links<uint> _ffi;
        private static ILinks<uint> _ffiUnitedMemoryLinks;
        private static Ffi.UInt32Links _ffiUInt32;
        private static ILinks<uint> _ffiUInt32UnitedMemoryLinks;

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
            _ffi = new Ffi.Links<uint>("db.links");
            _ffiUnitedMemoryLinks = _ffi;

            File.Delete("db.links1");
            _ffiUInt32 = new Ffi.UInt32Links("db.links1");
            _ffiUInt32UnitedMemoryLinks = _ffiUInt32.DecorateWithAutomaticUniquenessAndUsagesResolution();
        }

        [GlobalCleanup]
        public static void Cleanup()
        {
            _splitMemory.Dispose();
            _unitedMemory.Dispose();
             _ffi.Dispose();
            File.Delete("db.links");
            _ffiUInt32.Dispose();
            File.Delete("db.links1");
        }

        [Benchmark]
        public void Split()
        {
            _splitMemoryLinks.TestMultipleRandomCreationsAndDeletions(N);
        }

        [Benchmark]
        public void United()
        {
            _unitedMemoryLinks.TestMultipleRandomCreationsAndDeletions(N);
        }

        [Benchmark]
        public void FfiUnited()
        {
            _ffiUnitedMemoryLinks.TestMultipleRandomCreationsAndDeletions(N);
        }

        [Benchmark]
        public void FfiUInt32United()
        {
            _ffiUInt32UnitedMemoryLinks.TestMultipleRandomCreationsAndDeletions(N);
        }
    }
}
*/
