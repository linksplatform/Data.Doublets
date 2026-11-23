using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Platform.Delegates;
using Platform.Data.Exceptions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators
{
    /// <summary>
    /// <para>
    /// Represents a decorator that prevents write operations (Create, Update, Delete) 
    /// from being executed during read operations (Each).
    /// </para>
    /// <para>
    /// This decorator prevents unsafe modifications to data structures during traversal,
    /// which can lead to corrupted indexes or infinite loops.
    /// </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public class ReadWriteValidationDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress>
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private static readonly ThreadLocal<int> _readOperationDepth = new(() => 0);

        /// <summary>
        /// <para>
        /// Gets a value indicating whether a read operation is currently in progress.
        /// </para>
        /// </summary>
        public static bool IsInReadOperation => _readOperationDepth.Value > 0;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="ReadWriteValidationDecorator{TLinkAddress}"/> instance.
        /// </para>
        /// </summary>
        /// <param name="links">
        /// <para>The links storage to decorate.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadWriteValidationDecorator(ILinks<TLinkAddress> links) : base(links)
        {
        }

        /// <summary>
        /// <para>
        /// Counts the links matching the specified restriction.
        /// This is a read operation and can be safely called during traversal.
        /// </para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction to apply.</para>
        /// </param>
        /// <returns>
        /// <para>The number of links matching the restriction.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Count(IList<TLinkAddress>? restriction)
        {
            return base.Count(restriction);
        }

        /// <summary>
        /// <para>
        /// Iterates through links matching the specified restriction.
        /// This method tracks the read operation state to prevent write operations during traversal.
        /// </para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction to apply.</para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler to call for each matching link.</para>
        /// </param>
        /// <returns>
        /// <para>The result of the traversal operation.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
        {
            _readOperationDepth.Value++;
            try
            {
                return base.Each(restriction, handler);
            }
            finally
            {
                _readOperationDepth.Value--;
            }
        }

        /// <summary>
        /// <para>
        /// Creates a new link with the specified substitution.
        /// This method throws an exception if called during a read operation.
        /// </para>
        /// </summary>
        /// <param name="substitution">
        /// <para>The substitution values for the new link.</para>
        /// </param>
        /// <param name="handler">
        /// <para>The write handler to call.</para>
        /// </param>
        /// <returns>
        /// <para>The address of the created link.</para>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <para>Thrown when attempting to create a link during a read operation.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            ThrowIfInReadOperation("Create");
            return base.Create(substitution, handler);
        }

        /// <summary>
        /// <para>
        /// Updates a link with the specified restriction and substitution.
        /// This method throws an exception if called during a read operation.
        /// </para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction to identify the link to update.</para>
        /// </param>
        /// <param name="substitution">
        /// <para>The new values for the link.</para>
        /// </param>
        /// <param name="handler">
        /// <para>The write handler to call.</para>
        /// </param>
        /// <returns>
        /// <para>The address of the updated link.</para>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <para>Thrown when attempting to update a link during a read operation.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            ThrowIfInReadOperation("Update");
            return base.Update(restriction, substitution, handler);
        }

        /// <summary>
        /// <para>
        /// Deletes a link matching the specified restriction.
        /// This method throws an exception if called during a read operation.
        /// </para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction to identify the link to delete.</para>
        /// </param>
        /// <param name="handler">
        /// <para>The write handler to call.</para>
        /// </param>
        /// <returns>
        /// <para>The result of the delete operation.</para>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <para>Thrown when attempting to delete a link during a read operation.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            ThrowIfInReadOperation("Delete");
            return base.Delete(restriction, handler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowIfInReadOperation(string operationName)
        {
            if (IsInReadOperation)
            {
                throw new InvalidOperationException($"Cannot perform write operation '{operationName}' during a read operation (Each). " +
                    "This is unsafe as it can lead to traversing indexes while changing them at the same time. " +
                    "Consider collecting the changes during traversal and applying them after the read operation completes.");
            }
        }
    }
}