using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory
{
    /// <summary>
    /// <para>
    /// The links header.
    /// </para>
    /// <para></para>
    /// </summary>
    public struct LinksHeader<TLinkAddress> : IEquatable<LinksHeader<TLinkAddress>> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {

        /// <summary>
        /// <para>
        /// The size.
        /// </para>
        /// <para></para>
        /// </summary>
        public static readonly long SizeInBytes = Structure<LinksHeader<TLinkAddress>>.Size;

        /// <summary>
        /// <para>
        /// The allocated links.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress AllocatedLinks;
        /// <summary>
        /// <para>
        /// The reserved links.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress ReservedLinks;
        /// <summary>
        /// <para>
        /// The free links.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress FreeLinks;
        /// <summary>
        /// <para>
        /// The first free link.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress FirstFreeLink;
        /// <summary>
        /// <para>
        /// The root as source.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress RootAsSource;
        /// <summary>
        /// <para>
        /// The root as target.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress RootAsTarget;
        /// <summary>
        /// <para>
        /// The last free link.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress LastFreeLink;
        /// <summary>
        /// <para>
        /// The reserved.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress Reserved8;

        /// <summary>
        /// <para>
        /// Determines whether this instance equals.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="obj">
        /// <para>The obj.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)  { return obj is LinksHeader<TLinkAddress> linksHeader ? Equals(linksHeader) : false;}

        /// <summary>
        /// <para>
        /// Determines whether this instance equals.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="other">
        /// <para>The other.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(LinksHeader<TLinkAddress> other)
            => AllocatedLinks ==  other.AllocatedLinks
            && ReservedLinks ==  other.ReservedLinks
            && FreeLinks ==  other.FreeLinks
            && FirstFreeLink ==  other.FirstFreeLink
            && RootAsSource ==  other.RootAsSource
            && RootAsTarget ==  other.RootAsTarget
            && LastFreeLink ==  other.LastFreeLink
            && Reserved8 ==  other.Reserved8;

        /// <summary>
        /// <para>
        /// Gets the hash code.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The int</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()  { return (AllocatedLinks, ReservedLinks, FreeLinks, FirstFreeLink, RootAsSource, RootAsTarget, LastFreeLink, Reserved8).GetHashCode();}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(LinksHeader<TLinkAddress> left, LinksHeader<TLinkAddress> right)  { return left.Equals(right);}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(LinksHeader<TLinkAddress> left, LinksHeader<TLinkAddress> right)  { return !(left == right);}
    }

    /// <summary>
    /// <para>
    /// Represents a metadata-aware size type that can store any number of links of any type.
    /// This enables flexible link storage by providing a way to attach metadata to size information.
    /// For simplicity, this version stores the raw value as-is and provides metadata access through methods.
    /// </para>
    /// <para></para>
    /// </summary>
    public struct MetadataAwareSizeType<TLinkAddress> : IEquatable<MetadataAwareSizeType<TLinkAddress>> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// The raw size value that can represent both traditional size and extended metadata.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress Value;

        /// <summary>
        /// <para>
        /// Gets or sets the count/size value.
        /// For backward compatibility, this returns the full value.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Value = value;
        }

        /// <summary>
        /// <para>
        /// Gets metadata information from the size value.
        /// In this simplified implementation, metadata is represented by specific value ranges.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress GetMetadata()
        {
            // Simple metadata extraction: use modular arithmetic
            // This is a placeholder implementation that can be extended
            return Value;
        }

        /// <summary>
        /// <para>
        /// Sets metadata information.
        /// In this simplified implementation, we store the metadata value directly.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="metadata">The metadata value to store.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMetadata(TLinkAddress metadata) => Value = metadata;

        /// <summary>
        /// <para>
        /// Initializes a new instance with specified count and metadata.
        /// For simplicity, this takes the count as the primary value.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="count">The count value.</param>
        /// <param name="metadata">The metadata value (currently unused in simple implementation).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MetadataAwareSizeType(TLinkAddress count, TLinkAddress metadata)
        {
            // For now, prioritize count for backward compatibility
            Value = count;
        }

        /// <summary>
        /// <para>
        /// Implicit conversion from TLinkAddress to MetadataAwareSizeType.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MetadataAwareSizeType<TLinkAddress>(TLinkAddress value)
            => new() { Value = value };

        /// <summary>
        /// <para>
        /// Implicit conversion from MetadataAwareSizeType to TLinkAddress.
        /// Returns the stored value for backward compatibility.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="sizeType">The metadata-aware size type.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TLinkAddress(MetadataAwareSizeType<TLinkAddress> sizeType)
            => sizeType.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(MetadataAwareSizeType<TLinkAddress> other) => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is MetadataAwareSizeType<TLinkAddress> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MetadataAwareSizeType<TLinkAddress> left, MetadataAwareSizeType<TLinkAddress> right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MetadataAwareSizeType<TLinkAddress> left, MetadataAwareSizeType<TLinkAddress> right) => !(left == right);
    }
}
