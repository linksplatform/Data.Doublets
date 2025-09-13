using System;
using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Transactions
{
    /// <summary>
    /// <para>
    /// Defines the links transaction interface.
    /// </para>
    /// <para></para>
    /// </summary>
    public interface ILinksTransaction<TLinkAddress> : IDisposable where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the transaction id.
        /// </para>
        /// <para></para>
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// <para>
        /// Gets a value indicating whether this transaction is committed.
        /// </para>
        /// <para></para>
        /// </summary>
        bool IsCommitted { get; }

        /// <summary>
        /// <para>
        /// Gets a value indicating whether this transaction is rolled back.
        /// </para>
        /// <para></para>
        /// </summary>
        bool IsRolledBack { get; }

        /// <summary>
        /// <para>
        /// Gets the state transitions tracked in this transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        IReadOnlyDictionary<TLinkAddress, LinkStateTransition<TLinkAddress>> StateTransitions { get; }

        /// <summary>
        /// <para>
        /// Records a state transition for the specified link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">
        /// <para>The link address.</para>
        /// <para></para>
        /// </param>
        /// <param name="beforeState">
        /// <para>The state before transition.</para>
        /// <para></para>
        /// </param>
        /// <param name="afterState">
        /// <para>The state after transition.</para>
        /// <para></para>
        /// </param>
        void RecordTransition(TLinkAddress linkAddress, IList<TLinkAddress>? beforeState, IList<TLinkAddress>? afterState);

        /// <summary>
        /// <para>
        /// Gets the current state of the specified link within this transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">
        /// <para>The link address.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The current link state or null if not modified in transaction.</para>
        /// <para></para>
        /// </returns>
        IList<TLinkAddress>? GetCurrentState(TLinkAddress linkAddress);

        /// <summary>
        /// <para>
        /// Checks if there are any conflicts with the specified transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="otherTransaction">
        /// <para>The other transaction to check against.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>True if there are conflicts, false otherwise.</para>
        /// <para></para>
        /// </returns>
        bool HasConflictsWith(ILinksTransaction<TLinkAddress> otherTransaction);

        /// <summary>
        /// <para>
        /// Commits the transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        void Commit();

        /// <summary>
        /// <para>
        /// Rolls back the transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        void Rollback();
    }
}