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
    public class FfiVsNativeCountBenchmarks
    {
        private static SplitMemoryLinks<uint> _splitMemory;
        private static ILinks<uint> _splitMemoryLinks;
        private static UnitedMemoryLinks<uint> _unitedMemory;
        private static ILinks<uint> _unitedMemoryLinks;
        private static FFI.UnitedMemoryLinks<uint> _ffiUnitedMemory;
        private static ILinks<uint> _ffiUnitedMemoryLinks;
        private static FFI.UInt32UnitedMemoryLinks _ffiUInt32UnitedMemory;
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
            _ffiUnitedMemory = new FFI.UnitedMemoryLinks<uint>("db.links");
            _ffiUnitedMemoryLinks = _ffiUnitedMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();

            File.Delete("db.links1");
            _ffiUInt32UnitedMemory = new FFI.UInt32UnitedMemoryLinks("db.links1");
            _ffiUInt32UnitedMemoryLinks = _ffiUInt32UnitedMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();

            for (int i = 0; i < 500; i++)
            {
                _splitMemoryLinks.Create();
                _unitedMemoryLinks.Create();
                _ffiUnitedMemoryLinks.Create();
                _ffiUInt32UnitedMemoryLinks.Create();
            }
        }

        [GlobalCleanup]
        public static void Cleanup()
        {
            _splitMemory.Dispose();
            _unitedMemory.Dispose();
             _ffiUnitedMemory.Dispose();
            File.Delete("db.links");
            _ffiUInt32UnitedMemory.Dispose();
            File.Delete("db.links1");
        }

        [Benchmark]
        public void Split()
        {
            _splitMemoryLinks.Count(new Link<uint>(_splitMemoryLinks.Constants.Any, _splitMemoryLinks.Constants.Any, _splitMemoryLinks.Constants.Any));
        }

        [Benchmark]
        public void United()
        {
            _unitedMemoryLinks.Count(new Link<uint>(_unitedMemoryLinks.Constants.Any, _unitedMemoryLinks.Constants.Any, _unitedMemoryLinks.Constants.Any));
        }

        [Benchmark]
        public void FfiUnited()
        {
            _ffiUnitedMemoryLinks.Count(new Link<uint>(_ffiUnitedMemoryLinks.Constants.Any, _ffiUnitedMemoryLinks.Constants.Any, _ffiUnitedMemoryLinks.Constants.Any));
        }

        [Benchmark]
        public void FfiUInt32United()
        {
            _ffiUInt32UnitedMemoryLinks.Count(new Link<uint>(_ffiUInt32UnitedMemoryLinks.Constants.Any, _ffiUInt32UnitedMemoryLinks.Constants.Any, _ffiUInt32UnitedMemoryLinks.Constants.Any));
        }
    }
}
