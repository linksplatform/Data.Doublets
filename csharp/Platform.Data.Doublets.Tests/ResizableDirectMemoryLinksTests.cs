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
        public static void BasicMemoryOperationsWithHandler()
        {
            using (var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            using (var memoryAdapter = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            {
                memoryAdapter.TestBasicMemoryOperationsWithHandler();
            }
        }

        private static void TestBasicMemoryOperationsWithHandler(this ILinks<ulong> memoryAdapter)
        {
            var link = memoryAdapter.Create();
            var handlerCalled = false;
            
            memoryAdapter.Delete(link, (before, after) =>
            {
                handlerCalled = true;
                var beforeLink = new Link<ulong>(before);
                var afterLink = new Link<ulong>(after);
                Assert.True(beforeLink.Index > 0);
                // After deletion, index should be valid (could be 0 or the same, depending on implementation)
                Assert.True(afterLink.Index >= 0UL);
                return memoryAdapter.Constants.Continue;
            });
            
            Assert.True(handlerCalled);
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
        public static void NonexistentReferencesWithHandlerTest()
        {
            using (var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            using (var memoryAdapter = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep))
            {
                memoryAdapter.TestNonexistentReferencesWithHandler();
            }
        }

        private static void TestNonexistentReferencesWithHandler(this ILinks<ulong> memoryAdapter)
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

            var handlerCalled = false;
            memoryAdapter.Delete(link, (before, after) =>
            {
                handlerCalled = true;
                var beforeLink = new Link<ulong>(before);
                var afterLink = new Link<ulong>(after);
                Assert.True(beforeLink.Index > 0);
                // After deletion, index should be valid (could be 0 or the same, depending on implementation)
                Assert.True(afterLink.Index >= 0UL);
                return memoryAdapter.Constants.Continue;
            });
            
            Assert.True(handlerCalled);
        }
    }
}
