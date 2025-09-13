using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Provides extension methods for working with BitArray index decorators.
///     </para>
/// </summary>
public static class BitArrayLinksIndexExtensions
{
    /// <summary>
    ///     <para>
    ///         Decorates the specified links with a BitArray index.
    ///     </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of the link address.</typeparam>
    /// <param name="links">The links to decorate.</param>
    /// <param name="useIndexForReads">Whether to use the index for read operations.</param>
    /// <param name="maintainIndexOnWrites">Whether to maintain the index on write operations.</param>
    /// <returns>The decorated links with BitArray index.</returns>
    public static BitArrayLinksIndex<TLinkAddress> WithBitArrayIndex<TLinkAddress>(
        this ILinks<TLinkAddress> links,
        bool useIndexForReads = true,
        bool maintainIndexOnWrites = true)
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        return new BitArrayLinksIndex<TLinkAddress>(links, useIndexForReads, maintainIndexOnWrites);
    }
}