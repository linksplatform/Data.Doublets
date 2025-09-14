using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.CriterionMatchers;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class CriterionMatchersTests
    {
        [Fact]
        public static void TargetMatcherTest()
        {
            Using<ulong>(links =>
            {
                var link1 = links.GetOrCreate(1UL, 2UL);
                var link2 = links.GetOrCreate(3UL, 4UL);
                var link3 = links.GetOrCreate(5UL, 2UL);
                
                var targetMatcher = new TargetMatcher<ulong>(links, 2UL);
                
                // Should match link1 and link3 (both have target 2)
                Assert.True(targetMatcher.IsMatched(link1));
                Assert.False(targetMatcher.IsMatched(link2));
                Assert.True(targetMatcher.IsMatched(link3));
            });
        }

        [Fact]
        public static void SourceMatcherTest()
        {
            Using<ulong>(links =>
            {
                var link1 = links.GetOrCreate(1UL, 2UL);
                var link2 = links.GetOrCreate(3UL, 4UL);
                var link3 = links.GetOrCreate(1UL, 5UL);
                
                var sourceMatcher = new SourceMatcher<ulong>(links, 1UL);
                
                // Should match link1 and link3 (both have source 1)
                Assert.True(sourceMatcher.IsMatched(link1));
                Assert.False(sourceMatcher.IsMatched(link2));
                Assert.True(sourceMatcher.IsMatched(link3));
            });
        }

        [Fact]
        public static void CountUsagesWithMatchersTest()
        {
            Using<ulong>(links =>
            {
                // Create a self-referencing link
                var selfRef = links.GetOrCreate(1UL, 1UL);
                var link1 = links.GetOrCreate(selfRef, 2UL);
                var link2 = links.GetOrCreate(3UL, selfRef);
                
                // CountUsages should handle self-references correctly using matchers
                var usages = links.CountUsages(selfRef);
                
                // Should count 2 usages (as source in link1, as target in link2)
                // but not count the self-reference
                Assert.Equal(expected: 2UL, actual: usages);
            });
        }

        [Fact]
        public static void EqualsWithMatchersTest()
        {
            Using<ulong>(links =>
            {
                var link1 = links.GetOrCreate(1UL, 2UL);
                var link2 = links.GetOrCreate(3UL, 4UL);
                
                // Test equals method using matchers internally
                Assert.True(links.Equals(link1, 1UL, 2UL));
                Assert.False(links.Equals(link1, 1UL, 4UL));
                Assert.False(links.Equals(link1, 3UL, 2UL));
                Assert.True(links.Equals(link2, 3UL, 4UL));
            });
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress,int,TLinkAddress>, IBitwiseOperators<TLinkAddress,TLinkAddress,TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open("linksLogger.txt", FileMode.Create, FileAccess.Write))
            {
                LoggingDecorator<TLinkAddress> decoratedStorage = new(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }
        }
    }
}