using System;
using System.Collections.Generic;
using Xunit;
using Platform.Data.Doublets.Patterns;
using Platform.Data.Doublets.Memory.United.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Tests.Patterns
{
    public class PatternTests : IDisposable
    {
        private readonly UnitedMemoryLinks<ulong> _links;
        private readonly PatternManager<ulong> _patternManager;

        public PatternTests()
        {
            _links = new UnitedMemoryLinks<ulong>(new Platform.Memory.HeapResizableDirectMemory());
            _patternManager = new PatternManager<ulong>();
        }

        public void Dispose()
        {
            _links.Dispose();
        }

        [Fact]
        public void AnyPatternMatchesAllLinks()
        {
            // Arrange
            var link1 = _links.Create();
            var link2 = _links.Create();
            var anyPattern = PatternFactory<ulong>.Any();

            // Act
            var variables1 = new Dictionary<string, ulong>();
            var variables2 = new Dictionary<string, ulong>();
            var match1 = anyPattern.Matches(link1, _links, variables1);
            var match2 = anyPattern.Matches(link2, _links, variables2);

            // Assert
            Assert.True(match1);
            Assert.True(match2);
        }

        [Fact]
        public void LiteralPatternMatchesOnlySpecificLink()
        {
            // Arrange
            var specificLink = _links.Create();
            var otherLink = _links.Create();
            var literalPattern = PatternFactory<ulong>.Literal(specificLink);

            // Act
            var variables1 = new Dictionary<string, ulong>();
            var variables2 = new Dictionary<string, ulong>();
            var matchSpecific = literalPattern.Matches(specificLink, _links, variables1);
            var matchOther = literalPattern.Matches(otherLink, _links, variables2);

            // Assert
            Assert.True(matchSpecific);
            Assert.False(matchOther);
        }

        [Fact]
        public void VariablePatternBindsToAnyLink()
        {
            // Arrange
            var link = _links.Create();
            var variablePattern = PatternFactory<ulong>.Variable("x");

            // Act
            var variables = new Dictionary<string, ulong>();
            var match = variablePattern.Matches(link, _links, variables);

            // Assert
            Assert.True(match);
            Assert.Equal(link, variables["x"]);
        }

        [Fact]
        public void VariablePatternConsistentBinding()
        {
            // Arrange
            var link1 = _links.Create();
            var link2 = _links.Create();
            var variablePattern = PatternFactory<ulong>.Variable("x");

            // Act
            var variables = new Dictionary<string, ulong>();
            variablePattern.Matches(link1, _links, variables);
            var match = variablePattern.Matches(link2, _links, variables);

            // Assert
            Assert.False(match); // Should not match because variable is already bound to link1
            Assert.Equal(link1, variables["x"]);
        }

        [Fact]
        public void PointPatternMatchesOnlySelfReferences()
        {
            // Arrange
            var selfRef = _links.GetOrCreate(1UL, 1UL);
            var normalLink = _links.GetOrCreate(1UL, 2UL);
            var pointPattern = PatternFactory<ulong>.Point();

            // Act
            var variables1 = new Dictionary<string, ulong>();
            var variables2 = new Dictionary<string, ulong>();
            var matchSelfRef = pointPattern.Matches(selfRef, _links, variables1);
            var matchNormal = pointPattern.Matches(normalLink, _links, variables2);

            // Assert
            Assert.True(matchSelfRef);
            Assert.False(matchNormal);
        }

        [Fact]
        public void OrPatternMatchesAnyChildPattern()
        {
            // Arrange
            var link1 = _links.Create();
            var link2 = _links.Create();
            var literal1 = PatternFactory<ulong>.Literal(link1);
            var literal2 = PatternFactory<ulong>.Literal(link2);
            var orPattern = PatternFactory<ulong>.Or(literal1, literal2);

            // Act
            var variables1 = new Dictionary<string, ulong>();
            var variables2 = new Dictionary<string, ulong>();
            var variables3 = new Dictionary<string, ulong>();
            var match1 = orPattern.Matches(link1, _links, variables1);
            var match2 = orPattern.Matches(link2, _links, variables2);
            var matchOther = orPattern.Matches(_links.Create(), _links, variables3);

            // Assert
            Assert.True(match1);
            Assert.True(match2);
            Assert.False(matchOther);
        }

        [Fact]
        public void AndPatternRequiresAllChildPatterns()
        {
            // Arrange
            var link = _links.Create();
            var anyPattern = PatternFactory<ulong>.Any();
            var literalPattern = PatternFactory<ulong>.Literal(link);
            var andPattern = PatternFactory<ulong>.And(anyPattern, literalPattern);

            // Act
            var variables1 = new Dictionary<string, ulong>();
            var variables2 = new Dictionary<string, ulong>();
            var matchCorrect = andPattern.Matches(link, _links, variables1);
            var matchIncorrect = andPattern.Matches(_links.Create(), _links, variables2);

            // Assert
            Assert.True(matchCorrect);
            Assert.False(matchIncorrect);
        }

        [Fact]
        public void NotPatternNegatesChildPattern()
        {
            // Arrange
            var link1 = _links.Create();
            var link2 = _links.Create();
            var literalPattern = PatternFactory<ulong>.Literal(link1);
            var notPattern = PatternFactory<ulong>.Not(literalPattern);

            // Act
            var variables1 = new Dictionary<string, ulong>();
            var variables2 = new Dictionary<string, ulong>();
            var matchLink1 = notPattern.Matches(link1, _links, variables1);
            var matchLink2 = notPattern.Matches(link2, _links, variables2);

            // Assert
            Assert.False(matchLink1); // Should not match the literal
            Assert.True(matchLink2); // Should match anything that's not the literal
        }

        [Fact]
        public void PatternParsingBasicPatterns()
        {
            // Arrange & Act
            var anyPattern = _patternManager.ParsePattern("*");
            var variablePattern = _patternManager.ParsePattern("$x");
            var pointPattern = _patternManager.ParsePattern("point");

            // Assert
            Assert.NotNull(anyPattern);
            Assert.NotNull(variablePattern);
            Assert.NotNull(pointPattern);
        }

        [Fact]
        public void PatternParsingDoubletPattern()
        {
            // Arrange
            var doublet = _links.GetOrCreate(1UL, 2UL);
            
            // Act
            var doubletPattern = _patternManager.ParsePattern("(1 2)");

            // Assert
            Assert.NotNull(doubletPattern);
            
            var variables = new Dictionary<string, ulong>();
            var matches = doubletPattern.Matches(doublet, _links, variables);
            Assert.True(matches);
        }

        [Fact]
        public void PatternParsingLogicalOperators()
        {
            // Arrange & Act
            var orPattern = _patternManager.ParsePattern("(or $x $y)");
            var andPattern = _patternManager.ParsePattern("(and * point)");
            var notPattern = _patternManager.ParsePattern("(not 1)");

            // Assert
            Assert.NotNull(orPattern);
            Assert.NotNull(andPattern);
            Assert.NotNull(notPattern);
        }

        [Fact]
        public void FindMatchesReturnsCorrectResults()
        {
            // Arrange
            var link1 = _links.GetOrCreate(1UL, 1UL); // Point
            var link2 = _links.GetOrCreate(1UL, 2UL); // Not a point
            var pointPattern = PatternFactory<ulong>.Point();

            // Act
            var matches = _patternManager.FindMatches(pointPattern, _links);

            // Assert
            Assert.True(matches.ContainsKey(link1));
            Assert.False(matches.ContainsKey(link2));
        }

        [Fact]
        public void GreaterThanPatternMatchesCorrectly()
        {
            // Arrange
            var smallLink = 1UL;
            var largeLink = 10UL;
            var gtPattern = PatternFactory<ulong>.GreaterThan(5UL);

            // Act
            var variables1 = new Dictionary<string, ulong>();
            var variables2 = new Dictionary<string, ulong>();
            var matchSmall = gtPattern.Matches(smallLink, _links, variables1);
            var matchLarge = gtPattern.Matches(largeLink, _links, variables2);

            // Assert
            Assert.False(matchSmall);
            Assert.True(matchLarge);
        }

        [Fact]
        public void LessThanPatternMatchesCorrectly()
        {
            // Arrange
            var smallLink = 1UL;
            var largeLink = 10UL;
            var ltPattern = PatternFactory<ulong>.LessThan(5UL);

            // Act
            var variables1 = new Dictionary<string, ulong>();
            var variables2 = new Dictionary<string, ulong>();
            var matchSmall = ltPattern.Matches(smallLink, _links, variables1);
            var matchLarge = ltPattern.Matches(largeLink, _links, variables2);

            // Assert
            Assert.True(matchSmall);
            Assert.False(matchLarge);
        }

        [Fact]
        public void UserDefinedPatternsWork()
        {
            // Arrange
            var definitions = @"
                (pattern (name ""custom-point"") ($x: $x $x))
            ";
            _patternManager.DefinePatterns(definitions);

            var selfRef = _links.GetOrCreate(1UL, 1UL);
            var normalLink = _links.GetOrCreate(1UL, 2UL);

            // Act
            var customPattern = _patternManager.ParsePattern("custom-point");
            var variables1 = new Dictionary<string, ulong>();
            var variables2 = new Dictionary<string, ulong>();
            var matchSelfRef = customPattern.Matches(selfRef, _links, variables1);
            var matchNormal = customPattern.Matches(normalLink, _links, variables2);

            // Assert
            Assert.True(matchSelfRef);
            Assert.False(matchNormal);
        }
    }
}