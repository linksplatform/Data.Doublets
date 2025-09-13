using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents a links storage that supports shared access between multiple processes.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <seealso cref="ILinks{TLinkAddress}"/>
    public class SharedAccessLinks<TLinkAddress> : ILinks<TLinkAddress>, IDisposable 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress,int,TLinkAddress>, IBitwiseOperators<TLinkAddress,TLinkAddress,TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private readonly SharedAccessUnitedMemoryLinks<TLinkAddress> _links;

        /// <summary>
        /// <para>
        /// Gets the constants value.
        /// </para>
        /// <para></para>
        /// </summary>
        public LinksConstants<TLinkAddress> Constants
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _links.Constants;
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SharedAccessLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="databaseFilePath">
        /// <para>The full path to the database file.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedAccessLinks(string databaseFilePath)
        {
            _links = new SharedAccessUnitedMemoryLinks<TLinkAddress>(databaseFilePath);
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SharedAccessLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="databaseFilePath">
        /// <para>The full path to the database file.</para>
        /// <para></para>
        /// </param>
        /// <param name="memoryReservationStep">
        /// <para>The memory reservation step in bytes.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedAccessLinks(string databaseFilePath, long memoryReservationStep)
        {
            _links = new SharedAccessUnitedMemoryLinks<TLinkAddress>(databaseFilePath, memoryReservationStep);
        }

        /// <summary>
        /// <para>
        /// Counts the links that match the specified restriction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of links that match the restriction.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Count(IList<TLinkAddress>? restriction)
        {
            return _links.ExecuteRead(() => _links.Count(restriction));
        }

        /// <summary>
        /// <para>
        /// Iterates through all links that match the specified restriction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler to call for each matching link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The result of the iteration.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
        {
            return _links.ExecuteRead(() => _links.Each(restriction, handler));
        }

        /// <summary>
        /// <para>
        /// Creates a new link with the specified substitution.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="substitution">
        /// <para>The substitution (source, target).</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler to call after creation.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The address of the created link.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            return _links.ExecuteWrite(() => _links.Create(substitution, handler));
        }

        /// <summary>
        /// <para>
        /// Updates the link that matches the restriction with the specified substitution.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction to find the link to update.</para>
        /// <para></para>
        /// </param>
        /// <param name="substitution">
        /// <para>The new values for the link.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler to call after update.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The address of the updated link.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            return _links.ExecuteWrite(() => _links.Update(restriction, substitution, handler));
        }

        /// <summary>
        /// <para>
        /// Deletes the link that matches the specified restriction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction to find the link to delete.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler to call after deletion.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The address of the deleted link.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            return _links.ExecuteWrite(() => _links.Delete(restriction, handler));
        }

        /// <summary>
        /// <para>
        /// Disposes the shared access links resources.
        /// </para>
        /// <para></para>
        /// </summary>
        public void Dispose()
        {
            _links?.Dispose();
        }
    }
}