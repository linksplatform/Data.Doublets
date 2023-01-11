using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Memory;
using Platform.Singletons;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic;

/// <summary>
///     <para>
///         Represents the split memory links.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="SplitMemoryLinksBase{TLinkAddress}" />
public unsafe class SplitMemoryLinks<TLinkAddress> : SplitMemoryLinksBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{
    private readonly Func<ILinksTreeMethods<TLinkAddress>> _createExternalSourceTreeMethods;
    private readonly Func<ILinksTreeMethods<TLinkAddress>> _createExternalTargetTreeMethods;
    private readonly Func<ILinksTreeMethods<TLinkAddress>> _createInternalSourceTreeMethods;
    private readonly Func<ILinksTreeMethods<TLinkAddress>> _createInternalTargetTreeMethods;
    private byte* _header;
    private byte* _linksDataParts;
    private byte* _linksIndexParts;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="SplitMemoryLinks" /> instance.
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
    public SplitMemoryLinks(string dataMemory, string indexMemory) : this(dataMemory: new FileMappedResizableDirectMemory(path: dataMemory), indexMemory: new FileMappedResizableDirectMemory(path: indexMemory)) { }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="SplitMemoryLinks" /> instance.
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
    public SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory) : this(dataMemory: dataMemory, indexMemory: indexMemory, memoryReservationStep: DefaultLinksSizeStep) { }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="SplitMemoryLinks" /> instance.
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
    /// <param name="memoryReservationStep">
    ///     <para>A memory reservation step.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, long memoryReservationStep) : this(dataMemory: dataMemory, indexMemory: indexMemory, memoryReservationStep: memoryReservationStep, constants: Default<LinksConstants<TLinkAddress>>.Instance, indexTreeType: IndexTreeType.Default, useLinkedList: true) { }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="SplitMemoryLinks" /> instance.
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
    /// <param name="memoryReservationStep">
    ///     <para>A memory reservation step.</para>
    ///     <para></para>
    /// </param>
    /// <param name="constants">
    ///     <para>A constants.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, long memoryReservationStep, LinksConstants<TLinkAddress> constants) : this(dataMemory: dataMemory, indexMemory: indexMemory, memoryReservationStep: memoryReservationStep, constants: constants, indexTreeType: IndexTreeType.Default, useLinkedList: true) { }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="SplitMemoryLinks" /> instance.
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
    /// <param name="memoryReservationStep">
    ///     <para>A memory reservation step.</para>
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
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, long memoryReservationStep, LinksConstants<TLinkAddress> constants, IndexTreeType indexTreeType, bool useLinkedList) : base(dataMemory: dataMemory, indexMemory: indexMemory, memoryReservationStep: memoryReservationStep, constants: constants, useLinkedList: useLinkedList)
    {
        if (indexTreeType == IndexTreeType.SizeBalancedTree)
        {
            _createInternalSourceTreeMethods = () => new InternalLinksSourcesSizeBalancedTreeMethods<TLinkAddress>(constants: Constants, linksDataParts: _linksDataParts, linksIndexParts: _linksIndexParts, header: _header);
            _createExternalSourceTreeMethods = () => new ExternalLinksSourcesSizeBalancedTreeMethods<TLinkAddress>(constants: Constants, linksDataParts: _linksDataParts, linksIndexParts: _linksIndexParts, header: _header);
            _createInternalTargetTreeMethods = () => new InternalLinksTargetsSizeBalancedTreeMethods<TLinkAddress>(constants: Constants, linksDataParts: _linksDataParts, linksIndexParts: _linksIndexParts, header: _header);
            _createExternalTargetTreeMethods = () => new ExternalLinksTargetsSizeBalancedTreeMethods<TLinkAddress>(constants: Constants, linksDataParts: _linksDataParts, linksIndexParts: _linksIndexParts, header: _header);
        }
        else
        {
            _createInternalSourceTreeMethods = () => new InternalLinksSourcesRecursionlessSizeBalancedTreeMethods<TLinkAddress>(constants: Constants, linksDataParts: _linksDataParts, linksIndexParts: _linksIndexParts, header: _header);
            _createExternalSourceTreeMethods = () => new ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods<TLinkAddress>(constants: Constants, linksDataParts: _linksDataParts, linksIndexParts: _linksIndexParts, header: _header);
            _createInternalTargetTreeMethods = () => new InternalLinksTargetsRecursionlessSizeBalancedTreeMethods<TLinkAddress>(constants: Constants, linksDataParts: _linksDataParts, linksIndexParts: _linksIndexParts, header: _header);
            _createExternalTargetTreeMethods = () => new ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods<TLinkAddress>(constants: Constants, linksDataParts: _linksDataParts, linksIndexParts: _linksIndexParts, header: _header);
        }
        Init(dataMemory: dataMemory, indexMemory: indexMemory);
    }

    /// <summary>
    ///     <para>
    ///         Sets the pointers using the specified data memory.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="dataMemory">
    ///     <para>The data memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="indexMemory">
    ///     <para>The index memory.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetPointers(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
    {
        _linksDataParts = (byte*)dataMemory.Pointer;
        _linksIndexParts = (byte*)indexMemory.Pointer;
        _header = _linksIndexParts;
        if (_useLinkedList)
        {
            InternalSourcesListMethods = new InternalLinksSourcesLinkedListMethods<TLinkAddress>(constants: Constants, linksDataParts: _linksDataParts, linksIndexParts: _linksIndexParts);
        }
        else
        {
            InternalSourcesTreeMethods = _createInternalSourceTreeMethods();
        }
        ExternalSourcesTreeMethods = _createExternalSourceTreeMethods();
        InternalTargetsTreeMethods = _createInternalTargetTreeMethods();
        ExternalTargetsTreeMethods = _createExternalTargetTreeMethods();
        UnusedLinksListMethods = new UnusedLinksListMethods<TLinkAddress>(links: _linksDataParts, header: _header);
    }

    /// <summary>
    ///     <para>
    ///         Resets the pointers.
    ///     </para>
    ///     <para></para>
    /// </summary>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void ResetPointers()
    {
        base.ResetPointers();
        _linksDataParts = null;
        _linksIndexParts = null;
        _header = null;
    }

    /// <summary>
    ///     <para>
    ///         Gets the header reference.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>A ref links header of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override ref LinksHeader<TLinkAddress> GetHeaderReference()
    {
        return ref AsRef<LinksHeader<TLinkAddress>>(source: _header);
    }

    /// <summary>
    ///     <para>
    ///         Gets the link data part reference using the specified link index.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="linkIndex">
    ///     <para>The link index.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>A ref raw link data part of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress linkIndex)
    {
        return ref AsRef<RawLinkDataPart<TLinkAddress>>(source: _linksDataParts + LinkDataPartSizeInBytes * long.CreateTruncating(value: linkIndex));
    }

    /// <summary>
    ///     <para>
    ///         Gets the link index part reference using the specified link index.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="linkIndex">
    ///     <para>The link index.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>A ref raw link index part of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override ref RawLinkIndexPart<TLinkAddress> GetLinkIndexPartReference(TLinkAddress linkIndex)
    {
        return ref AsRef<RawLinkIndexPart<TLinkAddress>>(source: _linksIndexParts + LinkIndexPartSizeInBytes * long.CreateTruncating(value: linkIndex));
    }
}
