using System;
using Platform.Data.Doublets.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests.Memory
{
    public static class SystemMemoryInfoTests
    {
        [Fact]
        public static void GetTotalPhysicalMemoryTest()
        {
            var totalMemory = SystemMemoryInfo.GetTotalPhysicalMemory();
            
            // Should return a positive value
            Assert.True(totalMemory > 0);
            
            // Should be at least 1 MB (very conservative lower bound)
            Assert.True(totalMemory >= 1024 * 1024);
            
            // Should be less than 1 TB (conservative upper bound for testing)
            Assert.True(totalMemory <= 1024L * 1024 * 1024 * 1024);
        }

        [Fact]
        public static void GetAvailablePhysicalMemoryTest()
        {
            var availableMemory = SystemMemoryInfo.GetAvailablePhysicalMemory();
            var totalMemory = SystemMemoryInfo.GetTotalPhysicalMemory();
            
            // Should return a positive value
            Assert.True(availableMemory > 0);
            
            // Available memory should not exceed total memory
            Assert.True(availableMemory <= totalMemory);
            
            // Should be at least some memory available (conservative check)
            Assert.True(availableMemory >= 1024 * 1024); // At least 1 MB
        }

        [Fact]
        public static void GetTotalDiskSpaceTest()
        {
            var currentDirectory = Environment.CurrentDirectory;
            var totalDiskSpace = SystemMemoryInfo.GetTotalDiskSpace(currentDirectory);
            
            // Should return a positive value
            Assert.True(totalDiskSpace > 0);
            
            // Should be at least 1 GB (modern systems)
            Assert.True(totalDiskSpace >= 1024L * 1024 * 1024);
        }

        [Fact]
        public static void GetAvailableDiskSpaceTest()
        {
            var currentDirectory = Environment.CurrentDirectory;
            var availableDiskSpace = SystemMemoryInfo.GetAvailableDiskSpace(currentDirectory);
            var totalDiskSpace = SystemMemoryInfo.GetTotalDiskSpace(currentDirectory);
            
            // Should return a positive value
            Assert.True(availableDiskSpace > 0);
            
            // Available disk space should not exceed total disk space
            Assert.True(availableDiskSpace <= totalDiskSpace);
        }

        [Fact]
        public static void DiskSpaceConsistencyTest()
        {
            var path1 = Environment.CurrentDirectory;
            var path2 = System.IO.Path.Combine(Environment.CurrentDirectory, "subdir");
            
            var totalSpace1 = SystemMemoryInfo.GetTotalDiskSpace(path1);
            var totalSpace2 = SystemMemoryInfo.GetTotalDiskSpace(path2);
            
            // Both paths should report the same total disk space (same drive)
            Assert.Equal(totalSpace1, totalSpace2);
            
            var availableSpace1 = SystemMemoryInfo.GetAvailableDiskSpace(path1);
            var availableSpace2 = SystemMemoryInfo.GetAvailableDiskSpace(path2);
            
            // Available space should be the same (or very close) for paths on same drive
            Assert.Equal(availableSpace1, availableSpace2);
        }

        [Fact]
        public static void MemoryValuesReasonablenessTest()
        {
            var totalMemory = SystemMemoryInfo.GetTotalPhysicalMemory();
            var availableMemory = SystemMemoryInfo.GetAvailablePhysicalMemory();
            
            // Available should be at least 10% of total (systems usually have some free RAM)
            Assert.True(availableMemory >= totalMemory * 0.1);
            
            // Available should be at most 95% of total (some is always used by OS)
            Assert.True(availableMemory <= totalMemory * 0.95);
        }
    }
}