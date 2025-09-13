using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents the type of change operation.
    /// </para>
    /// <para></para>
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// <para>
        /// A create operation.
        /// </para>
        /// <para></para>
        /// </summary>
        Create,

        /// <summary>
        /// <para>
        /// An update operation.
        /// </para>
        /// <para></para>
        /// </summary>
        Update,

        /// <summary>
        /// <para>
        /// A delete operation.
        /// </para>
        /// <para></para>
        /// </summary>
        Delete
    }

    /// <summary>
    /// <para>
    /// Represents a single change operation in a change set.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public interface IChange<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the type of this change operation.
        /// </para>
        /// <para></para>
        /// </summary>
        ChangeType Type { get; }

        /// <summary>
        /// <para>
        /// Gets the address of the link being changed.
        /// </para>
        /// <para></para>
        /// </summary>
        TLinkAddress LinkAddress { get; }

        /// <summary>
        /// <para>
        /// Gets the old values of the link (before the change).
        /// For create operations, this is null.
        /// </para>
        /// <para></para>
        /// </summary>
        IList<TLinkAddress>? OldValues { get; }

        /// <summary>
        /// <para>
        /// Gets the new values of the link (after the change).
        /// For delete operations, this is null.
        /// </para>
        /// <para></para>
        /// </summary>
        IList<TLinkAddress>? NewValues { get; }

        /// <summary>
        /// <para>
        /// Gets the timestamp when this change was created.
        /// </para>
        /// <para></para>
        /// </summary>
        long Timestamp { get; }
    }
}