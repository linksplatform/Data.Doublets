using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Platform.Unsafe;
using Platform.Data.Doublets.Memory.United;
using Platform.Data.Doublets.Memory.Split;

namespace Experiments
{
    struct TestStruct
    {
        public static readonly int SizeField = System.Runtime.CompilerServices.Unsafe.SizeOf<TestStruct>();
        public ulong Value1;
        public ulong Value2;
        public ulong Value3;
        public ulong Value4;
    }

    class QuickSizeTest
    {
        static void Main()
        {
            const int iterations = 10_000_000;
            
            Console.WriteLine("=== Quick Size Comparison Test ===");
            Console.WriteLine($"Iterations: {iterations:N0}");
            Console.WriteLine();

            // Test simple struct
            TestSimpleStruct(iterations);
            Console.WriteLine();

            // Test RawLink
            TestRawLink(iterations);
            Console.WriteLine();

            // Test RawLinkDataPart
            TestRawLinkDataPart(iterations);
        }

        static void TestSimpleStruct(int iterations)
        {
            Console.WriteLine("--- TestStruct (4 x ulong) ---");
            
            // Test Unsafe.SizeOf method
            var sw = Stopwatch.StartNew();
            long totalSizeOf = 0;
            for (int i = 0; i < iterations; i++)
            {
                totalSizeOf += System.Runtime.CompilerServices.Unsafe.SizeOf<TestStruct>();
            }
            sw.Stop();
            var sizeOfTime = sw.Elapsed;
            
            // Test Size field
            sw.Restart();
            long totalField = 0;
            for (int i = 0; i < iterations; i++)
            {
                totalField += TestStruct.SizeField;
            }
            sw.Stop();
            var fieldTime = sw.Elapsed;
            
            Console.WriteLine($"Unsafe.SizeOf<T>(): {sizeOfTime.TotalMilliseconds:F3} ms (Result: {totalSizeOf / iterations})");
            Console.WriteLine($"Size field:        {fieldTime.TotalMilliseconds:F3} ms (Result: {totalField / iterations})");
            
            var ratio = sizeOfTime.TotalMilliseconds / fieldTime.TotalMilliseconds;
            Console.WriteLine($"Ratio (SizeOf/Field): {ratio:F2}x");
        }

        static void TestRawLink(int iterations)
        {
            Console.WriteLine("--- RawLink<ulong> (8 x ulong) ---");
            
            // Test Unsafe.SizeOf method
            var sw = Stopwatch.StartNew();
            long totalSizeOf = 0;
            for (int i = 0; i < iterations; i++)
            {
                totalSizeOf += System.Runtime.CompilerServices.Unsafe.SizeOf<RawLink<ulong>>();
            }
            sw.Stop();
            var sizeOfTime = sw.Elapsed;
            
            // Test Size field
            sw.Restart();
            long totalField = 0;
            for (int i = 0; i < iterations; i++)
            {
                totalField += RawLink<ulong>.SizeInBytes;
            }
            sw.Stop();
            var fieldTime = sw.Elapsed;
            
            // Test Platform.Unsafe.Structure approach  
            sw.Restart();
            long totalPlatformUnsafe = 0;
            for (int i = 0; i < iterations; i++)
            {
                totalPlatformUnsafe += Structure<RawLink<ulong>>.Size;
            }
            sw.Stop();
            var platformUnsafeTime = sw.Elapsed;
            
            Console.WriteLine($"Unsafe.SizeOf<T>():  {sizeOfTime.TotalMilliseconds:F3} ms (Result: {totalSizeOf / iterations})");
            Console.WriteLine($"Size field:         {fieldTime.TotalMilliseconds:F3} ms (Result: {totalField / iterations})");
            Console.WriteLine($"Platform.Unsafe:    {platformUnsafeTime.TotalMilliseconds:F3} ms (Result: {totalPlatformUnsafe / iterations})");
            
            var ratio1 = sizeOfTime.TotalMilliseconds / fieldTime.TotalMilliseconds;
            var ratio2 = platformUnsafeTime.TotalMilliseconds / fieldTime.TotalMilliseconds;
            Console.WriteLine($"Ratio (SizeOf/Field): {ratio1:F2}x");
            Console.WriteLine($"Ratio (Platform.Unsafe/Field): {ratio2:F2}x");
        }

        static void TestRawLinkDataPart(int iterations)
        {
            Console.WriteLine("--- RawLinkDataPart<ulong> (2 x ulong) ---");
            
            // Test Unsafe.SizeOf method
            var sw = Stopwatch.StartNew();
            long totalSizeOf = 0;
            for (int i = 0; i < iterations; i++)
            {
                totalSizeOf += System.Runtime.CompilerServices.Unsafe.SizeOf<RawLinkDataPart<ulong>>();
            }
            sw.Stop();
            var sizeOfTime = sw.Elapsed;
            
            // Test Size field
            sw.Restart();
            long totalField = 0;
            for (int i = 0; i < iterations; i++)
            {
                totalField += RawLinkDataPart<ulong>.SizeInBytes;
            }
            sw.Stop();
            var fieldTime = sw.Elapsed;
            
            // Test Platform.Unsafe.Structure approach  
            sw.Restart();
            long totalPlatformUnsafe = 0;
            for (int i = 0; i < iterations; i++)
            {
                totalPlatformUnsafe += Structure<RawLinkDataPart<ulong>>.Size;
            }
            sw.Stop();
            var platformUnsafeTime = sw.Elapsed;
            
            Console.WriteLine($"Unsafe.SizeOf<T>():  {sizeOfTime.TotalMilliseconds:F3} ms (Result: {totalSizeOf / iterations})");
            Console.WriteLine($"Size field:         {fieldTime.TotalMilliseconds:F3} ms (Result: {totalField / iterations})");
            Console.WriteLine($"Platform.Unsafe:    {platformUnsafeTime.TotalMilliseconds:F3} ms (Result: {totalPlatformUnsafe / iterations})");
            
            var ratio1 = sizeOfTime.TotalMilliseconds / fieldTime.TotalMilliseconds;
            var ratio2 = platformUnsafeTime.TotalMilliseconds / fieldTime.TotalMilliseconds;
            Console.WriteLine($"Ratio (SizeOf/Field): {ratio1:F2}x");
            Console.WriteLine($"Ratio (Platform.Unsafe/Field): {ratio2:F2}x");
        }
    }
}