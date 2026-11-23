using System;
using System.IO;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests.Memory
{
    /// <summary>
    ///     <para>
    ///         Usage examples for the enhanced memory allocation strategies.
    ///         These examples demonstrate how to use the new pre-allocation features
    ///         to reduce memory reallocations and improve performance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static class PreAllocationUsageExample
    {
        [Fact]
        public static void BasicRAMPreAllocationExample()
        {
            // Example: Pre-allocate 75% of available RAM for optimal performance
            var tempFile = Path.GetTempFileName();

            try
            {
                // Configuration for 75% RAM pre-allocation
                var config = MemoryAllocationConfiguration.Default75PercentRAM;
                config.EnableAllocationLogging = true;

                var logMessages = new System.Collections.Generic.List<string>();

                using var links = new EnhancedUnitedMemoryLinks<ulong>(
                    new FileMappedResizableDirectMemory(tempFile),
                    config,
                    message => Console.WriteLine(message) // In practice, log to your preferred system
                );

                Console.WriteLine("=== Memory Allocation Example ===");
                Console.WriteLine(links.GetAllocationStatistics());
                Console.WriteLine();

                // Perform operations - these should have minimal reallocations
                Console.WriteLine("Creating 1000 links...");
                for (int i = 0; i < 1000; i++)
                {
                    var link = links.Create();
                    if (i == 0)
                    {
                        Console.WriteLine($"First link created: {link}");
                    }
                }

                Console.WriteLine("Final statistics:");
                Console.WriteLine(links.GetAllocationStatistics());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public static void CustomPreAllocationExample()
        {
            // Example: Pre-allocate a specific amount of memory
            var tempFile = Path.GetTempFileName();

            try
            {
                var config = new MemoryAllocationConfiguration
                {
                    Strategy = MemoryAllocationStrategy.PreAllocateCustom,
                    CustomPreAllocationSize = 50 * 1024 * 1024, // 50 MB
                    EnableAllocationLogging = true
                };

                using var links = new EnhancedUnitedMemoryLinks<uint>(tempFile, config, 
                    message => Console.WriteLine($"[LOG] {message}"));

                Console.WriteLine("=== Custom Pre-allocation Example ===");
                Console.WriteLine(links.GetAllocationStatistics());

                // Test the pre-allocated capacity
                var maxLinks = (links.Memory.ReservedCapacity - EnhancedUnitedMemoryLinks<uint>.LinkHeaderSizeInBytes) 
                               / EnhancedUnitedMemoryLinks<uint>.LinkSizeInBytes;
                
                Console.WriteLine($"Theoretical max links with current allocation: {maxLinks}");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public static void SplitMemoryPreAllocationExample()
        {
            // Example: Using split memory with disk-based pre-allocation
            var tempDataFile = Path.GetTempFileName();
            var tempIndexFile = Path.GetTempFileName();

            try
            {
                var config = new MemoryAllocationConfiguration
                {
                    Strategy = MemoryAllocationStrategy.PreAllocateByDiskSpace,
                    PreAllocationPercentage = 0.001, // 0.1% of disk space (conservative for testing)
                    MaximumPreAllocationSize = 10 * 1024 * 1024, // Cap at 10MB for example
                    EnableAllocationLogging = true
                };

                using var links = new EnhancedSplitMemoryLinks<ulong>(
                    new FileMappedResizableDirectMemory(tempDataFile),
                    new FileMappedResizableDirectMemory(tempIndexFile),
                    config,
                    message => Console.WriteLine($"[SPLIT LOG] {message}")
                );

                Console.WriteLine("=== Split Memory Pre-allocation Example ===");
                Console.WriteLine(links.GetAllocationStatistics());

                // Demonstrate that data and index are handled separately
                Console.WriteLine($"Data memory reserved: {links.DataMemory.ReservedCapacity:N0} bytes");
                Console.WriteLine($"Index memory reserved: {links.IndexMemory.ReservedCapacity:N0} bytes");
            }
            finally
            {
                File.Delete(tempDataFile);
                File.Delete(tempIndexFile);
            }
        }

        [Fact]
        public static void ConservativePreAllocationExample()
        {
            // Example: Conservative approach with bounds and fallback
            var tempFile = Path.GetTempFileName();

            try
            {
                var config = new MemoryAllocationConfiguration
                {
                    Strategy = MemoryAllocationStrategy.PreAllocateBySystemRAM,
                    PreAllocationPercentage = 0.25, // 25% of RAM
                    MinimumPreAllocationSize = 1 * 1024 * 1024, // At least 1MB
                    MaximumPreAllocationSize = 100 * 1024 * 1024, // At most 100MB
                    EnableAllocationLogging = true
                };

                using var links = new EnhancedUnitedMemoryLinks<uint>(tempFile, config,
                    message => Console.WriteLine($"[CONSERVATIVE] {message}"));

                Console.WriteLine("=== Conservative Pre-allocation Example ===");
                Console.WriteLine(links.GetAllocationStatistics());

                // Show how bounds are applied
                var calculatedSize = config.CalculatePreAllocationSize();
                Console.WriteLine($"Calculated pre-allocation size: {calculatedSize:N0} bytes");
                Console.WriteLine($"Actual reserved capacity: {links.Memory.ReservedCapacity:N0} bytes");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public static void PerformanceComparisonExample()
        {
            // Example: Compare performance between incremental and pre-allocation
            var incrementalFile = Path.GetTempFileName();
            var preAllocatedFile = Path.GetTempFileName();

            try
            {
                Console.WriteLine("=== Performance Comparison Example ===");

                // Test incremental allocation
                var incrementalConfig = MemoryAllocationConfiguration.DefaultIncremental;
                var incrementalTime = MeasureCreationTime(incrementalFile, incrementalConfig, 5000);
                Console.WriteLine($"Incremental allocation time: {incrementalTime.TotalMilliseconds:F2} ms");

                // Test pre-allocation
                var preAllocationConfig = new MemoryAllocationConfiguration
                {
                    Strategy = MemoryAllocationStrategy.PreAllocateCustom,
                    CustomPreAllocationSize = 10 * 1024 * 1024 // 10 MB
                };
                var preAllocationTime = MeasureCreationTime(preAllocatedFile, preAllocationConfig, 5000);
                Console.WriteLine($"Pre-allocation time: {preAllocationTime.TotalMilliseconds:F2} ms");

                if (preAllocationTime < incrementalTime)
                {
                    var improvement = ((incrementalTime.TotalMilliseconds - preAllocationTime.TotalMilliseconds) 
                                     / incrementalTime.TotalMilliseconds) * 100;
                    Console.WriteLine($"Pre-allocation was {improvement:F1}% faster");
                }
            }
            finally
            {
                File.Delete(incrementalFile);
                File.Delete(preAllocatedFile);
            }
        }

        private static TimeSpan MeasureCreationTime(string filename, MemoryAllocationConfiguration config, int linkCount)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            using var links = new EnhancedUnitedMemoryLinks<uint>(
                new FileMappedResizableDirectMemory(filename), config);

            for (int i = 0; i < linkCount; i++)
            {
                links.Create();
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        [Fact]
        public static void SystemInfoExample()
        {
            // Example: Display system information used for pre-allocation decisions
            Console.WriteLine("=== System Information Example ===");
            
            try
            {
                var totalRAM = SystemMemoryInfo.GetTotalPhysicalMemory();
                var availableRAM = SystemMemoryInfo.GetAvailablePhysicalMemory();
                var totalDisk = SystemMemoryInfo.GetTotalDiskSpace(Environment.CurrentDirectory);
                var availableDisk = SystemMemoryInfo.GetAvailableDiskSpace(Environment.CurrentDirectory);

                Console.WriteLine($"Total RAM: {totalRAM / (1024.0 * 1024 * 1024):F2} GB");
                Console.WriteLine($"Available RAM: {availableRAM / (1024.0 * 1024 * 1024):F2} GB");
                Console.WriteLine($"Total Disk: {totalDisk / (1024.0 * 1024 * 1024):F2} GB");
                Console.WriteLine($"Available Disk: {availableDisk / (1024.0 * 1024 * 1024):F2} GB");

                // Show what 75% allocation would mean
                var ram75Config = MemoryAllocationConfiguration.Default75PercentRAM;
                var ram75Size = ram75Config.CalculatePreAllocationSize();
                Console.WriteLine($"75% RAM pre-allocation would be: {ram75Size / (1024.0 * 1024):F2} MB");

                var disk75Config = MemoryAllocationConfiguration.Default75PercentDisk;
                var disk75Size = disk75Config.CalculatePreAllocationSize();
                Console.WriteLine($"75% Disk pre-allocation would be: {disk75Size / (1024.0 * 1024 * 1024):F2} GB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting system info: {ex.Message}");
            }
        }
    }
}