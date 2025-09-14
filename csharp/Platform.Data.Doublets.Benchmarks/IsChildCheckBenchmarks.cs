using System.Collections.Generic;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using Platform.Converters;
using Platform.Numbers;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    /// <summary>
    /// Benchmarks to determine the best performance approach for checking if a bit flag indicates a child exists.
    /// This benchmark compares three different implementations:
    /// 1. EqualityComparer approach (from GitHub issue line 56)
    /// 2. EqualityComparer with unchecked (from GitHub issue line 78) 
    /// 3. UncheckedConverter approach (current implementation)
    /// </summary>
    [SimpleJob(warmupCount: 1, targetCount: 3)]
    [MemoryDiagnoser]
    public class IsChildCheckBenchmarks
    {
        private const int IterationCount = 1000;
        private static readonly UncheckedConverter<ulong, bool> _addressToBoolConverter = UncheckedConverter<ulong, bool>.Default;
        private readonly ulong[] _testValues;

        public IsChildCheckBenchmarks()
        {
            // Create a variety of test values with different bit patterns
            _testValues = new ulong[IterationCount];
            for (int i = 0; i < IterationCount; i++)
            {
                _testValues[i] = (ulong)(i * 17 + 42); // Mix of values with various bit patterns
            }
        }

        [Benchmark(Baseline = true)]
        public bool EqualityComparerApproach()
        {
            bool result = false;
            for (int i = 0; i < _testValues.Length; i++)
            {
                var value = _testValues[i];
                // Simulate GetLeftIsChild - check bit at position 4
                var bitValue = Bit<ulong>.PartialRead(target: value, shift: 4, limit: 1);
                result ^= !EqualityComparer<ulong>.Default.Equals(bitValue, default);
            }
            return result;
        }

        [Benchmark]
        public bool EqualityComparerUncheckedApproach()
        {
            bool result = false;
            for (int i = 0; i < _testValues.Length; i++)
            {
                var value = _testValues[i];
                unchecked
                {
                    // Simulate GetRightIsChild - check bit at position 3  
                    var bitValue = Bit<ulong>.PartialRead(target: value, shift: 3, limit: 1);
                    result ^= !EqualityComparer<ulong>.Default.Equals(bitValue, default);
                }
            }
            return result;
        }

        [Benchmark]
        public bool UncheckedConverterApproach()
        {
            bool result = false;
            for (int i = 0; i < _testValues.Length; i++)
            {
                var value = _testValues[i];
                // Current implementation approach
                var bitValue = Bit<ulong>.PartialRead(target: value, shift: 4, limit: 1);
                result ^= _addressToBoolConverter.Convert(source: bitValue);
            }
            return result;
        }

        [Benchmark]
        public bool DirectBitCheckApproach()
        {
            bool result = false;
            for (int i = 0; i < _testValues.Length; i++)
            {
                var value = _testValues[i];
                // Direct bit manipulation - most optimal approach
                result ^= ((value >> 4) & 1) != 0;
            }
            return result;
        }

        [Benchmark]
        public bool DirectBitCheckUncheckedApproach()
        {
            bool result = false;
            for (int i = 0; i < _testValues.Length; i++)
            {
                var value = _testValues[i];
                unchecked
                {
                    // Direct bit manipulation with unchecked - test if unchecked helps
                    result ^= ((value >> 4) & 1) != 0;
                }
            }
            return result;
        }

        // Additional benchmark for the right child bit (position 3)
        [Benchmark]
        public bool RightChildEqualityComparerApproach()
        {
            bool result = false;
            for (int i = 0; i < _testValues.Length; i++)
            {
                var value = _testValues[i];
                var bitValue = Bit<ulong>.PartialRead(target: value, shift: 3, limit: 1);
                result ^= !EqualityComparer<ulong>.Default.Equals(bitValue, default);
            }
            return result;
        }

        [Benchmark]
        public bool RightChildUncheckedConverterApproach()
        {
            bool result = false;
            for (int i = 0; i < _testValues.Length; i++)
            {
                var value = _testValues[i];
                var bitValue = Bit<ulong>.PartialRead(target: value, shift: 3, limit: 1);
                result ^= _addressToBoolConverter.Convert(source: bitValue);
            }
            return result;
        }
    }
}