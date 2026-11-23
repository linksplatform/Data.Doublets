using System;
using System.IO;
using System.Numerics;
using Platform.Memory;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Memory.Split.Generic;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class LinksFactoryTests
    {
        [Fact]
        public static void CreateWithDefaultOptionsTest()
        {
            TestWithTempFile(filePath =>
            {
                Using<ulong>(filePath, options => {
                    var links = LinksFactory.Create<ulong>(filePath, options);
                    Assert.NotNull(links);
                    Assert.IsAssignableFrom<UnitedMemoryLinks<ulong>>(links); // Default is United
                });
            });
        }

        [Fact]
        public static void CreateWithSplitDataFromIndexesOptionTest()
        {
            TestWithTempFile(filePath =>
            {
                Using<ulong>(filePath, options => {
                    options.SplitDataFromIndexes = true;
                    var links = LinksFactory.Create<ulong>(filePath, options);
                    Assert.NotNull(links);
                    Assert.IsAssignableFrom<SplitMemoryLinks<ulong>>(links);
                });
            });
        }

        [Fact]
        public static void CreateWithUnitedMemoryTest()
        {
            TestWithTempFile(filePath =>
            {
                Using<ulong>(filePath, options => {
                    options.SplitDataFromIndexes = false;
                    var links = LinksFactory.Create<ulong>(filePath, options);
                    Assert.NotNull(links);
                    Assert.IsAssignableFrom<UnitedMemoryLinks<ulong>>(links);
                });
            });
        }

        [Fact]
        public static void CreateWithMemoryInstanceTest()
        {
            Using<ulong>((string)null, options => {
                var memory = new HeapResizableDirectMemory();
                var links = LinksFactory.Create<ulong>(memory, options);
                Assert.NotNull(links);
                Assert.IsAssignableFrom<UnitedMemoryLinks<ulong>>(links);
            });
        }

        [Fact]
        public static void CreateWithSeparateMemoriesTest()
        {
            Using<ulong>((string)null, options => {
                var dataMemory = new HeapResizableDirectMemory();
                var indexMemory = new HeapResizableDirectMemory();
                var links = LinksFactory.Create<ulong>(dataMemory, indexMemory, options);
                Assert.NotNull(links);
                Assert.IsAssignableFrom<SplitMemoryLinks<ulong>>(links);
            });
        }

        [Fact]
        public static void CreateWithInvalidSplitOptionThrowsExceptionTest()
        {
            Using<ulong>((string)null, options => {
                options.SplitDataFromIndexes = true;
                var memory = new HeapResizableDirectMemory();
                
                Assert.Throws<ArgumentException>(() => LinksFactory.Create<ulong>(memory, options));
            });
        }

        [Fact]
        public static void CreateWithCustomOptionsTest()
        {
            TestWithTempFile(filePath =>
            {
                Using<ulong>(filePath, options => {
                    options.MemoryReservationStep = 2048;
                    options.IndexTreeType = IndexTreeType.SizeBalancedTree;
                    options.UseLinkedList = false;
                    options.SplitDataFromIndexes = true;
                    
                    var links = LinksFactory.Create<ulong>(filePath, options);
                    Assert.NotNull(links);
                    Assert.IsAssignableFrom<SplitMemoryLinks<ulong>>(links);
                    
                    // Test basic operations work
                    var link = links.Create();
                    Assert.True(links.Exists(link));
                });
            });
        }

        [Fact]
        public static void CreateSimpleWithFilePathTest()
        {
            TestWithTempFile(filePath =>
            {
                var links = LinksFactory.Create<ulong>(filePath);
                Assert.NotNull(links);
                Assert.IsAssignableFrom<UnitedMemoryLinks<ulong>>(links);
                
                // Test basic operations work
                var link = links.Create();
                Assert.True(links.Exists(link));
            });
        }

        [Fact]
        public static void LinksOptionsCloneTest()
        {
            var originalOptions = new LinksOptions<ulong>
            {
                SplitDataFromIndexes = true,
                MemoryReservationStep = 4096,
                IndexTreeType = IndexTreeType.SizeBalancedTree,
                UseLinkedList = false
            };

            var clonedOptions = originalOptions.Clone();

            Assert.NotSame(originalOptions, clonedOptions);
            Assert.Equal(originalOptions.SplitDataFromIndexes, clonedOptions.SplitDataFromIndexes);
            Assert.Equal(originalOptions.MemoryReservationStep, clonedOptions.MemoryReservationStep);
            Assert.Equal(originalOptions.IndexTreeType, clonedOptions.IndexTreeType);
            Assert.Equal(originalOptions.UseLinkedList, clonedOptions.UseLinkedList);
            Assert.Same(originalOptions.Constants, clonedOptions.Constants); // Constants reference should be same
        }

        [Fact]
        public static void LinksOptionsDefaultValuesTest()
        {
            var options = new LinksOptions<ulong>();
            
            Assert.False(options.SplitDataFromIndexes); // Default should be false (United)
            Assert.Equal(1048576, options.MemoryReservationStep); // 1MB default
            Assert.Equal(IndexTreeType.Default, options.IndexTreeType);
            Assert.True(options.UseLinkedList);
            Assert.NotNull(options.Constants);
        }

        private static void Using<TLinkAddress>(string filePath, Action<LinksOptions<TLinkAddress>> action) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var options = new LinksOptions<TLinkAddress>();
            action(options);
        }

        private static void TestWithTempFile(Action<string> action)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.Delete(tempFile); // Delete the empty file created by GetTempFileName
                action(tempFile);
            }
            finally
            {
                // Cleanup temp files that might have been created
                try
                {
                    if (File.Exists(tempFile)) File.Delete(tempFile);
                    if (File.Exists(tempFile + ".data")) File.Delete(tempFile + ".data");
                    if (File.Exists(tempFile + ".index")) File.Delete(tempFile + ".index");
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }
        }
    }
}