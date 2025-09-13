using System;
using System.IO;
using System.Linq;
using Platform.Data.Doublets.Memory.United.Generic;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// <para>
    /// Tests for the BitStringLinksExtensions functionality integrated with ILinks.
    /// </para>
    /// <para></para>
    /// </summary>
    public class BitStringLinksExtensionsTests : IDisposable
    {
        private readonly string _tempFile;

        public BitStringLinksExtensionsTests()
        {
            _tempFile = Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
        }

        /// <summary>
        /// <para>
        /// Tests building a bitstring index from links.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void BuildBitStringIndexTest()
        {
            // Arrange
            using var links = new UnitedMemoryLinks<ulong>(_tempFile);
            var bitStringIndex = new BitStringIndex<ulong>();

            // Create some test links
            var link1 = links.GetOrCreate<ulong>(1, 2);
            var link2 = links.GetOrCreate<ulong>(2, 3);
            var link3 = links.GetOrCreate<ulong>(1, 3);

            // Act
            links.BuildBitStringIndex(bitStringIndex);

            // Assert - Check that bitstrings were created for sources and targets
            var bitString1 = bitStringIndex.GetBitString(1);
            var bitString2 = bitStringIndex.GetBitString(2);
            var bitString3 = bitStringIndex.GetBitString(3);

            Assert.NotNull(bitString1);
            Assert.NotNull(bitString2);
            Assert.NotNull(bitString3);

            // Link 1 appears as source in link1 and link3
            var frequency1 = bitStringIndex.GetFrequency(1);
            Assert.True(frequency1 >= 2);

            // Link 2 appears as source in link2 and target in link1
            var frequency2 = bitStringIndex.GetFrequency(2);
            Assert.True(frequency2 >= 2);
        }

        /// <summary>
        /// <para>
        /// Tests searching for sequences containing all fragments.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void SearchSequencesContainingAllFragmentsTest()
        {
            // Arrange
            using var links = new UnitedMemoryLinks<ulong>(_tempFile);
            var bitStringIndex = new BitStringIndex<ulong>();

            // Create test links forming sequences
            var link1 = links.GetOrCreate<ulong>(1, 2); // Link contains fragments 1 and 2
            var link2 = links.GetOrCreate<ulong>(2, 3); // Link contains fragments 2 and 3  
            var link3 = links.GetOrCreate<ulong>(1, 3); // Link contains fragments 1 and 3
            var link4 = links.GetOrCreate<ulong>(1, 4); // Link contains fragments 1 and 4

            links.BuildBitStringIndex(bitStringIndex);

            // Act - Search for links containing both fragments 1 and 2
            var results = links.SearchSequencesContainingAllFragments(bitStringIndex, 1UL, 2UL).ToList();

            // Assert
            Assert.NotEmpty(results);
            Assert.True(results.Contains(link1)); // link1 should contain both 1 and 2
        }

        /// <summary>
        /// <para>
        /// Tests optimized search that orders fragments by frequency.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void SearchSequencesContainingAllFragmentsOptimizedTest()
        {
            // Arrange
            using var links = new UnitedMemoryLinks<ulong>(_tempFile);
            var bitStringIndex = new BitStringIndex<ulong>();

            // Create links where fragment 1 is more frequent than fragment 9
            for (int i = 0; i < 5; i++)
            {
                links.GetOrCreate<ulong>(1, (ulong)(2 + i)); // Fragment 1 appears 5 times
            }
            var rareLink = links.GetOrCreate<ulong>(9, 10); // Fragment 9 appears only once

            links.BuildBitStringIndex(bitStringIndex);

            // Act
            var results = links.SearchSequencesContainingAllFragmentsOptimized(bitStringIndex, 1UL, 9UL).ToList();

            // Assert - Should find the intersection (empty since no link contains both 1 and 9)
            Assert.Empty(results);

            // But searching for just fragment 1 should return multiple results
            var results1 = links.SearchSequencesContainingAllFragmentsOptimized(bitStringIndex, 1UL).ToList();
            Assert.True(results1.Count >= 5);
        }

        /// <summary>
        /// <para>
        /// Tests searching for sequences containing pairs of fragments.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void SearchSequencesContainingPairsTest()
        {
            // Arrange
            using var links = new UnitedMemoryLinks<ulong>(_tempFile);
            var bitStringIndex = new BitStringIndex<ulong>();

            // Create links
            var link1 = links.GetOrCreate<ulong>(1, 2);
            var link2 = links.GetOrCreate<ulong>(3, 4);
            var link3 = links.GetOrCreate<ulong>(1, 4); // Contains both 1 and 4

            links.BuildBitStringIndex(bitStringIndex);

            // Act - Search for links containing pairs (1,2) and (3,4)
            var results = links.SearchSequencesContainingPairs(bitStringIndex, (1UL, 2UL), (3UL, 4UL)).ToList();

            // Assert - Should be empty since no single link contains all four fragments
            Assert.Empty(results);

            // Search for pair (1,4) which should return link3
            var results2 = links.SearchSequencesContainingPairs(bitStringIndex, (1UL, 4UL)).ToList();
            Assert.True(results2.Contains(link3));
        }

        /// <summary>
        /// <para>
        /// Tests getting fragment frequency from links.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void GetFragmentFrequencyTest()
        {
            // Arrange
            using var links = new UnitedMemoryLinks<ulong>(_tempFile);
            var bitStringIndex = new BitStringIndex<ulong>();

            // Create multiple links with fragment 1
            links.GetOrCreate<ulong>(1, 2);
            links.GetOrCreate<ulong>(1, 3);
            links.GetOrCreate<ulong>(1, 4);
            links.GetOrCreate<ulong>(5, 6); // Different fragment

            links.BuildBitStringIndex(bitStringIndex);

            // Act
            var frequency1 = links.GetFragmentFrequency(bitStringIndex, 1UL);
            var frequency5 = links.GetFragmentFrequency(bitStringIndex, 5UL);

            // Assert
            Assert.True(frequency1 >= 3); // Fragment 1 appears in at least 3 links
            Assert.True(frequency5 >= 1); // Fragment 5 appears in at least 1 link
        }

        /// <summary>
        /// <para>
        /// Tests updating bitstring index when creating and deleting links.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void UpdateBitStringIndexOnCreateAndDeleteTest()
        {
            // Arrange
            using var links = new UnitedMemoryLinks<ulong>(_tempFile);
            var bitStringIndex = new BitStringIndex<ulong>();

            // Act - Create a link and update index
            var linkAddress = links.GetOrCreate<ulong>(1, 2);
            links.UpdateBitStringIndexOnCreate<ulong>(bitStringIndex, linkAddress, 1UL, 2UL);

            // Assert - Fragments should be indexed
            var frequency1Before = bitStringIndex.GetFrequency(1UL);
            var frequency2Before = bitStringIndex.GetFrequency(2UL);
            Assert.True(frequency1Before > 0);
            Assert.True(frequency2Before > 0);

            // Act - Delete the link and update index
            links.Delete(linkAddress);
            links.UpdateBitStringIndexOnDelete<ulong>(bitStringIndex, linkAddress, 1UL, 2UL);

            // Assert - Frequencies should be reduced
            var frequency1After = bitStringIndex.GetFrequency(1UL);
            var frequency2After = bitStringIndex.GetFrequency(2UL);
            Assert.True(frequency1After < frequency1Before);
            Assert.True(frequency2After < frequency2Before);
        }

        /// <summary>
        /// <para>
        /// Tests the complete workflow of building and using bitstring index.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void CompleteWorkflowTest()
        {
            // Arrange
            using var links = new UnitedMemoryLinks<ulong>(_tempFile);
            var bitStringIndex = new BitStringIndex<ulong>();

            // Create a sequence-like structure
            var word1 = links.GetOrCreate<ulong>(1, 2); // "ab"
            var word2 = links.GetOrCreate<ulong>(2, 3); // "bc"
            var word3 = links.GetOrCreate<ulong>(3, 4); // "cd"
            var sentence1 = links.GetOrCreate<ulong>(word1, word2); // "ab bc" 
            var sentence2 = links.GetOrCreate<ulong>(word2, word3); // "bc cd"

            // Build the index
            links.BuildBitStringIndex(bitStringIndex);

            // Act - Search for sequences containing word parts
            var results = links.SearchSequencesContainingAllFragments(bitStringIndex, 2UL, 3UL).ToList();

            // Assert
            Assert.NotEmpty(results);
            // word2 directly contains 2 and 3
            Assert.True(results.Contains(word2));

            // Test optimized search
            var optimizedResults = links.SearchSequencesContainingAllFragmentsOptimized(bitStringIndex, 1UL, 2UL).ToList();
            Assert.True(optimizedResults.Contains(word1));

            // Test pair search for consecutive elements
            var pairResults = links.SearchSequencesContainingPairs(bitStringIndex, (2UL, 3UL)).ToList();
            Assert.True(pairResults.Contains(word2));
        }
    }
}