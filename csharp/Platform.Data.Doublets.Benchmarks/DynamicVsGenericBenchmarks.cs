using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using BenchmarkDotNet.Attributes;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    /// <summary>
    /// Performance comparison between different approaches for generic operations.
    /// Based on https://stackoverflow.com/a/8122675/710069
    /// </summary>
    [SimpleJob]
    [MemoryDiagnoser]
    public class DynamicVsGenericBenchmarks
    {
        private const int Iterations = 1000;
        private readonly ulong[] _values;

        public DynamicVsGenericBenchmarks()
        {
            _values = new ulong[Iterations];
            var random = new System.Random(42);
            for (int i = 0; i < Iterations; i++)
            {
                _values[i] = (ulong)random.Next(1, 100);
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            // Warm up expression compilation
            ExpressionAdd(1UL, 2UL);
            ExpressionEquals(1UL, 2UL);
        }

        [Benchmark(Description = "INumber Addition")]
        public ulong INumberAddition()
        {
            ulong sum = 0;
            for (int i = 0; i < Iterations - 1; i++)
            {
                sum += INumberAdd(_values[i], _values[i + 1]);
            }
            return sum;
        }

        [Benchmark(Description = "Dynamic Addition")]
        public ulong DynamicAddition()
        {
            ulong sum = 0;
            for (int i = 0; i < Iterations - 1; i++)
            {
                sum += DynamicAdd(_values[i], _values[i + 1]);
            }
            return sum;
        }

        [Benchmark(Description = "Expression Addition")]
        public ulong ExpressionAddition()
        {
            ulong sum = 0;
            for (int i = 0; i < Iterations - 1; i++)
            {
                sum += ExpressionAdd(_values[i], _values[i + 1]);
            }
            return sum;
        }

        [Benchmark(Description = "IEqualityOperators Equality")]
        public int IEqualityOperatorsEquality()
        {
            int equalCount = 0;
            for (int i = 0; i < Iterations - 1; i++)
            {
                if (IEqualityOperatorsEquals(_values[i], _values[i + 1]))
                    equalCount++;
            }
            return equalCount;
        }

        [Benchmark(Description = "EqualityComparer Equality")]
        public int EqualityComparerEquality()
        {
            int equalCount = 0;
            for (int i = 0; i < Iterations - 1; i++)
            {
                if (EqualityComparerEquals(_values[i], _values[i + 1]))
                    equalCount++;
            }
            return equalCount;
        }

        [Benchmark(Description = "Dynamic Equality")]
        public int DynamicEquality()
        {
            int equalCount = 0;
            for (int i = 0; i < Iterations - 1; i++)
            {
                if (DynamicEquals(_values[i], _values[i + 1]))
                    equalCount++;
            }
            return equalCount;
        }

        [Benchmark(Description = "Expression Equality")]
        public int ExpressionEquality()
        {
            int equalCount = 0;
            for (int i = 0; i < Iterations - 1; i++)
            {
                if (ExpressionEquals(_values[i], _values[i + 1]))
                    equalCount++;
            }
            return equalCount;
        }

        // Implementation methods

        // INumber approach (NET 7+ - best performance)
        private static T INumberAdd<T>(T a, T b) where T : INumber<T>
        {
            return a + b;
        }

        // Dynamic approach (slowest)
        private static T DynamicAdd<T>(T a, T b)
        {
            dynamic dynamicA = a;
            dynamic dynamicB = b;
            dynamic result = dynamicA + dynamicB;
            return (T)Convert.ChangeType(result, typeof(T));
        }

        // Expression-based approach (moderate performance)
        private static readonly ConcurrentDictionary<Type, object> _addFunctions = 
            new ConcurrentDictionary<Type, object>();

        private static T ExpressionAdd<T>(T a, T b)
        {
            var addFunc = (Func<T, T, T>)_addFunctions.GetOrAdd(typeof(T), type =>
            {
                var paramA = Expression.Parameter(type, "a");
                var paramB = Expression.Parameter(type, "b");
                
                Expression left = paramA;
                Expression right = paramB;
                
                // For byte and other smaller types, convert to int for arithmetic
                if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
                {
                    left = Expression.Convert(paramA, typeof(int));
                    right = Expression.Convert(paramB, typeof(int));
                }
                
                Expression body = Expression.Add(left, right);
                
                // Convert back if needed
                if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
                {
                    body = Expression.Convert(body, type);
                }
                
                return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
            });

            return addFunc(a, b);
        }

        // IEqualityOperators approach (NET 7+ - best performance)
        private static bool IEqualityOperatorsEquals<T>(T a, T b) where T : IEqualityOperators<T, T, bool>
        {
            return a == b;
        }

        // EqualityComparer approach (good performance)
        private static bool EqualityComparerEquals<T>(T a, T b)
        {
            return EqualityComparer<T>.Default.Equals(a, b);
        }

        // Dynamic approach (slowest)
        private static bool DynamicEquals<T>(T a, T b)
        {
            dynamic dynamicA = a;
            dynamic dynamicB = b;
            return (bool)(dynamicA == dynamicB);
        }

        // Expression-based approach (moderate performance)
        private static readonly ConcurrentDictionary<Type, object> _equalityFunctions = 
            new ConcurrentDictionary<Type, object>();

        private static bool ExpressionEquals<T>(T a, T b)
        {
            var equalityFunc = (Func<T, T, bool>)_equalityFunctions.GetOrAdd(typeof(T), type =>
            {
                var paramA = Expression.Parameter(type, "a");
                var paramB = Expression.Parameter(type, "b");
                var body = Expression.Equal(paramA, paramB);
                return Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB).Compile();
            });

            return equalityFunc(a, b);
        }
    }
}