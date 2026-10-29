using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Ranges;
using Platform.Collections.Lists;
using Platform.Random;
using Platform.Setters;
using Platform.Converters;
using Platform.Numbers;
using Platform.Data.Exceptions;
using Platform.Data.Doublets.Decorators;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents the links extensions.
    /// </para>
    /// <para></para>
    /// </summary>
    public static partial class ILinksExtensions
    {
        #region Ensure
        // TODO: May be move to EnsureExtensions or make it both there and here

        /// <summary>
        /// <para>
        /// Ensures the link exists using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// <para>sequence[{i}]</para>
        /// <para></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkExists<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction) where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            restriction ??= Array.Empty<TLinkAddress>();
            for (var i = 0; i < restriction.Count; i++)
            {
                if (!links.Exists(restriction[i]))
                {
                    throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], $"sequence[{i}]");
                }
            }
        }

        /// <summary>
        /// <para>
        /// Ensures the inner reference exists using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="reference">
        /// <para>The reference.</para>
        /// <para></para>
        /// </param>
        /// <param name="argumentName">
        /// <para>The argument name.</para>
        /// <para></para>
        /// </param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// <para></para>
        /// <para></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureInnerReferenceExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress reference, string argumentName) where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.Constants.IsInternalReference(reference) && !links.Exists(reference))
            {
                throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(reference, argumentName);
            }
        }

        /// <summary>
        /// <para>
        /// Ensures the inner reference exists using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <param name="argumentName">
        /// <para>The argument name.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureInnerReferenceExists<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction, string argumentName) where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            restriction ??= Array.Empty<TLinkAddress>();
            for (int i = 0; i < restriction.Count; i++)
            {
                links.EnsureInnerReferenceExists(restriction[i], argumentName);
            }
        }

        /// <summary>
        /// <para>
        /// Ensures the link is any or exists using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// <para>sequence[{i}]</para>
        /// <para></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsAnyOrExists<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction) where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            restriction ??= Array.Empty<TLinkAddress>();
            var any = links.Constants.Any;
            for (var i = 0; i < restriction.Count; i++)
            {
                if (restriction[i] != any && !links.Exists(restriction[i]))
                {
                    throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], $"sequence[{i}]");
                }
            }
        }

        /// <summary>
        /// <para>
        /// Ensures the link is any or exists using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="argumentName">
        /// <para>The argument name.</para>
        /// <para></para>
        /// </param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// <para></para>
        /// <para></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsAnyOrExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, string argumentName) where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (link != links.Constants.Any && !links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
            }
        }

        /// <summary>
        /// <para>
        /// Ensures the link is itself or exists using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="argumentName">
        /// <para>The argument name.</para>
        /// <para></para>
        /// </param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// <para></para>
        /// <para></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsItselfOrExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, string argumentName) where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (link != links.Constants.Itself && !links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
            }
        }

        /// <summary>Throws when a link with the specified source and target exists.</summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureDoesNotExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target) where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.Exists(source, target))
            {
                throw new LinkWithSameValueAlreadyExistsException();
            }
        }

        /// <summary>Throws when another link uses the specified link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureNoUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link) where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.HasUsages(link))
            {
                throw new ArgumentLinkHasDependenciesException<TLinkAddress>(link);
            }
        }

        /// <summary>Creates all requested internal link addresses, retaining existing links.</summary>
        /// <param name="addresses">The addresses.</param>
        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCreated<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] addresses) where TLinkAddress : IUnsignedNumber<TLinkAddress> { links.EnsureCreated(links.Create, addresses); }

        /// <summary>Creates all requested internal link addresses, retaining existing links.</summary>
        /// <param name="addresses">The addresses.</param>
        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsurePointsCreated<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] addresses) where TLinkAddress : IUnsignedNumber<TLinkAddress> { links.EnsureCreated(links.CreatePoint, addresses); }

        /// <summary>Creates all requested internal link addresses, retaining existing links.</summary>
        /// <param name="addresses">The addresses.</param>
        /// <param name="creator">The creator.</param>
        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCreated<TLinkAddress>(this ILinks<TLinkAddress> links, Func<TLinkAddress> creator, params TLinkAddress[] addresses) where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var remaining = new HashSet<TLinkAddress>(addresses.Where(address =>
                address != links.Constants.Null &&
                Comparer<TLinkAddress>.Default.Compare(address, links.Constants.InternalReferencesRange.Maximum) <= 0 &&
                !links.Exists(address)));
            var requested = new HashSet<TLinkAddress>(remaining);
            var created = new HashSet<TLinkAddress>();
            // Keep intermediate allocations until every requested address exists;
            // deleting one early could make the allocator return it repeatedly.
            while (remaining.Count > 0)
            {
                var address = creator();
                if (address == links.Constants.Null || !created.Add(address))
                {
                    throw new InvalidOperationException("The creator did not allocate a new link.");
                }
                remaining.Remove(address);
            }
            foreach (var address in created.Where(address => !requested.Contains(address)))
            {
                links.Delete(address, null);
            }
        }

        #endregion
    }
}
