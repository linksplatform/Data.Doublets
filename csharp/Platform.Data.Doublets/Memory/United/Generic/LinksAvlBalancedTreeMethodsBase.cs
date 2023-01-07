using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Collections.Methods.Trees;
using Platform.Converters;
using Platform.Delegates;
using Platform.Numbers;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.United.Generic;

/// <summary>
///     <para>
///         Represents the links avl balanced tree methods base.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="SizedAndThreadedAVLBalancedTreeMethods{TLinkAddress}" />
/// <seealso cref="ILinksTreeMethods{TLinkAddress}" />
public abstract unsafe class LinksAvlBalancedTreeMethodsBase<TLinkAddress> : SizedAndThreadedAVLBalancedTreeMethods<TLinkAddress>, ILinksTreeMethods<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress> , IShiftOperators<TLinkAddress,int,TLinkAddress>, IBitwiseOperators<TLinkAddress,TLinkAddress,TLinkAddress>, IMinMaxValue<TLinkAddress>
{
    private static readonly UncheckedConverter<TLinkAddress, long> _addressToInt64Converter = UncheckedConverter<TLinkAddress, long>.Default;
    private static readonly UncheckedConverter<TLinkAddress, int> _addressToInt32Converter = UncheckedConverter<TLinkAddress, int>.Default;
    private static readonly UncheckedConverter<bool, TLinkAddress> _boolToAddressConverter = UncheckedConverter<bool, TLinkAddress>.Default;
    private static readonly UncheckedConverter<TLinkAddress, bool> _addressToBoolConverter = UncheckedConverter<TLinkAddress, bool>.Default;
    private static readonly UncheckedConverter<int, TLinkAddress> _int32ToAddressConverter = UncheckedConverter<int, TLinkAddress>.Default;

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
    ///         The links.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly byte* Links;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksAvlBalancedTreeMethodsBase" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="constants">
    ///     <para>A constants.</para>
    ///     <para></para>
    /// </param>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    /// <param name="header">
    ///     <para>A header.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected LinksAvlBalancedTreeMethodsBase(LinksConstants<TLinkAddress> constants, byte* links, byte* header)
    {
        Links = links;
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
            if (GreaterOrEqualThan(first: index, second: GetSize(node: root)))
            {
                return TLinkAddress.Zero;
            }
            while (root != TLinkAddress.Zero)
            {
                var left = GetLeftOrDefault(node: root);
                var leftSize = GetSizeOrZero(node: left);
                if (LessThan(first: index, second: leftSize))
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
            ref var rootLink = ref GetLinkReference(link: root);
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
            if (LessOrEqualThan(first: @base, second: link))
            {
                root = GetRightOrDefault(node: root);
            }
            else
            {
                totalRightIgnore = Add(first: totalRightIgnore, second: GetRightSize(node: root) + TLinkAddress.One);
                root = GetLeftOrDefault(node: root);
            }
        }
        root = GetTreeRoot();
        var totalLeftIgnore = TLinkAddress.Zero;
        while (root != TLinkAddress.Zero)
        {
            var @base = GetBasePartValue(link: root);
            if (GreaterOrEqualThan(first: @base, second: link))
            {
                root = GetLeftOrDefault(node: root);
            }
            else
            {
                totalLeftIgnore = Add(first: totalLeftIgnore, second: GetLeftSize(node: root) + TLinkAddress.One);
                root = GetRightOrDefault(node: root);
            }
        }
        return (Subtract(first: total) - (totalRightIgnore), second: totalLeftIgnore);
    }

    /// <summary>
    ///     <para>
    ///         Eaches the usage using the specified link.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="link">
    ///     <para>The link.</para>
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
    public TLinkAddress EachUsage(TLinkAddress link, ReadHandler<TLinkAddress>? handler)
    {
        var root = GetTreeRoot();
        if (root == TLinkAddress.Zero)
        {
            return Continue;
        }
        TLinkAddress first = TLinkAddress.Zero, current = root;
        while (current != TLinkAddress.Zero)
        {
            var @base = GetBasePartValue(link: current);
            if (GreaterOrEqualThan(first: @base, second: link))
            {
                if (AreEqual(first: @base, second: link))
                {
                    first = current;
                }
                current = GetLeftOrDefault(node: current);
            }
            else
            {
                current = GetRightOrDefault(node: current);
            }
        }
        if (first != TLinkAddress.Zero)
        {
            current = first;
            while (true)
            {
                if (AreEqual(first: handler(link: GetLinkValues(linkIndex: current)), second: Break))
                {
                    return Break;
                }
                current = GetNext(node: current);
                if (current == TLinkAddress.Zero || !AreEqual(first: GetBasePartValue(link: current), second: link))
                {
                    break;
                }
            }
        }
        return Continue;
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
    ///         Gets the link reference using the specified link.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="link">
    ///     <para>The link.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>A ref raw link of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual ref RawLink<TLinkAddress> GetLinkReference(TLinkAddress link)
    {
        return ref AsRef<RawLink<TLinkAddress>>(source: Links + RawLink<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(source: link));
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
        ref var link = ref GetLinkReference(link: linkIndex);
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
        ref var firstLink = ref GetLinkReference(link: first);
        ref var secondLink = ref GetLinkReference(link: second);
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
        ref var firstLink = ref GetLinkReference(link: first);
        ref var secondLink = ref GetLinkReference(link: second);
        return FirstIsToTheRightOfSecond(source: firstLink.Source, target: firstLink.Target, rootSource: secondLink.Source, rootTarget: secondLink.Target);
    }

    /// <summary>
    ///     <para>
    ///         Gets the size value using the specified value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="value">
    ///     <para>The value.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual TLinkAddress GetSizeValue(TLinkAddress value) 
    {
        return Bit<TLinkAddress>.PartialRead(target: value, shift: 5, limit: -5);
    }

    /// <summary>
    ///     <para>
    ///         Sets the size value using the specified stored value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="storedValue">
    ///     <para>The stored value.</para>
    ///     <para></para>
    /// </param>
    /// <param name="size">
    ///     <para>The size.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual void SetSizeValue(ref TLinkAddress storedValue, TLinkAddress size)
    {
        storedValue = Bit<TLinkAddress>.PartialWrite(target: storedValue, source: size, shift: 5, limit: -5);
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance get left is child value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="value">
    ///     <para>The value.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual bool GetLeftIsChildValue(TLinkAddress value)
    {
        return _addressToBoolConverter.Convert(source: Bit<TLinkAddress>.PartialRead(target: value, shift: 4, limit: 1));
        //return !EqualityComparer.Equals(Bit<TLinkAddress>.PartialRead(value, 4, 1), default);
    }

    /// <summary>
    ///     <para>
    ///         Sets the left is child value using the specified stored value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="storedValue">
    ///     <para>The stored value.</para>
    ///     <para></para>
    /// </param>
    /// <param name="value">
    ///     <para>The value.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual void SetLeftIsChildValue(ref TLinkAddress storedValue, bool value)
    {
        unchecked
        {
            var previousValue = storedValue;
            var modified = Bit<TLinkAddress>.PartialWrite(target: previousValue, source: _boolToAddressConverter.Convert(source: value), shift: 4, limit: 1);
            storedValue = modified;
        }
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance get right is child value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="value">
    ///     <para>The value.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual bool GetRightIsChildValue(TLinkAddress value)
    {
        return _addressToBoolConverter.Convert(source: Bit<TLinkAddress>.PartialRead(target: value, shift: 3, limit: 1));
        //return !EqualityComparer.Equals(Bit<TLinkAddress>.PartialRead(value, 3, 1), default);
    }

    /// <summary>
    ///     <para>
    ///         Sets the right is child value using the specified stored value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="storedValue">
    ///     <para>The stored value.</para>
    ///     <para></para>
    /// </param>
    /// <param name="value">
    ///     <para>The value.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual void SetRightIsChildValue(ref TLinkAddress storedValue, bool value)
    {
        unchecked
        {
            var previousValue = storedValue;
            var modified = Bit<TLinkAddress>.PartialWrite(target: previousValue, source: _boolToAddressConverter.Convert(source: value), shift: 3, limit: 1);
            storedValue = modified;
        }
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance is child.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="parent">
    ///     <para>The parent.</para>
    ///     <para></para>
    /// </param>
    /// <param name="possibleChild">
    ///     <para>The possible child.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected bool IsChild(TLinkAddress parent, TLinkAddress possibleChild)
    {
        var parentSize = GetSize(node: parent);
        var childSize = GetSizeOrZero(node: possibleChild);
        return GreaterThanZero(value: childSize) && LessOrEqualThan(first: childSize, second: parentSize);
    }

    /// <summary>
    ///     <para>
    ///         Gets the balance value using the specified stored value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="storedValue">
    ///     <para>The stored value.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The sbyte</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual sbyte GetBalanceValue(TLinkAddress storedValue)
    {
        unchecked
        {
            var value = _addressToInt32Converter.Convert(source: Bit<TLinkAddress>.PartialRead(target: storedValue, shift: 0, limit: 3));
            value |= 0xF8 * ((value & 4) >> 2); // if negative, then continue ones to the end of sbyte
            return (sbyte)value;
        }
    }

    /// <summary>
    ///     <para>
    ///         Sets the balance value using the specified stored value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="storedValue">
    ///     <para>The stored value.</para>
    ///     <para></para>
    /// </param>
    /// <param name="value">
    ///     <para>The value.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual void SetBalanceValue(ref TLinkAddress storedValue, sbyte value)
    {
        unchecked
        {
            var packagedValue = _int32ToAddressConverter.Convert(source: (((byte)value >> 5) & 4) | (value & 3));
            var modified = Bit<TLinkAddress>.PartialWrite(target: storedValue, source: packagedValue, shift: 0, limit: 3);
            storedValue = modified;
        }
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
        ref var link = ref GetLinkReference(link: node);
        sb.Append(value: ' ');
        sb.Append(value: link.Source);
        sb.Append(value: '-');
        sb.Append(value: '>');
        sb.Append(value: link.Target);
    }
}
