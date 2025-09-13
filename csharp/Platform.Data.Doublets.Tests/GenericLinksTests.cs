using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Xunit;

using Platform.Memory;

using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Tests
{
    public static class GenericLinksTests
    {
        [Fact]
        public static void CRUDTest()
        {
            Using<byte>(links => links.TestCRUDOperations());
            Using<ushort>(links => links.TestCRUDOperations());
            Using<uint>(links => links.TestCRUDOperations());
            Using<ulong>(links => links.TestCRUDOperations());
        }

        [Fact]
        public static void RawNumbersCRUDTest()
        {
            Using<byte>(links => links.TestRawNumbersCRUDOperations());
            Using<ushort>(links => links.TestRawNumbersCRUDOperations());
            Using<uint>(links => links.TestRawNumbersCRUDOperations());
            Using<ulong>(links => links.TestRawNumbersCRUDOperations());
        }

        [Fact]
        public static void MultipleRandomCreationsAndDeletionsTest()
        {
            Using<byte>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(16)); // Cannot use more because current implementation of tree cuts out 5 bits from the address space.
            Using<ushort>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
            Using<uint>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
            Using<ulong>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
        }

        [Fact]
        public static void SingleCharacterSequenceWithDeduplicationTest()
        {
            Using<byte>(links => TestSingleCharacterSequenceWithDeduplication(links));
            Using<ushort>(links => TestSingleCharacterSequenceWithDeduplication(links));
            Using<uint>(links => TestSingleCharacterSequenceWithDeduplication(links));
            Using<ulong>(links => TestSingleCharacterSequenceWithDeduplication(links));
        }

        private static void TestSingleCharacterSequenceWithDeduplication<TLinkAddress>(ILinks<TLinkAddress> links) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress,int,TLinkAddress>, IBitwiseOperators<TLinkAddress,TLinkAddress,TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            // Create a single character/element that will be used to build sequences
            var character = links.CreatePoint();

            // Test 1: Create sequences without deduplication - each link should be unique even if content is the same
            var sequenceWithoutDedup1 = links.CreateAndUpdate(character, character);
            var sequenceWithoutDedup2 = links.CreateAndUpdate(character, character);
            var sequenceWithoutDedup3 = links.CreateAndUpdate(character, character);

            // All three should be different link addresses despite having same content (without deduplication active)
            Assert.NotEqual(sequenceWithoutDedup1, sequenceWithoutDedup2);
            Assert.NotEqual(sequenceWithoutDedup2, sequenceWithoutDedup3);
            Assert.NotEqual(sequenceWithoutDedup1, sequenceWithoutDedup3);

            // Test 2: Test deduplication using GetOrCreate method which checks for existing links
            var sequenceGetOrCreate1 = links.GetOrCreate(character, character);
            var sequenceGetOrCreate2 = links.GetOrCreate(character, character);

            // GetOrCreate should return the same address for identical content
            Assert.Equal(sequenceGetOrCreate1, sequenceGetOrCreate2);

            // Test 3: Test with advanced deduplication (automatic uniqueness and usages resolution)
            var linksWithAdvancedDeduplication = links.DecorateWithAutomaticUniquenessAndUsagesResolution();
            var sequenceWithAdvancedDedup1 = linksWithAdvancedDeduplication.GetOrCreate(character, character);
            var sequenceWithAdvancedDedup2 = linksWithAdvancedDeduplication.GetOrCreate(character, character);

            // With advanced deduplication, should also return same address
            Assert.Equal(sequenceWithAdvancedDedup1, sequenceWithAdvancedDedup2);

            // Test 4: Create longer sequences full of the same character
            var longerSequence1 = links.CreateAndUpdate(character, sequenceWithoutDedup1);
            var longerSequence2 = links.CreateAndUpdate(character, sequenceWithoutDedup1);

            // Without deduplication, even with same elements, different addresses
            Assert.NotEqual(longerSequence1, longerSequence2);

            // Test 5: Test the same pattern with GetOrCreate for deduplication
            var longerSequenceDedup1 = links.GetOrCreate(character, sequenceGetOrCreate1);
            var longerSequenceDedup2 = links.GetOrCreate(character, sequenceGetOrCreate1);

            // With GetOrCreate, same content should result in same address
            Assert.Equal(longerSequenceDedup1, longerSequenceDedup2);

            // Test 6: Verify that we can create sequences full of single character elements
            // Create a sequence where both source and target are the same character
            var singleCharSequence = linksWithAdvancedDeduplication.GetOrCreate(character, character);
            
            // Create another sequence using the first sequence as source, character as target (building a chain)
            var extendedSequence = linksWithAdvancedDeduplication.GetOrCreate(singleCharSequence, character);
            
            // Verify the sequence content
            var singleCharSequenceLink = linksWithAdvancedDeduplication.GetLink(singleCharSequence);
            Assert.Equal(character, linksWithAdvancedDeduplication.GetSource(singleCharSequenceLink));
            Assert.Equal(character, linksWithAdvancedDeduplication.GetTarget(singleCharSequenceLink));
            
            var extendedSequenceLink = linksWithAdvancedDeduplication.GetLink(extendedSequence);
            Assert.Equal(singleCharSequence, linksWithAdvancedDeduplication.GetSource(extendedSequenceLink));
            Assert.Equal(character, linksWithAdvancedDeduplication.GetTarget(extendedSequenceLink));

            // Test 7: Demonstrate sequence full of single character by creating repetitive patterns
            // All these should be the same sequence since they have identical content
            var repeatedCharSequence1 = linksWithAdvancedDeduplication.GetOrCreate(character, character);
            var repeatedCharSequence2 = linksWithAdvancedDeduplication.GetOrCreate(character, character);
            var repeatedCharSequence3 = linksWithAdvancedDeduplication.GetOrCreate(character, character);
            
            Assert.Equal(repeatedCharSequence1, repeatedCharSequence2);
            Assert.Equal(repeatedCharSequence2, repeatedCharSequence3);

            // The test demonstrates different deduplication methods for sequences of single characters:
            // 1. CreateAndUpdate: Always creates new links (no deduplication)
            // 2. GetOrCreate: Returns existing link if found, creates new if not (built-in deduplication)
            // 3. Advanced deduplication decorators: Apply additional uniqueness and usage resolution
        }
        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) where TLinkAddress  : IUnsignedNumber<TLinkAddress> , IShiftOperators<TLinkAddress,int,TLinkAddress>, IBitwiseOperators<TLinkAddress,TLinkAddress,TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open("linksLogger.txt", FileMode.Create, FileAccess.Write))
            {
                LoggingDecorator<TLinkAddress> decoratedStorage = new(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }

            /*
            File.Delete("db.links");
            using var ffiLinks = new Ffi.Links<TLinkAddress>("db.links");
            action(ffiLinks);
        */
        }
    }
}
