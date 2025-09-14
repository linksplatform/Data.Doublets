using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Memory;
using Platform.Singletons;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic;

/// <summary>
///     <para>
///         Represents enhanced split memory links with configurable pre-allocation strategies.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="SplitMemoryLinks{TLinkAddress}" />
public unsafe class EnhancedSplitMemoryLinks<TLinkAddress> : SplitMemoryLinks<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{
    private readonly MemoryAllocationConfiguration _allocationConfiguration;
    private readonly Action<string>? _logger;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="EnhancedSplitMemoryLinks" /> instance with 75% RAM pre-allocation.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="dataMemory">
    ///     <para>A data memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="indexMemory">
    ///     <para>A index memory.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public EnhancedSplitMemoryLinks(string dataMemory, string indexMemory) 
        : this(dataMemory: new FileMappedResizableDirectMemory(path: dataMemory), 
               indexMemory: new FileMappedResizableDirectMemory(path: indexMemory),
               allocationConfiguration: MemoryAllocationConfiguration.Default75PercentRAM) 
    { }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="EnhancedSplitMemoryLinks" /> instance with custom allocation configuration.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="dataMemory">
    ///     <para>A data memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="indexMemory">
    ///     <para>A index memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="allocationConfiguration">
    ///     <para>Memory allocation configuration.</para>
    ///     <para></para>
    /// </param>
    /// <param name="logger">
    ///     <para>Optional logger for allocation decisions.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public EnhancedSplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, 
                                  MemoryAllocationConfiguration allocationConfiguration, Action<string>? logger = null) 
        : this(dataMemory: dataMemory, indexMemory: indexMemory, allocationConfiguration: allocationConfiguration,
               constants: Default<LinksConstants<TLinkAddress>>.Instance, indexTreeType: IndexTreeType.Default, 
               useLinkedList: true, logger: logger) 
    { }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="EnhancedSplitMemoryLinks" /> instance with full configuration options.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="dataMemory">
    ///     <para>A data memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="indexMemory">
    ///     <para>A index memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="allocationConfiguration">
    ///     <para>Memory allocation configuration.</para>
    ///     <para></para>
    /// </param>
    /// <param name="constants">
    ///     <para>A constants.</para>
    ///     <para></para>
    /// </param>
    /// <param name="indexTreeType">
    ///     <para>A index tree type.</para>
    ///     <para></para>
    /// </param>
    /// <param name="useLinkedList">
    ///     <para>A use linked list.</para>
    ///     <para></para>
    /// </param>
    /// <param name="logger">
    ///     <para>Optional logger for allocation decisions.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public EnhancedSplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, 
                                  MemoryAllocationConfiguration allocationConfiguration, LinksConstants<TLinkAddress> constants, 
                                  IndexTreeType indexTreeType, bool useLinkedList, Action<string>? logger = null) 
        : base(dataMemory: dataMemory, indexMemory: indexMemory, 
               memoryReservationStep: CalculateOptimalReservationStep(allocationConfiguration, dataMemory, indexMemory), 
               constants: constants, indexTreeType: indexTreeType, useLinkedList: useLinkedList)
    {
        _allocationConfiguration = allocationConfiguration ?? throw new ArgumentNullException(nameof(allocationConfiguration));
        _logger = logger;
        
        // Apply pre-allocation strategy
        ApplyPreAllocationStrategy(dataMemory, indexMemory);
    }

    /// <summary>
    ///     <para>
    ///         Gets the current memory allocation configuration.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public MemoryAllocationConfiguration AllocationConfiguration => _allocationConfiguration;

    /// <summary>
    ///     <para>
    ///         Gets the data memory instance for statistics and monitoring.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public IResizableDirectMemory DataMemory => _dataMemory;

    /// <summary>
    ///     <para>
    ///         Gets the index memory instance for statistics and monitoring.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public IResizableDirectMemory IndexMemory => _indexMemory;

    private static long CalculateOptimalReservationStep(MemoryAllocationConfiguration config, IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
    {
        if (config.Strategy == MemoryAllocationStrategy.Incremental)
        {
            return DefaultLinksSizeStep;
        }

        // For pre-allocation strategies, calculate a step based on the pre-allocated size
        var dataPath = dataMemory is FileMappedResizableDirectMemory ? 
            Environment.CurrentDirectory : null;
            
        var preAllocationSize = config.CalculatePreAllocationSize(dataPath);
        var linksCount = preAllocationSize / (LinkDataPartSizeInBytes + LinkIndexPartSizeInBytes);
        
        // Use a step that's about 10% of the pre-allocated size, but at least 1MB
        return Math.Max(DefaultLinksSizeStep, linksCount / 10);
    }

    private void ApplyPreAllocationStrategy(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
    {
        if (_allocationConfiguration.Strategy == MemoryAllocationStrategy.Incremental)
        {
            LogAllocation("Using incremental allocation strategy");
            return;
        }

        try
        {
            var dataPath = dataMemory is FileMappedResizableDirectMemory ? 
                Environment.CurrentDirectory : null;

            var preAllocationSize = _allocationConfiguration.CalculatePreAllocationSize(dataPath);
            
            // Calculate how much capacity to allocate for data and index
            // Split the allocation between data and index based on their size ratios
            var totalSizePerLink = LinkDataPartSizeInBytes + LinkIndexPartSizeInBytes;
            var maxLinks = preAllocationSize / totalSizePerLink;
            
            var targetDataCapacity = maxLinks * LinkDataPartSizeInBytes;
            var targetIndexCapacity = maxLinks * LinkIndexPartSizeInBytes;

            LogAllocation($"Pre-allocating memory: {preAllocationSize:N0} bytes total (strategy: {_allocationConfiguration.Strategy})");
            
            // Only pre-allocate if it's more than current capacity
            if (targetDataCapacity > dataMemory.ReservedCapacity)
            {
                LogAllocation($"Pre-allocating data memory: {targetDataCapacity:N0} bytes");
                dataMemory.ReservedCapacity = targetDataCapacity;
            }

            if (targetIndexCapacity > indexMemory.ReservedCapacity)
            {
                LogAllocation($"Pre-allocating index memory: {targetIndexCapacity:N0} bytes");
                indexMemory.ReservedCapacity = targetIndexCapacity;
            }

            LogAllocation($"Pre-allocation completed. Max links capacity: {maxLinks:N0}");
        }
        catch (Exception ex)
        {
            LogAllocation($"Pre-allocation failed, falling back to incremental: {ex.Message}");
            // Don't throw - fall back to incremental allocation
        }
    }

    private void LogAllocation(string message)
    {
        if (_allocationConfiguration.EnableAllocationLogging)
        {
            _logger?.Invoke($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] EnhancedSplitMemoryLinks: {message}");
        }
    }

    /// <summary>
    ///     <para>
    ///         Gets memory allocation statistics.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>Memory allocation statistics as a formatted string</para>
    ///     <para></para>
    /// </returns>
    public string GetAllocationStatistics()
    {
        var dataMemory = _dataMemory;
        var indexMemory = _indexMemory;
        var totalReserved = dataMemory.ReservedCapacity + indexMemory.ReservedCapacity;
        var totalUsed = dataMemory.UsedCapacity + indexMemory.UsedCapacity;
        var utilizationPercent = totalReserved > 0 ? (totalUsed * 100.0 / totalReserved) : 0;

        return $"Memory Statistics:\n" +
               $"  Strategy: {_allocationConfiguration.Strategy}\n" +
               $"  Data Memory: {dataMemory.UsedCapacity:N0} / {dataMemory.ReservedCapacity:N0} bytes\n" +
               $"  Index Memory: {indexMemory.UsedCapacity:N0} / {indexMemory.ReservedCapacity:N0} bytes\n" +
               $"  Total: {totalUsed:N0} / {totalReserved:N0} bytes ({utilizationPercent:F1}% utilized)\n" +
               $"  Links: {Total} allocated";
    }
}