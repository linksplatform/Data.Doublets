using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents a links storage that automatically merges a snapshot with change sets
    /// to provide a unified view of the data for both completed and uncompleted transactions.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public interface IAutomergedLinks<TLinkAddress> : ILinks<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the current snapshot being used as the base.
        /// </para>
        /// <para></para>
        /// </summary>
        ISnapshot<TLinkAddress> CurrentSnapshot { get; }

        /// <summary>
        /// <para>
        /// Gets the active change sets that are being merged with the snapshot.
        /// </para>
        /// <para></para>
        /// </summary>
        IReadOnlyList<IChangeSet<TLinkAddress>> ActiveChangeSets { get; }

        /// <summary>
        /// <para>
        /// Atomically swaps the current snapshot with a new one.
        /// This operation is thread-safe and lock-free.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="newSnapshot">The new snapshot to use as the base.</param>
        void SwapSnapshot(ISnapshot<TLinkAddress> newSnapshot);

        /// <summary>
        /// <para>
        /// Adds a new change set to be merged with the snapshot.
        /// This allows for immediate visibility of changes across threads.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="changeSet">The change set to add.</param>
        void AddChangeSet(IChangeSet<TLinkAddress> changeSet);

        /// <summary>
        /// <para>
        /// Removes a change set that has been incorporated into a new snapshot.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="changeSetId">The ID of the change set to remove.</param>
        void RemoveChangeSet(TLinkAddress changeSetId);

        /// <summary>
        /// <para>
        /// Creates a new change set for pending operations.
        /// Each thread can have its own change set to avoid conflicts.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>A new change set for storing pending operations.</returns>
        IChangeSet<TLinkAddress> CreateChangeSet();

        /// <summary>
        /// <para>
        /// Gets the merged view of a link considering both the snapshot and all change sets.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>The current state of the link or null if it doesn't exist.</returns>
        IList<TLinkAddress>? GetMergedLinkState(TLinkAddress linkAddress);
    }
}