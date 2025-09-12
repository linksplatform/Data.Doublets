using System;
using System.Collections.Generic;
using Xunit;
using Platform.Data.Doublets.Patterns;
using Platform.Data.Doublets.Memory.United.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Tests.Patterns
{
    public class SimplePatternTest
    {
        [Fact]
        public void BasicPatternCreationTest()
        {
            // Simple test to verify pattern node creation works
            var anyNode = new PatternNode<ulong>(PatternNodeType.Any);
            var literalNode = new PatternNode<ulong>(PatternNodeType.Literal, 42UL);
            var variableNode = new PatternNode<ulong>(PatternNodeType.Variable, "x");

            Assert.Equal(PatternNodeType.Any, anyNode.Type);
            Assert.Equal(PatternNodeType.Literal, literalNode.Type);
            Assert.Equal(42UL, literalNode.Value);
            Assert.Equal(PatternNodeType.Variable, variableNode.Type);
            Assert.Equal("x", variableNode.Value);
        }

        [Fact]
        public void PatternFactoryCreationTest()
        {
            // Test pattern factory methods
            var anyPattern = PatternFactory<ulong>.Any();
            var literalPattern = PatternFactory<ulong>.Literal(42UL);
            var variablePattern = PatternFactory<ulong>.Variable("test");

            Assert.NotNull(anyPattern);
            Assert.NotNull(literalPattern);
            Assert.NotNull(variablePattern);
        }
    }
}