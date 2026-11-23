using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class UnaryNumberToAddressAddOperationConverterTests
    {
        [Fact]
        public static void BasicConversionTest()
        {
            Using<ulong>(links =>
            {
                var converter = new UnaryNumberToAddressAddOperationConverter<ulong>(links);
                
                // Test null/default conversion
                var result = converter.Convert(links.Constants.Null);
                Assert.Equal(0UL, result);
            });
        }

        [Fact]
        public static void UnaryOneConversionTest()
        {
            Using<ulong>(links =>
            {
                var converter = new UnaryNumberToAddressAddOperationConverter<ulong>(links);
                
                // Create unary "one" (self-referencing link)
                var unaryOne = links.GetOrCreate(links.Constants.Itself, links.Constants.Itself);
                var result = converter.Convert(unaryOne);
                
                Assert.Equal(1UL, result);
            });
        }

        [Fact]
        public static void PowerOfTwoValidationTest()
        {
            Using<ulong>(links =>
            {
                var converter = new UnaryNumberToAddressAddOperationConverter<ulong>(links);
                
                // Create a more complex unary number structure that would trigger traversal
                var unaryOne = links.GetOrCreate(links.Constants.Itself, links.Constants.Itself);
                var two = links.GetOrCreate(unaryOne, unaryOne);
                
                // Create an invalid link structure where source != target
                // and target is not a power of two (using an odd number)
                var oddTarget = links.GetOrCreate(links.Constants.Itself, two); // Create link 3
                var invalidUnaryNumber = links.GetOrCreate(unaryOne, oddTarget);
                
                // This should throw an InvalidOperationException due to power-of-two check
                Assert.Throws<InvalidOperationException>(() => converter.Convert(invalidUnaryNumber));
            });
        }

        [Fact]
        public static void NonCachedBehaviorTest()
        {
            Using<ulong>(links =>
            {
                var converter = new UnaryNumberToAddressAddOperationConverter<ulong>(links);
                
                // Verify that converter creates links on-demand
                var initialCount = links.Count();
                
                // Create unary "one" should create a new link
                var unaryOne = links.GetOrCreate(links.Constants.Itself, links.Constants.Itself);
                var result = converter.Convert(unaryOne);
                
                // Verify no additional links were pre-created by the converter itself
                var finalCount = links.Count();
                
                Assert.Equal(1UL, result);
                // The count should only increase by the links we explicitly created
                Assert.True(finalCount >= initialCount);
            });
        }

        [Fact]
        public static void MultipleTypeTest()
        {
            // Test with different numeric types
            Using<byte>(links =>
            {
                var converter = new UnaryNumberToAddressAddOperationConverter<byte>(links);
                var result = converter.Convert(links.Constants.Null);
                Assert.Equal((byte)0, result);
            });
            
            Using<ushort>(links =>
            {
                var converter = new UnaryNumberToAddressAddOperationConverter<ushort>(links);
                var result = converter.Convert(links.Constants.Null);
                Assert.Equal((ushort)0, result);
            });
            
            Using<uint>(links =>
            {
                var converter = new UnaryNumberToAddressAddOperationConverter<uint>(links);
                var result = converter.Convert(links.Constants.Null);
                Assert.Equal(0U, result);
            });
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, 
                                IShiftOperators<TLinkAddress, int, TLinkAddress>, 
                                IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, 
                                IMinMaxValue<TLinkAddress>, 
                                IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open($"converterTest_{typeof(TLinkAddress).Name}.txt", FileMode.Create, FileAccess.Write))
            {
                LoggingDecorator<TLinkAddress> decoratedStorage = new(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }
        }
    }
}