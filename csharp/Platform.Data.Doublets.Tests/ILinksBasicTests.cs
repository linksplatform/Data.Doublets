﻿
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
    }
}
