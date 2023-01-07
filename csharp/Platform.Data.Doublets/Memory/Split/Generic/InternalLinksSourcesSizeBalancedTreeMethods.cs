using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic;

/// <summary>
///     <para>
///         Represents the internal links sources size balanced tree methods.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="InternalLinksSizeBalancedTreeMethodsBase{TLinkAddress}" />
public unsafe class InternalLinksSourcesSizeBalancedTreeMethods<TLinkAddress> : InternalLinksSizeBalancedTreeMethodsBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="InternalLinksSourcesSizeBalancedTreeMethods" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="constants">
    ///     <para>A constants.</para>
    ///     <para></para>
    /// </param>
    /// <param name="linksDataParts">
    ///     <para>A links data parts.</para>
    ///     <para></para>
    /// </param>
    /// <param name="linksIndexParts">
    ///     <para>A links index parts.</para>
    ///     <para></para>
    /// </param>
    /// <param name="header">
    ///     <para>A header.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public InternalLinksSourcesSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, byte* linksDataParts, byte* linksIndexParts, byte* header) : base(constants: constants, linksDataParts: linksDataParts, linksIndexParts: linksIndexParts, header: header) { }

    /// <summary>
    ///     <para>
    ///         Gets the left reference using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The ref link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override ref TLinkAddress GetLeftReference(TLinkAddress node)
    {
        return ref GetLinkIndexPartReference(link: node).LeftAsSource;
    }

    /// <summary>
    ///     <para>
    ///         Gets the right reference using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The ref link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override ref TLinkAddress GetRightReference(TLinkAddress node)
    {
        return ref GetLinkIndexPartReference(link: node).RightAsSource;
    }

    /// <summary>
    ///     <para>
    ///         Gets the left using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetLeft(TLinkAddress node)
    {
        return GetLinkIndexPartReference(link: node).LeftAsSource;
    }

    /// <summary>
    ///     <para>
    ///         Gets the right using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetRight(TLinkAddress node)
    {
        return GetLinkIndexPartReference(link: node).RightAsSource;
    }

    /// <summary>
    ///     <para>
    ///         Sets the left using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    /// <param name="left">
    ///     <para>The left.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetLeft(TLinkAddress node, TLinkAddress left)
    {
        GetLinkIndexPartReference(link: node).LeftAsSource = left;
    }

    /// <summary>
    ///     <para>
    ///         Sets the right using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    /// <param name="right">
    ///     <para>The right.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetRight(TLinkAddress node, TLinkAddress right)
    {
        GetLinkIndexPartReference(link: node).RightAsSource = right;
    }

    /// <summary>
    ///     <para>
    ///         Gets the size using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetSize(TLinkAddress node)
    {
        return GetLinkIndexPartReference(link: node).SizeAsSource;
    }

    /// <summary>
    ///     <para>
    ///         Sets the size using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    /// <param name="size">
    ///     <para>The size.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetSize(TLinkAddress node, TLinkAddress size)
    {
        GetLinkIndexPartReference(link: node).SizeAsSource = size;
    }

    /// <summary>
    ///     <para>
    ///         Gets the tree root using the specified link.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="link">
    ///     <para>The link.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetTreeRoot(TLinkAddress link)
    {
        return GetLinkIndexPartReference(link: link).RootAsSource;
    }

    /// <summary>
    ///     <para>
    ///         Gets the base part value using the specified link.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="link">
    ///     <para>The link.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetBasePartValue(TLinkAddress link)
    {
        return GetLinkDataPartReference(link: link).Source;
    }

    /// <summary>
    ///     <para>
    ///         Gets the key part value using the specified link.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="link">
    ///     <para>The link.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetKeyPartValue(TLinkAddress link)
    {
        return GetLinkDataPartReference(link: link).Target;
    }

    /// <summary>
    ///     <para>
    ///         Clears the node using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void ClearNode(TLinkAddress node)
    {
        ref var link = ref GetLinkIndexPartReference(link: node);
        link.LeftAsSource = TLinkAddress.Zero;
        link.RightAsSource = TLinkAddress.Zero;
        link.SizeAsSource = TLinkAddress.Zero;
    }

    /// <summary>
    ///     <para>
    ///         Searches the source.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="source">
    ///     <para>The source.</para>
    ///     <para></para>
    /// </param>
    /// <param name="target">
    ///     <para>The target.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    public override TLinkAddress Search(TLinkAddress source, TLinkAddress target)
    {
        return SearchCore(root: GetTreeRoot(link: source), key: target);
    }
}
