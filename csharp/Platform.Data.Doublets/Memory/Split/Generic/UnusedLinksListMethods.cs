using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Collections.Methods.Lists;
using Platform.Converters;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic;

/// <summary>
///     <para>
///         Represents the unused links list methods.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="AbsoluteCircularDoublyLinkedListMethods{TLinkAddress}" />
/// <seealso cref="ILinksListMethods{TLinkAddress}" />
public unsafe class UnusedLinksListMethods<TLinkAddress> : AbsoluteCircularDoublyLinkedListMethods<TLinkAddress>, ILinksListMethods<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{
    private readonly byte* _header;
    private readonly byte* _links;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="UnusedLinksListMethods" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    /// <param name="header">
    ///     <para>A header.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public UnusedLinksListMethods(byte* links, byte* header)
    {
        _links = links;
        _header = header;
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
    protected virtual ref LinksHeader<TLinkAddress> GetHeaderReference()
    {
        return ref AsRef<LinksHeader<TLinkAddress>>(source: _header);
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
        return ref AsRef<RawLinkDataPart<TLinkAddress>>(source: _links + RawLinkDataPart<TLinkAddress>.SizeInBytes * long.CreateTruncating(link));
    }

    /// <summary>
    ///     <para>
    ///         Gets the first.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetFirst()
    {
        return GetHeaderReference().FirstFreeLink;
    }

    /// <summary>
    ///     <para>
    ///         Gets the last.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetLast()
    {
        return GetHeaderReference().LastFreeLink;
    }

    /// <summary>
    ///     <para>
    ///         Gets the previous using the specified element.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="element">
    ///     <para>The element.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetPrevious(TLinkAddress element)
    {
        return GetLinkDataPartReference(link: element).Source;
    }

    /// <summary>
    ///     <para>
    ///         Gets the next using the specified element.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="element">
    ///     <para>The element.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetNext(TLinkAddress element)
    {
        return GetLinkDataPartReference(link: element).Target;
    }

    /// <summary>
    ///     <para>
    ///         Gets the size.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetSize()
    {
        return GetHeaderReference().FreeLinks;
    }

    /// <summary>
    ///     <para>
    ///         Sets the first using the specified element.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="element">
    ///     <para>The element.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetFirst(TLinkAddress element)
    {
        GetHeaderReference().FirstFreeLink = element;
    }

    /// <summary>
    ///     <para>
    ///         Sets the last using the specified element.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="element">
    ///     <para>The element.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetLast(TLinkAddress element)
    {
        GetHeaderReference().LastFreeLink = element;
    }

    /// <summary>
    ///     <para>
    ///         Sets the previous using the specified element.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="element">
    ///     <para>The element.</para>
    ///     <para></para>
    /// </param>
    /// <param name="previous">
    ///     <para>The previous.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetPrevious(TLinkAddress element, TLinkAddress previous)
    {
        GetLinkDataPartReference(link: element).Source = previous;
    }

    /// <summary>
    ///     <para>
    ///         Sets the next using the specified element.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="element">
    ///     <para>The element.</para>
    ///     <para></para>
    /// </param>
    /// <param name="next">
    ///     <para>The next.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetNext(TLinkAddress element, TLinkAddress next)
    {
        GetLinkDataPartReference(link: element).Target = next;
    }

    /// <summary>
    ///     <para>
    ///         Sets the size using the specified size.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="size">
    ///     <para>The size.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetSize(TLinkAddress size)
    {
        GetHeaderReference().FreeLinks = size;
    }
}
