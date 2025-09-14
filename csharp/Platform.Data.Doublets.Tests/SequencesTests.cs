using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Xunit;
using Platform.Memory;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Tests
{
    public static class SequencesTests
    {
        [Fact]
        public static void SequencesWithoutCompactificationTest()
        {
            Using<uint>(links =>
            {
                using var sequences = new Sequences<uint>(links, enableAutomaticCompactification: false);
                
                // Verify that automatic compactification is disabled
                Assert.False(sequences.IsAutomaticCompactificationEnabled);
                
                // Create some links
                var link1 = links.Create();
                var link2 = links.Create();
                var link3 = links.GetOrCreate(link1, link2);
                
                var countBefore = links.Count();
                
                // Dispose should not perform compactification
                sequences.Dispose();
                
                var countAfter = links.Count();
                Assert.Equal(countBefore, countAfter);
            });
        }

        [Fact]
        public static void SequencesWithCompactificationTest()
        {
            Using<uint>(links =>
            {
                using var sequences = new Sequences<uint>(links, enableAutomaticCompactification: true);
                
                // Verify that automatic compactification is enabled
                Assert.True(sequences.IsAutomaticCompactificationEnabled);
                
                // Create some duplicate links
                var link1 = links.Create();
                var link2 = links.Create();
                
                // Create the first link with specific source and target
                var originalLink = links.GetOrCreate(link1, link2);
                
                // Create another link with the same source and target (duplicate)
                var duplicateLink = links.CreateAndUpdate(link1, link2);
                
                // Verify we have duplicates
                Assert.NotEqual(originalLink, duplicateLink);
                Assert.Equal(links.GetSource(originalLink), links.GetSource(duplicateLink));
                Assert.Equal(links.GetTarget(originalLink), links.GetTarget(duplicateLink));
                
                var countBefore = links.Count();
                
                // Dispose should perform compactification and remove duplicates
                sequences.Dispose();
                
                var countAfter = links.Count();
                
                // Should have fewer links after compactification
                Assert.True(countAfter < countBefore, $"Expected count to decrease from {countBefore} to {countAfter}");
            });
        }

        [Fact]
        public static void ManualCompactificationTest()
        {
            Using<uint>(links =>
            {
                var sequences = new Sequences<uint>(links, enableAutomaticCompactification: false);
                
                // Create some duplicate links
                var link1 = links.Create();
                var link2 = links.Create();
                
                // Create the first link with specific source and target
                var originalLink = links.GetOrCreate(link1, link2);
                
                // Create another link with the same source and target (duplicate)
                var duplicateLink = links.CreateAndUpdate(link1, link2);
                
                // Verify we have duplicates
                Assert.NotEqual(originalLink, duplicateLink);
                
                var countBefore = links.Count();
                
                // Manually trigger compactification
                sequences.Compact();
                
                var countAfter = links.Count();
                
                // Should have fewer links after compactification
                Assert.True(countAfter < countBefore, $"Expected count to decrease from {countBefore} to {countAfter}");
                
                sequences.Dispose();
            });
        }

        [Fact]
        public static void CompactificationSkipsPointLinksTest()
        {
            Using<uint>(links =>
            {
                var sequences = new Sequences<uint>(links, enableAutomaticCompactification: true);
                
                // Create point links (links that reference themselves)
                var pointLink1 = links.CreatePoint();
                var pointLink2 = links.CreatePoint();
                
                // Create regular duplicate links
                var link1 = links.Create();
                var link2 = links.Create();
                var originalLink = links.GetOrCreate(link1, link2);
                var duplicateLink = links.CreateAndUpdate(link1, link2);
                
                var countBefore = links.Count();
                var pointLinksCountBefore = CountPointLinks(links);
                
                // Compactification should skip point links
                sequences.Compact();
                
                var countAfter = links.Count();
                var pointLinksCountAfter = CountPointLinks(links);
                
                // Point links should remain unchanged
                Assert.Equal(pointLinksCountBefore, pointLinksCountAfter);
                
                // But duplicates should be removed
                Assert.True(countAfter < countBefore, $"Expected count to decrease from {countBefore} to {countAfter}");
                
                sequences.Dispose();
            });
        }

        [Fact]
        public static void MultipleDisposeCallsTest()
        {
            Using<uint>(links =>
            {
                var sequences = new Sequences<uint>(links, enableAutomaticCompactification: true);
                
                // Create some links
                var link1 = links.Create();
                var link2 = links.Create();
                links.GetOrCreate(link1, link2);
                
                // Multiple dispose calls should be safe
                sequences.Dispose();
                sequences.Dispose(); // This should not throw or cause issues
                
                // Verify links are still accessible
                Assert.True(links.Count() > 0);
            });
        }

        private static uint CountPointLinks<TLinkAddress>(ILinks<TLinkAddress> links) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            uint count = 0;
            var constants = links.Constants;
            var query = new Link<TLinkAddress>(constants.Any, constants.Any, constants.Any);
            
            links.Each(link =>
            {
                var linkAddress = links.GetIndex(link);
                var source = links.GetSource(link);
                var target = links.GetTarget(link);
                
                if (source == linkAddress && target == linkAddress)
                {
                    count++;
                }
                
                return constants.Continue;
            }, query);
            
            return count;
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, 
                                  IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, 
                                  IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            try
            {
                action(unitedMemoryLinks);
            }
            finally
            {
                unitedMemoryLinks?.Dispose();
            }
        }
    }
}