using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Platform.Data.Doublets.Patterns;
using Platform.Data.Doublets.Memory.United.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Tests.Patterns
{
    public class PatternTransformationTests : IDisposable
    {
        private readonly UnitedMemoryLinks<ulong> _links;
        private readonly PatternManager<ulong> _patternManager;

        public PatternTransformationTests()
        {
            _links = new UnitedMemoryLinks<ulong>(new Platform.Memory.HeapResizableDirectMemory());
            _patternManager = new PatternManager<ulong>();
        }

        public void Dispose()
        {
            _links.Dispose();
        }

        [Fact]
        public void TransformOnceAppliesSingleTransformation()
        {
            // Arrange
            var originalLink = _links.GetOrCreate(1UL, 2UL);
            var sourcePattern = PatternFactory<ulong>.Doublet(
                PatternFactory<ulong>.Literal(1UL),
                PatternFactory<ulong>.Literal(2UL)
            );
            var targetPattern = PatternFactory<ulong>.Doublet(
                PatternFactory<ulong>.Literal(2UL),
                PatternFactory<ulong>.Literal(1UL)
            );

            // Act
            var transformed = _patternManager.Transformer.TransformOnce(sourcePattern, targetPattern, _links);

            // Assert
            Assert.True(transformed);
            var newSource = _links.GetSource(originalLink);
            var newTarget = _links.GetTarget(originalLink);
            Assert.Equal(2UL, newSource);
            Assert.Equal(1UL, newTarget);
        }

        [Fact]
        public void TransformOnceWithVariables()
        {
            // Arrange
            var link1 = _links.GetOrCreate(5UL, 6UL);
            var link2 = _links.GetOrCreate(7UL, 8UL);
            
            var sourcePattern = PatternFactory<ulong>.Doublet(
                PatternFactory<ulong>.Variable("x"),
                PatternFactory<ulong>.Variable("y")
            );
            var targetPattern = PatternFactory<ulong>.Doublet(
                PatternFactory<ulong>.Variable("y"),
                PatternFactory<ulong>.Variable("x")
            );

            // Act
            var transformed = _patternManager.Transformer.TransformOnce(sourcePattern, targetPattern, _links);

            // Assert
            Assert.True(transformed);
            
            // One of the links should have been transformed
            var link1Source = _links.GetSource(link1);
            var link1Target = _links.GetTarget(link1);
            var link2Source = _links.GetSource(link2);
            var link2Target = _links.GetTarget(link2);
            
            Assert.True(
                (link1Source == 6UL && link1Target == 5UL) ||
                (link2Source == 8UL && link2Target == 7UL)
            );
        }

        [Fact]
        public void ApplySubstitutionCreatesCorrectLink()
        {
            // Arrange
            var variables = new Dictionary<string, ulong>
            {
                ["x"] = 10UL,
                ["y"] = 20UL
            };
            
            var pattern = PatternFactory<ulong>.Doublet(
                PatternFactory<ulong>.Variable("x"),
                PatternFactory<ulong>.Variable("y")
            );

            // Act
            var result = _patternManager.Transformer.ApplySubstitution(pattern, variables, _links);

            // Assert
            Assert.NotEqual(0UL, result);
            Assert.Equal(10UL, _links.GetSource(result));
            Assert.Equal(20UL, _links.GetTarget(result));
        }

        [Fact]
        public void TransformAlwaysAppliesAllPossibleTransformations()
        {
            // Arrange
            var link1 = _links.GetOrCreate(1UL, 2UL);
            var link2 = _links.GetOrCreate(1UL, 2UL); // Same pattern
            var link3 = _links.GetOrCreate(3UL, 4UL); // Different pattern
            
            var sourcePattern = PatternFactory<ulong>.Doublet(
                PatternFactory<ulong>.Literal(1UL),
                PatternFactory<ulong>.Literal(2UL)
            );
            var targetPattern = PatternFactory<ulong>.Doublet(
                PatternFactory<ulong>.Literal(2UL),
                PatternFactory<ulong>.Literal(1UL)
            );

            // Act
            var transformCount = _patternManager.Transformer.TransformAlways(sourcePattern, targetPattern, _links);

            // Assert
            Assert.Equal(2, transformCount); // Should transform both matching links
            
            // Verify transformations
            Assert.Equal(2UL, _links.GetSource(link1));
            Assert.Equal(1UL, _links.GetTarget(link1));
            Assert.Equal(2UL, _links.GetSource(link2));
            Assert.Equal(1UL, _links.GetTarget(link2));
            
            // link3 should remain unchanged
            Assert.Equal(3UL, _links.GetSource(link3));
            Assert.Equal(4UL, _links.GetTarget(link3));
        }

        [Fact]
        public void PatternManagerTransformOnceWithStrings()
        {
            // Arrange
            var link = _links.GetOrCreate(1UL, 2UL);

            // Act
            var transformed = _patternManager.TransformOnce("($x $y)", "($y $x)", _links);

            // Assert
            Assert.True(transformed);
            Assert.Equal(2UL, _links.GetSource(link));
            Assert.Equal(1UL, _links.GetTarget(link));
        }

        [Fact]
        public void PatternManagerTransformAlwaysWithStrings()
        {
            // Arrange
            var link1 = _links.GetOrCreate(5UL, 10UL);
            var link2 = _links.GetOrCreate(6UL, 12UL);

            // Act - Transform any doublet to its reverse
            var transformCount = _patternManager.TransformAlways("($x $y)", "($y $x)", _links);

            // Assert
            Assert.Equal(2, transformCount);
            Assert.Equal(10UL, _links.GetSource(link1));
            Assert.Equal(5UL, _links.GetTarget(link1));
            Assert.Equal(12UL, _links.GetSource(link2));
            Assert.Equal(6UL, _links.GetTarget(link2));
        }

        [Fact]
        public void ComplexPatternTransformation()
        {
            // Arrange - Create some tree-like structures
            var leaf1 = _links.GetOrCreate(1UL, 1UL); // Point (leaf)
            var leaf2 = _links.GetOrCreate(2UL, 2UL); // Point (leaf)
            var branch = _links.GetOrCreate(leaf1, leaf2);

            // Transform: any link that has point as source should swap source and target
            var sourcePatternStr = "(point $y)";
            var targetPatternStr = "($y point)";

            // Act
            var transformed = _patternManager.TransformOnce(sourcePatternStr, targetPatternStr, _links);

            // Assert
            Assert.True(transformed);
            
            // The branch should now have leaf2 as source and leaf1 as target (if leaf1 was matched as point)
            var newSource = _links.GetSource(branch);
            var newTarget = _links.GetTarget(branch);
            
            // One of the leaves should now be the target and the other should be the source
            Assert.True(
                (newSource == leaf2 && newTarget == leaf1) ||
                (newSource == leaf1 && newTarget == leaf2)
            );
        }

        [Fact]
        public void DeletionTransformation()
        {
            // Arrange
            var linkToDelete = _links.GetOrCreate(100UL, 100UL);
            var linkToKeep = _links.GetOrCreate(100UL, 200UL);
            
            var initialCount = _links.Count();

            // Transform: delete all points (self-references)
            var sourcePattern = PatternFactory<ulong>.Point();
            var targetPattern = PatternFactory<ulong>.Literal(0UL); // Empty pattern for deletion

            // Act
            var transformed = _patternManager.Transformer.TransformOnce(sourcePattern, targetPattern, _links);

            // Assert
            Assert.True(transformed);
            Assert.False(_links.Exists(linkToDelete));
            Assert.True(_links.Exists(linkToKeep));
            Assert.Equal(initialCount - 1, _links.Count());
        }
    }
}