using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Collections.Methods.Trees;
using Platform.Converters;
using Platform.Delegates;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic;

/// <summary>
///     <para>
///         Represents the external links size balanced tree methods base.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="SizeBalancedTreeMethods{TLinkAddress}" />
/// <seealso cref="ILinksTreeMethods{TLinkAddress}" />
public abstract unsafe class ExternalLinksSizeBalancedTreeMethodsBase<TLinkAddress> : SizeBalancedTreeMethods<TLinkAddress>, ILinksTreeMethods<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{

    /// <summary>
    ///     <para>
    ///         The break.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly TLinkAddress Break;
    /// <summary>
    ///     <para>
    ///         The continue.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly TLinkAddress Continue;
    /// <summary>
    ///     <para>
    ///         The header.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly byte* Header;
    /// <summary>
    ///     <para>
    ///         The links data parts.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly byte* LinksDataParts;
    /// <summary>
    ///     <para>
    ///         The links index parts.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly byte* LinksIndexParts;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="ExternalLinksSizeBalancedTreeMethodsBase" /> instance.
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
    protected ExternalLinksSizeBalancedTreeMethodsBase(LinksConstants<TLinkAddress> constants, byte* linksDataParts, byte* linksIndexParts, byte* header)
    {
        LinksDataParts = linksDataParts;
        LinksIndexParts = linksIndexParts;
        Header = header;
        Break = constants.Break;
        Continue = constants.Continue;
    }

    /// <summary>
    ///     <para>
    ///         The zero.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress this[TLinkAddress index]
    {
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        get
        {
            var root = GetTreeRoot();
            if ((index >= GetSize(node: root)))
            {
                return TLinkAddress.Zero;
            }
            while (root != TLinkAddress.Zero)
            {
                var left = GetLeftOrDefault(node: root);
                var leftSize = GetSizeOrZero(node: left);
                if (index < leftSize)
                {
                    root = left;
                    continue;
                }
                if ((index == leftSize))
                {
                    return root;
                }
                root = GetRightOrDefault(node: root);
                index = (index) - (leftSize + TLinkAddress.One);
            }
            return TLinkAddress.Zero; // TODO: Impossible situation exception (only if tree structure broken)
        }
    }

    /// <summary>
    ///     Выполняет поиск и возвращает индекс связи с указанными Source (началом) и Target (концом).
    /// </summary>
    /// <param name="source">Индекс связи, которая является началом на искомой связи.</param>
    /// <param name="target">Индекс связи, которая является концом на искомой связи.</param>
    /// <returns>Индекс искомой связи.</returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public TLinkAddress Search(TLinkAddress source, TLinkAddress target)
    {
        var root = GetTreeRoot();
        while (root != TLinkAddress.Zero)
        {
            ref var rootLink = ref GetLinkDataPartReference(link: root);
            var rootSource = rootLink.Source;
            var rootTarget = rootLink.Target;
            if (FirstIsToTheLeftOfSecond(source: source, target: target, rootSource: rootSource, rootTarget: rootTarget)) // node.Key < root.Key
            {
                root = GetLeftOrDefault(node: root);
            }
            else if (FirstIsToTheRightOfSecond(source: source, target: target, rootSource: rootSource, rootTarget: rootTarget)) // node.Key > root.Key
            {
                root = GetRightOrDefault(node: root);
            }
            else // node.Key == root.Key
            {
                return root;
            }
        }
        return TLinkAddress.Zero;
    }

    // TODO: Return indices range instead of references count
    /// <summary>
    ///     <para>
    ///         Counts the usages using the specified link.
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
    public TLinkAddress CountUsages(TLinkAddress link)
    {
        var root = GetTreeRoot();
        var total = GetSize(node: root);
        var totalRightIgnore = TLinkAddress.Zero;
        while (root != TLinkAddress.Zero)
        {
            var @base = GetBasePartValue(link: root);
            if ((@base <= link))
            {
                root = GetRightOrDefault(node: root);
            }
            else
            {
                totalRightIgnore = (totalRightIgnore + GetRightSize(node: root) + TLinkAddress.One);
                root = GetLeftOrDefault(node: root);
            }
        }
        root = GetTreeRoot();
        var totalLeftIgnore = TLinkAddress.Zero;
        while (root != TLinkAddress.Zero)
        {
            var @base = GetBasePartValue(link: root);
            if ((@base >= link))
            {
                root = GetLeftOrDefault(node: root);
            }
            else
            {
                totalLeftIgnore = (totalLeftIgnore + GetLeftSize(node: root) + TLinkAddress.One);
                root = GetRightOrDefault(node: root);
            }
        }
        return total - totalRightIgnore - totalLeftIgnore;
    }

    /// <summary>
    ///     <para>
    ///         Eaches the usage using the specified base.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="@base">
    ///     <para>The base.</para>
    ///     <para></para>
    /// </param>
    /// <param name="handler">
    ///     <para>The handler.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public TLinkAddress EachUsage(TLinkAddress @base, ReadHandler<TLinkAddress>? handler)
    {
        return EachUsageCore(@base: @base, link: GetTreeRoot(), handler: handler);
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
    protected abstract TLinkAddress GetTreeRoot();

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
    protected abstract TLinkAddress GetBasePartValue(TLinkAddress link);

    /// <summary>
    ///     <para>
    ///         Determines whether this instance first is to the right of second.
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
    /// <param name="rootSource">
    ///     <para>The root source.</para>
    ///     <para></para>
    /// </param>
    /// <param name="rootTarget">
    ///     <para>The root target.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected abstract bool FirstIsToTheRightOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget);

    /// <summary>
    ///     <para>
    ///         Determines whether this instance first is to the left of second.
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
    /// <param name="rootSource">
    ///     <para>The root source.</para>
    ///     <para></para>
    /// </param>
    /// <param name="rootTarget">
    ///     <para>The root target.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected abstract bool FirstIsToTheLeftOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget);

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
    protected virtual ref LinksHeader<TLinkAddress> GetHeaderReference()
    {
        return ref AsRef<LinksHeader<TLinkAddress>>(source: Header);
    }

    /// <summary>
    ///     <para>
    ///         Gets the link data part reference using the specified link.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="link">
    ///     <para>The link.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>A ref raw link data part of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress link)
    {
        return ref AsRef<RawLinkDataPart<TLinkAddress>>(source: LinksDataParts + RawLinkDataPart<TLinkAddress>.SizeInBytes * long.CreateTruncating(link));
    }

    /// <summary>
    ///     <para>
    ///         Gets the link index part reference using the specified link.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="link">
    ///     <para>The link.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>A ref raw link index part of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual ref RawLinkIndexPart<TLinkAddress> GetLinkIndexPartReference(TLinkAddress link)
    {
        return ref AsRef<RawLinkIndexPart<TLinkAddress>>(source: LinksIndexParts + RawLinkIndexPart<TLinkAddress>.SizeInBytes * long.CreateTruncating(link));
    }

    /// <summary>
    ///     <para>
    ///         Gets the link values using the specified link index.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="linkIndex">
    ///     <para>The link index.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>A list of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual IList<TLinkAddress>? GetLinkValues(TLinkAddress linkIndex)
    {
        ref var link = ref GetLinkDataPartReference(link: linkIndex);
        return new Link<TLinkAddress>(index: linkIndex, source: link.Source, target: link.Target);
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance first is to the left of second.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="first">
    ///     <para>The first.</para>
    ///     <para></para>
    /// </param>
    /// <param name="second">
    ///     <para>The second.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override bool FirstIsToTheLeftOfSecond(TLinkAddress first, TLinkAddress second)
    {
        ref var firstLink = ref GetLinkDataPartReference(link: first);
        ref var secondLink = ref GetLinkDataPartReference(link: second);
        return FirstIsToTheLeftOfSecond(source: firstLink.Source, target: firstLink.Target, rootSource: secondLink.Source, rootTarget: secondLink.Target);
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance first is to the right of second.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="first">
    ///     <para>The first.</para>
    ///     <para></para>
    /// </param>
    /// <param name="second">
    ///     <para>The second.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override bool FirstIsToTheRightOfSecond(TLinkAddress first, TLinkAddress second)
    {
        ref var firstLink = ref GetLinkDataPartReference(link: first);
        ref var secondLink = ref GetLinkDataPartReference(link: second);
        return FirstIsToTheRightOfSecond(source: firstLink.Source, target: firstLink.Target, rootSource: secondLink.Source, rootTarget: secondLink.Target);
    }

    // TODO: 1. Move target, handler to separate object. 2. Use stack or walker 3. Use low-level MSIL stack.
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    private TLinkAddress EachUsageCore(TLinkAddress @base, TLinkAddress link, ReadHandler<TLinkAddress>? handler)
    {
        var @continue = Continue;
        if (link == TLinkAddress.Zero)
        {
            return @continue;
        }
        var linkBasePart = GetBasePartValue(link: link);
        var @break = Break;
        if ((linkBasePart > @base))
        {
            if ((EachUsageCore(@base: @base, link: GetLeftOrDefault(node: link), handler: handler) == @break))
            {
                return @break;
            }
        }
        else if ((linkBasePart < @base))
        {
            if ((EachUsageCore(@base: @base, link: GetRightOrDefault(node: link), handler: handler) == @break))
            {
                return @break;
            }
        }
        else //if (linkBasePart == @base)
        {
            if ((handler(link: GetLinkValues(linkIndex: link)) == @break))
            {
                return @break;
            }
            if ((EachUsageCore(@base: @base, link: GetLeftOrDefault(node: link), handler: handler) == @break))
            {
                return @break;
            }
            if ((EachUsageCore(@base: @base, link: GetRightOrDefault(node: link), handler: handler) == @break))
            {
                return @break;
            }
        }
        return @continue;
    }

    /// <summary>
    ///     <para>
    ///         Prints the node value using the specified node.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="node">
    ///     <para>The node.</para>
    ///     <para></para>
    /// </param>
    /// <param name="sb">
    ///     <para>The sb.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void PrintNodeValue(TLinkAddress node, StringBuilder sb)
    {
        ref var link = ref GetLinkDataPartReference(link: node);
        sb.Append(value: ' ');
        sb.Append(value: link.Source);
        sb.Append(value: '-');
        sb.Append(value: '>');
        sb.Append(value: link.Target);
    }
}
