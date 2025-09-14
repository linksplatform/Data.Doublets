using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq.Expressions;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class EqualityTests
    {
        [Fact]
        public static void GenericEqualityTest()
        {
            // Test IEqualityOperators approach (NET 7+) - recommended approach
            TestIEqualityOperatorsComparison<byte>();
            TestIEqualityOperatorsComparison<ushort>();
            TestIEqualityOperatorsComparison<uint>();
            TestIEqualityOperatorsComparison<ulong>();

            // Test EqualityComparer approach
            TestEqualityComparerComparison<byte>();
            TestEqualityComparerComparison<ushort>();
            TestEqualityComparerComparison<uint>();
            TestEqualityComparerComparison<ulong>();

            // Test dynamic approach - for comparison
            TestDynamicComparison<byte>();
            TestDynamicComparison<ushort>();
            TestDynamicComparison<uint>();
            TestDynamicComparison<ulong>();

            // Test expression-based approach
            TestExpressionComparison<byte>();
            TestExpressionComparison<ushort>();
            TestExpressionComparison<uint>();
            TestExpressionComparison<ulong>();
        }

        private static void TestIEqualityOperatorsComparison<T>() where T : IEqualityOperators<T, T, bool>
        {
            var a = GetOne<T>();
            var b = GetOne<T>();
            var c = GetTwo<T>();

            Assert.True(IEqualityOperatorsEquals(a, b));
            Assert.False(IEqualityOperatorsEquals(a, c));
        }

        private static void TestEqualityComparerComparison<T>()
        {
            var a = GetOne<T>();
            var b = GetOne<T>();
            var c = GetTwo<T>();

            Assert.True(EqualityComparerEquals(a, b));
            Assert.False(EqualityComparerEquals(a, c));
        }

        private static void TestDynamicComparison<T>()
        {
            var a = GetOne<T>();
            var b = GetOne<T>();
            var c = GetTwo<T>();

            Assert.True(DynamicEquals(a, b));
            Assert.False(DynamicEquals(a, c));
        }

        private static void TestExpressionComparison<T>()
        {
            var a = GetOne<T>();
            var b = GetOne<T>();
            var c = GetTwo<T>();

            Assert.True(ExpressionEquals(a, b));
            Assert.False(ExpressionEquals(a, c));
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
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, object> _equalityFunctions = 
            new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();

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

        // Helper methods to get typed values
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