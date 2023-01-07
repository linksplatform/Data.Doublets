using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Collections.Methods.Lists;
using Platform.Converters;
using Platform.Delegates;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic;

/// <summary>
///     <para>
///         Represents the internal links sources linked list methods.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="RelativeCircularDoublyLinkedListMethods{TLinkAddress}" />
public unsafe class InternalLinksSourcesLinkedListMethods<TLinkAddress> : RelativeCircularDoublyLinkedListMethods<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    private static readonly UncheckedConverter<TLinkAddress, long> _addressToInt64Converter = UncheckedConverter<TLinkAddress, long>.Default;
    private readonly byte* _linksDataParts;
    private readonly byte* _linksIndexParts;
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
    ///         Initializes a new <see cref="InternalLinksSourcesLinkedListMethods" /> instance.
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
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public InternalLinksSourcesLinkedListMethods(LinksConstants<TLinkAddress> constants, byte* linksDataParts, byte* linksIndexParts)
    {
        _linksDataParts = linksDataParts;
        _linksIndexParts = linksIndexParts;
        Break = constants.Break;
        Continue = constants.Continue;
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
        return ref AsRef<RawLinkDataPart<TLinkAddress>>(source: _linksDataParts + RawLinkDataPart<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(source: link));
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
        return ref AsRef<RawLinkIndexPart<TLinkAddress>>(source: _linksIndexParts + RawLinkIndexPart<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(source: link));
    }

    /// <summary>
    ///     <para>
    ///         Gets the first using the specified head.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="head">
    ///     <para>The head.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetFirst(TLinkAddress head)
    {
        return GetLinkIndexPartReference(link: head).RootAsSource;
    }

    /// <summary>
    ///     <para>
    ///         Gets the last using the specified head.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="head">
    ///     <para>The head.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetLast(TLinkAddress head)
    {
        var first = GetLinkIndexPartReference(link: head).RootAsSource;
        if (EqualToZero(value: first))
        {
            return first;
        }
        return GetPrevious(element: first);
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
        return GetLinkIndexPartReference(link: element).LeftAsSource;
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
        return GetLinkIndexPartReference(link: element).RightAsSource;
    }

    /// <summary>
    ///     <para>
    ///         Gets the size using the specified head.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="head">
    ///     <para>The head.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress GetSize(TLinkAddress head)
    {
        return GetLinkIndexPartReference(link: head).SizeAsSource;
    }

    /// <summary>
    ///     <para>
    ///         Sets the first using the specified head.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="head">
    ///     <para>The head.</para>
    ///     <para></para>
    /// </param>
    /// <param name="element">
    ///     <para>The element.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetFirst(TLinkAddress head, TLinkAddress element)
    {
        GetLinkIndexPartReference(link: head).RootAsSource = element;
    }

    /// <summary>
    ///     <para>
    ///         Sets the last using the specified head.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="head">
    ///     <para>The head.</para>
    ///     <para></para>
    /// </param>
    /// <param name="element">
    ///     <para>The element.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetLast(TLinkAddress head, TLinkAddress element)
    {
        //var first = GetLinkIndexPartReference(head).RootAsSource;
        //if (EqualToZero(first))
        //{
        //    SetFirst(head, element);
        //}
        //else
        //{
        //    SetPrevious(first, element);
        //}
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
        GetLinkIndexPartReference(link: element).LeftAsSource = previous;
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
        GetLinkIndexPartReference(link: element).RightAsSource = next;
    }

    /// <summary>
    ///     <para>
    ///         Sets the size using the specified head.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="head">
    ///     <para>The head.</para>
    ///     <para></para>
    /// </param>
    /// <param name="size">
    ///     <para>The size.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void SetSize(TLinkAddress head, TLinkAddress size)
    {
        GetLinkIndexPartReference(link: head).SizeAsSource = size;
    }

    /// <summary>
    ///     <para>
    ///         Counts the usages using the specified head.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="head">
    ///     <para>The head.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public TLinkAddress CountUsages(TLinkAddress head)
    {
        return GetSize(head: head);
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
    ///         Eaches the usage using the specified source.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="source">
    ///     <para>The source.</para>
    ///     <para></para>
    /// </param>
    /// <param name="handler">
    ///     <para>The handler.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The continue.</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public TLinkAddress EachUsage(TLinkAddress source, ReadHandler<TLinkAddress>? handler)
    {
        var @continue = Continue;
        var @break = Break;
        var current = GetFirst(head: source);
        var first = current;
        while (!EqualToZero(value: current))
        {
            if (AreEqual(first: handler(link: GetLinkValues(linkIndex: current)), second: @break))
            {
                return @break;
            }
            current = GetNext(element: current);
            if ((current == first))
            {
                return @continue;
            }
        }
        return @continue;
    }
}
