using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Memory;
using Platform.Singletons;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.United.Generic;

/// <summary>
///     <para>
///         Represents enhanced united memory links with configurable pre-allocation strategies.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="UnitedMemoryLinks{TLinkAddress}" />
public unsafe class EnhancedUnitedMemoryLinks<TLinkAddress> : UnitedMemoryLinks<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{
    private readonly MemoryAllocationConfiguration _allocationConfiguration;
    private readonly Action<string>? _logger;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="EnhancedUnitedMemoryLinks" /> instance with 75% RAM pre-allocation.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="address">
    ///     <para>File address.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EnhancedUnitedMemoryLinks(string address) 
        : this(address: address, allocationConfiguration: MemoryAllocationConfiguration.Default75PercentRAM) 
    { }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="EnhancedUnitedMemoryLinks" /> instance with custom allocation configuration.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="address">
    ///     <para>File address.</para>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EnhancedUnitedMemoryLinks(string address, MemoryAllocationConfiguration allocationConfiguration, Action<string>? logger = null) 
        : this(memory: new FileMappedResizableDirectMemory(path: address), 
               allocationConfiguration: allocationConfiguration, logger: logger) 
    { }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="EnhancedUnitedMemoryLinks" /> instance with memory and allocation configuration.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="memory">
    ///     <para>A memory.</para>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EnhancedUnitedMemoryLinks(IResizableDirectMemory memory, MemoryAllocationConfiguration allocationConfiguration, Action<string>? logger = null) 
        : this(memory: memory, allocationConfiguration: allocationConfiguration, 
               constants: Default<LinksConstants<TLinkAddress>>.Instance, 
               indexTreeType: IndexTreeType.Default, logger: logger) 
    { }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="EnhancedUnitedMemoryLinks" /> instance with full configuration options.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="memory">
    ///     <para>A memory.</para>
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
    /// <param name="logger">
    ///     <para>Optional logger for allocation decisions.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EnhancedUnitedMemoryLinks(IResizableDirectMemory memory, MemoryAllocationConfiguration allocationConfiguration, 
                                   LinksConstants<TLinkAddress> constants, IndexTreeType indexTreeType, Action<string>? logger = null) 
        : base(memory: memory, 
               memoryReservationStep: CalculateOptimalReservationStep(allocationConfiguration, memory), 
               constants: constants, indexTreeType: indexTreeType)
    {
        _allocationConfiguration = allocationConfiguration ?? throw new ArgumentNullException(nameof(allocationConfiguration));
        _logger = logger;
        
        // Apply pre-allocation strategy
        ApplyPreAllocationStrategy(memory);
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
    ///         Gets the memory instance for statistics and monitoring.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public IResizableDirectMemory Memory => _memory;

    private static long CalculateOptimalReservationStep(MemoryAllocationConfiguration config, IResizableDirectMemory memory)
    {
        if (config.Strategy == MemoryAllocationStrategy.Incremental)
        {
            return DefaultLinksSizeStep;
        }

        // For pre-allocation strategies, calculate a step based on the pre-allocated size
        var dataPath = memory is FileMappedResizableDirectMemory ? 
            Environment.CurrentDirectory : null;
            
        var preAllocationSize = config.CalculatePreAllocationSize(dataPath);
        var linksCount = preAllocationSize / LinkSizeInBytes;
        
        // Use a step that's about 10% of the pre-allocated size, but at least the default
        return Math.Max(DefaultLinksSizeStep, (linksCount * LinkSizeInBytes) / 10);
    }

    private void ApplyPreAllocationStrategy(IResizableDirectMemory memory)
    {
        if (_allocationConfiguration.Strategy == MemoryAllocationStrategy.Incremental)
        {
            LogAllocation("Using incremental allocation strategy");
            return;
        }

        try
        {
            var dataPath = memory is FileMappedResizableDirectMemory ? 
                Environment.CurrentDirectory : null;

            var preAllocationSize = _allocationConfiguration.CalculatePreAllocationSize(dataPath);
            
            // Calculate target capacity ensuring we have space for the header
            var targetCapacity = Math.Max(preAllocationSize, LinkHeaderSizeInBytes + LinkSizeInBytes);

            LogAllocation($"Pre-allocating memory: {preAllocationSize:N0} bytes requested (strategy: {_allocationConfiguration.Strategy})");
            
            // Only pre-allocate if it's more than current capacity
            if (targetCapacity > memory.ReservedCapacity)
            {
                LogAllocation($"Pre-allocating memory: {targetCapacity:N0} bytes");
                memory.ReservedCapacity = targetCapacity;
            }

            var maxLinks = (targetCapacity - LinkHeaderSizeInBytes) / LinkSizeInBytes;
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
            _logger?.Invoke($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] EnhancedUnitedMemoryLinks: {message}");
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
        var memory = _memory;
        var utilizationPercent = memory.ReservedCapacity > 0 ? (memory.UsedCapacity * 100.0 / memory.ReservedCapacity) : 0;
        var maxLinks = memory.ReservedCapacity > LinkHeaderSizeInBytes ? 
            (memory.ReservedCapacity - LinkHeaderSizeInBytes) / LinkSizeInBytes : 0;

        return $"Memory Statistics:\n" +
               $"  Strategy: {_allocationConfiguration.Strategy}\n" +
               $"  Memory: {memory.UsedCapacity:N0} / {memory.ReservedCapacity:N0} bytes ({utilizationPercent:F1}% utilized)\n" +
               $"  Links: {Total} / {maxLinks:N0} (max capacity)\n" +
               $"  Link Size: {LinkSizeInBytes} bytes";
    }
}