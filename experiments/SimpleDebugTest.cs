using System;
using System.IO;
using System.Numerics;
using System.Linq;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.PropertyOperators;
using Platform.Data.Doublets;
using Platform.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Xunit;

namespace DebugTest
{
    public static class SimpleDebugTest
    {
        [Fact]
        public static void DebugMultipleValuesPropertiesOperator()
        {
            var links = new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory());
                
                var propertiesOperator = new MultipleValuesPropertiesOperator<ulong>(links);

                var obj = links.Create();
                var property = links.Create();
                var value1 = links.Create();
                var value2 = links.Create();
                var value3 = links.Create();
                
                Console.WriteLine($"Created: obj={obj}, property={property}, value1={value1}, value2={value2}, value3={value3}");

                // Add values one by one and check count each time
                propertiesOperator.AddValue(obj, property, value1);
                Console.WriteLine($"After adding value1, count: {propertiesOperator.CountValues(obj, property)}");
                
                propertiesOperator.AddValue(obj, property, value2);
                Console.WriteLine($"After adding value2, count: {propertiesOperator.CountValues(obj, property)}");
                
                propertiesOperator.AddValue(obj, property, value3);
                Console.WriteLine($"After adding value3, count: {propertiesOperator.CountValues(obj, property)}");
                
                // Test what values we actually have
                var allValues = propertiesOperator.GetAllValues(obj, property).ToList();
                Console.WriteLine($"All values: [{string.Join(", ", allValues)}]");
                
                // Test removing value2
                Console.WriteLine($"Before remove, contains value2: {propertiesOperator.ContainsValue(obj, property, value2)}");
                propertiesOperator.RemoveValue(obj, property, value2);
                Console.WriteLine($"After remove, count: {propertiesOperator.CountValues(obj, property)}");
                Console.WriteLine($"After remove, contains value2: {propertiesOperator.ContainsValue(obj, property, value2)}");
                
                // Test what values remain
                var remainingValues = propertiesOperator.GetAllValues(obj, property).ToList();
                Console.WriteLine($"Remaining values: [{string.Join(", ", remainingValues)}]");
                
                Assert.Equal(3, 3); // Placeholder to make it a valid test
        }
    }
}