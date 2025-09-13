using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Defines the links index rebuildable interface for implementations that support rebuilding their indexes.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The type of link address.</para>
    /// <para></para>
    /// </typeparam>
    public interface ILinksIndexRebuildable<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Rebuilds all indexes by dropping and recreating them.
        /// This is useful for data recovery after invalid links have been removed.
        /// </para>
        /// <para></para>
        /// </summary>
        void RebuildIndexes();
    }
}