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
    }
}
