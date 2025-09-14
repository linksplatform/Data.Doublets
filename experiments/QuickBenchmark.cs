using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Platform.Converters;
using Platform.Numbers;

namespace QuickBenchmark
{
    /// <summary>
    /// Quick micro-benchmark to test different approaches for checking bit flags
    /// representing child existence in AVL tree nodes.
    /// </summary>
    class Program
    {
        private static readonly UncheckedConverter<ulong, bool> _addressToBoolConverter = UncheckedConverter<ulong, bool>.Default;
        private const int Iterations = 1_000_000; 
        
        static void Main(string[] args)
        {
            Console.WriteLine("Performance Comparison for Issue #85: IsChild Check Approaches");
            Console.WriteLine("=========================================================================");
            
            // Create test data - variety of bit patterns
            var testValues = new ulong[1000];
            for (int i = 0; i < testValues.Length; i++)
            {
                testValues[i] = (ulong)(i * 17 + 42);
            }
            
            // Warm up the JIT
            Console.WriteLine("Warming up JIT compiler...");
            RunEqualityComparerApproach(testValues, 1000);
            RunEqualityComparerUncheckedApproach(testValues, 1000);
            RunUncheckedConverterApproach(testValues, 1000);
            RunDirectBitCheckApproach(testValues, 1000);
            RunDirectBitCheckUncheckedApproach(testValues, 1000);
            
            Console.WriteLine("Running benchmarks...\n");
            
            // Test 1: EqualityComparer approach (GitHub issue line 56)
            var sw = Stopwatch.StartNew();
            var result1 = RunEqualityComparerApproach(testValues, Iterations);
            sw.Stop();
            Console.WriteLine($"1. EqualityComparer Approach:               {sw.ElapsedMilliseconds} ms (result: {result1})");
            
            // Test 2: EqualityComparer with unchecked (GitHub issue line 78)
            sw.Restart();
            var result2 = RunEqualityComparerUncheckedApproach(testValues, Iterations);
            sw.Stop();
            Console.WriteLine($"2. EqualityComparer Unchecked Approach:     {sw.ElapsedMilliseconds} ms (result: {result2})");
            
            // Test 3: UncheckedConverter approach (current implementation)
            sw.Restart();
            var result3 = RunUncheckedConverterApproach(testValues, Iterations);
            sw.Stop();
            Console.WriteLine($"3. UncheckedConverter Approach (Current):   {sw.ElapsedMilliseconds} ms (result: {result3})");
            
            // Test 4: Direct bit manipulation (optimal)
            sw.Restart();
            var result4 = RunDirectBitCheckApproach(testValues, Iterations);
            sw.Stop();
            Console.WriteLine($"4. Direct Bit Check Approach:              {sw.ElapsedMilliseconds} ms (result: {result4})");
            
            // Test 5: Direct bit manipulation with unchecked
            sw.Restart();
            var result5 = RunDirectBitCheckUncheckedApproach(testValues, Iterations);
            sw.Stop();
            Console.WriteLine($"5. Direct Bit Check Unchecked Approach:    {sw.ElapsedMilliseconds} ms (result: {result5})");
            
            Console.WriteLine("\n=========================================================================");
            Console.WriteLine("Analysis:");
            Console.WriteLine("- Lower numbers indicate better performance");
            Console.WriteLine("- All results should be identical to ensure correctness");
            Console.WriteLine("- Direct bit manipulation should be fastest");
            Console.WriteLine("- Unchecked keyword may provide minor improvement in some cases");
        }
        
        static bool RunEqualityComparerApproach(ulong[] testValues, int iterations)
        {
            bool result = false;
            int testIndex = 0;
            for (int i = 0; i < iterations; i++)
            {
                var value = testValues[testIndex];
                // Check bit at position 4 (left child)
                var bitValue = Bit<ulong>.PartialRead(target: value, shift: 4, limit: 1);
                result ^= !EqualityComparer<ulong>.Default.Equals(bitValue, default);
                
                testIndex = (testIndex + 1) % testValues.Length;
            }
            return result;
        }
        
        static bool RunEqualityComparerUncheckedApproach(ulong[] testValues, int iterations)
        {
            bool result = false;
            int testIndex = 0;
            for (int i = 0; i < iterations; i++)
            {
                var value = testValues[testIndex];
                unchecked
                {
                    // Check bit at position 3 (right child)
                    var bitValue = Bit<ulong>.PartialRead(target: value, shift: 3, limit: 1);
                    result ^= !EqualityComparer<ulong>.Default.Equals(bitValue, default);
                }
                
                testIndex = (testIndex + 1) % testValues.Length;
            }
            return result;
        }
        
        static bool RunUncheckedConverterApproach(ulong[] testValues, int iterations)
        {
            bool result = false;
            int testIndex = 0;
            for (int i = 0; i < iterations; i++)
            {
                var value = testValues[testIndex];
                // Current implementation approach
                var bitValue = Bit<ulong>.PartialRead(target: value, shift: 4, limit: 1);
                result ^= _addressToBoolConverter.Convert(source: bitValue);
                
                testIndex = (testIndex + 1) % testValues.Length;
            }
            return result;
        }
        
        static bool RunDirectBitCheckApproach(ulong[] testValues, int iterations)
        {
            bool result = false;
            int testIndex = 0;
            for (int i = 0; i < iterations; i++)
            {
                var value = testValues[testIndex];
                // Direct bit manipulation - most optimal approach
                result ^= ((value >> 4) & 1) != 0;
                
                testIndex = (testIndex + 1) % testValues.Length;
            }
            return result;
        }
        
        static bool RunDirectBitCheckUncheckedApproach(ulong[] testValues, int iterations)
        {
            bool result = false;
            int testIndex = 0;
            for (int i = 0; i < iterations; i++)
            {
                var value = testValues[testIndex];
                unchecked
                {
                    // Direct bit manipulation with unchecked
                    result ^= ((value >> 4) & 1) != 0;
                }
                
                testIndex = (testIndex + 1) % testValues.Length;
            }
            return result;
        }
    }
}