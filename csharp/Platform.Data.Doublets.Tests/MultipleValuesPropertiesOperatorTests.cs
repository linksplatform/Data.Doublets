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
    public static class MultipleValuesPropertiesOperatorTests
    {
        [Fact]
        public static void MultipleValuesPropertiesOperatorTest()
        {
            Using<ulong>(links =>
            {
                var propertiesOperator = new MultipleValuesPropertiesOperator<ulong>(links);

                // Test basic functionality
                var obj = links.Create();
                var property = links.Create();
                var value1 = links.Create();
                var value2 = links.Create();
                var value3 = links.Create();

                // Test adding values
                propertiesOperator.AddValue(obj, property, value1);
                propertiesOperator.AddValue(obj, property, value2);
                propertiesOperator.AddValue(obj, property, value3);

                // Test counting values
                Assert.Equal(3, propertiesOperator.CountValues(obj, property));

                // Test getting all values
                var allValues = propertiesOperator.GetAllValues(obj, property).ToList();
                Assert.Equal(3, allValues.Count);
                Assert.Contains(value1, allValues);
                Assert.Contains(value2, allValues);
                Assert.Contains(value3, allValues);

                // Test getting first value
                var firstValue = propertiesOperator.GetFirstValue(obj, property);
                Assert.True(firstValue == value1 || firstValue == value2 || firstValue == value3);

                // Test contains functionality
                Assert.True(propertiesOperator.ContainsValue(obj, property, value1));
                Assert.True(propertiesOperator.ContainsValue(obj, property, value2));
                Assert.True(propertiesOperator.ContainsValue(obj, property, value3));

                var nonExistentValue = links.Create();
                Assert.False(propertiesOperator.ContainsValue(obj, property, nonExistentValue));

                // Test removing a value
                propertiesOperator.RemoveValue(obj, property, value2);
                Assert.Equal(2, propertiesOperator.CountValues(obj, property));
                Assert.False(propertiesOperator.ContainsValue(obj, property, value2));
                Assert.True(propertiesOperator.ContainsValue(obj, property, value1));
                Assert.True(propertiesOperator.ContainsValue(obj, property, value3));

                // Test adding duplicate value (should not increase count)
                propertiesOperator.AddValue(obj, property, value1);
                Assert.Equal(2, propertiesOperator.CountValues(obj, property));

                // Test setting values (replace all)
                var newValue1 = links.Create();
                var newValue2 = links.Create();
                propertiesOperator.SetAllValues(obj, property, new[] { newValue1, newValue2 });

                Assert.Equal(2, propertiesOperator.CountValues(obj, property));
                Assert.True(propertiesOperator.ContainsValue(obj, property, newValue1));
                Assert.True(propertiesOperator.ContainsValue(obj, property, newValue2));
                Assert.False(propertiesOperator.ContainsValue(obj, property, value1));
                Assert.False(propertiesOperator.ContainsValue(obj, property, value3));

                // Test clearing all values
                propertiesOperator.ClearAllValues(obj, property);
                Assert.Equal(0, propertiesOperator.CountValues(obj, property));
                Assert.False(propertiesOperator.ContainsValue(obj, property, newValue1));
                Assert.False(propertiesOperator.ContainsValue(obj, property, newValue2));

                // Test operations on non-existent object-property combination
                var emptyObj = links.Create();
                var emptyProperty = links.Create();
                Assert.Equal(0, propertiesOperator.CountValues(emptyObj, emptyProperty));
                Assert.Equal(default(ulong), propertiesOperator.GetFirstValue(emptyObj, emptyProperty));
                Assert.False(propertiesOperator.ContainsValue(emptyObj, emptyProperty, value1));

                // Test remove on non-existent combination (should not throw)
                propertiesOperator.RemoveValue(emptyObj, emptyProperty, value1);
                Assert.Equal(0, propertiesOperator.CountValues(emptyObj, emptyProperty));

                // Test multiple objects with same property
                var obj2 = links.Create();
                var obj2Value1 = links.Create();
                var obj2Value2 = links.Create();

                propertiesOperator.AddValue(obj2, property, obj2Value1);
                propertiesOperator.AddValue(obj2, property, obj2Value2);

                // obj1 should still have no values (cleared above)
                Assert.Equal(0, propertiesOperator.CountValues(obj, property));
                
                // obj2 should have 2 values
                Assert.Equal(2, propertiesOperator.CountValues(obj2, property));
                Assert.True(propertiesOperator.ContainsValue(obj2, property, obj2Value1));
                Assert.True(propertiesOperator.ContainsValue(obj2, property, obj2Value2));

                // obj2 should not contain obj1's values
                Assert.False(propertiesOperator.ContainsValue(obj2, property, value1));
                Assert.False(propertiesOperator.ContainsValue(obj2, property, value3));
            });
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open("multipleValuesPropertiesOperatorTest.txt", FileMode.Create, FileAccess.Write))
            {
                LoggingDecorator<TLinkAddress> decoratedStorage = new(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }
        }
    }
}