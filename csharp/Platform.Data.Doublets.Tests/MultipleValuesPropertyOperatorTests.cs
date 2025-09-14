using System;
using System.IO;
using System.Numerics;
using System.Linq;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.PropertyOperators;
using Xunit;

using Platform.Memory;

using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Tests
{
    public static class MultipleValuesPropertyOperatorTests
    {
        [Fact]
        public static void MultipleValuesPropertyOperatorTest()
        {
            Using<ulong>(links =>
            {
                var propertyMarker = links.Create();
                var propertyValueMarker = links.Create();
                var propertyOperator = new MultipleValuesPropertyOperator<ulong>(links, propertyMarker, propertyValueMarker);

                // Test basic functionality
                var link = links.Create();
                var value1 = links.Create();
                var value2 = links.Create();
                var value3 = links.Create();

                // Test adding values
                propertyOperator.Add(link, value1);
                propertyOperator.Add(link, value2);
                propertyOperator.Add(link, value3);

                // Test counting values
                Assert.Equal(3, propertyOperator.Count(link));

                // Test getting all values
                var allValues = propertyOperator.GetAll(link).ToList();
                Assert.Equal(3, allValues.Count);
                Assert.Contains(value1, allValues);
                Assert.Contains(value2, allValues);
                Assert.Contains(value3, allValues);

                // Test getting first value
                var firstValue = propertyOperator.GetFirst(link);
                Assert.True(firstValue == value1 || firstValue == value2 || firstValue == value3);

                // Test contains functionality
                Assert.True(propertyOperator.Contains(link, value1));
                Assert.True(propertyOperator.Contains(link, value2));
                Assert.True(propertyOperator.Contains(link, value3));

                var nonExistentValue = links.Create();
                Assert.False(propertyOperator.Contains(link, nonExistentValue));

                // Test removing a value
                propertyOperator.Remove(link, value2);
                Assert.Equal(2, propertyOperator.Count(link));
                Assert.False(propertyOperator.Contains(link, value2));
                Assert.True(propertyOperator.Contains(link, value1));
                Assert.True(propertyOperator.Contains(link, value3));

                // Test adding duplicate value (should not increase count)
                propertyOperator.Add(link, value1);
                Assert.Equal(2, propertyOperator.Count(link));

                // Test setting values (replace all)
                var newValue1 = links.Create();
                var newValue2 = links.Create();
                propertyOperator.Set(link, new[] { newValue1, newValue2 });

                Assert.Equal(2, propertyOperator.Count(link));
                Assert.True(propertyOperator.Contains(link, newValue1));
                Assert.True(propertyOperator.Contains(link, newValue2));
                Assert.False(propertyOperator.Contains(link, value1));
                Assert.False(propertyOperator.Contains(link, value3));

                // Test clearing all values
                propertyOperator.Clear(link);
                Assert.Equal(0, propertyOperator.Count(link));
                Assert.False(propertyOperator.Contains(link, newValue1));
                Assert.False(propertyOperator.Contains(link, newValue2));

                // Test operations on non-existent property
                var emptyLink = links.Create();
                Assert.Equal(0, propertyOperator.Count(emptyLink));
                Assert.Equal(default(ulong), propertyOperator.GetFirst(emptyLink));
                Assert.False(propertyOperator.Contains(emptyLink, value1));

                // Test remove on non-existent property (should not throw)
                propertyOperator.Remove(emptyLink, value1);
                Assert.Equal(0, propertyOperator.Count(emptyLink));
            });
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open("multipleValuesPropertyOperatorTest.txt", FileMode.Create, FileAccess.Write))
            {
                LoggingDecorator<TLinkAddress> decoratedStorage = new(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }
        }
    }
}