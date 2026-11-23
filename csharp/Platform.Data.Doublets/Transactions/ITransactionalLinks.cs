using System;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Transactions
{
    /// <summary>
    /// <para>
    /// Defines the transactional links interface.
    /// </para>
    /// <para></para>
    /// </summary>
    public interface ITransactionalLinks<TLinkAddress> : ILinks<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets or sets the current transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        ILinksTransaction<TLinkAddress>? CurrentTransaction { get; set; }

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
        ILinksTransaction<TLinkAddress> BeginTransaction();

        /// <summary>
        /// <para>
        /// Commits the current transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// <para>
        /// Rolls back the current transaction.
        /// </para>
        /// <para></para>
        /// </summary>
        void RollbackTransaction();
    }
}