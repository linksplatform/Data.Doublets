using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory;

/// <summary>
///     <para>
///         Defines memory allocation strategy options.
///     </para>
///     <para></para>
/// </summary>
public enum MemoryAllocationStrategy
{
    /// <summary>
    ///     <para>
    ///         Default incremental allocation strategy.
    ///     </para>
    ///     <para></para>
    /// </summary>
    Incremental,

    /// <summary>
    ///     <para>
    ///         Pre-allocate based on system RAM.
    ///     </para>
    ///     <para></para>
    /// </summary>
    PreAllocateBySystemRAM,

    /// <summary>
    ///     <para>
    ///         Pre-allocate based on available disk space.
    ///     </para>
    ///     <para></para>
    /// </summary>
    PreAllocateByDiskSpace,

    /// <summary>
    ///     <para>
    ///         Pre-allocate a custom amount.
    ///     </para>
    ///     <para></para>
    /// </summary>
    PreAllocateCustom
}

/// <summary>
///     <para>
///         Configuration for memory allocation strategies.
///     </para>
///     <para></para>
/// </summary>
public class MemoryAllocationConfiguration
{
    /// <summary>
    ///     <para>
    ///         Gets or sets the allocation strategy.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public MemoryAllocationStrategy Strategy { get; set; } = MemoryAllocationStrategy.Incremental;

    /// <summary>
    ///     <para>
    ///         Gets or sets the percentage of system resource to pre-allocate (0.0 to 1.0).
    ///         Default is 0.75 (75%).
    ///     </para>
    ///     <para></para>
    /// </summary>
    public double PreAllocationPercentage { get; set; } = 0.75;

    /// <summary>
    ///     <para>
    ///         Gets or sets the custom pre-allocation size in bytes.
    ///         Only used when Strategy is PreAllocateCustom.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public long CustomPreAllocationSize { get; set; }

    /// <summary>
    ///     <para>
    ///         Gets or sets the minimum pre-allocation size in bytes.
    ///         Used as a safety lower bound.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public long MinimumPreAllocationSize { get; set; } = 1024 * 1024; // 1 MB

    /// <summary>
    ///     <para>
    ///         Gets or sets the maximum pre-allocation size in bytes.
    ///         Used as a safety upper bound.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public long MaximumPreAllocationSize { get; set; } = 16L * 1024 * 1024 * 1024; // 16 GB

    /// <summary>
    ///     <para>
    ///         Gets or sets whether to enable logging for allocation decisions.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public bool EnableAllocationLogging { get; set; } = false;

    /// <summary>
    ///     <para>
    ///         Gets or sets the path for disk space calculation.
    ///         Only used when Strategy is PreAllocateByDiskSpace.
    ///         If null, uses current directory.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public string? DiskPath { get; set; }

    /// <summary>
    ///     <para>
    ///         Gets a default configuration that enables 75% RAM pre-allocation.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static MemoryAllocationConfiguration Default75PercentRAM => new()
    {
        Strategy = MemoryAllocationStrategy.PreAllocateBySystemRAM,
        PreAllocationPercentage = 0.75
    };

    /// <summary>
    ///     <para>
    ///         Gets a default configuration that enables 75% disk space pre-allocation.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static MemoryAllocationConfiguration Default75PercentDisk => new()
    {
        Strategy = MemoryAllocationStrategy.PreAllocateByDiskSpace,
        PreAllocationPercentage = 0.75
    };

    /// <summary>
    ///     <para>
    ///         Gets a default configuration that uses incremental allocation.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static MemoryAllocationConfiguration DefaultIncremental => new()
    {
        Strategy = MemoryAllocationStrategy.Incremental
    };

    /// <summary>
    ///     <para>
    ///         Validates the configuration and throws if invalid.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <para>Thrown when configuration values are out of valid range</para>
    ///     <para></para>
    /// </exception>
    public void Validate()
    {
        if (PreAllocationPercentage < 0.0 || PreAllocationPercentage > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(PreAllocationPercentage), 
                "PreAllocationPercentage must be between 0.0 and 1.0");
        }

        if (MinimumPreAllocationSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumPreAllocationSize), 
                "MinimumPreAllocationSize must be non-negative");
        }

        if (MaximumPreAllocationSize < MinimumPreAllocationSize)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumPreAllocationSize), 
                "MaximumPreAllocationSize must be greater than or equal to MinimumPreAllocationSize");
        }

        if (Strategy == MemoryAllocationStrategy.PreAllocateCustom && CustomPreAllocationSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(CustomPreAllocationSize), 
                "CustomPreAllocationSize must be positive when using PreAllocateCustom strategy");
        }
    }

    /// <summary>
    ///     <para>
    ///         Calculates the optimal pre-allocation size based on the current configuration.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="dataPath">
    ///     <para>Path for disk space calculations (optional)</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The calculated pre-allocation size in bytes</para>
    ///     <para></para>
    /// </returns>
    public long CalculatePreAllocationSize(string? dataPath = null)
    {
        Validate();

        long calculatedSize = Strategy switch
        {
            MemoryAllocationStrategy.PreAllocateBySystemRAM => 
                (long)(SystemMemoryInfo.GetAvailablePhysicalMemory() * PreAllocationPercentage),
            
            MemoryAllocationStrategy.PreAllocateByDiskSpace => 
                (long)(SystemMemoryInfo.GetAvailableDiskSpace(DiskPath ?? dataPath ?? Environment.CurrentDirectory) * PreAllocationPercentage),
            
            MemoryAllocationStrategy.PreAllocateCustom => 
                CustomPreAllocationSize,
            
            _ => MinimumPreAllocationSize
        };

        // Apply bounds
        return Math.Max(MinimumPreAllocationSize, Math.Min(MaximumPreAllocationSize, calculatedSize));
    }
}