using System;
using Platform.Data.Doublets.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests.Memory
{
    public static class MemoryAllocationConfigurationTests
    {
        [Fact]
        public static void DefaultConfigurationsTest()
        {
            var ramConfig = MemoryAllocationConfiguration.Default75PercentRAM;
            Assert.Equal(MemoryAllocationStrategy.PreAllocateBySystemRAM, ramConfig.Strategy);
            Assert.Equal(0.75, ramConfig.PreAllocationPercentage);

            var diskConfig = MemoryAllocationConfiguration.Default75PercentDisk;
            Assert.Equal(MemoryAllocationStrategy.PreAllocateByDiskSpace, diskConfig.Strategy);
            Assert.Equal(0.75, diskConfig.PreAllocationPercentage);

            var incrementalConfig = MemoryAllocationConfiguration.DefaultIncremental;
            Assert.Equal(MemoryAllocationStrategy.Incremental, incrementalConfig.Strategy);
        }

        [Fact]
        public static void ValidateValidConfigurationTest()
        {
            var config = new MemoryAllocationConfiguration
            {
                Strategy = MemoryAllocationStrategy.PreAllocateBySystemRAM,
                PreAllocationPercentage = 0.5,
                MinimumPreAllocationSize = 1024,
                MaximumPreAllocationSize = 1024 * 1024 * 1024
            };

            // Should not throw
            config.Validate();
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        public static void ValidateInvalidPercentageTest(double percentage)
        {
            var config = new MemoryAllocationConfiguration
            {
                PreAllocationPercentage = percentage
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        }

        [Fact]
        public static void ValidateNegativeMinimumTest()
        {
            var config = new MemoryAllocationConfiguration
            {
                MinimumPreAllocationSize = -1
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        }

        [Fact]
        public static void ValidateMaximumLessThanMinimumTest()
        {
            var config = new MemoryAllocationConfiguration
            {
                MinimumPreAllocationSize = 1000,
                MaximumPreAllocationSize = 500
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        }

        [Fact]
        public static void ValidateCustomStrategyWithInvalidSizeTest()
        {
            var config = new MemoryAllocationConfiguration
            {
                Strategy = MemoryAllocationStrategy.PreAllocateCustom,
                CustomPreAllocationSize = 0
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        }

        [Fact]
        public static void CalculatePreAllocationSizeIncrementalTest()
        {
            var config = new MemoryAllocationConfiguration
            {
                Strategy = MemoryAllocationStrategy.Incremental,
                MinimumPreAllocationSize = 1000
            };

            var size = config.CalculatePreAllocationSize();
            Assert.Equal(1000, size);
        }

        [Fact]
        public static void CalculatePreAllocationSizeCustomTest()
        {
            var config = new MemoryAllocationConfiguration
            {
                Strategy = MemoryAllocationStrategy.PreAllocateCustom,
                CustomPreAllocationSize = 5000,
                MinimumPreAllocationSize = 1000,
                MaximumPreAllocationSize = 10000
            };

            var size = config.CalculatePreAllocationSize();
            Assert.Equal(5000, size);
        }

        [Fact]
        public static void CalculatePreAllocationSizeWithBoundsTest()
        {
            // Test lower bound
            var configLow = new MemoryAllocationConfiguration
            {
                Strategy = MemoryAllocationStrategy.PreAllocateCustom,
                CustomPreAllocationSize = 500,
                MinimumPreAllocationSize = 1000,
                MaximumPreAllocationSize = 10000
            };

            var sizeLow = configLow.CalculatePreAllocationSize();
            Assert.Equal(1000, sizeLow); // Should be clamped to minimum

            // Test upper bound
            var configHigh = new MemoryAllocationConfiguration
            {
                Strategy = MemoryAllocationStrategy.PreAllocateCustom,
                CustomPreAllocationSize = 15000,
                MinimumPreAllocationSize = 1000,
                MaximumPreAllocationSize = 10000
            };

            var sizeHigh = configHigh.CalculatePreAllocationSize();
            Assert.Equal(10000, sizeHigh); // Should be clamped to maximum
        }

        [Fact]
        public static void CalculatePreAllocationSizeRAMTest()
        {
            var config = new MemoryAllocationConfiguration
            {
                Strategy = MemoryAllocationStrategy.PreAllocateBySystemRAM,
                PreAllocationPercentage = 0.5,
                MinimumPreAllocationSize = 1000,
                MaximumPreAllocationSize = long.MaxValue
            };

            var size = config.CalculatePreAllocationSize();
            
            // Should be positive and at least the minimum
            Assert.True(size >= 1000);
            
            // Should be reasonable (not more than available RAM)
            var availableRAM = SystemMemoryInfo.GetAvailablePhysicalMemory();
            Assert.True(size <= availableRAM);
        }

        [Fact]
        public static void CalculatePreAllocationSizeDiskTest()
        {
            var config = new MemoryAllocationConfiguration
            {
                Strategy = MemoryAllocationStrategy.PreAllocateByDiskSpace,
                PreAllocationPercentage = 0.01, // Use small percentage for testing
                MinimumPreAllocationSize = 1000,
                MaximumPreAllocationSize = long.MaxValue
            };

            var size = config.CalculatePreAllocationSize();
            
            // Should be positive and at least the minimum
            Assert.True(size >= 1000);
            
            // Should be reasonable (not more than available disk space)
            var availableDisk = SystemMemoryInfo.GetAvailableDiskSpace(Environment.CurrentDirectory);
            Assert.True(size <= availableDisk);
        }

        [Fact]
        public static void ConfigurationImmutabilityTest()
        {
            var original = MemoryAllocationConfiguration.Default75PercentRAM;
            var modified = MemoryAllocationConfiguration.Default75PercentRAM;
            
            modified.PreAllocationPercentage = 0.5;
            
            // Original should not be affected
            Assert.Equal(0.75, original.PreAllocationPercentage);
            Assert.Equal(0.5, modified.PreAllocationPercentage);
        }
    }
}