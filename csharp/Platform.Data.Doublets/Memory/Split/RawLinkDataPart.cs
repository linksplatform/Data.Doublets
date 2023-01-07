using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split;

/// <summary>
///     <para>
///         The raw link data part.
///     </para>
///     <para></para>
/// </summary>
public struct RawLinkDataPart<TLinkAddress> : IEquatable<RawLinkDataPart<TLinkAddress>> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{

    /// <summary>
    ///     <para>
    ///         The size.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static readonly long SizeInBytes = Structure<RawLinkDataPart<TLinkAddress>>.Size;

    /// <summary>
    ///     <para>
    ///         The source.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress Source;
    /// <summary>
    ///     <para>
    ///         The target.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress Target;

    /// <summary>
    ///     <para>
    ///         Determines whether this instance equals.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="obj">
    ///     <para>The obj.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
    {
        return obj is RawLinkDataPart<TLinkAddress> link ? Equals(other: link) : false;
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance equals.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="other">
    ///     <para>The other.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public bool Equals(RawLinkDataPart<TLinkAddress> other)
    {
        return _equalityComparer.Equals(x: Source, y: other.Source) && _equalityComparer.Equals(x: Target, y: other.Target);
    }

    /// <summary>
    ///     <para>
    ///         Gets the hash code.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>The int</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return (Source, Target).GetHashCode();
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(RawLinkDataPart<TLinkAddress> left, RawLinkDataPart<TLinkAddress> right)
    {
        return left.Equals(other: right);
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RawLinkDataPart<TLinkAddress> left, RawLinkDataPart<TLinkAddress> right)
    {
        return !(left == right);
    }
}
