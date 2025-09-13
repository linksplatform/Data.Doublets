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

// XML documentation has been added for all public members

namespace Platform.Data.Doublets
{
    /// <summary>
    /// Provides extension methods for the <see cref="ILinks{TLinkAddress}"/> interface,
    /// offering additional functionality for link operations, querying, validation,
    /// and data manipulation in doublets data structures.
    /// </summary>
    public static class ILinksExtensions
    {
        /// <summary>
        /// Performs a specified number of random link creation operations on the links storage.
        /// This method creates links with randomly selected source and target addresses
        /// from the existing links in the storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="amountOfCreations">The number of random links to create.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunRandomCreations<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress amountOfCreations)  where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var random = RandomHelpers.Default;
            for (var i = TLinkAddress.Zero; i < amountOfCreations; i++)
            {
                var linksAddressRange = new Range<ulong>(ulong.CreateTruncating(TLinkAddress.Zero), ulong.CreateTruncating(links.Count()));
                var source = TLinkAddress.CreateTruncating(random.NextUInt64(linksAddressRange));
                var target = TLinkAddress.CreateTruncating(random.NextUInt64(linksAddressRange));
                links.GetOrCreate(source, target);
            }
        }

        /// <summary>
        /// Performs a specified number of random search operations on the links storage.
        /// This method searches for links with randomly selected source and target addresses
        /// from the existing links in the storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="amountOfSearches">The number of random searches to perform.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunRandomSearches<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress amountOfSearches)  where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var random = RandomHelpers.Default;
            for (var i = TLinkAddress.Zero; i < amountOfSearches; i++)
            {
                var linksAddressRange = new Range<ulong>(ulong.CreateTruncating(TLinkAddress.Zero), ulong.CreateTruncating(links.Count()));
                var source = TLinkAddress.CreateTruncating(random.NextUInt64(linksAddressRange));
                var target = TLinkAddress.CreateTruncating(random.NextUInt64(linksAddressRange));
                links.SearchOrDefault(source, target);
            }
        }

        /// <summary>
        /// Performs a specified number of random link deletion operations on the links storage.
        /// This method selects and deletes random links from the storage, ensuring that
        /// the total number of links doesn't fall below a calculated minimum.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="amountOfDeletions">The number of random links to delete.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunRandomDeletions<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress amountOfDeletions)  where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var random = RandomHelpers.Default;
            var linksCount = links.Count();
            var min = amountOfDeletions > linksCount ? TLinkAddress.Zero : linksCount - amountOfDeletions;
            for (var i = TLinkAddress.Zero; i < amountOfDeletions; i++)
            {
                linksCount = links.Count();
                if (linksCount <= min)
                {
                    break;
                }
                var linksAddressRange = new Range<ulong>(ulong.CreateTruncating(min), ulong.CreateTruncating(linksCount));
                var link = (random.NextUInt64(linksAddressRange));
                links.Delete<TLinkAddress>(TLinkAddress.CreateTruncating(link));
            }
        }

        /// <summary>
        /// Deletes a specific link from the storage, ensuring its values are reset
        /// before deletion if the link exists.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="linkToDelete">The address of the link to delete.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The result of the delete operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Delete<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkToDelete, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.Exists(linkToDelete))
            {
                links.EnforceResetValues(linkToDelete, handler);
            }
            return links.Delete(new LinkAddress<TLinkAddress>(linkToDelete), handler);
        }

        /// <summary>
        /// Deletes all links from the storage by iterating through all links
        /// and deleting them one by one in descending order.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to clear.</param>
        /// <remarks>
        /// TODO: There might be a simpler way to do this
        /// (e.g., just delete the file or resize it to remove all content)
        /// For example, through _header->AllocatedLinks in ResizableDirectMemoryLinks
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteAll<TLinkAddress>(this ILinks<TLinkAddress> links)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var comparer = Comparer<TLinkAddress>.Default;
            for (var i = links.Count(); comparer.Compare(i, default) > 0; i = --i)
            {
                links.Delete(i);
                if (links.Count() !=  --i)
                {
                    i = links.Count();
                }
            }
        }

        /// <summary>
        /// Gets the address of the first link found in the storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to search in.</param>
        /// <returns>The address of the first link found.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the storage contains no links.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no links were found during the search process.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress First<TLinkAddress>(this ILinks<TLinkAddress> links)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            TLinkAddress firstLink = default;
            if (links.Count() ==  default)
            {
                throw new InvalidOperationException("В хранилище нет связей.");
            }
            links.Each(new Link<TLinkAddress>(links.Constants.Any, links.Constants.Any, links.Constants.Any), link =>
            {
                firstLink = link[links.Constants.IndexPart];
                return links.Constants.Break;
            });
            if (firstLink ==  default)
            {
                throw new InvalidOperationException("В процессе поиска по хранилищу не было найдено связей.");
            }
            return firstLink;
        }

        /// <summary>
        /// Returns a single link that matches the specified query, or null if no link is found
        /// or if multiple links match the query.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to search in.</param>
        /// <param name="query">The query criteria for finding the link.</param>
        /// <returns>The single matching link, or null if zero or multiple links match.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLinkAddress>? SingleOrDefault<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? query)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            IList<TLinkAddress>? result = null;
            var count = 0;
            var constants = links.Constants;
            var @continue = constants.Continue;
            var @break = constants.Break;
            links.Each(query, linkHandler);
            return result;
            
            TLinkAddress linkHandler(IList<TLinkAddress>? link)
            {
                if (count == 0)
                {
                    result = link;
                    count++;
                    return @continue;
                }
                else
                {
                    result = null;
                    return @break;
                }
            }
        }

        #region Paths

        /// <summary>
        /// Checks whether a specific path exists in the links structure by following
        /// the connections from one link to another through source and target relationships.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to search in.</param>
        /// <param name="path">An array of link addresses representing the path to check.</param>
        /// <returns>True if the path exists, false otherwise.</returns>
        /// <remarks>
        /// TODO: How can what's below be correct? It's probably practically not applicable.
        /// It was supposed that it would be possible to convert the Stack formed during
        /// SequenceWalker traversal into a specific path from Source, Target to the link,
        /// but this is not always the case.
        /// TODO: Perhaps we need a method that specifically throws exceptions (EnsurePathExists)
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckPathExistance<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] path)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var current = path[0];
            //EnsureLinkExists(current, "path");
            if (!links.Exists(current))
            {
                return false;
            }
            var constants = links.Constants;
            for (var i = 1; i < path.Length; i++)
            {
                var next = path[i];
                var values = links.GetLink(current);
                var source = links.GetSource(values);
                var target = links.GetTarget(values);
                if (source ==  target && source ==  next)
                {
                    //throw new InvalidOperationException(string.Format("Невозможно выбрать путь, так как и Source и Target совпадают с элементом пути {0}.", next));
                    return false;
                }
                if (next !=  source && next !=  target)
                {
                    //throw new InvalidOperationException(string.Format("Невозможно продолжить путь через элемент пути {0}", next));
                    return false;
                }
                current = next;
            }
            return true;
        }

        /// <summary>
        /// Navigates through the links structure starting from a root link and following
        /// a path defined by integer keys, where each key represents an index into the link's components.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to navigate through.</param>
        /// <param name="root">The starting link address.</param>
        /// <param name="path">An array of integer indices defining the navigation path.</param>
        /// <returns>The final link address reached by following the path.</returns>
        /// <remarks>
        /// May require an additional stack for PathElement's when using SequenceWalker.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetByKeys<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress root, params int[] path)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            links.EnsureLinkExists(root, "root");
            var currentLink = root;
            for (var i = 0; i < path.Length; i++)
            {
                currentLink = links.GetLink(currentLink)[path[i]];
            }
            return currentLink;
        }

        /// <summary>
        /// Retrieves an element from a square matrix sequence stored as links
        /// by following a binary path determined by the element's index.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage containing the sequence.</param>
        /// <param name="root">The root link of the sequence.</param>
        /// <param name="size">The size of the sequence (must be a power of 2).</param>
        /// <param name="index">The index of the element to retrieve.</param>
        /// <returns>The link address of the element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the size is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetSquareMatrixSequenceElementByIndex<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress root, TLinkAddress size, TLinkAddress index)  where TLinkAddress : IUnsignedNumber<TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var constants = links.Constants;
            var source = constants.SourcePart;
            var target = constants.TargetPart;
            if (!Platform.Numbers.Math.IsPowerOfTwo(size))
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Sequences with sizes other than powers of two are not supported.");
            }
            var path = new BitArray(BitConverter.GetBytes(ulong.CreateTruncating(index)));
            var length = Bit.GetLowestPosition(ulong.CreateTruncating(size));
            links.EnsureLinkExists(root, "root");
            var currentLink = root;
            for (var i = length - 1; i >= 0; i--)
            {
                currentLink = links.GetLink(currentLink)[path[i] ? target : source];
            }
            return currentLink;
        }

        #endregion

        /// <summary>
        /// Returns the index (address) of the specified link.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <param name="link">The link represented as a list containing its address and contents.</param>
        /// <returns>The index of the specified link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetIndex<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return link[links.Constants.IndexPart];}

        /// <summary>
        /// Returns the source address of the specified link.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <param name="link">The address of the link.</param>
        /// <returns>The source address of the specified link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetSource<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.GetLink(link)[links.Constants.SourcePart];}

        /// <summary>
        /// Returns the source address of the specified link.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <param name="link">The link represented as a list containing its address and contents.</param>
        /// <returns>The source address of the specified link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetSource<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return link[links.Constants.SourcePart];}

        /// <summary>
        /// Returns the target address of the specified link.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <param name="link">The address of the link.</param>
        /// <returns>The target address of the specified link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetTarget<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.GetLink(link)[links.Constants.TargetPart];}

        /// <summary>
        /// Returns the target address of the specified link.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage.</param>
        /// <param name="link">The link represented as a list containing its address and contents.</param>
        /// <returns>The target address of the specified link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetTarget<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return link[links.Constants.TargetPart];}

        /// <summary>
        /// Retrieves all links that match the specified restriction criteria.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to search in.</param>
        /// <param name="restriction">The search criteria for filtering links.</param>
        /// <returns>A list of all links matching the restriction criteria.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<IList<TLinkAddress>?> All<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var allLinks = new List<IList<TLinkAddress>?>();
            var filler = new ListFiller<IList<TLinkAddress>?, TLinkAddress>(allLinks, links.Constants.Continue);
            links.Each(filler.AddAndReturnConstant, restriction);
            return allLinks;
        }

        /// <summary>
        /// Retrieves the addresses of all links that match the specified restriction criteria.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to search in.</param>
        /// <param name="restriction">The search criteria for filtering links.</param>
        /// <returns>A list of addresses of all links matching the restriction criteria.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLinkAddress>? AllIndices<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var allIndices = new List<TLinkAddress>();
            var filler = new ListFiller<TLinkAddress, TLinkAddress>(allIndices, links.Constants.Continue);
            links.Each(filler.AddFirstAndReturnConstant, restriction);
            return allIndices;
        }

        /// <summary>
        /// Determines whether a link with the specified source and target exists in the links storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to search in.</param>
        /// <param name="source">The source address of the link.</param>
        /// <param name="target">The target address of the link.</param>
        /// <returns>True if a link with the specified source and target exists, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return Comparer<TLinkAddress>.Default.Compare(links.Count(links.Constants.Any, source, target), default) > 0;}

        #region Ensure
        // TODO: May be move to EnsureExtensions or make it both there and here

        /// <summary>
        /// Validates that all links in the specified collection exist in the storage.
        /// Throws an exception if any link doesn't exist.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to validate against.</param>
        /// <param name="restriction">The collection of link addresses to validate.</param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// Thrown when a link in the collection doesn't exist.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkExists<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            for (var i = 0; i < restriction.Count; i++)
            {
                if (!links.Exists(restriction[i]))
                {
                    throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], $"sequence[{i}]");
                }
            }
        }

        /// <summary>
        /// Validates that an internal reference exists in the storage if it's an internal reference.
        /// External references are not validated.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to validate against.</param>
        /// <param name="reference">The reference address to validate.</param>
        /// <param name="argumentName">The name of the argument for error reporting.</param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// Thrown when an internal reference doesn't exist in the storage.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureInnerReferenceExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress reference, string argumentName)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.Constants.IsInternalReference(reference) && !links.Exists(reference))
            {
                throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(reference, argumentName);
            }
        }

        /// <summary>
        /// Validates that all internal references in the specified collection exist in the storage.
        /// External references are not validated.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to validate against.</param>
        /// <param name="restriction">The collection of reference addresses to validate.</param>
        /// <param name="argumentName">The name of the argument for error reporting.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureInnerReferenceExists<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction, string argumentName)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            for (int i = 0; i < restriction.Count; i++)
            {
                links.EnsureInnerReferenceExists(restriction[i], argumentName);
            }
        }

        /// <summary>
        /// Validates that each link in the collection either represents "Any" or exists in the storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to validate against.</param>
        /// <param name="restriction">The collection of link addresses to validate.</param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// Thrown when a link is not "Any" and doesn't exist in the storage.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsAnyOrExists<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var any = links.Constants.Any;
            for (var i = 0; i < restriction.Count; i++)
            {
                if (restriction[i] !=  any && !links.Exists(restriction[i]))
                {
                    throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], $"sequence[{i}]");
                }
            }
        }

        /// <summary>
        /// Validates that a link either represents "Any" or exists in the storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to validate against.</param>
        /// <param name="link">The link address to validate.</param>
        /// <param name="argumentName">The name of the argument for error reporting.</param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// Thrown when the link is not "Any" and doesn't exist in the storage.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsAnyOrExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, string argumentName)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (link !=  links.Constants.Any && !links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
            }
        }

        /// <summary>
        /// Validates that a link either represents "Itself" or exists in the storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to validate against.</param>
        /// <param name="link">The link address to validate.</param>
        /// <param name="argumentName">The name of the argument for error reporting.</param>
        /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
        /// Thrown when the link is not "Itself" and doesn't exist in the storage.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsItselfOrExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, string argumentName)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (link !=  links.Constants.Itself && !links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
            }
        }

        /// <summary>
        /// Validates that a link with the specified source and target does not exist in the storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to validate against.</param>
        /// <param name="source">The source address of the link.</param>
        /// <param name="target">The target address of the link.</param>
        /// <exception cref="LinkWithSameValueAlreadyExistsException">
        /// Thrown when a link with the same source and target already exists.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureDoesNotExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.Exists(source, target))
            {
                throw new LinkWithSameValueAlreadyExistsException();
            }
        }

        /// <summary>
        /// Validates that the specified link has no usages (references) from other links.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to validate against.</param>
        /// <param name="link">The link address to check for usages.</param>
        /// <exception cref="ArgumentLinkHasDependenciesException{TLinkAddress}">
        /// Thrown when the link has dependencies (usages by other links).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureNoUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.HasUsages(link))
            {
                throw new ArgumentLinkHasDependenciesException<TLinkAddress>(link);
            }
        }

        /// <summary>
        /// Ensures that links with the specified addresses exist in the storage, creating them if necessary.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="addresses">The addresses of links to ensure exist.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCreated<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] addresses)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.EnsureCreated(links.Create, addresses);}

        /// <summary>
        /// Ensures that point links (self-referencing links) with the specified addresses exist in the storage, creating them if necessary.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="addresses">The addresses of point links to ensure exist.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsurePointsCreated<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] addresses)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.EnsureCreated(links.CreatePoint, addresses);}

        /// <summary>
        /// Ensures that links with the specified addresses exist in the storage, using a custom creator function if necessary.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="creator">The function used to create new links.</param>
        /// <param name="addresses">The addresses of links to ensure exist.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCreated<TLinkAddress>(this ILinks<TLinkAddress> links, Func<TLinkAddress> creator, params TLinkAddress[] addresses)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var addressToUInt64Converter = CheckedConverter<TLinkAddress, TLinkAddress>.Default;
            var uInt64ToAddressConverter = CheckedConverter<TLinkAddress, TLinkAddress>.Default;
            var nonExistentAddresses = new HashSet<TLinkAddress>(addresses.Where(x => !links.Exists(x)));
            if (nonExistentAddresses.Count > 0)
            {
                var max = nonExistentAddresses.Max();
                max = uInt64ToAddressConverter.Convert(TLinkAddress.CreateTruncating(System.Math.Min(ulong.CreateTruncating(max), ulong.CreateTruncating(links.Constants.InternalReferencesRange.Maximum))));
                var createdLinks = new List<TLinkAddress>();
                TLinkAddress createdLink = creator();
                while (createdLink !=  max)
                {
                    createdLinks.Add(createdLink);
                }
                for (var i = 0; i < createdLinks.Count; i++)
                {
                    if (!nonExistentAddresses.Contains(createdLinks[i]))
                    {
                        links.Delete(createdLinks[i]);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Counts the number of times the specified link is used as a source or target by other links.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to search in.</param>
        /// <param name="link">The link address to count usages for.</param>
        /// <returns>The total number of usages of the specified link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress CountUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var values = links.GetLink(link);
            TLinkAddress usagesAsSource = links.Count(new Link<TLinkAddress>(constants.Any, link, constants.Any));
            if (links.GetSource(values) ==  link)
            {
                --usagesAsSource;
            }
            TLinkAddress usagesAsTarget = links.Count(new Link<TLinkAddress>(constants.Any, constants.Any, link));
            if (links.GetTarget(values) ==  link)
            {
                --usagesAsTarget;
            }
            return usagesAsSource + usagesAsTarget;
        }

        /// <summary>
        /// Determines whether the specified link is used as a source or target by any other links.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to search in.</param>
        /// <param name="link">The link address to check for usages.</param>
        /// <returns>True if the link has usages, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return Comparer<TLinkAddress>.Default.Compare(links.CountUsages(link), default) > 0;}

        /// <summary>
        /// Determines whether the specified link has the given source and target addresses.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage containing the link.</param>
        /// <param name="link">The address of the link to check.</param>
        /// <param name="source">The expected source address.</param>
        /// <param name="target">The expected target address.</param>
        /// <returns>True if the link has the specified source and target, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var values = links.GetLink(link);
            return links.GetSource(values) ==  source && links.GetTarget(values) ==  target;
        }

        /// <summary>
        /// Searches for a link with the specified source and target addresses.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to search in.</param>
        /// <param name="source">The source address of the link to find.</param>
        /// <param name="target">The target address of the link to find.</param>
        /// <returns>The address of the found link, or default if no link is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress SearchOrDefault<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var contants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(contants.Continue, contants.Break, default);
            links.Each(setter.SetFirstAndReturnFalse, contants.Any, source, target);
            return setter.Result;
        }

        /// <summary>
        /// Creates a point link (a link that points to itself) in the storage.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to create the point in.</param>
        /// <returns>The address of the created point link.</returns>
        public static TLinkAddress CreatePoint<TLinkAddress>(this ILinks<TLinkAddress> links)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.CreatePoint(setter.SetFirstFromNonNullSecondListAndReturnTrue);
            return setter.Result;
        }

        /// <summary>
        /// Creates a point link (a link that points to itself) in the storage with a custom write handler.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to create the point in.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The address of the created point link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress CreatePoint<TLinkAddress>(this ILinks<TLinkAddress> links, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            TLinkAddress link = default;
            TLinkAddress HandlerWrapper(IList<TLinkAddress>? before, IList<TLinkAddress>? after)
            {
                link = links.GetIndex(after);
                return handlerState.Handle(before, after);;
            }
            handlerState.Apply(links.Create(null, HandlerWrapper));
            handlerState.Apply(links.Update(link, link, link, HandlerWrapper));
            return handlerState.Result;
        }

        /// <summary>
        /// Creates a new link and immediately updates it with the specified source and target addresses.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="source">The source address for the new link.</param>
        /// <param name="target">The target address for the new link.</param>
        /// <returns>The address of the created and updated link.</returns>
        public static TLinkAddress CreateAndUpdate<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.CreateAndUpdate(source, target, setter.SetFirstFromNonNullSecondListAndReturnTrue);
            return setter.Result;
        }


        /// <summary>
        /// Creates a new link and immediately updates it with the specified source and target addresses,
        /// using a custom write handler.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="source">The source address for the new link.</param>
        /// <param name="target">The target address for the new link.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The address of the created and updated link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress CreateAndUpdate<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            TLinkAddress createdLink = default;
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            handlerState.Apply(links.Create(null, (before, after) =>
            {
                createdLink = links.GetIndex(after);
                return handlerState.Handle(before, after);;
            }));
            handlerState.Apply(links.Update(createdLink, source, target, handler));
            return handlerState.Result;
        }

        /// <summary>
        /// Updates the specified link with new source and target addresses.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="link">The address of the link to update.</param>
        /// <param name="newSource">The new source address for the link.</param>
        /// <param name="newTarget">The new target address for the link.</param>
        /// <returns>The address of the updated link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Update(new LinkAddress<TLinkAddress>(link), new Link<TLinkAddress>(link, newSource, newTarget));}

        /// <summary>
        /// Updates links based on the provided restriction parameters.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="restriction">The restriction parameters for the update operation.</param>
        /// <returns>The result of the update operation.</returns>
        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Update((IList<TLinkAddress>)restriction);}

        /// <summary>
        /// Updates links based on the provided restriction parameters with a custom write handler.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <param name="restriction">The restriction parameters for the update operation.</param>
        /// <returns>The result of the update operation.</returns>
        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, WriteHandler<TLinkAddress>? handler, params TLinkAddress[] restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Update(restriction, handler);}

        /// <summary>
        /// Updates links based on the provided restriction list.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="restriction">The restriction list for the update operation.</param>
        /// <returns>The result of the update operation.</returns>
        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.Update(restriction, setter.SetFirstFromNonNullSecondListAndReturnTrue);
            return setter.Result;
        }


        /// <summary>
        /// Updates links based on the provided restrictions with a custom write handler.
        /// The behavior depends on the number of elements in the restriction list.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="restriction">Restrictions on link content. Each restriction can have values: Constants.Null - the 0th link denoting a reference to void, Itself - requirement to set a reference to itself, 1..∞ specific address of another link.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The address of the updated link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            return restriction.Count switch
            {
                2 => links.MergeAndDelete(restriction[0], restriction[1], handler),
                4 => links.UpdateOrCreateOrGet(restriction[0], restriction[1], restriction[2], restriction[3], handler),
                _ => links.Update(restriction[0], restriction[1], restriction[2], handler)
            };
        }

        /// <summary>
        /// Updates the specified link with new source and target addresses, using a custom write handler.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="link">The address of the link to update.</param>
        /// <param name="newSource">The new source address for the link.</param>
        /// <param name="newTarget">The new target address for the link.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The address of the updated link.</returns>
        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Update(new LinkAddress<TLinkAddress>(link), new Link<TLinkAddress>(link, newSource, newTarget), handler);}

        /// <summary>
        /// Resolves a constant as a self-reference by replacing occurrences of the constant
        /// in the substitution with the link's own address.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="constant">The constant value to replace with self-reference.</param>
        /// <param name="restriction">The original restriction criteria.</param>
        /// <param name="substitution">The substitution link to process.</param>
        /// <returns>A new link with the constant replaced by self-reference.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLinkAddress>? ResolveConstantAsSelfReference<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress constant, IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var restrictionIndex = links.GetIndex(restriction);
            var substitutionIndex = links.GetIndex(substitution);
            if (substitutionIndex ==  default)
            {
                substitutionIndex = restrictionIndex;
            }
            var source = links.GetSource(substitution);
            var target = links.GetTarget(substitution);
            source = source ==  constant ? substitutionIndex : source;
            target = target ==  constant ? substitutionIndex : target;
            return new Link<TLinkAddress>(substitutionIndex, source, target);
        }

        /// <summary>
        /// Creates a link with the specified source and target if it doesn't exist,
        /// or returns the address of the existing link.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="source">The source address for the link.</param>
        /// <param name="target">The target address for the link.</param>
        /// <returns>The address of the existing or newly created link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetOrCreate<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var link = links.SearchOrDefault(source, target);
            if (EqualityComparer<TLinkAddress>.Default.Equals(link, default))
            {
                link = links.CreateAndUpdate(source, target);
            }
            return link;
        }

        /// <summary>
        /// Updates an existing link, creates a new one, or gets an existing link based on the provided parameters.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="source">The source address to search for.</param>
        /// <param name="target">The target address to search for.</param>
        /// <param name="newSource">The new source address for the link.</param>
        /// <param name="newTarget">The new target address for the link.</param>
        /// <returns>The address of the updated, created, or existing link.</returns>
        public static TLinkAddress UpdateOrCreateOrGet<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target, TLinkAddress newSource, TLinkAddress newTarget)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.UpdateOrCreateOrGet(source, target, newSource, newTarget, setter.SetFirstFromNonNullSecondListAndReturnTrue);
            return setter.Result;
        }

        /// <summary>
        /// Updates an existing link with the specified source and target, creates a new link if it doesn't exist,
        /// or returns the existing link if the new values match the current ones.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="source">The source address of the link to find.</param>
        /// <param name="target">The target address of the link to find.</param>
        /// <param name="newSource">The new source address for the link.</param>
        /// <param name="newTarget">The new target address for the link.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The address of the updated, created, or existing link.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress UpdateOrCreateOrGet<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target, TLinkAddress newSource, TLinkAddress newTarget, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var link = links.SearchOrDefault(source, target);
            if (link ==  default)
            {
                return links.CreateAndUpdate(newSource, newTarget, handler);
            }
            if (newSource ==  source && newTarget ==  target)
            {
                var linkStruct = new Link<TLinkAddress>(link, source, target); 
                return link;
            }
            return links.Update(link, newSource, newTarget, handler);
        }

        /// <summary>
        /// Deletes a link with the specified source and target addresses if it exists.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="source">The source address of the link to delete.</param>
        /// <param name="target">The target address of the link to delete.</param>
        /// <returns>The address of the deleted link, or default if no link was found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress DeleteIfExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var link = links.SearchOrDefault(source, target);
            if (!EqualityComparer<TLinkAddress>.Default.Equals(link, default))
            {
                links.Delete(link);
                return link;
            }
            return default;
        }

        /// <summary>
        /// Deletes multiple links specified in the provided list.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="deletedLinks">The list of link addresses to delete.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteMany<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? deletedLinks)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            for (int i = 0; i < deletedLinks.Count; i++)
            {
                links.Delete(deletedLinks[i]);
            }
        }

        /// <summary>
        /// Deletes all links that use the specified link as a source or target.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="linkIndex">The address of the link whose usages should be deleted.</param>
        public static void DeleteAllUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.DeleteAllUsages(linkIndex, null);}

        /// <summary>
        /// Deletes all links that use the specified link as a source or target, with a custom write handler.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="linkIndex">The address of the link whose usages should be deleted.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The result of the delete operation.</returns>
        /// <remarks>
        /// Before execution of this method ensure that deleted link is detached (all values - source and target are reset to null) or it might enter into infinite recursion.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress DeleteAllUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var any = constants.Any;
            var usagesAsSourceQuery = new Link<TLinkAddress>(any, linkIndex, any);
            var usagesAsTargetQuery = new Link<TLinkAddress>(any, any, linkIndex);
            var usages = new List<IList<TLinkAddress>?>();
            var usagesFiller = new ListFiller<IList<TLinkAddress>?, TLinkAddress>(usages, constants.Continue);
            links.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
            links.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            foreach (var usage in usages)
            {
                if (links.GetIndex(usage) ==  linkIndex || !links.Exists(links.GetIndex(usage)))
                {
                    continue;
                }
                handlerState.Apply(links.Delete(links.GetIndex(usage), handlerState.Handler));
            }
            return handlerState.Result;
        }

        /// <summary>
        /// Deletes all links that match the specified query criteria.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="query">The query criteria to match links for deletion.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteByQuery<TLinkAddress>(this ILinks<TLinkAddress> links, Link<TLinkAddress> query)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var queryResult = new List<TLinkAddress>();
            var queryResultFiller = new ListFiller<TLinkAddress, TLinkAddress>(queryResult, links.Constants.Continue);
            links.Each(queryResultFiller.AddFirstAndReturnConstant, query);
            foreach (var link in queryResult)
            {
                links.Delete(link);
            }
        }

        // TODO: Move to Platform.Data
        /// <summary>
        /// Determines whether all values (source and target) of the specified link are reset to null.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to check in.</param>
        /// <param name="linkIndex">The address of the link to check.</param>
        /// <returns>True if all values are reset to null, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreValuesReset<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var nullConstant = links.Constants.Null;
            var link = links.GetLink(linkIndex);
            for (int i = 1; i < link.Count; i++)
            {
                if (link[i] !=  nullConstant)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Resets all values (source and target) of the specified link to null.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="linkIndex">The address of the link to reset.</param>
        public static void ResetValues<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.ResetValues(linkIndex, null);}

        /// <summary>
        /// Resets all values (source and target) of the specified link to null with a custom write handler.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="linkIndex">The address of the link to reset.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The result of the reset operation.</returns>
        /// <remarks>
        /// TODO: Create a universal version of this method in Platform.Data (with using of for loop)
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress ResetValues<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var nullConstant = links.Constants.Null;
            var updateRequest = new Link<TLinkAddress>(linkIndex, nullConstant, nullConstant);
            return links.Update(updateRequest, handler);
        }

        /// <summary>
        /// Ensures that all values (source and target) of the specified link are reset to null,
        /// performing the reset only if they are not already reset.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="linkIndex">The address of the link to check and potentially reset.</param>
        public static void EnforceResetValues<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.EnforceResetValues(linkIndex, null);}


        /// <summary>
        /// Ensures that all values (source and target) of the specified link are reset to null,
        /// performing the reset only if they are not already reset, with a custom write handler.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="linkIndex">The address of the link to check and potentially reset.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The result of the reset operation, or Continue if no reset was needed.</returns>
        /// <remarks>
        /// TODO: Create a universal version of this method in Platform.Data (with using of for loop)
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress EnforceResetValues<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (!links.AreValuesReset(linkIndex))
            {
                return links.ResetValues(linkIndex, handler);
            }
            return links.Constants.Continue;
        }

        /// <summary>
        /// Merges all usages of an old link to point to a new link instead.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="oldLinkIndex">The address of the old link whose usages should be transferred.</param>
        /// <param name="newLinkIndex">The address of the new link that should receive the usages.</param>
        public static void MergeUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.MergeUsages(oldLinkIndex, newLinkIndex, null);}

        /// <summary>
        /// Merges two usage graphs by moving all children of the old link to become children of the new link.
        /// All links that reference the old link will be updated to reference the new link instead.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="oldLinkIndex">The address of the old link whose usages should be transferred.</param>
        /// <param name="newLinkIndex">The address of the new link that should receive the usages.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The result of the merge operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress MergeUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (oldLinkIndex ==  newLinkIndex)
            {
                return newLinkIndex;
            }
            var constants = links.Constants;
            var usagesAsSource = links.All(new Link<TLinkAddress>(constants.Any, oldLinkIndex, constants.Any));
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            for (var i = 0; i < usagesAsSource.Count; i++)
            {
                var usageAsSource = usagesAsSource[i];
                if (links.GetIndex(usageAsSource) ==  oldLinkIndex)
                {
                    continue;
                }
                var restriction = new LinkAddress<TLinkAddress>(links.GetIndex(usageAsSource));
                var substitution = new Link<TLinkAddress>(newLinkIndex, links.GetTarget(usageAsSource));
                handlerState.Apply(links.Update(restriction, substitution, handlerState.Handler));
            }
            var usagesAsTarget = links.All(new Link<TLinkAddress>(constants.Any, constants.Any, oldLinkIndex));
            for (var i = 0; i < usagesAsTarget.Count; i++)
            {
                var usageAsTarget = usagesAsTarget[i];
                if (links.GetIndex(usageAsTarget) ==  oldLinkIndex)
                {
                    continue;
                }
                var restriction = links.GetLink(links.GetIndex(usageAsTarget));
                var substitution = new Link<TLinkAddress>(links.GetTarget(usageAsTarget), newLinkIndex);
                handlerState.Apply(links.Update(restriction, substitution, handlerState.Handler));
            }
            return handlerState.Result;
        }

        /// <summary>
        /// Merges the usages of an old link to a new link and then deletes the old link.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="oldLinkIndex">The address of the old link to merge and delete.</param>
        /// <param name="newLinkIndex">The address of the new link to receive the usages.</param>
        /// <returns>The address of the new link.</returns>
        public static TLinkAddress MergeAndDelete<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (oldLinkIndex !=  newLinkIndex)
            {
                links.MergeUsages(oldLinkIndex, newLinkIndex);
                links.Delete(oldLinkIndex);
            }
            return newLinkIndex;
        }

        /// <summary>
        /// Replaces one link with another by merging usages and then deleting the old link.
        /// The replaced link is deleted, and its children are updated to reference the new link or deleted.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="oldLinkIndex">The address of the old link to merge and delete.</param>
        /// <param name="newLinkIndex">The address of the new link to receive the usages.</param>
        /// <param name="handler">Optional write handler for the operation.</param>
        /// <returns>The result of the merge and delete operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress MergeAndDelete<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            if (oldLinkIndex !=  newLinkIndex)
            {
                handlerState.Apply(links.MergeUsages(oldLinkIndex, newLinkIndex, handlerState.Handler));
                handlerState.Apply(links.Delete(oldLinkIndex, handlerState.Handler));
            }
            return handlerState.Result;
        }

        /// <summary>
        /// <para>
        /// Decorates the with automatic uniqueness and usages resolution using the specified links.
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
        /// <returns>
        /// <para>The links.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILinks<TLinkAddress> DecorateWithAutomaticUniquenessAndUsagesResolution<TLinkAddress>(this ILinks<TLinkAddress> links)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            links = new LinksCascadeUsagesResolver<TLinkAddress>(links);
            links = new NonNullContentsLinkDeletionResolver<TLinkAddress>(links);
            links = new LinksCascadeUniquenessAndUsagesResolver<TLinkAddress>(links);
            return links;
        }

        /// <summary>
        /// Formats a link as a string representation showing its index, source, and target.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage containing the link.</param>
        /// <param name="link">The link to format, represented as a list.</param>
        /// <returns>A string representation of the link in the format "(index: source target)".</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            return $"({links.GetIndex(link)}: {links.GetSource(link)} {links.GetTarget(link)})";
        }

        /// <summary>
        /// Formats a link as a string representation showing its index, source, and target.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage containing the link.</param>
        /// <param name="link">The address of the link to format.</param>
        /// <returns>A string representation of the link in the format "(index: source target)".</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Format(links.GetLink(link));}
        
        /// <summary>
        /// Determines whether any link in the specified sequence has the "Any" constant value.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to check against.</param>
        /// <param name="sequence">The sequence of link addresses to check.</param>
        /// <returns>True if any link in the sequence equals the "Any" constant, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AnyLinkIsAny<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] sequence) where TLinkAddress: IUnsignedNumber<TLinkAddress>
        {
            if (sequence == null)
            {
                return false;
            }
            var constants = links.Constants;
            for (var i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] == constants.Any)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Formats the structure of a link as a nested string representation, recursively showing
        /// the relationships between links while avoiding infinite loops.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage containing the structure.</param>
        /// <param name="linkIndex">The address of the root link to format.</param>
        /// <param name="isElement">A function to determine if a link should be treated as an element (terminal node).</param>
        /// <param name="renderIndex">Whether to include link indices in the output.</param>
        /// <param name="renderDebug">Whether to include debug information for visited/non-existent links.</param>
        /// <returns>A string representation of the link structure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatStructure<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex, Func<Link<TLinkAddress>, bool> isElement, bool renderIndex = false, bool renderDebug = false) where TLinkAddress: IUnsignedNumber<TLinkAddress>
        {
            var sb = new StringBuilder();
            var visited = new HashSet<TLinkAddress>();
            links.AppendStructure<TLinkAddress>(sb, visited, linkIndex, isElement, (innerSb, link) => innerSb.Append(link.Index), renderIndex, renderDebug);
            return sb.ToString();
        }

        /// <summary>
        /// Formats the structure of a link as a nested string representation with custom element formatting,
        /// recursively showing the relationships between links while avoiding infinite loops.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage containing the structure.</param>
        /// <param name="linkIndex">The address of the root link to format.</param>
        /// <param name="isElement">A function to determine if a link should be treated as an element (terminal node).</param>
        /// <param name="appendElement">A custom action to format element links.</param>
        /// <param name="renderIndex">Whether to include link indices in the output.</param>
        /// <param name="renderDebug">Whether to include debug information for visited/non-existent links.</param>
        /// <returns>A string representation of the link structure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatStructure<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex, Func<Link<TLinkAddress>, bool> isElement, Action<StringBuilder, Link<TLinkAddress>> appendElement, bool renderIndex = false, bool renderDebug = false) where TLinkAddress: IUnsignedNumber<TLinkAddress>
        {
            var sb = new StringBuilder();
            var visited = new HashSet<TLinkAddress>();
            links.AppendStructure<TLinkAddress>(sb, visited, linkIndex, isElement, appendElement, renderIndex, renderDebug);
            return sb.ToString();
        }

        /// <summary>
        /// Recursively appends the structure of a link to a StringBuilder, tracking visited links
        /// to avoid infinite loops and providing detailed formatting options.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage containing the structure.</param>
        /// <param name="sb">The StringBuilder to append the structure to.</param>
        /// <param name="visited">A set of visited link addresses to prevent infinite recursion.</param>
        /// <param name="linkIndex">The address of the link to process.</param>
        /// <param name="isElement">A function to determine if a link should be treated as an element (terminal node).</param>
        /// <param name="appendElement">A custom action to format element links.</param>
        /// <param name="renderIndex">Whether to include link indices in the output.</param>
        /// <param name="renderDebug">Whether to include debug information for visited/non-existent links.</param>
        /// <exception cref="ArgumentNullException">Thrown when sb is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendStructure<TLinkAddress>(this ILinks<TLinkAddress> links, StringBuilder sb, HashSet<TLinkAddress> visited, TLinkAddress linkIndex, Func<Link<TLinkAddress>, bool> isElement, Action<StringBuilder, Link<TLinkAddress>> appendElement, bool renderIndex = false, bool renderDebug = false) where TLinkAddress: IUnsignedNumber<TLinkAddress>
        {
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb));
            }
            if (linkIndex == links.Constants.Null || linkIndex == links.Constants.Any || linkIndex == links.Constants.Itself)
            {
                return;
            }
            if (links.Exists(linkIndex))
            {
                if (visited.Add(linkIndex))
                {
                    sb.Append('(');
                    var link = new Link<TLinkAddress>(links.GetLink(linkIndex));
                    if (renderIndex)
                    {
                        sb.Append(link.Index);
                        sb.Append(':');
                    }
                    if (link.Source == link.Index)
                    {
                        sb.Append(link.Index);
                    }
                    else
                    {
                        var source = new Link<TLinkAddress>(links.GetLink(link.Source));
                        if (isElement(source))
                        {
                            appendElement(sb, source);
                        }
                        else
                        {
                            links.AppendStructure<TLinkAddress>(sb, visited, source.Index, isElement, appendElement, renderIndex);
                        }
                    }
                    sb.Append(' ');
                    if (link.Target == link.Index)
                    {
                        sb.Append(link.Index);
                    }
                    else
                    {
                        var target = new Link<TLinkAddress>(links.GetLink(link.Target));
                        if (isElement(target))
                        {
                            appendElement(sb, target);
                        }
                        else
                        {
                            links.AppendStructure<TLinkAddress>(sb, visited, target.Index, isElement, appendElement, renderIndex);
                        }
                    }
                    sb.Append(')');
                }
                else
                {
                    if (renderDebug)
                    {
                        sb.Append('*');
                    }
                    sb.Append(linkIndex);
                }
            }
            else
            {
                if (renderDebug)
                {
                    sb.Append('~');
                }
                sb.Append(linkIndex);
            }
        }

        #region Garbage Collection

        /// <summary>
        /// Determines whether the specified link is considered garbage (has no incoming or outgoing references
        /// and is not a partial point).
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to check in.</param>
        /// <param name="link">The address of the link to check.</param>
        /// <returns>True if the link is garbage and can be safely deleted, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGarbage<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link) where  TLinkAddress : struct, IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool> 
        {
            return !links.IsPartialPoint(link) && links.Count(links.Constants.Any, link, links.Constants.Any) == TLinkAddress.Zero && links.Count(links.Constants.Any, links.Constants.Any, link) == TLinkAddress.Zero;
        }

        /// <summary>
        /// Clears garbage links starting from the specified link using the default garbage detection logic.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="link">The address of the link to start garbage collection from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearGarbage<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link) where TLinkAddress : struct, IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            ClearGarbage(links, link, links.IsGarbage);
        }
        
        /// <summary>
        /// Clears garbage links starting from the specified link using a custom garbage detection function.
        /// This method recursively processes the source and target of deleted links.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
        /// <param name="links">The links storage to operate on.</param>
        /// <param name="link">The address of the link to start garbage collection from.</param>
        /// <param name="isGarbage">A function to determine if a link should be considered garbage.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearGarbage<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, Func<TLinkAddress, bool> isGarbage) where  TLinkAddress : struct, IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool> 
        {
            if (isGarbage(link))
            {
                var contents = new Link<TLinkAddress>(links.GetLink(link));
                links.ResetValues(link);
                links.Delete(link);
                links.ClearGarbage(contents.Source, isGarbage);
                links.ClearGarbage(contents.Target, isGarbage);
            }
        }

        #endregion
    }
}
