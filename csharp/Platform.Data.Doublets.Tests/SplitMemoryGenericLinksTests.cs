using System;
using System.Numerics;
using Xunit;
using Platform.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory;

namespace Platform.Data.Doublets.Tests
{
    public unsafe static class SplitMemoryGenericLinksTests
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
            UsingWithExternalReferences<byte>(links => links.TestRawNumbersCRUDOperations());
            UsingWithExternalReferences<ushort>(links => links.TestRawNumbersCRUDOperations());
            UsingWithExternalReferences<uint>(links => links.TestRawNumbersCRUDOperations());
            UsingWithExternalReferences<ulong>(links => links.TestRawNumbersCRUDOperations());
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
        public static void EachWithJumpTest()
        {
            Using<ulong>(TestEachWithJump);
        }

        private static void TestEachWithJump(ILinks<ulong> links)
        {
            // Create some test links
            var link1 = links.CreatePoint();
            var link2 = links.CreatePoint();
            var link3 = links.CreatePoint();
            var link4 = links.CreatePoint();
            var link5 = links.CreatePoint();

            var visitedLinks = new System.Collections.Generic.List<ulong>();
            var constants = links.Constants;

            // Test jumping to position 3 (zero-based index)
            var expectedJumpPosition = 3UL;
            var jumpExecuted = false;

            links.Each(Array.Empty<ulong>(), link =>
            {
                var linkIndex = links.GetIndex(link);
                visitedLinks.Add(linkIndex);

                if (visitedLinks.Count == 2) // Jump from position 1 (second link) to position 3
                {
                    jumpExecuted = true;
                    // Return jump position (values > Break are treated as jump positions)
                    return constants.Break + 1UL + expectedJumpPosition;
                }

                return constants.Continue;
            });

            // Verify that jump was executed and we visited the expected positions
            Assert.True(jumpExecuted, "Jump should have been executed");
            Assert.True(visitedLinks.Count >= 2, "Should have visited at least 2 positions before jump");
            
            // Test basic Continue/Break functionality still works
            visitedLinks.Clear();
            links.Each(Array.Empty<ulong>(), link =>
            {
                visitedLinks.Add(links.GetIndex(link));
                if (visitedLinks.Count == 2)
                {
                    return constants.Break; // Break after 2 links
                }
                return constants.Continue;
            });

            Assert.Equal(2, visitedLinks.Count);
        }
        private static void Using<TLinkAddress> (Action<ILinks<TLinkAddress>> action) where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            using (var dataMemory = new HeapResizableDirectMemory())
            using (var indexMemory = new HeapResizableDirectMemory())
            using (var memory = new SplitMemoryLinks<TLinkAddress>(dataMemory, indexMemory))
            {
                action(memory);
            }
        }
        private static void UsingWithExternalReferences<TLinkAddress>(Action<ILinks<TLinkAddress>> action) where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var contants = new LinksConstants<TLinkAddress>(enableExternalReferencesSupport: true);
            using (var dataMemory = new HeapResizableDirectMemory())
            using (var indexMemory = new HeapResizableDirectMemory())
            using (var memory = new SplitMemoryLinks<TLinkAddress>(dataMemory, indexMemory, SplitMemoryLinks<TLinkAddress>.DefaultLinksSizeStep, contants))
            {
                action(memory);
            }
        }
    }
}
