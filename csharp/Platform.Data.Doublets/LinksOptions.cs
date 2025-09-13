using System;
using System.Numerics;
using Platform.Data.Doublets.Memory;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    ///     <para>
    ///         Represents configuration options for creating links instances.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    ///     <para>The link address type.</para>
    ///     <para></para>
    /// </typeparam>
    public class LinksOptions<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        /// <summary>
        ///     <para>
        ///         Gets or sets a value indicating whether to split data from indexes.
        ///         When true, uses SplitMemoryLinks with separate data and index memory.
        ///         When false, uses UnitedMemoryLinks with unified memory.
        ///     </para>
        ///     <para></para>
        /// </summary>
        public bool SplitDataFromIndexes { get; set; }

        /// <summary>
        ///     <para>
        ///         Gets or sets the memory reservation step in bytes.
        ///     </para>
        ///     <para></para>
        /// </summary>
        public long MemoryReservationStep { get; set; }

        /// <summary>
        ///     <para>
        ///         Gets or sets the links constants to use.
        ///     </para>
        ///     <para></para>
        /// </summary>
        public LinksConstants<TLinkAddress> Constants { get; set; }

        /// <summary>
        ///     <para>
        ///         Gets or sets the index tree type.
        ///     </para>
        ///     <para></para>
        /// </summary>
        public IndexTreeType IndexTreeType { get; set; }

        /// <summary>
        ///     <para>
        ///         Gets or sets a value indicating whether to use linked list for internal sources.
        ///         Only applies when SplitDataFromIndexes is true.
        ///     </para>
        ///     <para></para>
        /// </summary>
        public bool UseLinkedList { get; set; }

        /// <summary>
        ///     <para>
        ///         Initializes a new <see cref="LinksOptions{TLinkAddress}"/> instance with default values.
        ///     </para>
        ///     <para></para>
        /// </summary>
        public LinksOptions()
        {
            SplitDataFromIndexes = false;
            MemoryReservationStep = 1048576; // 1MB default step
            Constants = Platform.Singletons.Default<LinksConstants<TLinkAddress>>.Instance;
            IndexTreeType = IndexTreeType.Default;
            UseLinkedList = true;
        }

        /// <summary>
        ///     <para>
        ///         Creates a copy of this LinksOptions instance.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <returns>
        ///     <para>A new LinksOptions instance with the same settings.</para>
        ///     <para></para>
        /// </returns>
        public LinksOptions<TLinkAddress> Clone()
        {
            return new LinksOptions<TLinkAddress>
            {
                SplitDataFromIndexes = SplitDataFromIndexes,
                MemoryReservationStep = MemoryReservationStep,
                Constants = Constants,
                IndexTreeType = IndexTreeType,
                UseLinkedList = UseLinkedList
            };
        }
    }
}