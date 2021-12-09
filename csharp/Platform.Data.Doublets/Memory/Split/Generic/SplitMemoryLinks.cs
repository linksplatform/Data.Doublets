using System;
using System.Runtime.CompilerServices;
using Platform.Singletons;
using Platform.Memory;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic
{
    /// <summary>
    /// <para>
    /// Represents the split memory links.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <seealso cref="SplitMemoryLinksBase{TLink}"/>
    public unsafe class SplitMemoryLinks<TLink> : SplitMemoryLinksBase<TLink>
    {
        private readonly Func<ILinksTreeMethods<TLink>> _createInternalSourceTreeMethods;
        private readonly Func<ILinksTreeMethods<TLink>> _createExternalSourceTreeMethods;
        private readonly Func<ILinksTreeMethods<TLink>> _createInternalTargetTreeMethods;
        private readonly Func<ILinksTreeMethods<TLink>> _createExternalTargetTreeMethods;
        private byte* _header;
        private byte* _linksDataParts;
        private byte* _linksIndexParts;
        
        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SplitMemoryLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="dataMemory">
        /// <para>A data memory.</para>
        /// <para></para>
        /// </param>
        /// <param name="indexMemory">
        /// <para>A index memory.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitMemoryLinks(string dataMemory, string indexMemory) : this(new FileMappedResizableDirectMemory(dataMemory), new FileMappedResizableDirectMemory(indexMemory)) { }
        
        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SplitMemoryLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="dataMemory">
        /// <para>A data memory.</para>
        /// <para></para>
        /// </param>
        /// <param name="indexMemory">
        /// <para>A index memory.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory) : this(dataMemory, indexMemory, DefaultLinksSizeStep) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SplitMemoryLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="dataMemory">
        /// <para>A data memory.</para>
        /// <para></para>
        /// </param>
        /// <param name="indexMemory">
        /// <para>A index memory.</para>
        /// <para></para>
        /// </param>
        /// <param name="memoryReservationStep">
        /// <para>A memory reservation step.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, long memoryReservationStep) : this(dataMemory, indexMemory, memoryReservationStep, Default<LinksConstants<TLink>>.Instance, IndexTreeType.Default, useLinkedList: true) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SplitMemoryLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="dataMemory">
        /// <para>A data memory.</para>
        /// <para></para>
        /// </param>
        /// <param name="indexMemory">
        /// <para>A index memory.</para>
        /// <para></para>
        /// </param>
        /// <param name="memoryReservationStep">
        /// <para>A memory reservation step.</para>
        /// <para></para>
        /// </param>
        /// <param name="constants">
        /// <para>A constants.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, long memoryReservationStep, LinksConstants<TLink> constants) : this(dataMemory, indexMemory, memoryReservationStep, constants, IndexTreeType.Default, useLinkedList: true) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SplitMemoryLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="dataMemory">
        /// <para>A data memory.</para>
        /// <para></para>
        /// </param>
        /// <param name="indexMemory">
        /// <para>A index memory.</para>
        /// <para></para>
        /// </param>
        /// <param name="memoryReservationStep">
        /// <para>A memory reservation step.</para>
        /// <para></para>
        /// </param>
        /// <param name="constants">
        /// <para>A constants.</para>
        /// <para></para>
        /// </param>
        /// <param name="indexTreeType">
        /// <para>A index tree type.</para>
        /// <para></para>
        /// </param>
        /// <param name="useLinkedList">
        /// <para>A use linked list.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, long memoryReservationStep, LinksConstants<TLink> constants, IndexTreeType indexTreeType, bool useLinkedList) : base(dataMemory, indexMemory, memoryReservationStep, constants, useLinkedList)
        {
            if (indexTreeType == IndexTreeType.SizeBalancedTree)
            {
                _createInternalSourceTreeMethods = () => new InternalLinksSourcesSizeBalancedTreeMethods<TLink>(Constants, _linksDataParts, _linksIndexParts, _header);
                _createExternalSourceTreeMethods = () => new ExternalLinksSourcesSizeBalancedTreeMethods<TLink>(Constants, _linksDataParts, _linksIndexParts, _header);
                _createInternalTargetTreeMethods = () => new InternalLinksTargetsSizeBalancedTreeMethods<TLink>(Constants, _linksDataParts, _linksIndexParts, _header);
                _createExternalTargetTreeMethods = () => new ExternalLinksTargetsSizeBalancedTreeMethods<TLink>(Constants, _linksDataParts, _linksIndexParts, _header);
            }
            else
            {
                _createInternalSourceTreeMethods = () => new InternalLinksSourcesRecursionlessSizeBalancedTreeMethods<TLink>(Constants, _linksDataParts, _linksIndexParts, _header);
                _createExternalSourceTreeMethods = () => new ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods<TLink>(Constants, _linksDataParts, _linksIndexParts, _header);
                _createInternalTargetTreeMethods = () => new InternalLinksTargetsRecursionlessSizeBalancedTreeMethods<TLink>(Constants, _linksDataParts, _linksIndexParts, _header);
                _createExternalTargetTreeMethods = () => new ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods<TLink>(Constants, _linksDataParts, _linksIndexParts, _header);
            }
            Init(dataMemory, indexMemory);
        }

        /// <summary>
        /// <para>
        /// Sets the pointers using the specified data memory.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="dataMemory">
        /// <para>The data memory.</para>
        /// <para></para>
        /// </param>
        /// <param name="indexMemory">
        /// <para>The index memory.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetPointers(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
        {
            _linksDataParts = (byte*)dataMemory.Pointer;
            _linksIndexParts = (byte*)indexMemory.Pointer;
            _header = _linksIndexParts;
            if (_useLinkedList)
            {
                InternalSourcesListMethods = new InternalLinksSourcesLinkedListMethods<TLink>(Constants, _linksDataParts, _linksIndexParts);
            }
            else
            {
                InternalSourcesTreeMethods = _createInternalSourceTreeMethods();
            }
            ExternalSourcesTreeMethods = _createExternalSourceTreeMethods();
            InternalTargetsTreeMethods = _createInternalTargetTreeMethods();
            ExternalTargetsTreeMethods = _createExternalTargetTreeMethods();
            UnusedLinksListMethods = new UnusedLinksListMethods<TLink>(_linksDataParts, _header);
        }

        /// <summary>
        /// <para>
        /// Resets the pointers.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ResetPointers()
        {
            base.ResetPointers();
            _linksDataParts = null;
            _linksIndexParts = null;
            _header = null;
        }

        /// <summary>
        /// <para>
        /// Gets the header reference.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>A ref links header of t link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ref LinksHeader<TLink> GetHeaderReference() => ref AsRef<LinksHeader<TLink>>(_header);

        /// <summary>
        /// <para>
        /// Gets the link data part reference using the specified link index.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkIndex">
        /// <para>The link index.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A ref raw link data part of t link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ref RawLinkDataPart<TLink> GetLinkDataPartReference(TLink linkIndex) => ref AsRef<RawLinkDataPart<TLink>>(_linksDataParts + (LinkDataPartSizeInBytes * ConvertToInt64(linkIndex)));

        /// <summary>
        /// <para>
        /// Gets the link index part reference using the specified link index.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkIndex">
        /// <para>The link index.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A ref raw link index part of t link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ref RawLinkIndexPart<TLink> GetLinkIndexPartReference(TLink linkIndex) => ref AsRef<RawLinkIndexPart<TLink>>(_linksIndexParts + (LinkIndexPartSizeInBytes * ConvertToInt64(linkIndex)));
    }
}
