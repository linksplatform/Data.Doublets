using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Transactions
{
    /// <summary>
    /// <para>
    /// Represents a links transaction that tracks state transitions.
    /// </para>
    /// <para></para>
    /// </summary>
    public class LinksTransaction<TLinkAddress> : ILinksTransaction<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly Dictionary<TLinkAddress, LinkStateTransition<TLinkAddress>> _stateTransitions;
        private bool _isCommitted;
        private bool _isRolledBack;
        private bool _isDisposed;

        /// <summary>
        /// <para>
        /// Gets the transaction id.
        /// </para>
        /// <para></para>
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// <para>
        /// Gets a value indicating whether this transaction is committed.
        /// </para>
        /// <para></para>
        /// </summary>
        public bool IsCommitted => _isCommitted;

        /// <summary>
        /// <para>
        /// Gets a value indicating whether this transaction is rolled back.
        /// </para>
        /// <para></para>
        /// </summary>
        public bool IsRolledBack => _isRolledBack;

        /// <summary>
        /// <para>
        /// Gets the state transitions tracked in this transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        public IReadOnlyDictionary<TLinkAddress, LinkStateTransition<TLinkAddress>> StateTransitions =>
            _stateTransitions;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="LinksTransaction{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        public LinksTransaction()
        {
            Id = Guid.NewGuid();
            _stateTransitions = new Dictionary<TLinkAddress, LinkStateTransition<TLinkAddress>>();
        }

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
        public void RecordTransition(TLinkAddress linkAddress, IList<TLinkAddress>? beforeState, IList<TLinkAddress>? afterState)
        {
            ThrowIfFinalized();

            var transitionType = DetermineTransitionType(beforeState, afterState);
            
            // If we already have a transition for this link, we need to merge them
            if (_stateTransitions.TryGetValue(linkAddress, out var existing))
            {
                // Chain transitions: use original before state and new after state
                var newTransition = new LinkStateTransition<TLinkAddress>(
                    linkAddress, 
                    existing.BeforeState, 
                    afterState, 
                    DetermineTransitionType(existing.BeforeState, afterState));
                
                _stateTransitions[linkAddress] = newTransition;
            }
            else
            {
                var transition = new LinkStateTransition<TLinkAddress>(linkAddress, beforeState, afterState, transitionType);
                _stateTransitions[linkAddress] = transition;
            }
        }

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
        public IList<TLinkAddress>? GetCurrentState(TLinkAddress linkAddress)
        {
            return _stateTransitions.TryGetValue(linkAddress, out var transition) 
                ? transition.AfterState 
                : null;
        }

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
        public bool HasConflictsWith(ILinksTransaction<TLinkAddress> otherTransaction)
        {
            // Check if both transactions modify the same links
            var commonLinks = _stateTransitions.Keys.Intersect(otherTransaction.StateTransitions.Keys);
            
            foreach (var linkAddress in commonLinks)
            {
                var thisTransition = _stateTransitions[linkAddress];
                var otherTransition = otherTransaction.StateTransitions[linkAddress];
                
                // If both transactions attempt to modify the same link from different source states,
                // or apply different target states, there's a conflict
                if (!AreStatesEqual(thisTransition.BeforeState, otherTransition.BeforeState) ||
                    !AreStatesEqual(thisTransition.AfterState, otherTransition.AfterState))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// <para>
        /// Commits the transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        public void Commit()
        {
            ThrowIfFinalized();
            _isCommitted = true;
        }

        /// <summary>
        /// <para>
        /// Rolls back the transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        public void Rollback()
        {
            ThrowIfFinalized();
            _isRolledBack = true;
        }

        /// <summary>
        /// <para>
        /// Disposes the transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (!_isCommitted && !_isRolledBack)
                {
                    Rollback();
                }
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private static TransitionType DetermineTransitionType(IList<TLinkAddress>? beforeState, IList<TLinkAddress>? afterState)
        {
            if (beforeState == null && afterState != null)
                return TransitionType.Create;
            if (beforeState != null && afterState == null)
                return TransitionType.Delete;
            return TransitionType.Update;
        }

        private static bool AreStatesEqual(IList<TLinkAddress>? state1, IList<TLinkAddress>? state2)
        {
            if (state1 == null && state2 == null)
                return true;
            if (state1 == null || state2 == null)
                return false;
            if (state1.Count != state2.Count)
                return false;
            
            for (int i = 0; i < state1.Count; i++)
            {
                if (!EqualityComparer<TLinkAddress>.Default.Equals(state1[i], state2[i]))
                    return false;
            }
            
            return true;
        }

        private void ThrowIfFinalized()
        {
            if (_isCommitted)
                throw new InvalidOperationException("Transaction has already been committed.");
            if (_isRolledBack)
                throw new InvalidOperationException("Transaction has already been rolled back.");
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(LinksTransaction<TLinkAddress>));
        }
    }
}