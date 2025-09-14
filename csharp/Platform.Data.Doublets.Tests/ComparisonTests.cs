using System;
using System.Numerics;
using System.Linq.Expressions;
using Xunit;
using Platform.Converters;

namespace Platform.Data.Doublets.Tests
{
    public static class ComparisonTests
    {
        [Fact]
        public static void GenericAdditionTest()
        {
            // Test INumber approach (NET 7+) - recommended approach
            TestINumberAddition<byte>();
            TestINumberAddition<ushort>();
            TestINumberAddition<uint>();
            TestINumberAddition<ulong>();

            // Test dynamic approach - for comparison
            TestDynamicAddition<byte>();
            TestDynamicAddition<ushort>();
            TestDynamicAddition<uint>();
            TestDynamicAddition<ulong>();

            // Test expression-based approach
            TestExpressionAddition<byte>();
            TestExpressionAddition<ushort>();
            TestExpressionAddition<uint>();
            TestExpressionAddition<ulong>();
        }

        private static void TestINumberAddition<T>() where T : INumber<T>
        {
            var a = T.One;
            var b = T.One;
            var result = INumberAdd(a, b);
            var expected = T.One + T.One;
            Assert.Equal(expected, result);
        }

        private static void TestDynamicAddition<T>()
        {
            var a = GetOne<T>();
            var b = GetOne<T>();
            var result = DynamicAdd(a, b);
            var expected = GetTwo<T>();
            Assert.Equal(expected, result);
        }

        private static void TestExpressionAddition<T>()
        {
            var a = GetOne<T>();
            var b = GetOne<T>();
            var result = ExpressionAdd(a, b);
            var expected = GetTwo<T>();
            Assert.Equal(expected, result);
        }

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
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, object> _addFunctions = 
            new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();

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

        // Helper methods to get typed values without INumber constraint
        private static T GetOne<T>()
        {
            return (T)Convert.ChangeType(1, typeof(T));
        }

        private static T GetTwo<T>()
        {
            return (T)Convert.ChangeType(2, typeof(T));
        }
    }
}