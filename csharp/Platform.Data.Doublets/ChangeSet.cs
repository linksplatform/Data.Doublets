using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Random;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents a set of changes that can be applied to a links storage.
    /// This implementation is thread-safe for concurrent reads and writes.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public class ChangeSet<TLinkAddress> : IChangeSet<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly ConcurrentDictionary<TLinkAddress, IChange<TLinkAddress>> _changes;
        private volatile bool _isCompleted;

        /// <summary>
        /// <para>
        /// Gets the unique identifier for this change set.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// <para>
        /// Gets a value indicating whether this change set is completed (immutable).
        /// </para>
        /// <para></para>
        /// </summary>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isCompleted;
        }

        /// <summary>
        /// <para>
        /// Gets the number of changes in this change set.
        /// </para>
        /// <para></para>
        /// </summary>
        public long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _changes.Count;
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="ChangeSet{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChangeSet()
        {
            Id = TLinkAddress.CreateTruncating(RandomHelpers.Default.NextUInt64());
            _changes = new ConcurrentDictionary<TLinkAddress, IChange<TLinkAddress>>();
            _isCompleted = false;
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="ChangeSet{TLinkAddress}"/> instance with a specific ID.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="id">The unique identifier for this change set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChangeSet(TLinkAddress id)
        {
            Id = id;
            _changes = new ConcurrentDictionary<TLinkAddress, IChange<TLinkAddress>>();
            _isCompleted = false;
        }

        /// <summary>
        /// <para>
        /// Adds a create operation to the change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link to create.</param>
        /// <param name="substitution">The substitution defining the link structure.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCreate(TLinkAddress linkAddress, IList<TLinkAddress> substitution)
        {
            ThrowIfCompleted();
            var change = Change<TLinkAddress>.Create(linkAddress, substitution);
            _changes.AddOrUpdate(linkAddress, change, (_, _) => change);
        }

        /// <summary>
        /// <para>
        /// Adds an update operation to the change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link to update.</param>
        /// <param name="oldValues">The old values of the link.</param>
        /// <param name="newValues">The new values of the link.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddUpdate(TLinkAddress linkAddress, IList<TLinkAddress> oldValues, IList<TLinkAddress> newValues)
        {
            ThrowIfCompleted();
            var change = Change<TLinkAddress>.Update(linkAddress, oldValues, newValues);
            _changes.AddOrUpdate(linkAddress, change, (_, existingChange) =>
            {
                // If there's already a change for this link, we need to handle the sequence properly
                return existingChange.Type switch
                {
                    ChangeType.Create => Change<TLinkAddress>.Create(linkAddress, newValues), // Create + Update = Create with new values
                    ChangeType.Update => Change<TLinkAddress>.Update(linkAddress, existingChange.OldValues!, newValues), // Update + Update = Update with original old values
                    ChangeType.Delete => throw new InvalidOperationException("Cannot update a deleted link"),
                    _ => change
                };
            });
        }

        /// <summary>
        /// <para>
        /// Adds a delete operation to the change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link to delete.</param>
        /// <param name="values">The values of the link being deleted.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddDelete(TLinkAddress linkAddress, IList<TLinkAddress> values)
        {
            ThrowIfCompleted();
            var change = Change<TLinkAddress>.Delete(linkAddress, values);
            _changes.AddOrUpdate(linkAddress, change, (_, existingChange) =>
            {
                // If there's already a change for this link, we need to handle the sequence properly
                return existingChange.Type switch
                {
                    ChangeType.Create => throw new InvalidOperationException("Cannot delete a link that was just created in the same change set"), // Could be handled by removing the entry instead
                    ChangeType.Update => Change<TLinkAddress>.Delete(linkAddress, existingChange.OldValues!), // Update + Delete = Delete with original values
                    ChangeType.Delete => existingChange, // Delete + Delete = Keep first delete
                    _ => change
                };
            });
        }

        /// <summary>
        /// <para>
        /// Gets the current state of a link considering all changes in this change set.
        /// Returns null if the link doesn't exist in this change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>The current state of the link or null if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<TLinkAddress>? GetLinkState(TLinkAddress linkAddress)
        {
            if (_changes.TryGetValue(linkAddress, out var change))
            {
                return change.Type switch
                {
                    ChangeType.Create => change.NewValues,
                    ChangeType.Update => change.NewValues,
                    ChangeType.Delete => null, // Link was deleted
                    _ => null
                };
            }
            return null; // Link not found in this change set
        }

        /// <summary>
        /// <para>
        /// Checks if a link exists in this change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>True if the link exists, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TLinkAddress linkAddress)
        {
            if (_changes.TryGetValue(linkAddress, out var change))
            {
                return change.Type != ChangeType.Delete;
            }
            return false;
        }

        /// <summary>
        /// <para>
        /// Marks this change set as completed, making it immutable.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Complete()
        {
            _isCompleted = true;
        }

        /// <summary>
        /// <para>
        /// Gets all changes in this change set for applying to a snapshot.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>An enumerable of all changes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IChange<TLinkAddress>> GetChanges()
        {
            return _changes.Values.OrderBy(c => c.Timestamp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfCompleted()
        {
            if (_isCompleted)
            {
                throw new InvalidOperationException("Cannot modify a completed change set");
            }
        }
    }
}