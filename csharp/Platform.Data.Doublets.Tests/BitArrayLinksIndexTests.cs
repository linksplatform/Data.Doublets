using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class BitArrayLinksIndexTests
    {
        [Fact]
        public static void BasicCRUDTest()
        {
            Using<ulong>(links =>
            {
                var indexedLinks = new BitArrayLinksIndex<ulong>(links);
                
                // Test basic CRUD operations with the index
                indexedLinks.TestCRUDOperations();
            });
        }

        [Fact]
        public static void IndexSearchPerformanceTest()
        {
            Using<ulong>(links =>
            {
                var indexedLinks = new BitArrayLinksIndex<ulong>(links, useIndexForReads: true, maintainIndexOnWrites: true);
                var constants = links.Constants;
                
                // Create some test links
                var link1 = indexedLinks.Create(new ulong[] { 1, 2 });
                var link2 = indexedLinks.Create(new ulong[] { 1, 3 });
                var link3 = indexedLinks.Create(new ulong[] { 2, 3 });
                
                // Test counting with source filter
                var countWithSource1 = indexedLinks.Count(new ulong[] { constants.Any, 1, constants.Any });
                Assert.Equal(2UL, countWithSource1); // link1 and link2 have source 1
                
                // Test counting with target filter
                var countWithTarget3 = indexedLinks.Count(new ulong[] { constants.Any, constants.Any, 3 });
                Assert.Equal(2UL, countWithTarget3); // link2 and link3 have target 3
                
                // Test counting with both source and target
                var countWithSourceAndTarget = indexedLinks.Count(new ulong[] { constants.Any, 1, 3 });
                Assert.Equal(1UL, countWithSourceAndTarget); // only link2 has source 1 and target 3
            });
        }

        [Fact]
        public static void IndexMaintenanceTest()
        {
            Using<ulong>(links =>
            {
                var indexedLinks = new BitArrayLinksIndex<ulong>(links, useIndexForReads: true, maintainIndexOnWrites: true);
                var constants = links.Constants;
                
                // Create a link
                var link1 = indexedLinks.Create(new ulong[] { 1, 2 });
                
                // Verify it's indexed correctly
                var count1 = indexedLinks.Count(new ulong[] { constants.Any, 1, constants.Any });
                Assert.Equal(1UL, count1);
                
                // Update the link
                indexedLinks.Update(new ulong[] { link1, constants.Any, constants.Any }, new ulong[] { link1, 3, 4 });
                
                // Verify old index is removed and new index is added
                var countOldSource = indexedLinks.Count(new ulong[] { constants.Any, 1, constants.Any });
                Assert.Equal(0UL, countOldSource);
                
                var countNewSource = indexedLinks.Count(new ulong[] { constants.Any, 3, constants.Any });
                Assert.Equal(1UL, countNewSource);
                
                // Delete the link
                indexedLinks.Delete(new ulong[] { link1, constants.Any, constants.Any });
                
                // Verify index is cleaned up
                var countAfterDelete = indexedLinks.Count(new ulong[] { constants.Any, 3, constants.Any });
                Assert.Equal(0UL, countAfterDelete);
            });
        }

        [Fact]
        public static void IndexIterationTest()
        {
            Using<ulong>(links =>
            {
                var indexedLinks = new BitArrayLinksIndex<ulong>(links, useIndexForReads: true, maintainIndexOnWrites: true);
                var constants = links.Constants;
                
                // Create test links
                var link1 = indexedLinks.Create(new ulong[] { 1, 2 });
                var link2 = indexedLinks.Create(new ulong[] { 1, 3 });
                var link3 = indexedLinks.Create(new ulong[] { 2, 3 });
                
                // Test iteration with source filter
                var foundLinks = new System.Collections.Generic.List<ulong>();
                indexedLinks.Each(new ulong[] { constants.Any, 1, constants.Any }, link =>
                {
                    foundLinks.Add(link[constants.IndexPart]);
                    return constants.Continue;
                });
                
                Assert.Equal(2, foundLinks.Count);
                Assert.Contains(link1, foundLinks);
                Assert.Contains(link2, foundLinks);
            });
        }

        [Fact]
        public static void ConfigurabilityTest()
        {
            Using<ulong>(links =>
            {
                // Test with index disabled for reads
                var indexDisabledForReads = new BitArrayLinksIndex<ulong>(links, useIndexForReads: false, maintainIndexOnWrites: true);
                var constants = links.Constants;
                
                var link1 = indexDisabledForReads.Create(new ulong[] { 1, 2 });
                
                // Should fall back to underlying storage for count (not using index)
                var count = indexDisabledForReads.Count(new ulong[] { constants.Any, 1, constants.Any });
                Assert.Equal(1UL, count); // Should still work, just not using the index
                
                // Test with index maintenance disabled
                var indexMaintenanceDisabled = new BitArrayLinksIndex<ulong>(links, useIndexForReads: true, maintainIndexOnWrites: false);
                
                // Should not maintain indexes but still allow reads
                Assert.False(indexMaintenanceDisabled.MaintainIndexOnWrites);
                Assert.True(indexMaintenanceDisabled.UseIndexForReads);
            });
        }

        [Fact]
        public static void IndexRebuildTest()
        {
            Using<ulong>(links =>
            {
                // Create some links first
                var link1 = links.Create(new ulong[] { 1, 2 });
                var link2 = links.Create(new ulong[] { 1, 3 });
                
                // Now create index (should initialize from existing links)
                var indexedLinks = new BitArrayLinksIndex<ulong>(links, useIndexForReads: true, maintainIndexOnWrites: true);
                var constants = links.Constants;
                
                // Verify it found existing links
                var count = indexedLinks.Count(new ulong[] { constants.Any, 1, constants.Any });
                Assert.Equal(2UL, count);
                
                // Test manual rebuild
                indexedLinks.ClearIndexes();
                var countAfterClear = indexedLinks.Count(new ulong[] { constants.Any, 1, constants.Any });
                Assert.Equal(0UL, countAfterClear); // Index should be empty
                
                indexedLinks.RebuildIndexes();
                var countAfterRebuild = indexedLinks.Count(new ulong[] { constants.Any, 1, constants.Any });
                Assert.Equal(2UL, countAfterRebuild); // Index should be rebuilt
            });
        }

        [Fact]
        public static void MultipleDataTypesTest()
        {
            // Test with different numeric types
            Using<byte>(links => new BitArrayLinksIndex<byte>(links).TestCRUDOperations());
            Using<ushort>(links => new BitArrayLinksIndex<ushort>(links).TestCRUDOperations());
            Using<uint>(links => new BitArrayLinksIndex<uint>(links).TestCRUDOperations());
            Using<ulong>(links => new BitArrayLinksIndex<ulong>(links).TestCRUDOperations());
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, 
                                  IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, 
                                  IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open($"bitarray_test_{typeof(TLinkAddress).Name}.txt", FileMode.Create, FileAccess.Write))
            {
                var decoratedStorage = new LoggingDecorator<TLinkAddress>(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }
        }
    }
}