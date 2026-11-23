using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Xunit;
using Platform.Memory;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Tests
{
    public static class BitByBitSequenceAddressingTests
    {
        [Fact]
        public static void GetBitByBitSequenceElementByIndexTest()
        {
            Using<ulong>(TestBitByBitAddressing);
        }

        [Fact]
        public static void GetBitByBitSequenceElementByIndexWithLargeIndicesTest()
        {
            Using<ulong>(TestBitByBitAddressingWithLargeIndices);
        }

        private static void TestBitByBitAddressing<TLinkAddress>(ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>, IShiftOperators<TLinkAddress, int, TLinkAddress>
        {
            // Create a test tree structure
            // Root link with source and target
            var root = links.Create();
            var sourceLink = links.Create();  
            var targetLink = links.Create();
            
            // Update root to have source and target
            root = links.Update(root, sourceLink, targetLink);

            // Create second level: source.source, source.target, target.source, target.target
            var sourceSource = links.Create();
            var sourceTarget = links.Create(); 
            var targetSource = links.Create();
            var targetTarget = links.Create();

            sourceLink = links.Update(sourceLink, sourceSource, sourceTarget);
            targetLink = links.Update(targetLink, targetSource, targetTarget);

            // Create third level for source.source branch
            var sourceSourceSource = links.Create();
            var sourceSourceTarget = links.Create();
            sourceSource = links.Update(sourceSource, sourceSourceSource, sourceSourceTarget);

            // Test the bit-by-bit addressing
            var zero = TLinkAddress.Zero;
            var one = TLinkAddress.One;
            var two = one + one;
            var three = two + one;
            var four = three + one;
            var five = four + one;
            var six = five + one;
            var seven = six + one;

            // Test first level (depth 1)
            Assert.Equal(sourceLink, links.GetBitByBitSequenceElementByIndex(root, zero)); // 0 → source
            Assert.Equal(targetLink, links.GetBitByBitSequenceElementByIndex(root, one));  // 1 → target

            // Test second level (depth 2)
            Assert.Equal(sourceSource, links.GetBitByBitSequenceElementByIndex(root, two));   // 2 → source.source
            Assert.Equal(sourceTarget, links.GetBitByBitSequenceElementByIndex(root, three)); // 3 → source.target
            Assert.Equal(targetSource, links.GetBitByBitSequenceElementByIndex(root, four));  // 4 → target.source
            Assert.Equal(targetTarget, links.GetBitByBitSequenceElementByIndex(root, five));  // 5 → target.target

            // Test third level (depth 3) - only for source.source branch which we've set up
            Assert.Equal(sourceSourceSource, links.GetBitByBitSequenceElementByIndex(root, six));   // 6 → source.source.source
            Assert.Equal(sourceSourceTarget, links.GetBitByBitSequenceElementByIndex(root, seven)); // 7 → source.source.target
        }

        private static void TestBitByBitAddressingWithLargeIndices<TLinkAddress>(ILinks<TLinkAddress> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>, IShiftOperators<TLinkAddress, int, TLinkAddress>
        {
            // Create a deeper tree structure to test larger indices
            var root = links.Create();
            var sourceLink = links.Create();
            var targetLink = links.Create();
            root = links.Update(root, sourceLink, targetLink);

            // Build a tree with 4 levels deep
            var currentLinks = new TLinkAddress[] { sourceLink, targetLink };
            for (int depth = 2; depth <= 4; depth++)
            {
                var nextLevel = new TLinkAddress[currentLinks.Length * 2];
                for (int i = 0; i < currentLinks.Length; i++)
                {
                    var leftChild = links.Create();
                    var rightChild = links.Create();
                    currentLinks[i] = links.Update(currentLinks[i], leftChild, rightChild);
                    nextLevel[i * 2] = leftChild;
                    nextLevel[i * 2 + 1] = rightChild;
                }
                currentLinks = nextLevel;
            }

            // Test accessing elements at various depths without size limitations
            var zero = TLinkAddress.Zero;
            var one = TLinkAddress.One;
            var fourteen = TLinkAddress.CreateTruncating(14);  // Index 14 should work (requires 4 levels)
            var thirty = TLinkAddress.CreateTruncating(30);    // Index 30 should work (requires 5 levels if tree is deep enough)

            // These should not throw size limitation exceptions as the original GetSquareMatrixSequenceElementByIndex would
            try
            {
                var result14 = links.GetBitByBitSequenceElementByIndex(root, fourteen);
                // Result should be non-zero if the addressing works
                Assert.NotEqual(zero, result14);
            }
            catch (ArgumentOutOfRangeException)
            {
                // This should NOT happen with the new bit-by-bit addressing
                Assert.True(false, "GetBitByBitSequenceElementByIndex should not have size limitations");
            }

            // Test that we don't get the old power-of-two size limitation error
            var result0 = links.GetBitByBitSequenceElementByIndex(root, zero);
            var result1 = links.GetBitByBitSequenceElementByIndex(root, one);
            
            Assert.NotEqual(zero, result0);
            Assert.NotEqual(zero, result1);
            Assert.NotEqual(result0, result1); // Should be different (source vs target)
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open("bitByBitAddressingLogger.txt", FileMode.Create, FileAccess.Write))
            {
                LoggingDecorator<TLinkAddress> decoratedStorage = new(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }
        }
    }
}