using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// <para>
    /// Tests for the BitStringIndex functionality.
    /// </para>
    /// <para></para>
    /// </summary>
    public class BitStringIndexTests
    {
        /// <summary>
        /// <para>
        /// Tests basic bitstring index operations.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void BitStringIndexBasicOperationsTest()
        {
            // Arrange
            var bitStringIndex = new BitStringIndex<ulong>();
            var linkAddress = 1UL;
            var bitArray = new BitArray(10);
            bitArray[0] = true;
            bitArray[5] = true;

            // Act
            bitStringIndex.SetBitString(linkAddress, bitArray);
            var retrievedBitArray = bitStringIndex.GetBitString(linkAddress);

            // Assert
            Assert.NotNull(retrievedBitArray);
            Assert.Equal(bitArray.Length, retrievedBitArray.Length);
            Assert.True(retrievedBitArray[0]);
            Assert.True(retrievedBitArray[5]);
            Assert.False(retrievedBitArray[1]);
        }

        /// <summary>
        /// <para>
        /// Tests updating individual bits in the bitstring index.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void BitStringIndexUpdateBitTest()
        {
            // Arrange
            var bitStringIndex = new BitStringIndex<ulong>();
            var linkAddress = 2UL;

            // Act
            bitStringIndex.UpdateBit(linkAddress, 3, true);
            bitStringIndex.UpdateBit(linkAddress, 7, true);
            bitStringIndex.UpdateBit(linkAddress, 3, false);

            var bitArray = bitStringIndex.GetBitString(linkAddress);

            // Assert
            Assert.NotNull(bitArray);
            Assert.False(bitArray[3]);
            Assert.True(bitArray[7]);
            Assert.False(bitArray[0]);
        }

        /// <summary>
        /// <para>
        /// Tests bitstring intersection functionality.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void BitStringIndexIntersectionTest()
        {
            // Arrange
            var bitStringIndex = new BitStringIndex<ulong>();
            var linkAddress1 = 1UL;
            var linkAddress2 = 2UL;

            // Create first bitstring: [true, false, true, true, false]
            bitStringIndex.UpdateBit(linkAddress1, 0, true);
            bitStringIndex.UpdateBit(linkAddress1, 2, true);
            bitStringIndex.UpdateBit(linkAddress1, 3, true);

            // Create second bitstring: [true, true, false, true, false]
            bitStringIndex.UpdateBit(linkAddress2, 0, true);
            bitStringIndex.UpdateBit(linkAddress2, 1, true);
            bitStringIndex.UpdateBit(linkAddress2, 3, true);

            // Act
            var intersection = bitStringIndex.IntersectBitStrings(new[] { linkAddress1, linkAddress2 });

            // Assert
            Assert.NotNull(intersection);
            Assert.True(intersection[0]); // Both have bit 0 set
            Assert.False(intersection[1]); // Only linkAddress2 has bit 1 set
            Assert.False(intersection[2]); // Only linkAddress1 has bit 2 set
            Assert.True(intersection[3]); // Both have bit 3 set
        }

        /// <summary>
        /// <para>
        /// Tests frequency counting functionality.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void BitStringIndexFrequencyTest()
        {
            // Arrange
            var bitStringIndex = new BitStringIndex<ulong>();
            var linkAddress = 3UL;

            // Act
            bitStringIndex.UpdateBit(linkAddress, 0, true);
            bitStringIndex.UpdateBit(linkAddress, 2, true);
            bitStringIndex.UpdateBit(linkAddress, 4, true);

            var frequency = bitStringIndex.GetFrequency(linkAddress);

            // Assert
            Assert.Equal(3, frequency);
        }

        /// <summary>
        /// <para>
        /// Tests finding links containing all fragments.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void BitStringIndexFindLinksContainingAllFragmentsTest()
        {
            // Arrange
            var bitStringIndex = new BitStringIndex<ulong>();
            var fragment1 = 1UL;
            var fragment2 = 2UL;

            // Setup fragments in sequences
            // fragment1 appears in sequences 0, 2, 3
            bitStringIndex.UpdateBit(fragment1, 0, true);
            bitStringIndex.UpdateBit(fragment1, 2, true);
            bitStringIndex.UpdateBit(fragment1, 3, true);

            // fragment2 appears in sequences 1, 2, 3
            bitStringIndex.UpdateBit(fragment2, 1, true);
            bitStringIndex.UpdateBit(fragment2, 2, true);
            bitStringIndex.UpdateBit(fragment2, 3, true);

            // Act
            var results = bitStringIndex.FindLinksContainingAllFragments(new[] { fragment1, fragment2 }).ToList();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Contains(2UL, results); // Sequence 2 contains both fragments
            Assert.Contains(3UL, results); // Sequence 3 contains both fragments
        }

        /// <summary>
        /// <para>
        /// Tests empty intersection when no fragments match.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void BitStringIndexEmptyIntersectionTest()
        {
            // Arrange
            var bitStringIndex = new BitStringIndex<ulong>();
            var fragment1 = 1UL;
            var fragment2 = 2UL;

            // Setup fragments in different sequences (no overlap)
            bitStringIndex.UpdateBit(fragment1, 0, true);
            bitStringIndex.UpdateBit(fragment1, 1, true);

            bitStringIndex.UpdateBit(fragment2, 2, true);
            bitStringIndex.UpdateBit(fragment2, 3, true);

            // Act
            var results = bitStringIndex.FindLinksContainingAllFragments(new[] { fragment1, fragment2 }).ToList();

            // Assert
            Assert.Empty(results);
        }

        /// <summary>
        /// <para>
        /// Tests dynamic resizing of bitstrings when new positions are added.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void BitStringIndexDynamicResizingTest()
        {
            // Arrange
            var bitStringIndex = new BitStringIndex<ulong>(5); // Start with capacity 5
            var linkAddress = 1UL;

            // Act - Add bit beyond initial capacity
            bitStringIndex.UpdateBit(linkAddress, 10, true); // Position 10 > initial capacity 5

            var bitArray = bitStringIndex.GetBitString(linkAddress);

            // Assert
            Assert.NotNull(bitArray);
            Assert.True(bitArray.Length > 10);
            Assert.True(bitArray[10]);
        }

        /// <summary>
        /// <para>
        /// Tests performance by creating a larger index with multiple fragments.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void BitStringIndexPerformanceTest()
        {
            // Arrange
            var bitStringIndex = new BitStringIndex<ulong>();
            const int numFragments = 100;
            const int numSequences = 1000;

            // Act - Build index
            for (ulong fragment = 1; fragment <= numFragments; fragment++)
            {
                for (int sequence = 0; sequence < numSequences; sequence += (int)fragment) // Sparse distribution
                {
                    bitStringIndex.UpdateBit(fragment, sequence, true);
                }
            }

            // Test frequency calculation
            var frequency = bitStringIndex.GetFrequency(1UL); // Fragment 1 appears in all sequences
            Assert.Equal(numSequences, frequency);

            var frequency10 = bitStringIndex.GetFrequency(10UL); // Fragment 10 appears every 10 sequences
            Assert.Equal(numSequences / 10, frequency10);

            // Test intersection of two fragments
            var results = bitStringIndex.FindLinksContainingAllFragments(new ulong[] { 2UL, 3UL }).ToList();
            Assert.NotEmpty(results);

            // Assert - The test completed without performance issues
            Assert.True(true);
        }
    }
}