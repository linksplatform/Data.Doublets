using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Platform.Timestamps;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class TemporalResolverTests
    {
        [Fact]
        public static void BasicTemporalOperationsTest()
        {
            Using<ulong>(links => TestBasicTemporalOperations(links));
        }

        [Fact]
        public static void TemporalUpdateTest()
        {
            Using<ulong>(links => TestTemporalUpdate(links));
        }

        [Fact]
        public static void TemporalDeleteTest()
        {
            Using<ulong>(links => TestTemporalDelete(links));
        }

        [Fact]
        public static void TemporalQueryTest()
        {
            Using<ulong>(links => TestTemporalQuery(links));
        }

        private static void TestBasicTemporalOperations<TLinkAddress>(ILinks<TLinkAddress> links) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var temporalLinks = new TemporalResolver<TLinkAddress>(links);
            
            // Create a link
            var link = temporalLinks.Create();
            Assert.True(link > temporalLinks.Constants.Null);
            
            // Verify the link exists
            Assert.True(temporalLinks.Count() > temporalLinks.Constants.Null);
        }

        private static void TestTemporalUpdate<TLinkAddress>(ILinks<TLinkAddress> links) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var temporalLinks = new TemporalResolver<TLinkAddress>(links);
            
            // Create initial link
            var initialLink = temporalLinks.Create();
            var initialCount = temporalLinks.Count();
            
            // Update the link (should create a new version)
            var source = TLinkAddress.CreateChecked(1);
            var target = TLinkAddress.CreateChecked(2);
            var updatedLink = temporalLinks.Update(new[] { initialLink }, new[] { source, target });
            
            // Should have more links now (original + new version + timestamp links)
            Assert.True(temporalLinks.Count() > initialCount);
            Assert.True(updatedLink != initialLink);
        }

        private static void TestTemporalDelete<TLinkAddress>(ILinks<TLinkAddress> links) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var temporalLinks = new TemporalResolver<TLinkAddress>(links);
            
            // Create initial link
            var initialLink = temporalLinks.Create();
            var initialCount = temporalLinks.Count();
            
            // Delete the link (should create a deletion record)
            var deletionRecord = temporalLinks.Delete(new[] { initialLink });
            
            // Should have more links now (original + deletion record + timestamp link)
            Assert.True(temporalLinks.Count() > initialCount);
            Assert.True(deletionRecord > temporalLinks.Constants.Null);
        }

        private static void TestTemporalQuery<TLinkAddress>(ILinks<TLinkAddress> links) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var temporalLinks = new TemporalResolver<TLinkAddress>(links);
            
            // Record timestamps for different operations
            var timestamp1 = new UniqueTimestampFactory().Create();
            System.Threading.Thread.Sleep(1); // Ensure different timestamps
            
            // Create initial link
            var initialLink = temporalLinks.Create();
            
            var timestamp2 = new UniqueTimestampFactory().Create();
            System.Threading.Thread.Sleep(1);
            
            // Update the link
            var source = TLinkAddress.CreateChecked(1);
            var target = TLinkAddress.CreateChecked(2);
            temporalLinks.Update(new[] { initialLink }, new[] { source, target });
            
            var timestamp3 = new UniqueTimestampFactory().Create();
            System.Threading.Thread.Sleep(1);
            
            // Delete the link
            temporalLinks.Delete(new[] { initialLink });
            
            var timestamp4 = new UniqueTimestampFactory().Create();
            
            // Test temporal queries
            var countAtStart = temporalLinks.CountAt(null, timestamp1);
            var countAfterCreate = temporalLinks.CountAt(null, timestamp2);
            var countAfterUpdate = temporalLinks.CountAt(null, timestamp3);
            var countAfterDelete = temporalLinks.CountAt(null, timestamp4);
            
            // Verify that counts change over time appropriately
            // Note: These assertions might need adjustment based on the actual implementation
            // as we're dealing with timestamp links and deletion records
            Assert.True(countAfterCreate > countAtStart);
            Assert.True(countAfterUpdate >= countAfterCreate);
            Assert.True(countAfterDelete >= countAfterUpdate);
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            action(unitedMemoryLinks);
        }
    }
}