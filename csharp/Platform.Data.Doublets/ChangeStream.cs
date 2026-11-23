using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// A lock-free implementation of change stream for distributing change sets to subscribers.
    /// Uses ConcurrentQueue for thread-safe operations without locks.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public class ChangeStream<TLinkAddress> : IChangeStream<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly ConcurrentQueue<IChangeSet<TLinkAddress>> _changeQueue;
        private readonly ConcurrentDictionary<int, Action<IChangeSet<TLinkAddress>>> _subscribers;
        private readonly int _maxQueueSize;
        private volatile int _nextSubscriberId;

        /// <summary>
        /// <para>
        /// Gets the number of active subscribers.
        /// </para>
        /// <para></para>
        /// </summary>
        public int SubscriberCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _subscribers.Count;
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="ChangeStream{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="maxQueueSize">The maximum number of change sets to keep in the queue for catch-up.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChangeStream(int maxQueueSize = 1000)
        {
            _changeQueue = new ConcurrentQueue<IChangeSet<TLinkAddress>>();
            _subscribers = new ConcurrentDictionary<int, Action<IChangeSet<TLinkAddress>>>();
            _maxQueueSize = maxQueueSize;
            _nextSubscriberId = 1;
        }

        /// <summary>
        /// <para>
        /// Publishes a change set to all subscribers.
        /// This operation is lock-free and non-blocking.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="changeSet">The change set to publish.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Publish(IChangeSet<TLinkAddress> changeSet)
        {
            // Add to queue for catch-up functionality
            _changeQueue.Enqueue(changeSet);
            
            // Trim queue if it gets too large
            while (_changeQueue.Count > _maxQueueSize && _changeQueue.TryDequeue(out _))
            {
                // Remove oldest items
            }

            // Notify all subscribers
            foreach (var subscriber in _subscribers.Values)
            {
                try
                {
                    subscriber(changeSet);
                }
                catch
                {
                    // Ignore subscriber errors to prevent one bad subscriber from affecting others
                }
            }
        }

        /// <summary>
        /// <para>
        /// Subscribes to receive change sets from this stream.
        /// Returns a subscription that can be used to receive changes.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="handler">The handler to call when a change set is published.</param>
        /// <returns>A subscription that can be disposed to unsubscribe.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Subscribe(Action<IChangeSet<TLinkAddress>> handler)
        {
            var subscriberId = Interlocked.Increment(ref _nextSubscriberId);
            _subscribers.TryAdd(subscriberId, handler);
            return new Subscription(() => _subscribers.TryRemove(subscriberId, out _));
        }

        /// <summary>
        /// <para>
        /// Gets all change sets that have been published since the specified timestamp.
        /// This allows subscribers to catch up on missed changes.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="since">The timestamp since which to get changes.</param>
        /// <returns>An enumerable of change sets published since the specified time.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IChangeSet<TLinkAddress>> GetChangesSince(long since)
        {
            return _changeQueue
                .Where(cs => cs.GetChanges().Any() && cs.GetChanges().Min(c => c.Timestamp) >= since)
                .OrderBy(cs => cs.GetChanges().Min(c => c.Timestamp));
        }

        private class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;
            private volatile bool _disposed;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _unsubscribe();
                }
            }
        }
    }
}