using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents a set of changes (create, update, delete operations) that can be applied to a links storage.
    /// Change sets are used to batch operations and allow for delayed writes to the underlying storage.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public interface IChangeSet<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the unique identifier for this change set.
        /// </para>
        /// <para></para>
        /// </summary>
        TLinkAddress Id { get; }

        /// <summary>
        /// <para>
        /// Gets a value indicating whether this change set is completed (immutable).
        /// </para>
        /// <para></para>
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// <para>
        /// Gets the number of changes in this change set.
        /// </para>
        /// <para></para>
        /// </summary>
        long Count { get; }

        /// <summary>
        /// <para>
        /// Adds a create operation to the change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link to create.</param>
        /// <param name="substitution">The substitution defining the link structure.</param>
        void AddCreate(TLinkAddress linkAddress, IList<TLinkAddress> substitution);

        /// <summary>
        /// <para>
        /// Adds an update operation to the change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link to update.</param>
        /// <param name="oldValues">The old values of the link.</param>
        /// <param name="newValues">The new values of the link.</param>
        void AddUpdate(TLinkAddress linkAddress, IList<TLinkAddress> oldValues, IList<TLinkAddress> newValues);

        /// <summary>
        /// <para>
        /// Adds a delete operation to the change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link to delete.</param>
        /// <param name="values">The values of the link being deleted.</param>
        void AddDelete(TLinkAddress linkAddress, IList<TLinkAddress> values);

        /// <summary>
        /// <para>
        /// Gets the current state of a link considering all changes in this change set.
        /// Returns null if the link doesn't exist in this change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>The current state of the link or null if not found.</returns>
        IList<TLinkAddress>? GetLinkState(TLinkAddress linkAddress);

        /// <summary>
        /// <para>
        /// Checks if a link exists in this change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>True if the link exists, false otherwise.</returns>
        bool Contains(TLinkAddress linkAddress);

        /// <summary>
        /// <para>
        /// Marks this change set as completed, making it immutable.
        /// </para>
        /// <para></para>
        /// </summary>
        void Complete();

        /// <summary>
        /// <para>
        /// Gets all changes in this change set for applying to a snapshot.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>An enumerable of all changes.</returns>
        IEnumerable<IChange<TLinkAddress>> GetChanges();
    }
}