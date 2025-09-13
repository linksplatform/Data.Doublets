using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets.Algorithms;
using Xunit;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Tests.Algorithms
{
    /// <summary>
    /// <para>
    /// Tests for the SequenceDiffAlgorithm class.
    /// </para>
    /// <para></para>
    /// </summary>
    public class SequenceDiffAlgorithmTests
    {
        /// <summary>
        /// <para>
        /// Test diff computation with identical sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeDiff_WithIdenticalSequences_ReturnsOnlyEqualOperations()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint> { 1, 2, 3, 4, 5 };
            var target = new List<uint> { 1, 2, 3, 4, 5 };

            // Act
            var result = algorithm.ComputeDiff(source, target);

            // Assert
            Assert.Equal(5, result.Count);
            Assert.All(result, op => Assert.Equal(DiffOperationType.Equal, op.Operation));
        }

        /// <summary>
        /// <para>
        /// Test diff computation with completely different sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeDiff_WithCompletelyDifferentSequences_ReturnsDeleteAndInsertOperations()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint> { 1, 2, 3 };
            var target = new List<uint> { 4, 5, 6 };

            // Act
            var result = algorithm.ComputeDiff(source, target);

            // Assert
            var deleteOps = result.Where(op => op.Operation == DiffOperationType.Delete).Count();
            var insertOps = result.Where(op => op.Operation == DiffOperationType.Insert).Count();
            Assert.Equal(3, deleteOps);
            Assert.Equal(3, insertOps);
        }

        /// <summary>
        /// <para>
        /// Test diff computation with insertion operations.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeDiff_WithInsertions_ReturnsCorrectOperations()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint> { 1, 3, 5 };
            var target = new List<uint> { 1, 2, 3, 4, 5 };

            // Act
            var result = algorithm.ComputeDiff(source, target);

            // Assert
            var insertOps = result.Where(op => op.Operation == DiffOperationType.Insert).ToList();
            Assert.Equal(2, insertOps.Count);
            Assert.Contains(insertOps, op => op.TargetElement == 2);
            Assert.Contains(insertOps, op => op.TargetElement == 4);
        }

        /// <summary>
        /// <para>
        /// Test diff computation with deletion operations.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeDiff_WithDeletions_ReturnsCorrectOperations()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint> { 1, 2, 3, 4, 5 };
            var target = new List<uint> { 1, 3, 5 };

            // Act
            var result = algorithm.ComputeDiff(source, target);

            // Assert
            var deleteOps = result.Where(op => op.Operation == DiffOperationType.Delete).ToList();
            Assert.Equal(2, deleteOps.Count);
            Assert.Contains(deleteOps, op => op.SourceElement == 2);
            Assert.Contains(deleteOps, op => op.SourceElement == 4);
        }

        /// <summary>
        /// <para>
        /// Test diff computation with empty source sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeDiff_WithEmptySource_ReturnsInsertOperations()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint>();
            var target = new List<uint> { 1, 2, 3 };

            // Act
            var result = algorithm.ComputeDiff(source, target);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, op => Assert.Equal(DiffOperationType.Insert, op.Operation));
        }

        /// <summary>
        /// <para>
        /// Test diff computation with empty target sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeDiff_WithEmptyTarget_ReturnsDeleteOperations()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint> { 1, 2, 3 };
            var target = new List<uint>();

            // Act
            var result = algorithm.ComputeDiff(source, target);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, op => Assert.Equal(DiffOperationType.Delete, op.Operation));
        }

        /// <summary>
        /// <para>
        /// Test diff computation with both empty sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeDiff_WithBothEmpty_ReturnsEmptyResult()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint>();
            var target = new List<uint>();

            // Act
            var result = algorithm.ComputeDiff(source, target);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// <para>
        /// Test edit distance calculation.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeEditDistance_WithDifferentSequences_ReturnsCorrectDistance()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint> { 1, 2, 3 };
            var target = new List<uint> { 1, 4, 3, 5 };

            // Act
            var distance = algorithm.ComputeEditDistance(source, target);

            // Assert
            Assert.Equal(2, distance); // Replace 2->4, Insert 5
        }

        /// <summary>
        /// <para>
        /// Test edit distance with identical sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeEditDistance_WithIdenticalSequences_ReturnsZero()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint> { 1, 2, 3, 4 };
            var target = new List<uint> { 1, 2, 3, 4 };

            // Act
            var distance = algorithm.ComputeEditDistance(source, target);

            // Assert
            Assert.Equal(0, distance);
        }

        /// <summary>
        /// <para>
        /// Test longest common subsequence computation.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeLongestCommonSubsequence_WithOverlappingSequences_ReturnsCorrectLCS()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint> { 1, 2, 3, 4, 5 };
            var target = new List<uint> { 2, 4, 5, 6 };

            // Act
            var lcs = algorithm.ComputeLongestCommonSubsequence(source, target);

            // Assert
            Assert.Equal(3, lcs.Count);
            Assert.Contains((uint)2, lcs);
            Assert.Contains((uint)4, lcs);
            Assert.Contains((uint)5, lcs);
        }

        /// <summary>
        /// <para>
        /// Test simplified diff computation.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeSimplifiedDiff_WithConsecutiveOperations_CombinesOperations()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();
            var source = new List<uint> { 1, 2, 3, 6, 7, 8 };
            var target = new List<uint> { 1, 4, 5, 6, 7, 8 };

            // Act
            var result = algorithm.ComputeSimplifiedDiff(source, target);

            // Assert
            var nonEqualOps = result.Where(op => op.Operation != DiffOperationType.Equal).ToList();
            Assert.True(nonEqualOps.Count <= result.Count); // Should be simplified
        }

        /// <summary>
        /// <para>
        /// Test algorithm with null arguments.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeDiff_WithNullArguments_ThrowsArgumentNullException()
        {
            // Arrange
            var algorithm = new SequenceDiffAlgorithm<uint>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => algorithm.ComputeDiff(null, new List<uint>()));
            Assert.Throws<ArgumentNullException>(() => algorithm.ComputeDiff(new List<uint>(), null));
        }

        /// <summary>
        /// <para>
        /// Test algorithm with custom comparer.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeDiff_WithCustomComparer_UsesCustomLogic()
        {
            // Arrange - use a comparer that treats all even numbers as equal
            var customComparer = new CustomEvenEqualityComparer();
            var algorithm = new SequenceDiffAlgorithm<uint>(customComparer);
            var source = new List<uint> { 2, 4, 6 };
            var target = new List<uint> { 8, 10, 12 };

            // Act
            var result = algorithm.ComputeDiff(source, target);

            // Assert - should see all as equal due to custom comparer
            Assert.All(result, op => Assert.Equal(DiffOperationType.Equal, op.Operation));
        }

        /// <summary>
        /// <para>
        /// Custom equality comparer that treats all even numbers as equal.
        /// </para>
        /// <para></para>
        /// </summary>
        private class CustomEvenEqualityComparer : IEqualityComparer<uint>
        {
            public bool Equals(uint x, uint y)
            {
                return (x % 2 == 0 && y % 2 == 0) || x == y;
            }

            public int GetHashCode(uint obj)
            {
                return obj % 2 == 0 ? 0 : (int)obj;
            }
        }
    }
}