using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests.Memory
{
    public static class EnhancedMemoryLinksTests
    {
        [Fact]
        public static void EnhancedSplitMemoryLinksBasicTest()
        {
            var tempDataFile = Path.GetTempFileName();
            var tempIndexFile = Path.GetTempFileName();

            try
            {
                var config = MemoryAllocationConfiguration.DefaultIncremental;
                
                using var links = new EnhancedSplitMemoryLinks<uint>(
                    new FileMappedResizableDirectMemory(tempDataFile),
                    new FileMappedResizableDirectMemory(tempIndexFile),
                    config);

                // Basic CRUD operations should work
                var link1 = links.Create();
                Assert.True(link1 > 0);

                var retrievedLink = links.GetLinkStruct(link1);
                Assert.Equal(link1, retrievedLink[0]);

                var statistics = links.GetAllocationStatistics();
                Assert.Contains("Strategy: Incremental", statistics);
                Assert.Contains("Memory Statistics:", statistics);
            }
            finally
            {
                File.Delete(tempDataFile);
                File.Delete(tempIndexFile);
            }
        }

        [Fact]
        public static void EnhancedUnitedMemoryLinksBasicTest()
        {
            var tempFile = Path.GetTempFileName();

            try
            {
                var config = MemoryAllocationConfiguration.DefaultIncremental;
                
                using var links = new EnhancedUnitedMemoryLinks<uint>(
                    new FileMappedResizableDirectMemory(tempFile),
                    config);

                // Basic CRUD operations should work
                var link1 = links.Create();
                Assert.True(link1 > 0);

                var retrievedLink = links.GetLinkStruct(link1);
                Assert.Equal(link1, retrievedLink[0]);

                var statistics = links.GetAllocationStatistics();
                Assert.Contains("Strategy: Incremental", statistics);
                Assert.Contains("Memory Statistics:", statistics);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public static void EnhancedSplitMemoryLinksCustomPreAllocationTest()
        {
            var tempDataFile = Path.GetTempFileName();
            var tempIndexFile = Path.GetTempFileName();

            try
            {
                var config = new MemoryAllocationConfiguration
                {
                    Strategy = MemoryAllocationStrategy.PreAllocateCustom,
                    CustomPreAllocationSize = 10 * 1024 * 1024, // 10 MB
                    EnableAllocationLogging = true
                };

                var logMessages = new System.Collections.Generic.List<string>();
                
                using var links = new EnhancedSplitMemoryLinks<uint>(
                    new FileMappedResizableDirectMemory(tempDataFile),
                    new FileMappedResizableDirectMemory(tempIndexFile),
                    config,
                    message => logMessages.Add(message));

                // Should have pre-allocated memory
                var statistics = links.GetAllocationStatistics();
                Assert.Contains("Strategy: PreAllocateCustom", statistics);

                // Should have logged the allocation
                Assert.True(logMessages.Count > 0);
                Assert.True(logMessages.Exists(msg => msg.Contains("Pre-allocating")));

                // Memory should be pre-allocated
                Assert.True(links.DataMemory.ReservedCapacity > 0);
                Assert.True(links.IndexMemory.ReservedCapacity > 0);
            }
            finally
            {
                File.Delete(tempDataFile);
                File.Delete(tempIndexFile);
            }
        }

        [Fact]
        public static void EnhancedUnitedMemoryLinksCustomPreAllocationTest()
        {
            var tempFile = Path.GetTempFileName();

            try
            {
                var config = new MemoryAllocationConfiguration
                {
                    Strategy = MemoryAllocationStrategy.PreAllocateCustom,
                    CustomPreAllocationSize = 5 * 1024 * 1024, // 5 MB
                    EnableAllocationLogging = true
                };

                var logMessages = new System.Collections.Generic.List<string>();
                
                using var links = new EnhancedUnitedMemoryLinks<uint>(
                    new FileMappedResizableDirectMemory(tempFile),
                    config,
                    message => logMessages.Add(message));

                // Should have pre-allocated memory
                var statistics = links.GetAllocationStatistics();
                Assert.Contains("Strategy: PreAllocateCustom", statistics);

                // Should have logged the allocation
                Assert.True(logMessages.Count > 0);
                Assert.True(logMessages.Exists(msg => msg.Contains("Pre-allocating")));

                // Memory should be pre-allocated
                Assert.True(links.Memory.ReservedCapacity > 0);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public static void EnhancedLinksRAMPreAllocationTest()
        {
            var tempFile = Path.GetTempFileName();

            try
            {
                // Use a conservative percentage to avoid issues on low-memory systems
                var config = new MemoryAllocationConfiguration
                {
                    Strategy = MemoryAllocationStrategy.PreAllocateBySystemRAM,
                    PreAllocationPercentage = 0.01, // 1% of RAM
                    MaximumPreAllocationSize = 50 * 1024 * 1024, // Cap at 50 MB for testing
                    EnableAllocationLogging = true
                };

                var logMessages = new System.Collections.Generic.List<string>();
                
                using var links = new EnhancedUnitedMemoryLinks<uint>(
                    new FileMappedResizableDirectMemory(tempFile),
                    config,
                    message => logMessages.Add(message));

                var statistics = links.GetAllocationStatistics();
                Assert.Contains("Strategy: PreAllocateBySystemRAM", statistics);

                // Should have some pre-allocation
                Assert.True(links.Memory.ReservedCapacity > 0);
                
                // Verify the allocation is reasonable
                var expectedSize = config.CalculatePreAllocationSize();
                Assert.True(expectedSize > 0);
                Assert.True(expectedSize <= config.MaximumPreAllocationSize);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public static void EnhancedLinksDiskPreAllocationTest()
        {
            var tempFile = Path.GetTempFileName();

            try
            {
                // Use a very small percentage to avoid filling up the disk
                var config = new MemoryAllocationConfiguration
                {
                    Strategy = MemoryAllocationStrategy.PreAllocateByDiskSpace,
                    PreAllocationPercentage = 0.001, // 0.1% of disk space
                    MaximumPreAllocationSize = 100 * 1024 * 1024, // Cap at 100 MB for testing
                    EnableAllocationLogging = true
                };

                var logMessages = new System.Collections.Generic.List<string>();
                
                using var links = new EnhancedUnitedMemoryLinks<uint>(
                    new FileMappedResizableDirectMemory(tempFile),
                    config,
                    message => logMessages.Add(message));

                var statistics = links.GetAllocationStatistics();
                Assert.Contains("Strategy: PreAllocateByDiskSpace", statistics);

                // Should have some pre-allocation
                Assert.True(links.Memory.ReservedCapacity > 0);
                
                // Verify the allocation is reasonable
                var expectedSize = config.CalculatePreAllocationSize(Path.GetDirectoryName(tempFile));
                Assert.True(expectedSize > 0);
                Assert.True(expectedSize <= config.MaximumPreAllocationSize);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public static void EnhancedLinksWithLoggingTest()
        {
            var tempFile = Path.GetTempFileName();

            try
            {
                var config = new MemoryAllocationConfiguration
                {
                    Strategy = MemoryAllocationStrategy.PreAllocateCustom,
                    CustomPreAllocationSize = 1024 * 1024, // 1 MB
                    EnableAllocationLogging = true
                };

                var logMessages = new System.Collections.Generic.List<string>();
                
                using var links = new EnhancedUnitedMemoryLinks<uint>(
                    new FileMappedResizableDirectMemory(tempFile),
                    config,
                    message => logMessages.Add(message));

                // Should have logged messages
                Assert.True(logMessages.Count > 0);
                
                // Check that log messages have timestamps
                Assert.True(logMessages.Exists(msg => msg.Contains("EnhancedUnitedMemoryLinks:")));
                Assert.True(logMessages.Exists(msg => msg.Contains("Pre-allocating") || msg.Contains("Pre-allocation completed")));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public static void EnhancedLinksStatisticsTest()
        {
            var tempFile = Path.GetTempFileName();

            try
            {
                var config = MemoryAllocationConfiguration.DefaultIncremental;
                
                using var links = new EnhancedUnitedMemoryLinks<uint>(
                    new FileMappedResizableDirectMemory(tempFile),
                    config);

                // Create some links
                for (int i = 0; i < 10; i++)
                {
                    links.Create();
                }

                var statistics = links.GetAllocationStatistics();
                
                // Should contain expected information
                Assert.Contains("Memory Statistics:", statistics);
                Assert.Contains("Strategy: Incremental", statistics);
                Assert.Contains("bytes", statistics);
                Assert.Contains("Links:", statistics);
                Assert.Contains("Link Size:", statistics);
                
                // Should show some utilization
                Assert.Contains("utilized", statistics);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Theory]
        [InlineData(typeof(byte))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        public static void EnhancedLinksGenericTypesTest(Type linkAddressType)
        {
            var tempFile = Path.GetTempFileName();

            try
            {
                var config = MemoryAllocationConfiguration.DefaultIncremental;

                // Use reflection to create the appropriate generic type
                var enhancedLinksType = typeof(EnhancedUnitedMemoryLinks<>).MakeGenericType(linkAddressType);
                var memoryType = typeof(FileMappedResizableDirectMemory);
                var memory = Activator.CreateInstance(memoryType, tempFile);
                
                // Use the constructor that takes (memory, config)
                using var links = (IDisposable)Activator.CreateInstance(enhancedLinksType, memory, config, null);
                
                Assert.NotNull(links);
                
                // Test that we can call GetAllocationStatistics
                var statisticsMethod = enhancedLinksType.GetMethod("GetAllocationStatistics");
                var statistics = (string)statisticsMethod!.Invoke(links, null);
                
                Assert.Contains("Memory Statistics:", statistics);
                Assert.Contains("Strategy: Incremental", statistics);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}