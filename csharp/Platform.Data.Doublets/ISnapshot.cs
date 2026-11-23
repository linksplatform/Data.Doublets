using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents an immutable snapshot of the links storage at a specific point in time.
    /// Snapshots are used as the stable base for reading operations and can be atomically swapped.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public interface ISnapshot<TLinkAddress> : ILinks<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the unique identifier for this snapshot.
        /// </para>
        /// <para></para>
        /// </summary>
        TLinkAddress Id { get; }

        /// <summary>
        /// <para>
        /// Gets the timestamp when this snapshot was created.
        /// </para>
        /// <para></para>
        /// </summary>
        long Timestamp { get; }

        /// <summary>
        /// <para>
        /// Gets the number of links in this snapshot.
        /// </para>
        /// <para></para>
        /// </summary>
        long LinkCount { get; }

        /// <summary>
        /// <para>
        /// Creates a new snapshot by applying the given change sets to this snapshot.
        /// The original snapshot remains unchanged.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="changeSets">The change sets to apply.</param>
        /// <returns>A new snapshot with the changes applied.</returns>
        ISnapshot<TLinkAddress> ApplyChanges(IEnumerable<IChangeSet<TLinkAddress>> changeSets);

        /// <summary>
        /// <para>
        /// Creates a copy of this snapshot for atomic swapping.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>A copy of this snapshot.</returns>
        ISnapshot<TLinkAddress> Clone();
    }
}