using System;
using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents a lock-free stream for distributing change sets to interested threads.
    /// This allows for real-time synchronization of changes across multiple threads without blocking.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public interface IChangeStream<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Publishes a change set to all subscribers.
        /// This operation should be lock-free and non-blocking.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="changeSet">The change set to publish.</param>
        void Publish(IChangeSet<TLinkAddress> changeSet);

        /// <summary>
        /// <para>
        /// Subscribes to receive change sets from this stream.
        /// Returns a subscription that can be used to receive changes.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="handler">The handler to call when a change set is published.</param>
        /// <returns>A subscription that can be disposed to unsubscribe.</returns>
        IDisposable Subscribe(Action<IChangeSet<TLinkAddress>> handler);

        /// <summary>
        /// <para>
        /// Gets all change sets that have been published since the specified timestamp.
        /// This allows subscribers to catch up on missed changes.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="since">The timestamp since which to get changes.</param>
        /// <returns>An enumerable of change sets published since the specified time.</returns>
        IEnumerable<IChangeSet<TLinkAddress>> GetChangesSince(long since);

        /// <summary>
        /// <para>
        /// Gets the number of active subscribers.
        /// </para>
        /// <para></para>
        /// </summary>
        int SubscriberCount { get; }
    }
}