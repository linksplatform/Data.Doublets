using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets.Algorithms;
using Platform.Data.Doublets.Memory.United.Generic;
using Xunit;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Tests.Algorithms
{
    /// <summary>
    /// <para>
    /// Tests for the DiffExtensions class.
    /// </para>
    /// <para></para>
    /// </summary>
    public class DiffExtensionsTests : IDisposable
    {
        private readonly UnitedMemoryLinks<uint> _links;

        /// <summary>
        /// <para>
        /// Initializes test setup.
        /// </para>
        /// <para></para>
        /// </summary>
        public DiffExtensionsTests()
        {
            _links = new UnitedMemoryLinks<uint>(new Platform.Memory.HeapResizableDirectMemory());
        }

        /// <summary>
        /// <para>
        /// Test sequence diff computation using extension method.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeSequenceDiff_WithDifferentSequences_ReturnsCorrectOperations()
        {
            // Arrange
            var source = new List<uint> { 1, 2, 3 };
            var target = new List<uint> { 1, 4, 3, 5 };

            // Act
            var result = _links.ComputeSequenceDiff(source, target);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, op => op.Operation == DiffOperationType.Equal);
            Assert.Contains(result, op => op.Operation == DiffOperationType.Delete || op.Operation == DiffOperationType.Insert);
        }

        /// <summary>
        /// <para>
        /// Test simplified sequence diff computation.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeSimplifiedSequenceDiff_WithSequences_ReturnsDiff()
        {
            // Arrange
            var source = new List<uint> { 1, 2, 3, 4 };
            var target = new List<uint> { 1, 3, 4, 5 };

            // Act
            var result = _links.ComputeSimplifiedSequenceDiff(source, target);

            // Assert
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// <para>
        /// Test sequence edit distance computation.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeSequenceEditDistance_WithDifferentSequences_ReturnsPositiveDistance()
        {
            // Arrange
            var source = new List<uint> { 1, 2, 3 };
            var target = new List<uint> { 1, 4, 3, 5 };

            // Act
            var distance = _links.ComputeSequenceEditDistance(source, target);

            // Assert
            Assert.True(distance > 0);
        }

        /// <summary>
        /// <para>
        /// Test sequence edit distance with identical sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeSequenceEditDistance_WithIdenticalSequences_ReturnsZero()
        {
            // Arrange
            var source = new List<uint> { 1, 2, 3 };
            var target = new List<uint> { 1, 2, 3 };

            // Act
            var distance = _links.ComputeSequenceEditDistance(source, target);

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
        public void ComputeLongestCommonSubsequence_WithOverlappingSequences_ReturnsLCS()
        {
            // Arrange
            var source = new List<uint> { 1, 2, 3, 4, 5 };
            var target = new List<uint> { 2, 4, 5, 6 };

            // Act
            var lcs = _links.ComputeLongestCommonSubsequence(source, target);

            // Assert
            Assert.NotEmpty(lcs);
            Assert.Contains((uint)2, lcs);
            Assert.Contains((uint)4, lcs);
            Assert.Contains((uint)5, lcs);
        }

        /// <summary>
        /// <para>
        /// Test sequence extraction from links.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ExtractSequence_WithValidStartLink_ReturnsSequence()
        {
            // Arrange
            var link1 = _links.Create();
            var link2 = _links.Create();
            var link3 = _links.Create();
            
            // Create a simple chain: link1 -> link2 -> link3 -> link3 (self-reference to end)
            _links.Update(link1, link1, link2);
            _links.Update(link2, link2, link3);
            _links.Update(link3, link3, link3);

            // Act
            var sequence = _links.ExtractSequence(link1);

            // Assert
            Assert.NotEmpty(sequence);
            Assert.Contains(link1, sequence);
        }

        /// <summary>
        /// <para>
        /// Test sequence extraction with non-existent link.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ExtractSequence_WithNonExistentLink_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentLink = (uint)999999;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _links.ExtractSequence(nonExistentLink));
        }

        /// <summary>
        /// <para>
        /// Test extension methods with null arguments.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ExtensionMethods_WithNullArguments_ThrowArgumentNullException()
        {
            // Arrange
            var validSequence = new List<uint> { 1, 2, 3 };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => DiffExtensions.ComputeSequenceDiff<uint>(null, validSequence, validSequence));
            Assert.Throws<ArgumentNullException>(() => _links.ComputeSequenceDiff(null, validSequence));
            Assert.Throws<ArgumentNullException>(() => _links.ComputeSequenceDiff(validSequence, null));
        }

        /// <summary>
        /// <para>
        /// Test sequence diff with empty sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeSequenceDiff_WithEmptySequences_ReturnsEmptyResult()
        {
            // Arrange
            var emptySource = new List<uint>();
            var emptyTarget = new List<uint>();

            // Act
            var result = _links.ComputeSequenceDiff(emptySource, emptyTarget);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// <para>
        /// Test sequence diff with one empty sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ComputeSequenceDiff_WithOneEmptySequence_ReturnsInsertOrDeleteOperations()
        {
            // Arrange
            var emptySequence = new List<uint>();
            var nonEmptySequence = new List<uint> { 1, 2, 3 };

            // Act
            var result1 = _links.ComputeSequenceDiff(emptySequence, nonEmptySequence);
            var result2 = _links.ComputeSequenceDiff(nonEmptySequence, emptySequence);

            // Assert
            Assert.Equal(3, result1.Count);
            Assert.All(result1, op => Assert.Equal(DiffOperationType.Insert, op.Operation));
            
            Assert.Equal(3, result2.Count);
            Assert.All(result2, op => Assert.Equal(DiffOperationType.Delete, op.Operation));
        }

        /// <summary>
        /// <para>
        /// Test sequence extraction with maximum length limit.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void ExtractSequence_WithMaxLength_RespectsLimit()
        {
            // Arrange
            var link1 = _links.Create();
            var link2 = _links.Create();
            var link3 = _links.Create();
            var link4 = _links.Create();
            
            // Create a chain longer than our limit
            _links.Update(link1, link1, link2);
            _links.Update(link2, link2, link3);
            _links.Update(link3, link3, link4);
            _links.Update(link4, link4, link1); // Create cycle

            // Act
            var sequence = _links.ExtractSequence(link1, maxLength: 2);

            // Assert
            Assert.True(sequence.Count <= 2);
        }

        /// <summary>
        /// <para>
        /// Disposes test resources.
        /// </para>
        /// <para></para>
        /// </summary>
        public void Dispose()
        {
            _links?.Dispose();
        }
    }
}