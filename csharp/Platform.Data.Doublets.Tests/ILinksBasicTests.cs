
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

        [Fact]
        public static void CreateAndUpdateWithIListParameter()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var source = links.CreatePoint();
            var target = links.CreatePoint();

            // Test CreateAndUpdate with IList parameter (2 elements)
            var restriction = new uint[] { source, target };
            var result1 = links.CreateAndUpdate(restriction);
            
            Assert.True(result1 > 0);
            Assert.Equal(4U, links.Count()); // 3 points + 1 created link

            // Test CreateAndUpdate with params array
            var result2 = links.CreateAndUpdate(source, target);
            
            Assert.True(result2 > 0);
            Assert.Equal(5U, links.Count()); // 3 points + 2 created links
        }

        [Fact]
        public static void CreateAndUpdateWithIListParameterAndHandler()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var source = links.CreatePoint();
            var target = links.CreatePoint();
            
            bool handlerCalled = false;
            var restriction = new uint[] { source, target };
            
            var result = links.CreateAndUpdate(restriction, (before, after) =>
            {
                handlerCalled = true;
                return links.Constants.Continue;
            });
            
            Assert.True(result > 0);
            Assert.True(handlerCalled);
            Assert.Equal(4U, links.Count()); // 3 points + 1 created link
        }

        [Fact]
        public static void CreateAndUpdateWithIListParameterInvalidCount()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var source = links.CreatePoint();
            var target = links.CreatePoint();
            var extra = links.CreatePoint();

            // Test with invalid count (3 elements instead of 2)
            var restriction = new uint[] { source, target, extra };
            
            Assert.Throws<System.ArgumentException>(() => links.CreateAndUpdate(restriction));
        }
    }
}
