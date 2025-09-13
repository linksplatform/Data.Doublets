using System;
using System.Numerics;
using Platform.Memory;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    ///     <para>
    ///         Factory for creating links instances based on configuration options.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static class LinksFactory
    {
        /// <summary>
        ///     <para>
        ///         Creates a links instance using the specified file path and options.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        ///     <para>The link address type.</para>
        ///     <para></para>
        /// </typeparam>
        /// <param name="filePath">
        ///     <para>The file path for storage.</para>
        ///     <para></para>
        /// </param>
        /// <param name="options">
        ///     <para>The configuration options.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>A links instance configured according to the options.</para>
        ///     <para></para>
        /// </returns>
        public static ILinks<TLinkAddress> Create<TLinkAddress>(string filePath, LinksOptions<TLinkAddress> options)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            if (options.SplitDataFromIndexes)
            {
                var dataFilePath = filePath + ".data";
                var indexFilePath = filePath + ".index";
                
                return new SplitMemoryLinks<TLinkAddress>(
                    dataMemory: new FileMappedResizableDirectMemory(dataFilePath, options.MemoryReservationStep),
                    indexMemory: new FileMappedResizableDirectMemory(indexFilePath, options.MemoryReservationStep),
                    memoryReservationStep: options.MemoryReservationStep,
                    constants: options.Constants,
                    indexTreeType: options.IndexTreeType,
                    useLinkedList: options.UseLinkedList);
            }
            else
            {
                return new UnitedMemoryLinks<TLinkAddress>(
                    memory: new FileMappedResizableDirectMemory(filePath, options.MemoryReservationStep),
                    memoryReservationStep: options.MemoryReservationStep,
                    constants: options.Constants,
                    indexTreeType: options.IndexTreeType);
            }
        }

        /// <summary>
        ///     <para>
        ///         Creates a links instance using the specified file path with default options.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        ///     <para>The link address type.</para>
        ///     <para></para>
        /// </typeparam>
        /// <param name="filePath">
        ///     <para>The file path for storage.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>A links instance with default configuration.</para>
        ///     <para></para>
        /// </returns>
        public static ILinks<TLinkAddress> Create<TLinkAddress>(string filePath)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            return Create(filePath, new LinksOptions<TLinkAddress>());
        }

        /// <summary>
        ///     <para>
        ///         Creates a links instance using the specified memory and options.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        ///     <para>The link address type.</para>
        ///     <para></para>
        /// </typeparam>
        /// <param name="memory">
        ///     <para>The memory instance for storage.</para>
        ///     <para></para>
        /// </param>
        /// <param name="options">
        ///     <para>The configuration options.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>A links instance configured according to the options.</para>
        ///     <para></para>
        /// </returns>
        public static ILinks<TLinkAddress> Create<TLinkAddress>(IResizableDirectMemory memory, LinksOptions<TLinkAddress> options)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            if (options.SplitDataFromIndexes)
            {
                throw new ArgumentException("SplitDataFromIndexes option requires separate data and index memory instances. Use the overload with dataMemory and indexMemory parameters.");
            }

            return new UnitedMemoryLinks<TLinkAddress>(
                memory: memory,
                memoryReservationStep: options.MemoryReservationStep,
                constants: options.Constants,
                indexTreeType: options.IndexTreeType);
        }

        /// <summary>
        ///     <para>
        ///         Creates a split memory links instance using separate data and index memory and options.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        ///     <para>The link address type.</para>
        ///     <para></para>
        /// </typeparam>
        /// <param name="dataMemory">
        ///     <para>The data memory instance.</para>
        ///     <para></para>
        /// </param>
        /// <param name="indexMemory">
        ///     <para>The index memory instance.</para>
        ///     <para></para>
        /// </param>
        /// <param name="options">
        ///     <para>The configuration options.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>A split memory links instance configured according to the options.</para>
        ///     <para></para>
        /// </returns>
        public static ILinks<TLinkAddress> Create<TLinkAddress>(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, LinksOptions<TLinkAddress> options)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            return new SplitMemoryLinks<TLinkAddress>(
                dataMemory: dataMemory,
                indexMemory: indexMemory,
                memoryReservationStep: options.MemoryReservationStep,
                constants: options.Constants,
                indexTreeType: options.IndexTreeType,
                useLinkedList: options.UseLinkedList);
        }
    }
}