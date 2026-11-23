using System;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class LinkReturningExtensionsTests
    {
        [Fact]
        public static void SearchOrDefaultAsLinkTest()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var source = links.CreatePoint();
            var target = links.CreatePoint();
            var linkAddress = links.CreateAndUpdate(source, target);

            // Test existing link
            var foundLink = links.SearchOrDefaultAsLink(source, target);
            Assert.False(foundLink.IsNull());
            Assert.Equal(linkAddress, foundLink.Index);
            Assert.Equal(source, foundLink.Source);
            Assert.Equal(target, foundLink.Target);

            // Test non-existing link
            var anotherSource = links.CreatePoint();
            var notFoundLink = links.SearchOrDefaultAsLink(anotherSource, target);
            Assert.True(notFoundLink.IsNull());
        }

        [Fact]
        public static void GetOrCreateAsLinkTest()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var source = links.CreatePoint();
            var target = links.CreatePoint();

            // Test creation
            var createdLink = links.GetOrCreateAsLink(source, target);
            Assert.False(createdLink.IsNull());
            Assert.Equal(source, createdLink.Source);
            Assert.Equal(target, createdLink.Target);

            // Test getting existing
            var existingLink = links.GetOrCreateAsLink(source, target);
            Assert.Equal(createdLink.Index, existingLink.Index);
            Assert.Equal(createdLink.Source, existingLink.Source);
            Assert.Equal(createdLink.Target, existingLink.Target);
        }

        [Fact]
        public static void CreateAndUpdateAsLinkTest()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var source = links.CreatePoint();
            var target = links.CreatePoint();

            var createdLink = links.CreateAndUpdateAsLink(source, target);
            Assert.False(createdLink.IsNull());
            Assert.Equal(source, createdLink.Source);
            Assert.Equal(target, createdLink.Target);

            // Verify link exists in storage
            Assert.True(links.Exists(createdLink.Index));
        }

        [Fact]
        public static void CreatePointAsLinkTest()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var pointLink = links.CreatePointAsLink();
            Assert.False(pointLink.IsNull());
            Assert.Equal(pointLink.Index, pointLink.Source);
            Assert.Equal(pointLink.Index, pointLink.Target);
        }

        [Fact]
        public static void UpdateAsLinkTest()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var originalSource = links.CreatePoint();
            var originalTarget = links.CreatePoint();
            var linkAddress = links.CreateAndUpdate(originalSource, originalTarget);

            var newSource = links.CreatePoint();
            var newTarget = links.CreatePoint();

            var updatedLink = links.UpdateAsLink(linkAddress, newSource, newTarget);
            Assert.False(updatedLink.IsNull());
            Assert.Equal(linkAddress, updatedLink.Index);
            Assert.Equal(newSource, updatedLink.Source);
            Assert.Equal(newTarget, updatedLink.Target);
        }

        [Fact]
        public static void FirstAsLinkTest()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var firstPoint = links.CreatePoint();

            var firstLink = links.FirstAsLink();
            Assert.False(firstLink.IsNull());
            Assert.Equal(firstPoint, firstLink.Index);
        }

        [Fact]
        public static void FirstAsLinkThrowsOnEmptyStorageTest()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            Assert.Throws<InvalidOperationException>(() => links.FirstAsLink());
        }
    }
}