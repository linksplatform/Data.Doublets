using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Decorators;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// <para>
    /// Tests for basic non-indexed links implementation and indexed decorator.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class BasicLinksTests
    {
        [Fact]
        public static void BasicLinksCreateAndDeleteTest()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new BasicUnitedMemoryLinks<uint>(memory);

            // Test basic operations
            var point = links.CreatePoint();
            Assert.True(point > 0);
            Assert.Equal(1U, links.Count());

            var link = links.Create(point, point);
            Assert.True(link > 0);
            Assert.Equal(2U, links.Count());

            links.Delete(link);
            Assert.Equal(1U, links.Count());

            links.Delete(point);
            Assert.Equal(0U, links.Count());
        }

        [Fact]
        public static void IndexedDecoratorTest()
        {
            var memory = new HeapResizableDirectMemory();
            var basicLinks = new BasicUnitedMemoryLinks<uint>(memory);
            var indexedLinks = new IndexedLinksDecorator<uint>(basicLinks);

            // Test operations with indexing
            var point1 = indexedLinks.CreatePoint();
            var point2 = indexedLinks.CreatePoint();
            var link = indexedLinks.Create(point1, point2);

            Assert.Equal(3U, indexedLinks.Count());

            // Test indexed search
            var restriction = new uint[] { indexedLinks.Constants.Any, point1, indexedLinks.Constants.Any };
            var count = indexedLinks.Count(restriction);
            Assert.Equal(1U, count);

            indexedLinks.Delete(link);
            indexedLinks.Delete(point1);
            indexedLinks.Delete(point2);
            Assert.Equal(0U, indexedLinks.Count());
        }

        [Fact]
        public static void LinksBuilderBasicModeTest()
        {
            var links = LinksBuilder<uint>
                .InMemory()
                .UseBasicIndexing()
                .Build();

            TestBasicOperations(links);
        }

        [Fact]
        public static void LinksBuilderHashIndexedModeTest()
        {
            var links = LinksBuilder<uint>
                .InMemory()
                .UseHashIndexing()
                .Build();

            TestBasicOperations(links);
        }

        [Fact]
        public static void LinksBuilderTreeIndexedModeTest()
        {
            var links = LinksBuilder<uint>
                .InMemory()
                .UseTreeIndexing()
                .Build();

            TestBasicOperations(links);
        }

        [Fact]
        public static void LinksBuilderSelectiveHashIndexingTest()
        {
            var links = LinksBuilder<uint>
                .InMemory()
                .UseHashIndexing(enableSourceIndex: true, enableTargetIndex: false, enableLinkIndex: true)
                .Build();

            TestBasicOperations(links);
        }

        private static void TestBasicOperations(ILinks<uint> links)
        {
            // Create some links
            var point1 = links.CreatePoint();
            var point2 = links.CreatePoint();
            var point3 = links.CreatePoint();

            var link1 = links.Create(point1, point2);
            var link2 = links.Create(point2, point3);
            var link3 = links.Create(point1, point3);

            // Test count
            Assert.Equal(6U, links.Count());

            // Test search by source
            var restriction = new uint[] { links.Constants.Any, point1, links.Constants.Any };
            var count = links.Count(restriction);
            Assert.True(count >= 2U); // Should find link1 and link3

            // Test search by target
            restriction = new uint[] { links.Constants.Any, links.Constants.Any, point3 };
            count = links.Count(restriction);
            Assert.True(count >= 2U); // Should find link2 and link3

            // Test search by specific source-target pair
            restriction = new uint[] { links.Constants.Any, point1, point2 };
            count = links.Count(restriction);
            Assert.Equal(1U, count); // Should find exactly link1

            // Test cleanup
            links.Delete(link1);
            links.Delete(link2);
            links.Delete(link3);
            links.Delete(point1);
            links.Delete(point2);
            links.Delete(point3);

            Assert.Equal(0U, links.Count());
        }

        [Fact]
        public static void ComparePerformanceBetweenImplementations()
        {
            const int linksToCreate = 1000;

            // Test basic implementation
            var basicMemory = new HeapResizableDirectMemory();
            var basicLinks = new BasicUnitedMemoryLinks<uint>(basicMemory);
            var basicTime = MeasureCreationTime(basicLinks, linksToCreate);

            // Test hash indexed implementation
            var indexedMemory = new HeapResizableDirectMemory();
            var indexedBasicLinks = new BasicUnitedMemoryLinks<uint>(indexedMemory);
            var indexedLinks = new IndexedLinksDecorator<uint>(indexedBasicLinks);
            var indexedTime = MeasureCreationTime(indexedLinks, linksToCreate);

            // Test tree indexed implementation
            var treeMemory = new HeapResizableDirectMemory();
            var treeLinks = new UnitedMemoryLinks<uint>(treeMemory);
            var treeTime = MeasureCreationTime(treeLinks, linksToCreate);

            // All implementations should work correctly
            Assert.Equal((uint)(linksToCreate * 2), basicLinks.Count()); // points + links
            Assert.Equal((uint)(linksToCreate * 2), indexedLinks.Count());
            Assert.Equal((uint)(linksToCreate * 2), treeLinks.Count());

            // Performance comparison is informational
            // (actual performance depends on the operations and data access patterns)
        }

        private static long MeasureCreationTime(ILinks<uint> links, int count)
        {
            var start = System.Diagnostics.Stopwatch.GetTimestamp();

            for (int i = 0; i < count; i++)
            {
                var point = links.CreatePoint();
                links.Create(point, point);
            }

            var end = System.Diagnostics.Stopwatch.GetTimestamp();
            return end - start;
        }
    }
}