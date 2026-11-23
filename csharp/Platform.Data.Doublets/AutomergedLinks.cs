using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents a links storage that automatically merges a snapshot with change sets
    /// to provide a unified view of the data for both completed and uncompleted transactions.
    /// This implementation uses atomic operations for thread-safe snapshot swapping.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public class AutomergedLinks<TLinkAddress> : IAutomergedLinks<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private volatile ISnapshot<TLinkAddress> _currentSnapshot;
        private readonly ConcurrentDictionary<TLinkAddress, IChangeSet<TLinkAddress>> _changeSets;
        private readonly IChangeStream<TLinkAddress> _changeStream;
        private volatile int _nextChangeSetId;

        /// <summary>
        /// <para>
        /// Gets the constants for this links storage.
        /// </para>
        /// <para></para>
        /// </summary>
        public LinksConstants<TLinkAddress> Constants
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _currentSnapshot.Constants;
        }

        /// <summary>
        /// <para>
        /// Gets the current snapshot being used as the base.
        /// </para>
        /// <para></para>
        /// </summary>
        public ISnapshot<TLinkAddress> CurrentSnapshot
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _currentSnapshot;
        }

        /// <summary>
        /// <para>
        /// Gets the active change sets that are being merged with the snapshot.
        /// </para>
        /// <para></para>
        /// </summary>
        public IReadOnlyList<IChangeSet<TLinkAddress>> ActiveChangeSets
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _changeSets.Values.ToList();
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="AutomergedLinks{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="initialSnapshot">The initial snapshot to use as the base.</param>
        /// <param name="changeStream">The change stream for distributing changes (optional).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AutomergedLinks(ISnapshot<TLinkAddress> initialSnapshot, IChangeStream<TLinkAddress>? changeStream = null)
        {
            _currentSnapshot = initialSnapshot ?? throw new ArgumentNullException(nameof(initialSnapshot));
            _changeSets = new ConcurrentDictionary<TLinkAddress, IChangeSet<TLinkAddress>>();
            _changeStream = changeStream ?? new ChangeStream<TLinkAddress>();
            _nextChangeSetId = 1;
        }

        /// <summary>
        /// <para>
        /// Counts links matching the specified restriction, considering both snapshot and change sets.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">The restriction to apply.</param>
        /// <returns>The number of matching links.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Count(IList<TLinkAddress>? restriction)
        {
            // Start with the snapshot count
            var count = TLinkAddress.CreateTruncating(_currentSnapshot.Count(restriction));
            
            // Adjust for changes in change sets
            foreach (var changeSet in _changeSets.Values)
            {
                foreach (var change in changeSet.GetChanges())
                {
                    // This is a simplified implementation
                    // A full implementation would need to properly handle restrictions
                    // and count changes that match the restriction
                    switch (change.Type)
                    {
                        case ChangeType.Create:
                            if (MatchesRestriction(change.NewValues, restriction))
                            {
                                count = TLinkAddress.CreateTruncating(ulong.CreateTruncating(count) + 1);
                            }
                            break;
                        case ChangeType.Delete:
                            if (MatchesRestriction(change.OldValues, restriction))
                            {
                                count = TLinkAddress.CreateTruncating(ulong.CreateTruncating(count) - 1);
                            }
                            break;
                        case ChangeType.Update:
                            // Update doesn't change count
                            break;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// <para>
        /// Iterates over links matching the specified restriction, considering both snapshot and change sets.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">The restriction to apply.</param>
        /// <param name="handler">The handler to call for each matching link.</param>
        /// <returns>The result of the iteration.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
        {
            if (handler == null)
            {
                return Constants.Continue;
            }

            // First iterate over snapshot
            var result = _currentSnapshot.Each(restriction, (link) =>
            {
                // Check if this link is modified or deleted in any change set
                var mergedState = GetMergedLinkState(link[Constants.IndexPart]);
                if (mergedState != null && MatchesRestriction(mergedState, restriction))
                {
                    return handler(new TLinkAddress[] { link[Constants.IndexPart], mergedState[Constants.SourcePart], mergedState[Constants.TargetPart] });
                }
                else if (mergedState == null)
                {
                    // Link was deleted in a change set
                    return Constants.Continue;
                }
                else if (MatchesRestriction(link, restriction))
                {
                    return handler(link);
                }
                return Constants.Continue;
            });

            if (result != Constants.Continue)
            {
                return result;
            }

            // Then iterate over new links in change sets
            foreach (var changeSet in _changeSets.Values)
            {
                foreach (var change in changeSet.GetChanges())
                {
                    if (change.Type == ChangeType.Create && change.NewValues != null && MatchesRestriction(change.NewValues, restriction))
                    {
                        var linkArray = new TLinkAddress[] { change.LinkAddress, change.NewValues[Constants.SourcePart], change.NewValues[Constants.TargetPart] };
                        result = handler(linkArray);
                        if (result != Constants.Continue)
                        {
                            return result;
                        }
                    }
                }
            }

            return Constants.Continue;
        }

        /// <summary>
        /// <para>
        /// Creates a new link in a change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="substitution">The substitution defining the link structure.</param>
        /// <param name="handler">The handler to call during creation.</param>
        /// <returns>The address of the created link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            var changeSet = CreateChangeSet();
            var linkAddress = GenerateNewLinkAddress();
            
            changeSet.AddCreate(linkAddress, substitution ?? new TLinkAddress[0]);
            
            handler?.Invoke(new TLinkAddress[] { linkAddress, substitution?[Constants.SourcePart] ?? Constants.Null, substitution?[Constants.TargetPart] ?? Constants.Null }, 
                           new TLinkAddress[] { linkAddress, substitution?[Constants.SourcePart] ?? Constants.Null, substitution?[Constants.TargetPart] ?? Constants.Null });
            
            _changeStream.Publish(changeSet);
            return linkAddress;
        }

        /// <summary>
        /// <para>
        /// Updates a link in a change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">The restriction to identify the link to update.</param>
        /// <param name="substitution">The new values for the link.</param>
        /// <param name="handler">The handler to call during update.</param>
        /// <returns>The address of the updated link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            if (restriction == null || restriction.Count == 0)
            {
                return Constants.Null;
            }

            var linkAddress = restriction[Constants.IndexPart];
            var oldValues = GetMergedLinkState(linkAddress);
            if (oldValues == null)
            {
                return Constants.Null; // Link doesn't exist
            }

            var changeSet = CreateChangeSet();
            changeSet.AddUpdate(linkAddress, oldValues, substitution ?? new TLinkAddress[0]);
            
            handler?.Invoke(new TLinkAddress[] { linkAddress, oldValues[Constants.SourcePart], oldValues[Constants.TargetPart] }, 
                           new TLinkAddress[] { linkAddress, substitution?[Constants.SourcePart] ?? Constants.Null, substitution?[Constants.TargetPart] ?? Constants.Null });
            
            _changeStream.Publish(changeSet);
            return linkAddress;
        }

        /// <summary>
        /// <para>
        /// Deletes a link in a change set.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">The restriction to identify the link to delete.</param>
        /// <param name="handler">The handler to call during deletion.</param>
        /// <returns>The address of the deleted link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            if (restriction == null || restriction.Count == 0)
            {
                return Constants.Null;
            }

            var linkAddress = restriction[Constants.IndexPart];
            var oldValues = GetMergedLinkState(linkAddress);
            if (oldValues == null)
            {
                return Constants.Null; // Link doesn't exist
            }

            var changeSet = CreateChangeSet();
            changeSet.AddDelete(linkAddress, oldValues);
            
            handler?.Invoke(new TLinkAddress[] { linkAddress, oldValues[Constants.SourcePart], oldValues[Constants.TargetPart] }, 
                           new TLinkAddress[] { linkAddress, oldValues[Constants.SourcePart], oldValues[Constants.TargetPart] });
            
            _changeStream.Publish(changeSet);
            return linkAddress;
        }

        /// <summary>
        /// <para>
        /// Atomically swaps the current snapshot with a new one.
        /// This operation is thread-safe and lock-free.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="newSnapshot">The new snapshot to use as the base.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapSnapshot(ISnapshot<TLinkAddress> newSnapshot)
        {
            _currentSnapshot = newSnapshot ?? throw new ArgumentNullException(nameof(newSnapshot));
        }

        /// <summary>
        /// <para>
        /// Adds a new change set to be merged with the snapshot.
        /// This allows for immediate visibility of changes across threads.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="changeSet">The change set to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChangeSet(IChangeSet<TLinkAddress> changeSet)
        {
            _changeSets.TryAdd(changeSet.Id, changeSet);
        }

        /// <summary>
        /// <para>
        /// Removes a change set that has been incorporated into a new snapshot.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="changeSetId">The ID of the change set to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveChangeSet(TLinkAddress changeSetId)
        {
            _changeSets.TryRemove(changeSetId, out _);
        }

        /// <summary>
        /// <para>
        /// Creates a new change set for pending operations.
        /// Each thread can have its own change set to avoid conflicts.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>A new change set for storing pending operations.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IChangeSet<TLinkAddress> CreateChangeSet()
        {
            var id = TLinkAddress.CreateTruncating(Interlocked.Increment(ref _nextChangeSetId));
            var changeSet = new ChangeSet<TLinkAddress>(id);
            _changeSets.TryAdd(id, changeSet);
            return changeSet;
        }

        /// <summary>
        /// <para>
        /// Gets the merged view of a link considering both the snapshot and all change sets.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>The current state of the link or null if it doesn't exist.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<TLinkAddress>? GetMergedLinkState(TLinkAddress linkAddress)
        {
            // Check change sets first (most recent changes)
            foreach (var changeSet in _changeSets.Values.OrderByDescending(cs => cs.GetChanges().FirstOrDefault()?.Timestamp ?? 0))
            {
                var state = changeSet.GetLinkState(linkAddress);
                if (state != null)
                {
                    return state;
                }
                if (changeSet.Contains(linkAddress))
                {
                    return null; // Link was deleted
                }
            }

            // Fallback to snapshot
            TLinkAddress[]? linkData = null;
            _currentSnapshot.Each(new TLinkAddress[] { linkAddress }, link =>
            {
                linkData = link.ToArray();
                return Constants.Break;
            });

            return linkData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesRestriction(IList<TLinkAddress>? values, IList<TLinkAddress>? restriction)
        {
            if (restriction == null || restriction.Count == 0)
            {
                return true;
            }

            if (values == null)
            {
                return false;
            }

            for (int i = 0; i < Math.Min(restriction.Count, values.Count); i++)
            {
                if (!EqualityComparer<TLinkAddress>.Default.Equals(restriction[i], Constants.Any) && 
                    !EqualityComparer<TLinkAddress>.Default.Equals(restriction[i], values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress GenerateNewLinkAddress()
        {
            // This is a simplified implementation
            // In a real implementation, this would need to ensure uniqueness across the entire system
            return TLinkAddress.CreateTruncating(uint.CreateTruncating(_currentSnapshot.LinkCount) + uint.CreateTruncating(_changeSets.Values.Sum(cs => cs.Count)) + 1u);
        }
    }
}