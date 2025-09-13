using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests.Converters
{
    public static class ByteArrayConvertersTests
    {
        [Fact]
        public static void ByteArrayToSequenceAndBackTest()
        {
            Using<uint>(links => TestByteArrayConversions(links));
            Using<ulong>(links => TestByteArrayConversions(links));
        }

        [Fact]
        public static void EmptyByteArrayTest()
        {
            Using<uint>(links => TestEmptyByteArray(links));
            Using<ulong>(links => TestEmptyByteArray(links));
        }

        [Fact]
        public static void SingleByteTest()
        {
            Using<uint>(links => TestSingleByte(links));
            Using<ulong>(links => TestSingleByte(links));
        }

        [Fact]
        public static void MultipleByteArraysTest()
        {
            Using<uint>(links => TestMultipleByteArrays(links));
            Using<ulong>(links => TestMultipleByteArrays(links));
        }

        [Fact]
        public static void ExtensionMethodsTest()
        {
            Using<uint>(links => TestExtensionMethods(links));
            Using<ulong>(links => TestExtensionMethods(links));
        }

        private static void TestByteArrayConversions<TLinkAddress>(ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var toSequenceConverter = new ByteArrayToSequenceConverter<TLinkAddress>(links);
            var toByteArrayConverter = new SequenceToByteArrayConverter<TLinkAddress>(links);

            // Test various byte arrays
            var testArrays = new byte[][]
            {
                new byte[] { 0x01, 0x02, 0x03 },
                new byte[] { 0xFF, 0x00, 0x80, 0x7F },
                new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, // "Hello" in ASCII
                new byte[] { 0x00 },
                new byte[] { 0xFF },
                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }
            };

            foreach (var original in testArrays)
            {
                var sequence = toSequenceConverter.Convert(original);
                Assert.NotEqual(links.Constants.Null, sequence);
                Assert.True(links.Exists(sequence));

                var converted = toByteArrayConverter.Convert(sequence);
                Assert.Equal(original.Length, converted.Length);
                
                for (int i = 0; i < original.Length; i++)
                {
                    Assert.Equal(original[i], converted[i]);
                }
            }
        }

        private static void TestEmptyByteArray<TLinkAddress>(ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var toSequenceConverter = new ByteArrayToSequenceConverter<TLinkAddress>(links);
            var toByteArrayConverter = new SequenceToByteArrayConverter<TLinkAddress>(links);

            var emptyArray = Array.Empty<byte>();
            var sequence = toSequenceConverter.Convert(emptyArray);
            
            Assert.Equal(links.Constants.Null, sequence);

            var converted = toByteArrayConverter.Convert(sequence);
            Assert.Empty(converted);
        }

        private static void TestSingleByte<TLinkAddress>(ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var toSequenceConverter = new ByteArrayToSequenceConverter<TLinkAddress>(links);
            var toByteArrayConverter = new SequenceToByteArrayConverter<TLinkAddress>(links);

            var singleByteArray = new byte[] { 0x42 };
            var sequence = toSequenceConverter.Convert(singleByteArray);
            
            Assert.NotEqual(links.Constants.Null, sequence);
            Assert.True(links.Exists(sequence));

            var converted = toByteArrayConverter.Convert(sequence);
            Assert.Single(converted);
            Assert.Equal(0x42, converted[0]);
        }

        private static void TestMultipleByteArrays<TLinkAddress>(ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var toSequenceConverter = new ByteArrayToSequenceConverter<TLinkAddress>(links);
            var toByteArrayConverter = new SequenceToByteArrayConverter<TLinkAddress>(links);

            var array1 = new byte[] { 0x01, 0x02 };
            var array2 = new byte[] { 0x03, 0x04 };
            var array3 = new byte[] { 0x01, 0x02 }; // Same as array1

            var sequence1 = toSequenceConverter.Convert(array1);
            var sequence2 = toSequenceConverter.Convert(array2);
            var sequence3 = toSequenceConverter.Convert(array3);

            // Different arrays should create different sequences
            Assert.NotEqual(sequence1, sequence2);
            
            // Same arrays should create the same sequence (due to GetOrCreate)
            Assert.Equal(sequence1, sequence3);

            // Verify conversions back
            var converted1 = toByteArrayConverter.Convert(sequence1);
            var converted2 = toByteArrayConverter.Convert(sequence2);

            Assert.Equal(array1, converted1);
            Assert.Equal(array2, converted2);
        }

        private static void TestExtensionMethods<TLinkAddress>(ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var originalArray = new byte[] { 0x10, 0x20, 0x30, 0x40 };
            
            var sequence = links.CreateSequenceFromByteArray(originalArray);
            Assert.NotEqual(links.Constants.Null, sequence);
            Assert.True(links.Exists(sequence));

            var convertedArray = links.ConvertSequenceToByteArray(sequence);
            Assert.Equal(originalArray, convertedArray);
        }

        [Fact]
        public static void NullArgumentsTest()
        {
            Using<uint>(links => TestNullArguments(links));
        }

        private static void TestNullArguments<TLinkAddress>(ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            // Test constructor with null links
            Assert.Throws<ArgumentNullException>(() => new ByteArrayToSequenceConverter<TLinkAddress>(null));
            Assert.Throws<ArgumentNullException>(() => new SequenceToByteArrayConverter<TLinkAddress>(null));

            // Test Convert method with null array
            var toSequenceConverter = new ByteArrayToSequenceConverter<TLinkAddress>(links);
            Assert.Throws<ArgumentNullException>(() => toSequenceConverter.Convert(null));
        }

        [Fact]
        public static void NonExistentSequenceTest()
        {
            Using<uint>(links => TestNonExistentSequence(links));
        }

        private static void TestNonExistentSequence<TLinkAddress>(ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var toByteArrayConverter = new SequenceToByteArrayConverter<TLinkAddress>(links);
            
            // Create a non-existent link address
            var nonExistentAddress = TLinkAddress.CreateTruncating(999999);
            
            Assert.Throws<ArgumentException>(() => toByteArrayConverter.Convert(nonExistentAddress));
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, 
                IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, 
                IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open($"byteArrayConvertersTest_{typeof(TLinkAddress).Name}.txt", FileMode.Create, FileAccess.Write))
            {
                var decoratedStorage = new LoggingDecorator<TLinkAddress>(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }
        }
    }
}