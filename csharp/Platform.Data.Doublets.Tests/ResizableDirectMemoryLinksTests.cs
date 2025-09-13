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
        public static void AutomaticGapsFillingDisabledTest()
        {
            using (var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            using (var memoryAdapter = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep, enableAutomaticGapsFilling: false))
            {
                memoryAdapter.TestAutomaticGapsFillingDisabled();
            }
        }
        
        private static void TestAutomaticGapsFillingDisabled(this ILinks<ulong> memoryAdapter)
        {
            // Create some links
            var link1 = memoryAdapter.Create();
            var link2 = memoryAdapter.Create();
            var link3 = memoryAdapter.Create();
            
            // Delete a link to create a gap
            memoryAdapter.Delete(link2);
            
            // When automatic gaps filling is disabled, the query should still work
            // but it may access deleted links (which is expected for append-only databases)
            var visitedLinks = new System.Collections.Generic.List<ulong>();
            memoryAdapter.Each(foundLink =>
            {
                visitedLinks.Add(memoryAdapter.GetIndex(foundLink));
                return _constants.Continue;
            });
            
            // Test that we can query specific existing links
            var existingLinks = new System.Collections.Generic.List<ulong>();
            memoryAdapter.Each(foundLink =>
            {
                existingLinks.Add(memoryAdapter.GetIndex(foundLink));
                return _constants.Continue;
            }, link1);
            
            Assert.True(existingLinks.Count == 1);
            Assert.True(existingLinks[0] == link1);
            
            // Clean up remaining links
            memoryAdapter.Delete(link1);
            memoryAdapter.Delete(link3);
        }
        
        [Fact]
        public static void AutomaticGapsFillingEnabledTest()
        {
            using (var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            using (var memoryAdapter = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep, enableAutomaticGapsFilling: true))
            {
                memoryAdapter.TestAutomaticGapsFillingEnabled();
            }
        }
        
        private static void TestAutomaticGapsFillingEnabled(this ILinks<ulong> memoryAdapter)
        {
            // Create some links
            var link1 = memoryAdapter.Create();
            var link2 = memoryAdapter.Create();
            var link3 = memoryAdapter.Create();
            
            // Delete a link to create a gap
            memoryAdapter.Delete(link2);
            
            // When automatic gaps filling is enabled (default), deleted links should be skipped
            var visitedLinks = new System.Collections.Generic.List<ulong>();
            memoryAdapter.Each(foundLink =>
            {
                visitedLinks.Add(memoryAdapter.GetIndex(foundLink));
                return _constants.Continue;
            });
            
            // Should not contain the deleted link
            Assert.True(!visitedLinks.Contains(link2));
            
            // But should contain the existing links
            Assert.True(visitedLinks.Contains(link1));
            Assert.True(visitedLinks.Contains(link3));
            
            // Clean up remaining links
            memoryAdapter.Delete(link1);
            memoryAdapter.Delete(link3);
        }
    }
}
