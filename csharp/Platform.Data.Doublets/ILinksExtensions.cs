using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
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
    public static class ILinksExtensions
    {
        /// <summary>
        /// <para>
        /// Runs the random creations using the specified links.
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
        /// <param name="amountOfCreations">
        /// <para>The amount of creations.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunRandomCreations<TLinkAddress>(this ILinks<TLinkAddress> links, ulong amountOfCreations)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var random = RandomHelpers.Default;
            var addressToUInt64Converter = UncheckedConverter<TLinkAddress, ulong>.Default;
            var uInt64ToAddressConverter = UncheckedConverter<ulong, TLinkAddress>.Default;
            for (var i = 0UL; i < amountOfCreations; i++)
            {
                var linksAddressRange = new Range<ulong>(0, addressToUInt64Converter.Convert(links.Count()));
                var source = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                var target = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                links.GetOrCreate(source, target);
            }
        }

        /// <summary>
        /// <para>
        /// Runs the random searches using the specified links.
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
        /// <param name="amountOfSearches">
        /// <para>The amount of searches.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunRandomSearches<TLinkAddress>(this ILinks<TLinkAddress> links, ulong amountOfSearches)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var random = RandomHelpers.Default;
            var addressToUInt64Converter = UncheckedConverter<TLinkAddress, ulong>.Default;
            var uInt64ToAddressConverter = UncheckedConverter<ulong, TLinkAddress>.Default;
            for (var i = 0UL; i < amountOfSearches; i++)
            {
                var linksAddressRange = new Range<ulong>(0, addressToUInt64Converter.Convert(links.Count()));
                var source = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                var target = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                links.SearchOrDefault(source, target);
            }
        }

        /// <summary>
        /// <para>
        /// Runs the random deletions using the specified links.
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
        /// <param name="amountOfDeletions">
        /// <para>The amount of deletions.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunRandomDeletions<TLinkAddress>(this ILinks<TLinkAddress> links, ulong amountOfDeletions)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var random = RandomHelpers.Default;
            var addressToUInt64Converter = UncheckedConverter<TLinkAddress, ulong>.Default;
            var uInt64ToAddressConverter = UncheckedConverter<ulong, TLinkAddress>.Default;
            var linksCount = addressToUInt64Converter.Convert(links.Count());
            var min = amountOfDeletions > linksCount ? 0UL : linksCount - amountOfDeletions;
            for (var i = 0UL; i < amountOfDeletions; i++)
            {
                linksCount = addressToUInt64Converter.Convert(links.Count());
                if (linksCount <= min)
                {
                    break;
                }
                var linksAddressRange = new Range<ulong>(min, linksCount);
                var link = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                links.Delete(link);
            }
        }

        /// <summary>
        /// <para>
        /// Deletes the links.
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
        /// <param name="linkToDelete">
        /// <para>The link to delete.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Delete<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkToDelete, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.Exists(linkToDelete))
            {
                links.EnforceResetValues(linkToDelete, handler);
            }
            return links.Delete(new LinkAddress<TLinkAddress>(linkToDelete), handler);
        }

        /// <remarks>
        /// TODO: Возможно есть очень простой способ это сделать.
        /// (Например просто удалить файл, или изменить его размер таким образом,
        /// чтобы удалился весь контент)
        /// Например через _header->AllocatedLinks в ResizableDirectMemoryLinks
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
        /// <para>
        /// Firsts the links.
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
        /// <exception cref="InvalidOperationException">
        /// <para>В процессе поиска по хранилищу не было найдено связей.</para>
        /// <para></para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>В хранилище нет связей.</para>
        /// <para></para>
        /// </exception>
        /// <returns>
        /// <para>The first link.</para>
        /// <para></para>
        /// </returns>
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
        /// <para>
        /// Singles the or default using the specified links.
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
        /// <param name="query">
        /// <para>The query.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The result.</para>
        /// <para></para>
        /// </returns>
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

        /// <remarks>
        /// TODO: Как так? Как то что ниже может быть корректно?
        /// Скорее всего практически не применимо
        /// Предполагалось, что можно было конвертировать формируемый в проходе через SequenceWalker 
        /// Stack в конкретный путь из Source, Target до связи, но это не всегда так.
        /// TODO: Возможно нужен метод, который именно выбрасывает исключения (EnsurePathExists)
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

        /// <remarks>
        /// Может потребовать дополнительного стека для PathElement's при использовании SequenceWalker.
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
        /// <para>
        /// Gets the square matrix sequence element by index using the specified links.
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
        /// <param name="root">
        /// <para>The root.</para>
        /// <para></para>
        /// </param>
        /// <param name="size">
        /// <para>The size.</para>
        /// <para></para>
        /// </param>
        /// <param name="index">
        /// <para>The index.</para>
        /// <para></para>
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>Sequences with sizes other than powers of two are not supported.</para>
        /// <para></para>
        /// </exception>
        /// <returns>
        /// <para>The current link.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetSquareMatrixSequenceElementByIndex<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress root, ulong size, ulong index)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var source = constants.SourcePart;
            var target = constants.TargetPart;
            if (!Platform.Numbers.Math.IsPowerOfTwo(size))
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Sequences with sizes other than powers of two are not supported.");
            }
            var path = new BitArray(BitConverter.GetBytes(index));
            var length = Bit.GetLowestPosition(size);
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
        /// Возвращает индекс указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Связь представленная списком, состоящим из её адреса и содержимого.</param>
        /// <returns>Индекс начальной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetIndex<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return link[links.Constants.IndexPart];}

        /// <summary>
        /// Возвращает индекс начальной (Source) связи для указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс начальной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetSource<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.GetLink(link)[links.Constants.SourcePart];}

        /// <summary>
        /// Возвращает индекс начальной (Source) связи для указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Связь представленная списком, состоящим из её адреса и содержимого.</param>
        /// <returns>Индекс начальной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetSource<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return link[links.Constants.SourcePart];}

        /// <summary>
        /// Возвращает индекс конечной (Target) связи для указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс конечной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetTarget<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.GetLink(link)[links.Constants.TargetPart];}

        /// <summary>
        /// Возвращает индекс конечной (Target) связи для указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Связь представленная списком, состоящим из её адреса и содержимого.</param>
        /// <returns>Индекс конечной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress GetTarget<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return link[links.Constants.TargetPart];}

        /// <summary>
        /// <para>
        /// Alls the links.
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
        /// <returns>
        /// <para>A list of i list t link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<IList<TLinkAddress>?> All<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var allLinks = new List<IList<TLinkAddress>?>();
            var filler = new ListFiller<IList<TLinkAddress>?, TLinkAddress>(allLinks, links.Constants.Continue);
            links.Each(filler.AddAndReturnConstant, restriction);
            return allLinks;
        }

        /// <summary>
        /// <para>
        /// Alls the indices using the specified links.
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
        /// <returns>
        /// <para>A list of t link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLinkAddress>? AllIndices<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var allIndices = new List<TLinkAddress>();
            var filler = new ListFiller<TLinkAddress, TLinkAddress>(allIndices, links.Constants.Continue);
            links.Each(filler.AddFirstAndReturnConstant, restriction);
            return allIndices;
        }

        /// <summary>
        /// Возвращает значение, определяющее существует ли связь с указанными началом и концом в хранилище связей.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Начало связи.</param>
        /// <param name="target">Конец связи.</param>
        /// <returns>Значение, определяющее существует ли связь.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return Comparer<TLinkAddress>.Default.Compare(links.Count(links.Constants.Any, source, target), default) > 0;}

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
        public static void EnsureInnerReferenceExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress reference, string argumentName)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
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
        public static void EnsureInnerReferenceExists<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction, string argumentName)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
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
        public static void EnsureLinkIsAnyOrExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, string argumentName)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (link !=  links.Constants.Any && !links.Exists(link))
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
        public static void EnsureLinkIsItselfOrExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, string argumentName)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (link !=  links.Constants.Itself && !links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
            }
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureDoesNotExists<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.Exists(source, target))
            {
                throw new LinkWithSameValueAlreadyExistsException();
            }
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureNoUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links.HasUsages(link))
            {
                throw new ArgumentLinkHasDependenciesException<TLinkAddress>(link);
            }
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCreated<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] addresses)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.EnsureCreated(links.Create, addresses);}

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsurePointsCreated<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] addresses)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.EnsureCreated(links.CreatePoint, addresses);}

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCreated<TLinkAddress>(this ILinks<TLinkAddress> links, Func<TLinkAddress> creator, params TLinkAddress[] addresses)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var addressToUInt64Converter = CheckedConverter<TLinkAddress, ulong>.Default;
            var uInt64ToAddressConverter = CheckedConverter<ulong, TLinkAddress>.Default;
            var nonExistentAddresses = new HashSet<TLinkAddress>(addresses.Where(x => !links.Exists(x)));
            if (nonExistentAddresses.Count > 0)
            {
                var max = nonExistentAddresses.Max();
                max = uInt64ToAddressConverter.Convert(System.Math.Min(addressToUInt64Converter.Convert(max), addressToUInt64Converter.Convert(links.Constants.InternalReferencesRange.Maximum)));
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

        /// <param name="links">Хранилище связей.</param>
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

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return Comparer<TLinkAddress>.Default.Compare(links.CountUsages(link), default) > 0;}

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var values = links.GetLink(link);
            return links.GetSource(values) ==  source && links.GetTarget(values) ==  target;
        }

        /// <summary>
        /// Выполняет поиск связи с указанными Source (началом) и Target (концом).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Индекс связи, которая является началом для искомой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для искомой связи.</param>
        /// <returns>Индекс искомой связи с указанными Source (началом) и Target (концом).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress SearchOrDefault<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var contants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(contants.Continue, contants.Break, default);
            links.Each(setter.SetFirstAndReturnFalse, contants.Any, source, target);
            return setter.Result;
        }

        public static TLinkAddress CreatePoint<TLinkAddress>(this ILinks<TLinkAddress> links)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.CreatePoint(setter.SetFirstFromNonNullSecondListAndReturnTrue);
            return setter.Result;
        }

        /// <param name="links">Хранилище связей.</param>
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

        public static TLinkAddress CreateAndUpdate<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.CreateAndUpdate(source, target, setter.SetFirstFromNonNullSecondListAndReturnTrue);
            return setter.Result;
        }


        /// <param name="links">Хранилище связей.</param>
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
        /// Обновляет связь с указанными началом (Source) и концом (Target)
        /// на связь с указанными началом (NewSource) и концом (NewTarget).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс обновляемой связи.</param>
        /// <param name="newSource">Индекс связи, которая является началом связи, на которую выполняется обновление.</param>
        /// <param name="newTarget">Индекс связи, которая является концом связи, на которую выполняется обновление.</param>
        /// <returns>Индекс обновлённой связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Update(new LinkAddress<TLinkAddress>(link), new Link<TLinkAddress>(link, newSource, newTarget));}

        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, params TLinkAddress[] restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Update((IList<TLinkAddress>)restriction);}

        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, WriteHandler<TLinkAddress>? handler, params TLinkAddress[] restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Update(restriction, handler);}

        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.Update(restriction, setter.SetFirstFromNonNullSecondListAndReturnTrue);
            return setter.Result;
        }


        /// <summary>
        /// Обновляет связь с указанными началом (Source) и концом (Target)
        /// на связь с указанными началом (NewSource) и концом (NewTarget).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="restriction">Ограничения на содержимое связей. Каждое ограничение может иметь значения: Constants.Null - 0-я связь, обозначающая ссылку на пустоту, Itself - требование установить ссылку на себя, 1..∞ конкретный адрес другой связи.</param>
        /// <returns>Индекс обновлённой связи.</returns>
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

        public static TLinkAddress Update<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Update(new LinkAddress<TLinkAddress>(link), new Link<TLinkAddress>(link, newSource, newTarget), handler);}

        /// <summary>
        /// <para>
        /// Resolves the constant as self reference using the specified links.
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
        /// <param name="constant">
        /// <para>The constant.</para>
        /// <para></para>
        /// </param>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        /// <param name="substitution">
        /// <para>The substitution.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A list of t link</para>
        /// <para></para>
        /// </returns>
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
        /// Создаёт связь (если она не существовала), либо возвращает индекс существующей связи с указанными Source (началом) и Target (концом).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Индекс связи, которая является началом на создаваемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для создаваемой связи.</param>
        /// <returns>Индекс связи, с указанным Source (началом) и Target (концом)</returns>
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

        public static TLinkAddress UpdateOrCreateOrGet<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target, TLinkAddress newSource, TLinkAddress newTarget)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            var setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.UpdateOrCreateOrGet(source, target, newSource, newTarget, setter.SetFirstFromNonNullSecondListAndReturnTrue);
            return setter.Result;
        }

        /// <summary>
        /// Обновляет связь с указанными началом (Source) и концом (Target)
        /// на связь с указанными началом (NewSource) и концом (NewTarget).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Индекс связи, которая является началом обновляемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом обновляемой связи.</param>
        /// <param name="newSource">Индекс связи, которая является началом связи, на которую выполняется обновление.</param>
        /// <param name="newTarget">Индекс связи, которая является концом связи, на которую выполняется обновление.</param>
        /// <returns>Индекс обновлённой связи.</returns>
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

        /// <summary>Удаляет связь с указанными началом (Source) и концом (Target).</summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Индекс связи, которая является началом удаляемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом удаляемой связи.</param>
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

        /// <summary>Удаляет несколько связей.</summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="deletedLinks">Список адресов связей к удалению.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteMany<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? deletedLinks)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            for (int i = 0; i < deletedLinks.Count; i++)
            {
                links.Delete(deletedLinks[i]);
            }
        }

        public static void DeleteAllUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.DeleteAllUsages(linkIndex, null);}

        /// <remarks>Before execution of this method ensure that deleted link is detached (all values - source and target are reset to null) or it might enter into infinite recursion.</remarks>
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
        /// <para>
        /// Deletes the by query using the specified links.
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
        /// <param name="query">
        /// <para>The query.</para>
        /// <para></para>
        /// </param>
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
        /// <para>
        /// Determines whether are values reset.
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
        /// <param name="linkIndex">
        /// <para>The link index.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
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

        public static void ResetValues<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.ResetValues(linkIndex, null);}

        // TODO: Create a universal version of this method in Platform.Data (with using of for loop)
        /// <summary>
        /// <para>
        /// Resets the values using the specified links.
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
        /// <param name="linkIndex">
        /// <para>The link index.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress ResetValues<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var nullConstant = links.Constants.Null;
            var updateRequest = new Link<TLinkAddress>(linkIndex, nullConstant, nullConstant);
            return links.Update(updateRequest, handler);
        }

        public static void EnforceResetValues<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.EnforceResetValues(linkIndex, null);}


        // TODO: Create a universal version of this method in Platform.Data (with using of for loop)
        /// <summary>
        /// <para>
        /// Enforces the reset values using the specified links.
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
        /// <param name="linkIndex">
        /// <para>The link index.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress EnforceResetValues<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex, WriteHandler<TLinkAddress>? handler)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (!links.AreValuesReset(linkIndex))
            {
                return links.ResetValues(linkIndex, handler);
            }
            return links.Constants.Continue;
        }

        public static void MergeUsages<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{links.MergeUsages(oldLinkIndex, newLinkIndex, null);}

        /// <summary>
        /// Merging two usages graphs, all children of old link moved to be children of new link or deleted.
        /// </summary>
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
        /// Replace one link with another (replaced link is deleted, children are updated or deleted).
        /// </summary>
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
        /// <para>
        /// Formats the links.
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
        /// <returns>
        /// <para>The string</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            return $"({links.GetIndex(link)}: {links.GetSource(link)} {links.GetTarget(link)})";
        }

        /// <summary>
        /// <para>
        /// Formats the links.
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
        /// <returns>
        /// <para>The string</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)  where TLinkAddress : IUnsignedNumber<TLinkAddress>{return links.Format(links.GetLink(link));}
    }
}
