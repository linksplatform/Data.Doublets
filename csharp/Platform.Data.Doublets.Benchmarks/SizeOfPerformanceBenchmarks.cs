using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Platform.Unsafe;
using Platform.Data.Doublets.Memory.United;
using Platform.Data.Doublets.Memory.Split;
using TLinkAddress = System.UInt64;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    /// <summary>
    /// Benchmarks comparing performance of Unsafe.SizeOf() method vs Size field approach.
    /// This addresses issue #90: Compare Unsafe.SizeOf() method (with inlining) and Size field performance
    /// </summary>
    [SimpleJob]
    [MemoryDiagnoser]
    [MarkdownExporter]
    [RPlotExporter]
    public class SizeOfPerformanceBenchmarks
    {
        // Sample structures for testing
        private struct SimpleStruct
        {
            public static readonly int SizeField = System.Runtime.CompilerServices.Unsafe.SizeOf<SimpleStruct>();
            public TLinkAddress Value1;
            public TLinkAddress Value2;
        }

        private struct ComplexStruct
        {
            public static readonly int SizeField = System.Runtime.CompilerServices.Unsafe.SizeOf<ComplexStruct>();
            public TLinkAddress Value1;
            public TLinkAddress Value2;
            public TLinkAddress Value3;
            public TLinkAddress Value4;
            public TLinkAddress Value5;
            public TLinkAddress Value6;
            public TLinkAddress Value7;
            public TLinkAddress Value8;
        }

        private const int OperationCount = 1000;

        [GlobalSetup]
        public void Setup()
        {
            // Warmup JIT
            for (int i = 0; i < 100; i++)
            {
                GetSizeViaUnsafeSizeOf<SimpleStruct>();
                GetSizeViaField<SimpleStruct>();
                GetSizeViaUnsafeSizeOf<RawLink<TLinkAddress>>();
                GetRawLinkSizeViaField();
            }
        }

        [Benchmark(Baseline = true)]
        public int SimpleStruct_UnsafeSizeOf()
        {
            int total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetSizeViaUnsafeSizeOf<SimpleStruct>();
            }
            return total;
        }

        [Benchmark]
        public int SimpleStruct_SizeField()
        {
            int total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetSizeViaField<SimpleStruct>();
            }
            return total;
        }

        [Benchmark]
        public int ComplexStruct_UnsafeSizeOf()
        {
            int total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetSizeViaUnsafeSizeOf<ComplexStruct>();
            }
            return total;
        }

        [Benchmark]
        public int ComplexStruct_SizeField()
        {
            int total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetSizeViaField<ComplexStruct>();
            }
            return total;
        }

        [Benchmark]
        public long RawLink_UnsafeSizeOf()
        {
            long total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetSizeViaUnsafeSizeOf<RawLink<TLinkAddress>>();
            }
            return total;
        }

        [Benchmark]
        public long RawLink_SizeField()
        {
            long total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetRawLinkSizeViaField();
            }
            return total;
        }

        [Benchmark]
        public long RawLink_PlatformUnsafeSize()
        {
            long total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetRawLinkSizeViaPlatformUnsafe();
            }
            return total;
        }

        [Benchmark]
        public long RawLinkDataPart_UnsafeSizeOf()
        {
            long total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetSizeViaUnsafeSizeOf<RawLinkDataPart<TLinkAddress>>();
            }
            return total;
        }

        [Benchmark]
        public long RawLinkDataPart_SizeField()
        {
            long total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetRawLinkDataPartSizeViaField();
            }
            return total;
        }

        [Benchmark]
        public long RawLinkDataPart_PlatformUnsafeSize()
        {
            long total = 0;
            for (int i = 0; i < OperationCount; i++)
            {
                total += GetRawLinkDataPartSizeViaPlatformUnsafe();
            }
            return total;
        }

        // Individual size retrieval methods with aggressive inlining
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetSizeViaUnsafeSizeOf<T>() where T : struct
        {
            return System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetSizeViaField<T>() where T : struct
        {
            if (typeof(T) == typeof(SimpleStruct))
            {
                return SimpleStruct.SizeField;
            }
            if (typeof(T) == typeof(ComplexStruct))
            {
                return ComplexStruct.SizeField;
            }
            throw new NotSupportedException($"Type {typeof(T)} not supported");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetRawLinkSizeViaField()
        {
            return RawLink<TLinkAddress>.SizeInBytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetRawLinkSizeViaPlatformUnsafe()
        {
            return Structure<RawLink<TLinkAddress>>.Size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetRawLinkDataPartSizeViaField()
        {
            return RawLinkDataPart<TLinkAddress>.SizeInBytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetRawLinkDataPartSizeViaPlatformUnsafe()
        {
            return Structure<RawLinkDataPart<TLinkAddress>>.Size;
        }
    }
}