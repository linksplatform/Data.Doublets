using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;
using Platform.Threading.Synchronization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Transactions
{
    /// <summary>
    /// <para>
    /// Represents a transactional decorator for ILinks that provides transaction support.
    /// </para>
    /// <para></para>
    /// </summary>
    public class TransactionalLinksDecorator<TLinkAddress> : ITransactionalLinks<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly ILinks<TLinkAddress> _innerLinks;
        private readonly ISynchronization _synchronization;
        private ILinksTransaction<TLinkAddress>? _currentTransaction;

        /// <summary>
        /// <para>
        /// Gets the constants value.
        /// </para>
        /// <para></para>
        /// </summary>
        public LinksConstants<TLinkAddress> Constants => _innerLinks.Constants;

        /// <summary>
        /// <para>
        /// Gets or sets the current transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        public ILinksTransaction<TLinkAddress>? CurrentTransaction
        {
            get => _currentTransaction;
            set => _currentTransaction = value;
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="TransactionalLinksDecorator{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="innerLinks">
        /// <para>The inner links implementation.</para>
        /// <para></para>
        /// </param>
        public TransactionalLinksDecorator(ILinks<TLinkAddress> innerLinks) 
            : this(innerLinks, new ReaderWriterLockSynchronization()) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="TransactionalLinksDecorator{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="innerLinks">
        /// <para>The inner links implementation.</para>
        /// <para></para>
        /// </param>
        /// <param name="synchronization">
        /// <para>The synchronization mechanism.</para>
        /// <para></para>
        /// </param>
        public TransactionalLinksDecorator(ILinks<TLinkAddress> innerLinks, ISynchronization synchronization)
        {
            _innerLinks = innerLinks ?? throw new ArgumentNullException(nameof(innerLinks));
            _synchronization = synchronization ?? throw new ArgumentNullException(nameof(synchronization));
        }

        /// <summary>
        /// <para>
        /// Begins a new transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The transaction instance.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILinksTransaction<TLinkAddress> BeginTransaction()
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("A transaction is already active. Nested transactions are not supported.");

            _currentTransaction = new LinksTransaction<TLinkAddress>();
            return _currentTransaction;
        }

        /// <summary>
        /// <para>
        /// Commits the current transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CommitTransaction()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No active transaction to commit.");

            CommitTransactionInternal(_currentTransaction);
            _currentTransaction = null;
        }

        /// <summary>
        /// <para>
        /// Rolls back the current transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RollbackTransaction()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No active transaction to rollback.");

            _currentTransaction.Rollback();
            _currentTransaction = null;
        }

        /// <summary>
        /// <para>
        /// Counts the restriction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link address</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Count(IList<TLinkAddress>? restriction)
        {
            // Read operations can be performed with or without transactions
            // When in transaction, we need to account for pending changes
            if (_currentTransaction != null)
            {
                return CountWithTransaction(restriction);
            }

            return _innerLinks.Count(restriction);
        }

        /// <summary>
        /// <para>
        /// Eaches the handler.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link address</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
        {
            // Read operations need to show the transaction view
            if (_currentTransaction != null)
            {
                return EachWithTransaction(restriction, handler);
            }

            return _innerLinks.Each(restriction, handler);
        }

        /// <summary>
        /// <para>
        /// Creates the substitution.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="substitution">
        /// <para>The substitution.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link address</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            if (_currentTransaction != null)
            {
                return CreateWithTransaction(substitution, handler);
            }

            return _innerLinks.Create(substitution, handler);
        }

        /// <summary>
        /// <para>
        /// Updates the restriction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <param name="substitution">
        /// <para>The substitution.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link address</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            if (_currentTransaction != null)
            {
                return UpdateWithTransaction(restriction, substitution, handler);
            }

            return _innerLinks.Update(restriction, substitution, handler);
        }

        /// <summary>
        /// <para>
        /// Deletes the restriction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link address</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            if (_currentTransaction != null)
            {
                return DeleteWithTransaction(restriction, handler);
            }

            return _innerLinks.Delete(restriction, handler);
        }

        private TLinkAddress CountWithTransaction(IList<TLinkAddress>? restriction)
        {
            // This is a simplified implementation
            // In a full implementation, we would need to account for:
            // 1. Links created in transaction
            // 2. Links deleted in transaction
            // 3. Links updated in transaction
            // For now, we delegate to the underlying implementation
            return _innerLinks.Count(restriction);
        }

        private TLinkAddress EachWithTransaction(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
        {
            // For reads within transactions, we need to return the current transactional state
            // This would require intercepting each link and checking if it has been modified
            // For now, we use a simplified approach
            return _innerLinks.Each(restriction, (link) =>
            {
                if (handler == null) return Constants.Continue;

                var linkAddress = this.GetIndex(link);
                var currentState = _currentTransaction!.GetCurrentState(linkAddress);
                
                // If the link has been modified in the transaction, use the transaction state
                return handler(currentState ?? link);
            });
        }

        private TLinkAddress CreateWithTransaction(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            // In a transaction, we track the operation but don't execute it immediately
            // For simplicity, we generate a temporary link address
            var newLinkAddress = GenerateTemporaryLinkAddress();
            var afterState = substitution != null && substitution.Count >= 3
                ? new List<TLinkAddress> { newLinkAddress, substitution[1], substitution[2] }
                : new List<TLinkAddress> { newLinkAddress, Constants.Null, Constants.Null };

            _currentTransaction!.RecordTransition(newLinkAddress, null, afterState);

            // Call handler if provided
            if (handler != null)
            {
                var result = handler(null, afterState);
                if (EqualityComparer<TLinkAddress>.Default.Equals(result, Constants.Break))
                {
                    return Constants.Break;
                }
            }

            return newLinkAddress;
        }

        private TLinkAddress UpdateWithTransaction(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            var linkAddress = this.GetIndex(restriction);
            var beforeState = _currentTransaction!.GetCurrentState(linkAddress) ?? _innerLinks.GetLink(linkAddress);
            
            _currentTransaction.RecordTransition(linkAddress, beforeState, substitution);

            // Call handler if provided
            if (handler != null)
            {
                var result = handler(beforeState, substitution);
                if (EqualityComparer<TLinkAddress>.Default.Equals(result, Constants.Break))
                {
                    return Constants.Break;
                }
            }

            return Constants.Continue;
        }

        private TLinkAddress DeleteWithTransaction(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            var linkAddress = this.GetIndex(restriction);
            var beforeState = _currentTransaction!.GetCurrentState(linkAddress) ?? _innerLinks.GetLink(linkAddress);
            
            _currentTransaction.RecordTransition(linkAddress, beforeState, null);

            // Call handler if provided
            if (handler != null)
            {
                var result = handler(beforeState, null);
                if (EqualityComparer<TLinkAddress>.Default.Equals(result, Constants.Break))
                {
                    return Constants.Break;
                }
            }

            return Constants.Continue;
        }

        private void CommitTransactionInternal(ILinksTransaction<TLinkAddress> transaction)
        {
            // Acquire write lock for atomic commit
            _synchronization.DoWrite(() =>
            {
                // Re-check for conflicts before committing
                // In a real implementation, this would check against other concurrent transactions
                
                // Apply all changes in order
                foreach (var transition in transaction.StateTransitions.Values)
                {
                    ApplyTransition(transition);
                }

                transaction.Commit();
                return default(TLinkAddress);
            });
        }

        private void ApplyTransition(LinkStateTransition<TLinkAddress> transition)
        {
            switch (transition.TransitionType)
            {
                case TransitionType.Create:
                    if (transition.AfterState != null)
                    {
                        _innerLinks.Create(transition.AfterState, null);
                    }
                    break;

                case TransitionType.Update:
                    if (transition.AfterState != null)
                    {
                        var restriction = new List<TLinkAddress> { transition.LinkAddress };
                        _innerLinks.Update(restriction, transition.AfterState, null);
                    }
                    break;

                case TransitionType.Delete:
                    var deleteRestriction = new List<TLinkAddress> { transition.LinkAddress };
                    _innerLinks.Delete(deleteRestriction, null);
                    break;
            }
        }

        private TLinkAddress GenerateTemporaryLinkAddress()
        {
            // This is a simplified approach - in practice, you would need a more sophisticated
            // method to generate temporary addresses that don't conflict with existing ones
            var count = _innerLinks.Count();
            return count + TLinkAddress.One;
        }
    }
}