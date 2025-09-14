using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using TLinkAddress = System.UInt64;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    [SimpleJob]
    [MemoryDiagnoser]
    public class GetZeroVsZeroFieldBenchmarks
    {
        [Params(1000, 10000, 100000, 1000000)]
        public int N;

        private TLinkAddress _accumulator;

        [GlobalSetup]
        public void Setup()
        {
            _accumulator = default;
        }

        [Benchmark]
        public TLinkAddress GetZeroMethodWithInlining()
        {
            var sum = _accumulator;
            for (int i = 0; i < N; i++)
            {
                sum += GetZero();
            }
            return sum;
        }

        [Benchmark]
        public TLinkAddress ZeroLiteral()
        {
            var sum = _accumulator;
            for (int i = 0; i < N; i++)
            {
                sum += 0UL;
            }
            return sum;
        }

        [Benchmark]
        public TLinkAddress DefaultKeyword()
        {
            var sum = _accumulator;
            for (int i = 0; i < N; i++)
            {
                sum += default(TLinkAddress);
            }
            return sum;
        }

        [Benchmark]
        public TLinkAddress GetZeroMethodWithoutInlining()
        {
            var sum = _accumulator;
            for (int i = 0; i < N; i++)
            {
                sum += GetZeroNoInlining();
            }
            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TLinkAddress GetZero()
        {
            return default;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TLinkAddress GetZeroNoInlining()
        {
            return default;
        }

        // Real-world usage scenarios based on actual codebase patterns
        
        [Benchmark]
        public bool ComparisonWithGetZero()
        {
            var result = false;
            for (int i = 0; i < N; i++)
            {
                var value = (TLinkAddress)(i % 100);
                result ^= (value == GetZero());
            }
            return result;
        }

        [Benchmark]
        public bool ComparisonWithZeroLiteral()
        {
            var result = false;
            for (int i = 0; i < N; i++)
            {
                var value = (TLinkAddress)(i % 100);
                result ^= (value == 0UL);
            }
            return result;
        }

        [Benchmark]
        public TLinkAddress ConditionalWithGetZero()
        {
            var sum = _accumulator;
            for (int i = 0; i < N; i++)
            {
                var value = (TLinkAddress)(i % 100);
                sum += (value == GetZero()) ? GetZero() : value;
            }
            return sum;
        }

        [Benchmark]
        public TLinkAddress ConditionalWithZeroLiteral()
        {
            var sum = _accumulator;
            for (int i = 0; i < N; i++)
            {
                var value = (TLinkAddress)(i % 100);
                sum += (value == 0UL) ? 0UL : value;
            }
            return sum;
        }

        // Memory allocation scenarios
        [Benchmark]
        public TLinkAddress[] ArrayInitializationWithGetZero()
        {
            var array = new TLinkAddress[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = GetZero();
            }
            return array;
        }

        [Benchmark]
        public TLinkAddress[] ArrayInitializationWithZeroLiteral()
        {
            var array = new TLinkAddress[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = 0UL;
            }
            return array;
        }
    }
}