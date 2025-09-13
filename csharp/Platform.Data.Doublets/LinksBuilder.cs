using System;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Provides a builder pattern for configuring links implementations with optional indexing.
    /// </para>
    /// <para>
    /// This builder allows users to choose between basic non-indexed implementations
    /// and various indexed implementations, making indexing optional and switchable.
    /// </para>
    /// </summary>
    public class LinksBuilder<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private IResizableDirectMemory? _memory;
        private string? _filePath;
        private long _memoryReservationStep = UnitedMemoryLinksBase<TLinkAddress>.DefaultLinksSizeStep;
        private LinksConstants<TLinkAddress>? _constants;
        private IndexingMode _indexingMode = IndexingMode.Basic;
        private IndexTreeType _treeType = IndexTreeType.SizeBalancedTree;
        private bool _enableSourceIndex = true;
        private bool _enableTargetIndex = true;
        private bool _enableLinkIndex = true;

        /// <summary>
        /// <para>
        /// Specifies the indexing mode for the links implementation.
        /// </para>
        /// <para></para>
        /// </summary>
        public enum IndexingMode
        {
            /// <summary>Basic non-indexed implementation (slower but simpler)</summary>
            Basic = 0,
            /// <summary>Hash-based indexing via decorator (medium performance)</summary>
            HashIndexed = 1,
            /// <summary>Tree-based indexing built into memory layer (fastest)</summary>
            TreeIndexed = 2
        }

        /// <summary>
        /// <para>
        /// Sets the memory implementation to use.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="memory">
        /// <para>The resizable direct memory implementation.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The builder instance for method chaining.</para>
        /// <para></para>
        /// </returns>
        public LinksBuilder<TLinkAddress> UseMemory(IResizableDirectMemory memory)
        {
            _memory = memory;
            _filePath = null;
            return this;
        }

        /// <summary>
        /// <para>
        /// Sets the file path for persistent storage.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="filePath">
        /// <para>The file path for the links database.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The builder instance for method chaining.</para>
        /// <para></para>
        /// </returns>
        public LinksBuilder<TLinkAddress> UseFile(string filePath)
        {
            _filePath = filePath;
            _memory = null;
            return this;
        }

        /// <summary>
        /// <para>
        /// Sets the memory reservation step size.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="stepSize">
        /// <para>The memory reservation step size in bytes.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The builder instance for method chaining.</para>
        /// <para></para>
        /// </returns>
        public LinksBuilder<TLinkAddress> WithMemoryReservationStep(long stepSize)
        {
            _memoryReservationStep = stepSize;
            return this;
        }

        /// <summary>
        /// <para>
        /// Sets the links constants to use.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="constants">
        /// <para>The links constants.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The builder instance for method chaining.</para>
        /// <para></para>
        /// </returns>
        public LinksBuilder<TLinkAddress> WithConstants(LinksConstants<TLinkAddress> constants)
        {
            _constants = constants;
            return this;
        }

        /// <summary>
        /// <para>
        /// Configures the links to use basic non-indexed implementation.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The builder instance for method chaining.</para>
        /// <para></para>
        /// </returns>
        public LinksBuilder<TLinkAddress> UseBasicIndexing()
        {
            _indexingMode = IndexingMode.Basic;
            return this;
        }

        /// <summary>
        /// <para>
        /// Configures the links to use hash-based indexing via decorator.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="enableSourceIndex">
        /// <para>Whether to enable source-based indexing.</para>
        /// <para></para>
        /// </param>
        /// <param name="enableTargetIndex">
        /// <para>Whether to enable target-based indexing.</para>
        /// <para></para>
        /// </param>
        /// <param name="enableLinkIndex">
        /// <para>Whether to enable link-based indexing.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The builder instance for method chaining.</para>
        /// <para></para>
        /// </returns>
        public LinksBuilder<TLinkAddress> UseHashIndexing(bool enableSourceIndex = true, bool enableTargetIndex = true, bool enableLinkIndex = true)
        {
            _indexingMode = IndexingMode.HashIndexed;
            _enableSourceIndex = enableSourceIndex;
            _enableTargetIndex = enableTargetIndex;
            _enableLinkIndex = enableLinkIndex;
            return this;
        }

        /// <summary>
        /// <para>
        /// Configures the links to use tree-based indexing built into memory layer.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="treeType">
        /// <para>The type of tree indexing to use.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The builder instance for method chaining.</para>
        /// <para></para>
        /// </returns>
        public LinksBuilder<TLinkAddress> UseTreeIndexing(IndexTreeType treeType = IndexTreeType.SizeBalancedTree)
        {
            _indexingMode = IndexingMode.TreeIndexed;
            _treeType = treeType;
            return this;
        }

        /// <summary>
        /// <para>
        /// Builds the configured links implementation.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The configured ILinks implementation.</para>
        /// <para></para>
        /// </returns>
        public ILinks<TLinkAddress> Build()
        {
            // Prepare memory
            IResizableDirectMemory memory = _memory ?? new FileMappedResizableDirectMemory(_filePath ?? "links.db", _memoryReservationStep);
            
            // Prepare constants
            var constants = _constants ?? LinksConstants<TLinkAddress>.Default;

            // Create base implementation based on indexing mode
            switch (_indexingMode)
            {
                case IndexingMode.Basic:
                    return new BasicUnitedMemoryLinks<TLinkAddress>(memory, _memoryReservationStep, constants);

                case IndexingMode.HashIndexed:
                    var basicLinks = new BasicUnitedMemoryLinks<TLinkAddress>(memory, _memoryReservationStep, constants);
                    return new IndexedLinksDecorator<TLinkAddress>(basicLinks, _enableSourceIndex, _enableTargetIndex, _enableLinkIndex);

                case IndexingMode.TreeIndexed:
                    return new UnitedMemoryLinks<TLinkAddress>(memory, _memoryReservationStep, constants, _treeType);

                default:
                    throw new ArgumentException($"Unknown indexing mode: {_indexingMode}");
            }
        }

        /// <summary>
        /// <para>
        /// Creates a new links builder with heap memory.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>A new links builder instance.</para>
        /// <para></para>
        /// </returns>
        public static LinksBuilder<TLinkAddress> InMemory()
        {
            return new LinksBuilder<TLinkAddress>().UseMemory(new HeapResizableDirectMemory());
        }

        /// <summary>
        /// <para>
        /// Creates a new links builder with file storage.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="filePath">
        /// <para>The file path for the links database.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A new links builder instance.</para>
        /// <para></para>
        /// </returns>
        public static LinksBuilder<TLinkAddress> WithFile(string filePath)
        {
            return new LinksBuilder<TLinkAddress>().UseFile(filePath);
        }
    }
}