using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Xunit;

using Platform.Memory;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Tests
{
    public static class SequenceOptimizationTests
    {
        [Fact]
        public static void ProcessTwoElementSequenceTest()
        {
            Using<uint>(links => TestProcessTwoElementSequence(links));
            Using<ulong>(links => TestProcessTwoElementSequence(links));
        }

        [Fact]
        public static void IsTwoElementSequenceTest()
        {
            Using<uint>(links => TestIsTwoElementSequence(links));
            Using<ulong>(links => TestIsTwoElementSequence(links));
        }

        private static void TestProcessTwoElementSequence<TLinkAddress>(ILinks<TLinkAddress> links) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            // Create two elements
            var element1 = links.CreatePoint();
            var element2 = links.CreatePoint();
            
            // Create a 2-element sequence (represented as a link with source=element1, target=element2)
            var sequence = links.GetOrCreate(element1, element2);
            
            // Test the optimization method - simulate combining elements like the LongRawNumberSequenceToNumberConverter
            var result = links.ProcessTwoElementSequence(sequence, 
                (acc, element) => TLinkAddress.CreateTruncating(ulong.CreateTruncating(acc) * 10 + ulong.CreateTruncating(element)), 
                TLinkAddress.Zero);
            
            // The result should be: 0 * 10 + element1, then (0 * 10 + element1) * 10 + element2
            var expectedResult = TLinkAddress.CreateTruncating(ulong.CreateTruncating(element1) * 10 + ulong.CreateTruncating(element2));
            
            Assert.Equal(expectedResult, result);
        }

        private static void TestIsTwoElementSequence<TLinkAddress>(ILinks<TLinkAddress> links) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            // Create two elements
            var element1 = links.CreatePoint();
            var element2 = links.CreatePoint();
            
            // Create a 2-element sequence
            var twoElementSequence = links.GetOrCreate(element1, element2);
            
            // Test that it's identified as a 2-element sequence
            Assert.True(links.IsTwoElementSequence(twoElementSequence));
            
            // Create a self-referencing link (not a valid 2-element sequence)
            var selfRef = links.GetOrCreate(element1, element1);
            
            // Test non-existent sequence
            var nonExistent = TLinkAddress.CreateTruncating(99999);
            Assert.False(links.IsTwoElementSequence(nonExistent));
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress,int,TLinkAddress>, 
                                 IBitwiseOperators<TLinkAddress,TLinkAddress,TLinkAddress>, IMinMaxValue<TLinkAddress>, 
                                 IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open("sequenceOptimizationTest.txt", FileMode.Create, FileAccess.Write))
            {
                LoggingDecorator<TLinkAddress> decoratedStorage = new(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }
        }
    }
}