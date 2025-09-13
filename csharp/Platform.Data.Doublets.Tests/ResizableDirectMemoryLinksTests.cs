using System.IO;
using Xunit;
using Platform.Singletons;
using Platform.Memory;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Tests
{
    public static class ResizableDirectMemoryLinksTests
    {
        private static readonly LinksConstants<ulong> _constants = Default<LinksConstants<ulong>>.Instance;

        [Fact]
        public static void BasicFileMappedMemoryTest()
        {
            var tempFilename = Path.GetTempFileName();
            using (var memoryAdapter = new TemporaryFileMappedResizableDirectMemory())
            {
                using (var unitedMemoryLinksStorage = new UnitedMemoryLinks<ulong>(memoryAdapter))
                {
                    unitedMemoryLinksStorage.TestBasicMemoryOperations();
                }
            }
            File.Delete(tempFilename);
        }

        [Fact]
        public static void BasicHeapMemoryTest()
        {
            using (var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            using (var memoryAdapter = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            {
                memoryAdapter.TestBasicMemoryOperations();
            }
        }
        private static void TestBasicMemoryOperations(this ILinks<ulong> memoryAdapter)
        {
            var link = memoryAdapter.Create();
            memoryAdapter.Delete(link);
        }

        [Fact]
        public static void NonexistentReferencesHeapMemoryTest()
        {
            using (var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            using (var memoryAdapter = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            {
                memoryAdapter.TestNonexistentReferences();
            }
        }
        private static void TestNonexistentReferences(this ILinks<ulong> memoryAdapter)
        {
            var link = memoryAdapter.Create();
            memoryAdapter.Update(link, ulong.MaxValue, ulong.MaxValue);
            var resultLink = _constants.Null;
            memoryAdapter.Each(foundLink =>
            {
                resultLink = memoryAdapter.GetIndex(foundLink);
                return _constants.Break;
            }, _constants.Any, ulong.MaxValue, ulong.MaxValue);
            Assert.True(resultLink == link);
            Assert.True(memoryAdapter.Count(ulong.MaxValue) == 0);
            memoryAdapter.Delete(link);
        }

        [Fact]
        public static void DataRecoveryTest()
        {
            using (var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            using (var memoryAdapter = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            {
                memoryAdapter.TestDataRecovery();
            }
        }
        
        private static void TestDataRecovery(this ILinks<ulong> memoryAdapter)
        {
            // Create some valid links
            var link1 = memoryAdapter.Create();
            var link2 = memoryAdapter.Create();
            var link3 = memoryAdapter.Create();
            
            // Create a link that references other links
            var validRefLink = memoryAdapter.Update(memoryAdapter.Create(), link1, link2);
            
            // Create a link to delete, then create another link referencing it
            var linkToDelete = memoryAdapter.Create();
            var invalidLink = memoryAdapter.Create();
            
            // Update invalid link to reference the link we'll delete
            memoryAdapter.Update(invalidLink, linkToDelete, linkToDelete);
            
            // Now delete the referenced link, making our other link invalid
            memoryAdapter.Delete(new[] { linkToDelete });
            
            // Count links before recovery
            var linksCountBefore = memoryAdapter.Count();
            
            // Perform recovery
            var removedCount = memoryAdapter.RecoverData();
            
            // Count links after recovery
            var linksCountAfter = memoryAdapter.Count();
            
            // Verify that invalid link was removed
            Assert.True(removedCount >= 1, $"Expected at least 1 invalid link to be removed, but got {removedCount}");
            Assert.True(linksCountAfter < linksCountBefore, "Links count should be reduced after recovery");
            
            // Verify that valid links still exist
            Assert.True(memoryAdapter.Exists(link1), "Valid link1 should still exist after recovery");
            Assert.True(memoryAdapter.Exists(link2), "Valid link2 should still exist after recovery"); 
            Assert.True(memoryAdapter.Exists(link3), "Valid link3 should still exist after recovery");
            Assert.True(memoryAdapter.Exists(validRefLink), "Valid reference link should still exist after recovery");
            
            // Clean up
            memoryAdapter.Delete(validRefLink);
            memoryAdapter.Delete(link3);
            memoryAdapter.Delete(link2);
            memoryAdapter.Delete(link1);
        }
    }
}
