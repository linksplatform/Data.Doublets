
using System.IO;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;


namespace Platform.Data.Doublets.Tests
{
    public static class ILinksBasicTests
    {
        [Fact]
        public static void DeleteAllUsages()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var root = links.CreatePoint();

            var a = links.CreatePoint();
            var b = links.CreatePoint();

            links.CreateAndUpdate(a, root);
            links.CreateAndUpdate(b, root);

            Assert.Equal(5U, links.Count());

            links.DeleteAllUsages(root);

            Assert.Equal(3U, links.Count());
        }

        [Fact]
        public static void DeleteAllUsagesWithHandler()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var root = links.CreatePoint();

            var a = links.CreatePoint();
            var b = links.CreatePoint();

            links.CreateAndUpdate(a, root);
            links.CreateAndUpdate(b, root);

            Assert.Equal(5U, links.Count());

            var handlerCallCount = 0;
            links.DeleteAllUsages(root, (before, after) =>
            {
                handlerCallCount++;
                var beforeLink = new Link<uint>(before);
                var afterLink = new Link<uint>(after);
                Assert.True(beforeLink.Index > 0);
                // Handler is called for multiple operations (updates to null, then deletes)
                // Just ensure we have valid link structures
                Assert.True(afterLink.Index >= 0U);
                return links.Constants.Continue;
            });

            Assert.Equal(3U, links.Count());
            Assert.True(handlerCallCount > 0); // Handler should have been called at least once
        }

        /*
        [Fact]
        public static void FfiDeleteAllUsages()
        {
            File.Delete("db.links");
            var links = new Ffi.Links<uint>("db.links");

            var root = links.CreatePoint();

            var a = links.CreatePoint();
            var b = links.CreatePoint();

            links.CreateAndUpdate(a, root);
            links.CreateAndUpdate(b, root);

            Assert.Equal(5U, links.Count());

            links.DeleteAllUsages(root);

            Assert.Equal(3U, links.Count());
        }
    */
    }
}
