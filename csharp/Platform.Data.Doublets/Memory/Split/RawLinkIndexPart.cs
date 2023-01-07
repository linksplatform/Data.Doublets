using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split;

/// <summary>
///     <para>
///         The raw link index part.
///     </para>
///     <para></para>
/// </summary>
public struct RawLinkIndexPart<TLinkAddress> : IEquatable<RawLinkIndexPart<TLinkAddress>> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{

    /// <summary>
    ///     <para>
    ///         The size.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static readonly long SizeInBytes = Structure<RawLinkIndexPart<TLinkAddress>>.Size;
    
    /// <summary>
    ///     <para>
    ///         The root as source.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress RootAsSource;
    /// <summary>
    ///     <para>
    ///         The left as source.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress LeftAsSource;
    /// <summary>
    ///     <para>
    ///         The right as source.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress RightAsSource;
    /// <summary>
    ///     <para>
    ///         The size as source.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress SizeAsSource;
    /// <summary>
    ///     <para>
    ///         The root as target.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress RootAsTarget;
    /// <summary>
    ///     <para>
    ///         The left as target.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress LeftAsTarget;
    /// <summary>
    ///     <para>
    ///         The right as target.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress RightAsTarget;
    /// <summary>
    ///     <para>
    ///         The size as target.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public TLinkAddress SizeAsTarget;

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
        return obj is RawLinkIndexPart<TLinkAddress> link ? Equals(other: link) : false;
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
    public bool Equals(RawLinkIndexPart<TLinkAddress> other)
    {
        return _equalityComparer.Equals(x: RootAsSource, y: other.RootAsSource) && _equalityComparer.Equals(x: LeftAsSource, y: other.LeftAsSource) && _equalityComparer.Equals(x: RightAsSource, y: other.RightAsSource) && _equalityComparer.Equals(x: SizeAsSource, y: other.SizeAsSource) && _equalityComparer.Equals(x: RootAsTarget, y: other.RootAsTarget) && _equalityComparer.Equals(x: LeftAsTarget, y: other.LeftAsTarget) && _equalityComparer.Equals(x: RightAsTarget, y: other.RightAsTarget) && _equalityComparer.Equals(x: SizeAsTarget, y: other.SizeAsTarget);
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
        return (RootAsSource, LeftAsSource, RightAsSource, SizeAsSource, RootAsTarget, LeftAsTarget, RightAsTarget, SizeAsTarget).GetHashCode();
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(RawLinkIndexPart<TLinkAddress> left, RawLinkIndexPart<TLinkAddress> right)
    {
        return left.Equals(other: right);
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RawLinkIndexPart<TLinkAddress> left, RawLinkIndexPart<TLinkAddress> right)
    {
        return !(left == right);
    }
}
