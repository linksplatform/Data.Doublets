using BenchmarkDotNet.Attributes;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Tests;
using Platform.Memory;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    /// <summary>
    /// <para>
    /// Represents the memory benchmarks.
    /// </para>
    /// <para></para>
    /// </summary>
    [SimpleJob]
    [MemoryDiagnoser]
    public class MemoryBenchmarks
    {
        /// <summary>
        /// <para>
        /// The split memory.
        /// </para>
        /// <para></para>
        /// </summary>
        private static SplitMemoryLinks<uint> _splitMemory;
        /// <summary>
        /// <para>
        /// The split memory links.
        /// </para>
        /// <para></para>
        /// </summary>
        private static ILinks<uint> _splitMemoryLinks;
        /// <summary>
        /// <para>
        /// The united memory.
        /// </para>
        /// <para></para>
        /// </summary>
        private static UnitedMemoryLinks<uint> _unitedMemory;
        /// <summary>
        /// <para>
        /// The united memory links.
        /// </para>
        /// <para></para>
        /// </summary>
        private static ILinks<uint> _unitedMemoryLinks;

        /// <summary>
        /// <para>
        /// Setup.
        /// </para>
        /// <para></para>
        /// </summary>
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
        }

        /// <summary>
        /// <para>
        /// Cleanups.
        /// </para>
        /// <para></para>
        /// </summary>
        [GlobalCleanup]
        public static void Cleanup()
        {
            _splitMemory.Dispose();
            _unitedMemory.Dispose();
        }

        /// <summary>
        /// <para>
        /// Splits this instance.
        /// </para>
        /// <para></para>
        /// </summary>
        [Benchmark]
        public void Split()
        {
            _splitMemoryLinks.TestMultipleRandomCreationsAndDeletions(1000);
        }

        /// <summary>
        /// <para>
        /// Uniteds this instance.
        /// </para>
        /// <para></para>
        /// </summary>
        [Benchmark]
        public void United()
        {
            _unitedMemoryLinks.TestMultipleRandomCreationsAndDeletions(1000);
        }
    }
}