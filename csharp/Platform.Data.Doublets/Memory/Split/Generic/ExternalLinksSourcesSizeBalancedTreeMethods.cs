using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic;

/// <summary>
///     <para>
///         Represents the external links sources size balanced tree methods.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="ExternalLinksSizeBalancedTreeMethodsBase{TLinkAddress}" />
public unsafe class ExternalLinksSourcesSizeBalancedTreeMethods<TLinkAddress> : ExternalLinksSizeBalancedTreeMethodsBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="ExternalLinksSourcesSizeBalancedTreeMethods" /> instance.
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
    public ExternalLinksSourcesSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, byte* linksDataParts, byte* linksIndexParts, byte* header) : base(constants: constants, linksDataParts: linksDataParts, linksIndexParts: linksIndexParts, header: header) { }

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
    ///         Gets the tree root.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetTreeRoot()
    {
        return GetHeaderReference().RootAsSource;
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
    ///         Determines whether this instance first is to the left of second.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="firstSource">
    ///     <para>The first source.</para>
    ///     <para></para>
    /// </param>
    /// <param name="firstTarget">
    ///     <para>The first target.</para>
    ///     <para></para>
    /// </param>
    /// <param name="secondSource">
    ///     <para>The second source.</para>
    ///     <para></para>
    /// </param>
    /// <param name="secondTarget">
    ///     <para>The second target.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget)
    {
        return LessThan(first: firstSource, second: secondSource) || ((firstSource == second: secondSource) && LessThan(first: firstTarget, second: secondTarget));
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance first is to the right of second.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="firstSource">
    ///     <para>The first source.</para>
    ///     <para></para>
    /// </param>
    /// <param name="firstTarget">
    ///     <para>The first target.</para>
    ///     <para></para>
    /// </param>
    /// <param name="secondSource">
    ///     <para>The second source.</para>
    ///     <para></para>
    /// </param>
    /// <param name="secondTarget">
    ///     <para>The second target.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget)
    {
        return GreaterThan(first: firstSource, second: secondSource) || ((firstSource == second: secondSource) && GreaterThan(first: firstTarget, second: secondTarget));
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
        link.LeftAsSource = Zero;
        link.RightAsSource = Zero;
        link.SizeAsSource = Zero;
    }
}
