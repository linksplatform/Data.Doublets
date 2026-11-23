using System.Collections.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class InvertedEachTests
    {
        [Fact]
        public static void InvertedEach_AllLinksPattern_ReturnsNothing()
        {
            // Arrange
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            // Create some test links
            var link1 = links.CreatePoint();
            var link2 = links.CreatePoint();
            var link3 = links.CreateAndUpdate(link1, link2);

            var foundLinks = new List<uint>();
            var constants = links.Constants;

            // Act - InvertedEach with (* *) should return nothing
            links.InvertedEach(foundLink =>
            {
                foundLinks.Add(links.GetIndex(foundLink));
                return constants.Continue;
            }, constants.Any, constants.Any);

            // Assert
            Assert.Empty(foundLinks);
        }

        [Fact]
        public static void InvertedEach_SpecificSourcePattern_ExcludesMatchingLinks()
        {
            // Arrange
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var point1 = links.CreatePoint(); // 1
            var point2 = links.CreatePoint(); // 2 
            var point3 = links.CreatePoint(); // 3
            var link12 = links.CreateAndUpdate(point1, point2); // 4: (1, 2)
            var link23 = links.CreateAndUpdate(point2, point3); // 5: (2, 3)
            var link13 = links.CreateAndUpdate(point1, point3); // 6: (1, 3)

            var foundLinks = new List<uint>();
            var constants = links.Constants;

            // Act - InvertedEach excluding all links with source = point1 (x *)
            links.InvertedEach(foundLink =>
            {
                foundLinks.Add(links.GetIndex(foundLink));
                return constants.Continue;
            }, point1, constants.Any);

            // Assert - Should find all links except those with point1 as source
            // Expected: point1, point2, point3, link23 (but not link12, link13)
            Assert.Contains(point1, foundLinks);
            Assert.Contains(point2, foundLinks);
            Assert.Contains(point3, foundLinks);
            Assert.Contains(link23, foundLinks);
            Assert.DoesNotContain(link12, foundLinks);
            Assert.DoesNotContain(link13, foundLinks);
        }

        [Fact]
        public static void InvertedEach_SpecificTargetPattern_ExcludesMatchingLinks()
        {
            // Arrange
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var point1 = links.CreatePoint(); // 1
            var point2 = links.CreatePoint(); // 2
            var point3 = links.CreatePoint(); // 3
            var link12 = links.CreateAndUpdate(point1, point2); // 4: (1, 2)
            var link23 = links.CreateAndUpdate(point2, point3); // 5: (2, 3)
            var link13 = links.CreateAndUpdate(point1, point3); // 6: (1, 3)

            var foundLinks = new List<uint>();
            var constants = links.Constants;

            // Act - InvertedEach excluding all links with target = point3 (* y)
            links.InvertedEach(foundLink =>
            {
                foundLinks.Add(links.GetIndex(foundLink));
                return constants.Continue;
            }, constants.Any, point3);

            // Assert - Should find all links except those with point3 as target
            // Expected: point1, point2, point3, link12 (but not link23, link13)
            Assert.Contains(point1, foundLinks);
            Assert.Contains(point2, foundLinks);
            Assert.Contains(point3, foundLinks);
            Assert.Contains(link12, foundLinks);
            Assert.DoesNotContain(link23, foundLinks);
            Assert.DoesNotContain(link13, foundLinks);
        }

        [Fact]
        public static void InvertedEach_SpecificLinkPattern_ExcludesExactLink()
        {
            // Arrange
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var point1 = links.CreatePoint(); // 1
            var point2 = links.CreatePoint(); // 2
            var point3 = links.CreatePoint(); // 3
            var link12 = links.CreateAndUpdate(point1, point2); // 4: (1, 2)
            var link23 = links.CreateAndUpdate(point2, point3); // 5: (2, 3)
            var link13 = links.CreateAndUpdate(point1, point3); // 6: (1, 3)

            var foundLinks = new List<uint>();
            var constants = links.Constants;

            // Act - InvertedEach excluding specific link (point1, point2)
            links.InvertedEach(foundLink =>
            {
                foundLinks.Add(links.GetIndex(foundLink));
                return constants.Continue;
            }, point1, point2);

            // Assert - Should find all links except the specific (1, 2) link
            // Expected: point1, point2, point3, link23, link13 (but not link12)
            Assert.Contains(point1, foundLinks);
            Assert.Contains(point2, foundLinks);
            Assert.Contains(point3, foundLinks);
            Assert.Contains(link23, foundLinks);
            Assert.Contains(link13, foundLinks);
            Assert.DoesNotContain(link12, foundLinks);
        }

        [Fact]
        public static void InvertedEach_NoRestriction_ReturnsAllLinks()
        {
            // Arrange
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var point1 = links.CreatePoint();
            var point2 = links.CreatePoint();
            var link12 = links.CreateAndUpdate(point1, point2);

            var foundLinksInverted = new List<uint>();
            var foundLinksRegular = new List<uint>();
            var constants = links.Constants;

            // Act - InvertedEach with no restriction should be same as regular Each
            links.InvertedEach(foundLink =>
            {
                foundLinksInverted.Add(links.GetIndex(foundLink));
                return constants.Continue;
            });

            links.Each(foundLink =>
            {
                foundLinksRegular.Add(links.GetIndex(foundLink));
                return constants.Continue;
            });

            // Assert
            Assert.Equal(foundLinksRegular.Count, foundLinksInverted.Count);
            foreach (var link in foundLinksRegular)
            {
                Assert.Contains(link, foundLinksInverted);
            }
        }

        [Fact]
        public static void InvertedEach_IListOverload_WorksCorrectly()
        {
            // Arrange
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            var point1 = links.CreatePoint();
            var point2 = links.CreatePoint();
            var point3 = links.CreatePoint();
            var link12 = links.CreateAndUpdate(point1, point2);

            var foundLinks = new List<uint>();
            var constants = links.Constants;
            var restriction = new List<uint> { point1, constants.Any };

            // Act - Use IList overload
            links.InvertedEach(restriction, foundLink =>
            {
                foundLinks.Add(links.GetIndex(foundLink));
                return constants.Continue;
            });

            // Assert - Should exclude links with source = point1
            Assert.Contains(point1, foundLinks);
            Assert.Contains(point2, foundLinks);
            Assert.Contains(point3, foundLinks);
            Assert.DoesNotContain(link12, foundLinks);
        }
    }
}