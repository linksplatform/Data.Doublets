using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Timestamps;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents a single change operation in a change set.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public class Change<TLinkAddress> : IChange<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the type of this change operation.
        /// </para>
        /// <para></para>
        /// </summary>
        public ChangeType Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// <para>
        /// Gets the address of the link being changed.
        /// </para>
        /// <para></para>
        /// </summary>
        public TLinkAddress LinkAddress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// <para>
        /// Gets the old values of the link (before the change).
        /// For create operations, this is null.
        /// </para>
        /// <para></para>
        /// </summary>
        public IList<TLinkAddress>? OldValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// <para>
        /// Gets the new values of the link (after the change).
        /// For delete operations, this is null.
        /// </para>
        /// <para></para>
        /// </summary>
        public IList<TLinkAddress>? NewValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// <para>
        /// Gets the timestamp when this change was created.
        /// </para>
        /// <para></para>
        /// </summary>
        public long Timestamp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="Change{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="type">The type of change operation.</param>
        /// <param name="linkAddress">The address of the link being changed.</param>
        /// <param name="oldValues">The old values of the link.</param>
        /// <param name="newValues">The new values of the link.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Change(ChangeType type, TLinkAddress linkAddress, IList<TLinkAddress>? oldValues, IList<TLinkAddress>? newValues)
        {
            Type = type;
            LinkAddress = linkAddress;
            OldValues = oldValues?.ToArray(); // Create defensive copy
            NewValues = newValues?.ToArray(); // Create defensive copy
            Timestamp = Timestamper.GetUtcTimestamp();
        }

        /// <summary>
        /// <para>
        /// Creates a create change.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link being created.</param>
        /// <param name="newValues">The values of the new link.</param>
        /// <returns>A new create change.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Change<TLinkAddress> Create(TLinkAddress linkAddress, IList<TLinkAddress> newValues)
        {
            return new Change<TLinkAddress>(ChangeType.Create, linkAddress, null, newValues);
        }

        /// <summary>
        /// <para>
        /// Creates an update change.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link being updated.</param>
        /// <param name="oldValues">The old values of the link.</param>
        /// <param name="newValues">The new values of the link.</param>
        /// <returns>A new update change.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Change<TLinkAddress> Update(TLinkAddress linkAddress, IList<TLinkAddress> oldValues, IList<TLinkAddress> newValues)
        {
            return new Change<TLinkAddress>(ChangeType.Update, linkAddress, oldValues, newValues);
        }

        /// <summary>
        /// <para>
        /// Creates a delete change.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">The address of the link being deleted.</param>
        /// <param name="oldValues">The values of the link being deleted.</param>
        /// <returns>A new delete change.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Change<TLinkAddress> Delete(TLinkAddress linkAddress, IList<TLinkAddress> oldValues)
        {
            return new Change<TLinkAddress>(ChangeType.Delete, linkAddress, oldValues, null);
        }
    }
}