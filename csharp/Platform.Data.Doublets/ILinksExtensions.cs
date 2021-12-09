using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Platform.Ranges;
using Platform.Collections.Arrays;
using Platform.Random;
using Platform.Setters;
using Platform.Converters;
using Platform.Numbers;
using Platform.Data.Exceptions;
using Platform.Data.Doublets.Decorators;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>Creates a static class ILinksExtensions.</para>
    /// <para>Создает статический класс ILinksExtensions.</para>
    /// </summary>
    public static class ILinksExtensions
    {
        /// <summary>
        /// <para>Creates links with random values in the specified links storage.</para>
        /// <para>Создаёт связи со случайными значениями в указанном хранилище связей.</para>
        /// </summary>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="amountOfCreations">
        /// <para>Number of operations to create links.</para>
        /// <para>Количество операция для создания связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunRandomCreations<TLink>(this ILinks<TLink> links, ulong amountOfCreations)
        {
            var random = RandomHelpers.Default;
            var addressToUInt64Converter = UncheckedConverter<TLink, ulong>.Default;
            var uInt64ToAddressConverter = UncheckedConverter<ulong, TLink>.Default;
            for (var i = 0UL; i < amountOfCreations; i++)
            {
                var linksAddressRange = new Range<ulong>(0, addressToUInt64Converter.Convert(links.Count()));
                var source = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                var target = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                links.GetOrCreate(source, target);
            }
        }

        /// <summary>
        /// <para>Search for links with random values in the specified link store.</para>
        /// <para>Ищет связи со случайными значениями в указанном хранилище связей.</para>
        /// </summary>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="amountOfSearches">
        /// <para>Number of operations to find links.</para>
        /// <para>Количество операция для поиска связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunRandomSearches<TLink>(this ILinks<TLink> links, ulong amountOfSearches)
        {
            var random = RandomHelpers.Default;
            var addressToUInt64Converter = UncheckedConverter<TLink, ulong>.Default;
            var uInt64ToAddressConverter = UncheckedConverter<ulong, TLink>.Default;
            for (var i = 0UL; i < amountOfSearches; i++)
            {
                var linksAddressRange = new Range<ulong>(0, addressToUInt64Converter.Convert(links.Count()));
                var source = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                var target = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                links.SearchOrDefault(source, target);
            }
        }

        /// <summary>
        /// <para>Removing links with random values in the specified link store.</para>
        /// <para>Удаляет связей со случайными значениями в указанном хранилище связей.</para>
        /// </summary>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="amountOfDeletions">
        /// <para>Number of operations to delete links.</para>
        /// <para>Количество операция для удаления связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunRandomDeletions<TLink>(this ILinks<TLink> links, ulong amountOfDeletions)
        {
            var random = RandomHelpers.Default;
            var addressToUInt64Converter = UncheckedConverter<TLink, ulong>.Default;
            var uInt64ToAddressConverter = UncheckedConverter<ulong, TLink>.Default;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Delete<TLink>(this ILinks<TLink> links, TLink linkToDelete) => links.Delete(new LinkAddress<TLink>(linkToDelete));

        /// <summary>
        /// <para>Removes all links in the specified store.</para>
        /// <para>Удаляет все связи в указанном хранилище.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <remarks>
        /// TODO: Возможно есть очень простой способ это сделать.
        /// (Например просто удалить файл, или изменить его размер таким образом,
        /// чтобы удалился весь контент)
        /// Например через _header->AllocatedLinks в ResizableDirectMemoryLinks
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteAll<TLink>(this ILinks<TLink> links)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            var comparer = Comparer<TLink>.Default;
            for (var i = links.Count(); comparer.Compare(i, default) > 0; i = Arithmetic.Decrement(i))
            {
                links.Delete(i);
                if (!equalityComparer.Equals(links.Count(), Arithmetic.Decrement(i)))
                {
                    i = links.Count();
                }
            }
        }

        /// <summary>
        /// <para>Looks for the first link in the first store.</para>
        /// <para>Ищет первую связь в первом хранилище.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink First<TLink>(this ILinks<TLink> links)
        {
            TLink firstLink = default;
            var equalityComparer = EqualityComparer<TLink>.Default;
            if (equalityComparer.Equals(links.Count(), default))
            {
                throw new InvalidOperationException("В хранилище нет связей.");
            }
            links.Each(links.Constants.Any, links.Constants.Any, link =>
            {
                firstLink = link[links.Constants.IndexPart];
                return links.Constants.Break;
            });
            if (equalityComparer.Equals(firstLink, default))
            {
                throw new InvalidOperationException("В процессе поиска по хранилищу не было найдено связей.");
            }
            return firstLink;
        }

        /// <summary>
        /// <para>Finds a link that satisfies the specified constraint, if there is one, otherwise returns the default link address.</para>
        /// <para>Находит связь, которая удовлетворяет указанном ограничению, если такая существует в единственном экземпляре, в противном случае возвращает адрес связи по умолчанию.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="query">
        /// <para>Limitation.</para>
        /// <para>Ограничение.</para>
        /// </param>
        /// <returns>
        /// <para>Return result.</para>
        /// <para>Возвращает результат.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLink> SingleOrDefault<TLink>(this ILinks<TLink> links, IList<TLink> query)
        {
            IList<TLink> result = null;
            var count = 0;
            var constants = links.Constants;
            var @continue = constants.Continue;
            var @break = constants.Break;
            links.Each(linkHandler, query);
            return result;
            
            TLink linkHandler(IList<TLink> link)
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
        /// <para>Checks the existence of a base along the given path.</para>
        /// <para>Проверяет существование базы по данному пути.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="path">
        /// <para>Links store path.</para>
        /// <para>Путь к хранилищу связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns true if there is a path to the link or false if there is no path to the link.</para>
        /// <para>Возвращает значение true в случае существание пути к связе или false в случае отстутсвии пути к связи.</para>
        /// </returns>
        /// <remarks>
        /// TODO: Как так? Как то что ниже может быть корректно?
        /// Скорее всего практически не применимо
        /// Предполагалось, что можно было конвертировать формируемый в проходе через SequenceWalker 
        /// Stack в конкретный путь из Source, Target до связи, но это не всегда так.
        /// TODO: Возможно нужен метод, который именно выбрасывает исключения (EnsurePathExists)
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckPathExistance<TLink>(this ILinks<TLink> links, params TLink[] path)
        {
            var current = path[0];
            //EnsureLinkExists(current, "path");
            if (!links.Exists(current))
            {
                return false;
            }
            var equalityComparer = EqualityComparer<TLink>.Default;
            var constants = links.Constants;
            for (var i = 1; i < path.Length; i++)
            {
                var next = path[i];
                var values = links.GetLink(current);
                var source = values[constants.SourcePart];
                var target = values[constants.TargetPart];
                if (equalityComparer.Equals(source, target) && equalityComparer.Equals(source, next))
                {
                    //throw new InvalidOperationException(string.Format("Невозможно выбрать путь, так как и Source и Target совпадают с элементом пути {0}.", next));
                    return false;
                }
                if (!equalityComparer.Equals(next, source) && !equalityComparer.Equals(next, target))
                {
                    //throw new InvalidOperationException(string.Format("Невозможно продолжить путь через элемент пути {0}", next));
                    return false;
                }
                current = next;
            }
            return true;
        }

        /// <summary>
        /// <para>Retrieved by key link by link address.</para>
        /// <para>Получает с помощью ключевой связи и с помощью адреса связи адресс главной связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="root">
        /// <para>The key link.</para>
        /// <para>Ключевая связь.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связи.</para>
        /// </param>
        /// <param name="path">
        /// <para>Main link path.</para>
        /// <para>Путь к главной связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the current link.</para>
        /// <para>Возвращеает текущую связь.</para>
        /// </returns>
        /// <remarks>
        /// Может потребовать дополнительного стека для PathElement's при использовании SequenceWalker.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetByKeys<TLink>(this ILinks<TLink> links, TLink root, params int[] path)
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
        /// <para>Picks up a matrix of slave link addresses using the master link index.</para>
        /// <para>Палучает матрицу адресов побойчных связей с помощью индекса главной связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="root">
        /// <para>The key link.</para>
        /// <para>Ключевая связь.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="size">
        /// <para>Link address matrix size.</para>
        /// <para>Размер матрицы адресов связи.</para>
        /// </param>
        /// <param name="index">
        /// <para>The link index.</para>
        /// <para>Индекс связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the current master link.</para>
        /// <para>Возвращает текущую главную связь.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetSquareMatrixSequenceElementByIndex<TLink>(this ILinks<TLink> links, TLink root, ulong size, ulong index)
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

        /// <para>Gets the index of the specified relationship in the specified storage.</para>
        /// <para>Получеает индекс указанной связи в указанном хранилище.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <returns>
        /// <para>Index of the specified link.</para>
        /// <para>Индекс указанной связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetIndex<TLink>(this ILinks<TLink> links, IList<TLink> link) => link[links.Constants.IndexPart];

        /// <summary>
        /// <para>Gets the start of a link in the specified link store.</para>
        /// <para>Получает начало связи в указанном хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <returns>
        /// <para>The beginning of the specified connection.</para>
        /// <para>Начало указанной связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetSource<TLink>(this ILinks<TLink> links, TLink link) => links.GetLink(link)[links.Constants.SourcePart];

        /// <summary>
        /// <para>Gets the start of a link in the specified link store.</para>
        /// <para>Получает начало связи в указанном хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="link">
        /// <para>The list of the links.</para>
        /// <para>Список связей.</para>
        /// </param>
        /// <returns>
        /// <para>The beginning of the specified connection.</para>
        /// <para>Начало указанной связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetSource<TLink>(this ILinks<TLink> links, IList<TLink> link) => link[links.Constants.SourcePart];

        /// <summary>
        /// <para>Gets the end of a link in the specified link store.</para>
        /// <para>Получает конец связи в указанном хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="link">
        /// <para>The list of the links.</para>
        /// <para>Список связей.</para>
        /// </param>
        /// <returns>
        /// <para>The end of the specified connection.</para>
        /// <para>Конец указанной связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetTarget<TLink>(this ILinks<TLink> links, TLink link) => links.GetLink(link)[links.Constants.TargetPart];

        /// <summary>
        /// <para>Gets the end of a link in the specified link store.</para>
        /// <para>Получает конец связи в указанном хранилище связей.</para>
        /// </summary>
        /// <param name="Tlink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </param>
        /// <typeparam name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </typeparam>
        /// <param name="link">
        /// <para>The list of the links.</para>
        /// <para>Список связей.</para>
        /// </param>
        /// <returns>
        /// <para>The end of the specified connection.</para>
        /// <para>Конец указанной связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetTarget<TLink>(this ILinks<TLink> links, IList<TLink> link) => link[links.Constants.TargetPart];

        /// <summary>
        /// <para>Loops through all the links that match the pattern, invoking a handler for each matching link.</para>
        /// <para>Выполняет проход по всем связям, соответствующим шаблону, вызывая обработчик (handler) для каждой подходящей связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
          /// <param name="handler">
        /// <para>Handler for every matching link.</para>
        /// <para>Обработчик каждой подходящей связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns True if the pass through the links was not interrupted and False otherwise.</para>
        /// <para>Возвращает значение True, в случае если проход по связям не был прерван и значение False в обратном случае.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Each<TLink>(this ILinks<TLink> links, Func<IList<TLink>, TLink> handler, params TLink[] restrictions)
            => EqualityComparer<TLink>.Default.Equals(links.Each(handler, restrictions), links.Constants.Continue);

        /// <summary>
        /// <para>Handles every relationship with the specified start and end.</para>
        /// <para>Обрабатывает каждую связь с указанным началом и концом.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="target">
        /// <para>The end of link.</para>
        /// <para>Конец связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="handler">
        /// <para>Handler for every matching link.</para>
        /// <para>Обработчик каждой подходящей связи.</para>
        /// </param>
        /// <param name="sourse">
        /// <para>The beggining of link.</para>
        /// <para>Начало связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the processing value for each relationship with the specified start and end..</para>
        /// <para>Возвращает значение обработки каждой связи с указанным началом и концом.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Each<TLink>(this ILinks<TLink> links, TLink source, TLink target, Func<TLink, bool> handler)
        {
            var constants = links.Constants;
            return links.Each(link => handler(link[constants.IndexPart]) ? constants.Continue : constants.Break, constants.Any, source, target);
        }

        /// <summary>
        /// <para>Using the template, it traverses each link and calls the link handler for each uploaded link..</para>
        /// <para>С помощью шаблона проходит по каждой связи в вызывает обработчик связи для каждой подъодящей связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="target">
        /// <para>The end of link.</para>
        /// <para>Конец связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="handler">
        /// <para>Handler for every matching link.</para>
        /// <para>Обработчик каждой подходящей связи.</para>
        /// </param>
        /// <param name="sourse">
        /// <para>The beggining of the link.</para>
        /// <para>Начало связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns true if the pass through the links was not interrupted and False otherwise.</para>
        /// <para>Вовзращает значение true, в случае если проход по связям не был прерван и False в обратном случае.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Each<TLink>(this ILinks<TLink> links, TLink source, TLink target, Func<IList<TLink>, TLink> handler) => links.Each(handler, links.Constants.Any, source, target);

        /// <summary>
        /// <para>Using constraints, checks the addresses of links in the link store.</para>
        /// <para>С помощью ограничений проверяет адреса связей в хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="restrictions">
        /// <para>Restrictions on communication addresses.</para>
        /// <para>Ограничения по адресам связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links Storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the value of a set of links in the case of a nonzero  set, and an empty set otherwise.</para>
        /// <para>Возвращает значение множества связей в случае ненулевого множества, и пустое множество в обратном случае.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<IList<TLink>> All<TLink>(this ILinks<TLink> links, params TLink[] restrictions)
        {
            var arraySize = CheckedConverter<TLink, ulong>.Default.Convert(links.Count(restrictions));
            if (arraySize > 0)
            {
                var array = new IList<TLink>[arraySize];
                var filler = new ArrayFiller<IList<TLink>, TLink>(array, links.Constants.Continue);
                links.Each(filler.AddAndReturnConstant, restrictions);
                return array;
            }
            else
            {
                return Array.Empty<IList<TLink>>();
            }
        }

        /// <summary>
        /// <para>Checks link addresses by indexes in link store.</para>
        /// <para>Проверяет адреса связей по индексам в хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="restrictions">
        /// <para>Restrictions on communication addresses.</para>
        /// <para>Ограничения по адресам связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the value of a set of links in the case of a nonzero  set, and an empty set otherwise.</para>
        /// <para>Возвращает значение множества связей в случае ненулевого множества, и пустое множество в обратном случае.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLink> AllIndices<TLink>(this ILinks<TLink> links, params TLink[] restrictions)
        {
            var arraySize = CheckedConverter<TLink, ulong>.Default.Convert(links.Count(restrictions));
            if (arraySize > 0)
            {
                var array = new TLink[arraySize];
                var filler = new ArrayFiller<TLink, TLink>(array, links.Constants.Continue);
                links.Each(filler.AddFirstAndReturnConstant, restrictions);
                return array;
            }
            else
            {
                return Array.Empty<TLink>();
            }
        }

        /// <summary>
        /// <para>Checks if a link exists with the specified start and end in the link store.</para>
        /// <para>Проверяет существует ли связь с указанным началом и концом в хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="source">
        /// <para>The beggining of link.</para>
        /// <para>Начало связи.</para>
        /// </param>
        /// <param name="target">
        /// <para>The end of link.</para>
        /// <para>Конец связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns a value that determines whether a link exists or not.</para>
        /// <para>Возвращает значение, определяющее существует связь или нет.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exists<TLink>(this ILinks<TLink> links, TLink source, TLink target) => Comparer<TLink>.Default.Compare(links.Count(links.Constants.Any, source, target), default) > 0;

        #region Ensure
        // TODO: May be move to EnsureExtensions or make it both there and here
        /// <summary>
        /// <para>Checks the connection for its existence.</para>
        /// <para>Проверяет связь на ее существование.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="restrictions">
        /// <para>Restrictions on communication addresses.</para>
        /// <para>Ограничения по адресам связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkExists<TLink>(this ILinks<TLink> links, IList<TLink> restrictions)
        {
            for (var i = 0; i < restrictions.Count; i++)
            {
                if (!links.Exists(restrictions[i]))
                {
                    throw new ArgumentLinkDoesNotExistsException<TLink>(restrictions[i], $"sequence[{i}]");
                }
            }
        }

        /// <summary>
        /// <para>Checks if a link exists for the specified link argument.</para>
        /// <para>Проверяет связь на существование по указанному аргументу связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="reference">
        /// <para>Restrictions on communication addresses.</para>
        /// <para>Ограничения по адресам связи.</para>
        /// </param>
        /// <param name="argumentName">
        /// <para>Argument name.</para>
        /// <para>Значение аргумента.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureInnerReferenceExists<TLink>(this ILinks<TLink> links, TLink reference, string argumentName)
        {
            if (links.Constants.IsInternalReference(reference) && !links.Exists(reference))
            {
                throw new ArgumentLinkDoesNotExistsException<TLink>(reference, argumentName);
            }
        }

        /// <summary>
        /// <para>Checks for the existence of a connection in the specified range of contacts.</para>
        /// <para>Проверяет на существованние связь в казанном диапазоне адресов связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="restrictions">
        /// <para>Restrictions on communication addresses.</para>
        /// <para>Ограничения по адресам связи.</para>
        /// </param>
        /// <param name="argumentName">
        /// <para>Argument name.</para>
        /// <para>Значение аргумента.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище свящей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureInnerReferenceExists<TLink>(this ILinks<TLink> links, IList<TLink> restrictions, string argumentName)
        {
            for (int i = 0; i < restrictions.Count; i++)
            {
                links.EnsureInnerReferenceExists(restrictions[i], argumentName);
            }
        }

        /// <summary>
        /// <para>Checks for connectivity by address link type.</para>
        /// <para>Проверяет на существование связи по типам адресов связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="reference">
        /// <para>Restrictions on communication addresses.</para>
        /// <para>Ограничения по адресам связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsAnyOrExists<TLink>(this ILinks<TLink> links, IList<TLink> restrictions)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            var any = links.Constants.Any;
            for (var i = 0; i < restrictions.Count; i++)
            {
                if (!equalityComparer.Equals(restrictions[i], any) && !links.Exists(restrictions[i]))
                {
                    throw new ArgumentLinkDoesNotExistsException<TLink>(restrictions[i], $"sequence[{i}]");
                }
            }
        }

        /// <summary>
        /// <para>Checks the existence of a link by the types of link addresses and the value of the link arguments.</para>
        /// <para>Проверяет на существование связи по типам адресов связей и значением аргументов связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <param name="argumentName">
        /// <para>Argument name.</para>
        /// <para>Значение аргумента.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsAnyOrExists<TLink>(this ILinks<TLink> links, TLink link, string argumentName)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(link, links.Constants.Any) && !links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLink>(link, argumentName);
            }
        }

        /// <summary>
        /// <para>Checks for links in the link store or outside the store by link address type.</para>
        /// <para>Проверяет существование связей в хранилище связей или вне хранилища по типам адрессов связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <param name="argumentName">
        /// <para>Argument name.</para>
        /// <para>Значение аргумента.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsItselfOrExists<TLink>(this ILinks<TLink> links, TLink link, string argumentName)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(link, links.Constants.Itself) && !links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLink>(link, argumentName);
            }
        }

        /// <summary>
        /// <para>Checks the link for its absence in the link repository.</para>
        /// <para>Проверяет связь на ее отстутсвие в хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <param name="sourse">
        /// <para>The beggining of link.</para>
        /// <para>Начало связи.</para>
        /// </param>
        /// <param name="target">
        /// <para>The end of link.</para>
        /// <para>Конец связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureDoesNotExists<TLink>(this ILinks<TLink> links, TLink source, TLink target)
        {
            if (links.Exists(source, target))
            {
                throw new LinkWithSameValueAlreadyExistsException();
            }
        }

        /// <summary>
        /// <para>Checks the link store for unused links.</para>
        /// <para>Проверяет хранилище связей на неиспользуемые связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureNoUsages<TLink>(this ILinks<TLink> links, TLink link)
        {
            if (links.HasUsages(link))
            {
                throw new ArgumentLinkHasDependenciesException<TLink>(link);
            }
        }

        /// <summary>
        /// <para>Checks the link for its presence in the link storage.</para>
        /// <para>Проверяет связь на ее наличие в хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="addresses">
        /// <para>The links addresses.</para>
        /// <para>Адреса связей.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCreated<TLink>(this ILinks<TLink> links, params TLink[] addresses) => links.EnsureCreated(links.Create, addresses);

        /// <summary>
        /// <para>Checks links for addresses.</para>
        /// <para>Проверяет связи на наличие адрессов.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="addresses">
        /// <para>The links addresses.</para>
        /// <para>Адреса связей.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsurePointsCreated<TLink>(this ILinks<TLink> links, params TLink[] addresses) => links.EnsureCreated(links.CreatePoint, addresses);

        /// <summary>
        /// <para>Checks if a link has been created with the specified address.</para>
        /// <para>Проверяет создана ли связь с указанным адрессом.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <param name="creator">
        /// <para>Parameter indicating whether the link is created or not.</para>
        /// <para>Параметр, указывающий создана связь или нет.</para>
        /// </param>
        /// <param name="addresses">
        /// <para>The links addresses.</para>
        /// <para>Адреса связей.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links Storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCreated<TLink>(this ILinks<TLink> links, Func<TLink> creator, params TLink[] addresses)
        {
            var addressToUInt64Converter = CheckedConverter<TLink, ulong>.Default;
            var uInt64ToAddressConverter = CheckedConverter<ulong, TLink>.Default;
            var nonExistentAddresses = new HashSet<TLink>(addresses.Where(x => !links.Exists(x)));
            if (nonExistentAddresses.Count > 0)
            {
                var max = nonExistentAddresses.Max();
                max = uInt64ToAddressConverter.Convert(System.Math.Min(addressToUInt64Converter.Convert(max), addressToUInt64Converter.Convert(links.Constants.InternalReferencesRange.Maximum)));
                var createdLinks = new List<TLink>();
                var equalityComparer = EqualityComparer<TLink>.Default;
                TLink createdLink = creator();
                while (!equalityComparer.Equals(createdLink, max))
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
        /// <para>Subtracts the number of times a link has been used in the link store.</para>
        /// <para>Подстчитывает количество использований связи в хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the number of times the link has been used with the specified start and end.</para>
        /// <para>Возвращает количество использований связи с указанным началом и концом.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink CountUsages<TLink>(this ILinks<TLink> links, TLink link)
        {
            var constants = links.Constants;
            var values = links.GetLink(link);
            TLink usagesAsSource = links.Count(new Link<TLink>(constants.Any, link, constants.Any));
            var equalityComparer = EqualityComparer<TLink>.Default;
            if (equalityComparer.Equals(values[constants.SourcePart], link))
            {
                usagesAsSource = Arithmetic<TLink>.Decrement(usagesAsSource);
            }
            TLink usagesAsTarget = links.Count(new Link<TLink>(constants.Any, constants.Any, link));
            if (equalityComparer.Equals(values[constants.TargetPart], link))
            {
                usagesAsTarget = Arithmetic<TLink>.Decrement(usagesAsTarget);
            }
            return Arithmetic<TLink>.Add(usagesAsSource, usagesAsTarget);
        }

        /// <summary>
        /// <para>Checks a link for its use in the link store.</para>
        /// <para>Проверяет связь на ее использование в хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the communication address.</para>
        /// <para>Возвращает адресс связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasUsages<TLink>(this ILinks<TLink> links, TLink link) => Comparer<TLink>.Default.Compare(links.CountUsages(link), default) > 0;

        /// <summary>
        /// <para>Checks links in the link store for a specified beginning and end of a link.</para>
        /// <para>Проверяет связи в хранилище связей на указанное начало и конец связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="target">
        /// <para>The end of link.</para>
        /// <para>Конец связи.</para>
        /// </param>
        /// <param name="sourse">
        /// <para>The beggining of link.</para>
        /// <para>Начало связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the addresses of equivalent links.</para>
        /// <para>Возвращает адреса эквивалентных связей.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<TLink>(this ILinks<TLink> links, TLink link, TLink source, TLink target)
        {
            var constants = links.Constants;
            var values = links.GetLink(link);
            var equalityComparer = EqualityComparer<TLink>.Default;
            return equalityComparer.Equals(values[constants.SourcePart], source) && equalityComparer.Equals(values[constants.TargetPart], target);
        }

        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="target">
        /// <para>The end of link.</para>
        /// <para>Конец связи.</para>
        /// </param>
        /// <param name="sourse">
        /// <para>The beggining of link.</para>
        /// <para>Начало связи.</para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the index of the link to find at the specified start or end.</para>
        /// <para>Возвращает индекс искомой связи с указанным началом или концом.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink SearchOrDefault<TLink>(this ILinks<TLink> links, TLink source, TLink target)
        {
            var contants = links.Constants;
            var setter = new Setter<TLink, TLink>(contants.Continue, contants.Break, default);
            links.Each(setter.SetFirstAndReturnFalse, contants.Any, source, target);
            return setter.Result;
        }

        /// <summary>
        /// <para>Creates a new link and link address in the link store.</para>
        /// <para>Создает новую связь и адресс связи в хранилище связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns an empty link value.</para>
        /// <para>Возврпащеает пустое значение связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink Create<TLink>(this ILinks<TLink> links) => links.Create(null);

        /// <summary>
        /// <para>Creates the value of the link address.</para>
        /// <para>Создает значение адреса связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns an update of the link address.</para>
        /// <para>Возвращает обновление адресса связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink CreatePoint<TLink>(this ILinks<TLink> links)
        {
            var link = links.Create();
            return links.Update(link, link, link);
        }

        /// <summary>
        /// <para>Creates and updates link address types with specified start and end.</para>
        /// <para>Создает и обновляет типы адреса связи с указанным началом и концом.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="sourse">
        /// <para>The beggining of link.</para>
        /// <para>Начало связи.</para>
        /// </param>
        /// <param name="target">
        /// <para>The end of link.</para>
        /// <para>Конец связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the end and start of the link.</para>
        /// <para>Возвращает значения конца и начала связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink CreateAndUpdate<TLink>(this ILinks<TLink> links, TLink source, TLink target) => links.Update(links.Create(), source, target);

        /// <summary>
        /// <para>Updates a link with the specified start and end to a link with the specified start and end.</para>
        /// <para>Обновляет связь с указанным началом и концом на связь с указанным началом и концом.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связи.</para>
        /// </param>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <param name="newSource">
        /// <para>The new beggining of link.</para>
        /// <para>Новое начало связи.</para>
        /// </param>
        /// <param name="newTarget">
        /// <para>The new end of link.</para>
        /// <para>Новый конец связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the index of the updated link.</para>
        /// <para>Возвращает индекс обновленной связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink Update<TLink>(this ILinks<TLink> links, TLink link, TLink newSource, TLink newTarget) => links.Update(new LinkAddress<TLink>(link), new Link<TLink>(link, newSource, newTarget));

        /// <summary>
        /// <para>In two or more equivalent links, combine or delete incoming and outgoing links to one main link.</para>
        /// <para>В двух или более эквивалентных свзять объединяет или удаляет входящие и выходящие связи к одной главной связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>The links type address.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="restrictions">
        /// <para>Ограничения на содержимое связей.</para>
        /// <para>Restrictions on the content of links.</para>
        /// </param>
        /// <returns>
        /// <para>Depending on the condition, it returns the link update, the index, the link number that was deleted.</para>
        /// <para>В зависимости от условия возвращает обновления связи, индекс, номер свзи которую удалил.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink Update<TLink>(this ILinks<TLink> links, params TLink[] restrictions)
        {
            if (restrictions.Length == 2)
            {
                return links.MergeAndDelete(restrictions[0], restrictions[1]);
            }
            if (restrictions.Length == 4)
            {
                return links.UpdateOrCreateOrGet(restrictions[0], restrictions[1], restrictions[2], restrictions[3]);
            }
            else
            {
                return links.Update(new LinkAddress<TLink>(restrictions[0]), restrictions);
            }
        }

        /// <summary>
        /// <para>Allows the use of constant values as independent communication addresses.</para>
        /// <para>Разрешает использование константных значений как самостоятельные адреса связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="constant">
        /// <para>Constant values.</para>
        /// <para>Константные значение.</para>
        /// </param>
        /// <param name="restrictions">
        /// <para>Constraints on constant values.</para>
        /// <para>Ограничения константных значений.</para>
        /// </param>
        /// <param name="substitution">
        /// <para>Changed communication addresses.</para>
        /// <para>Измененные адреса связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the link and the type of its address.</para>
        /// <para>Возвращает связь и тип ее адреса.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLink> ResolveConstantAsSelfReference<TLink>(this ILinks<TLink> links, TLink constant, IList<TLink> restrictions, IList<TLink> substitution)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            var constants = links.Constants;
            var restrictionsIndex = restrictions[constants.IndexPart];
            var substitutionIndex = substitution[constants.IndexPart];
            if (equalityComparer.Equals(substitutionIndex, default))
            {
                substitutionIndex = restrictionsIndex;
            }
            var source = substitution[constants.SourcePart];
            var target = substitution[constants.TargetPart];
            source = equalityComparer.Equals(source, constant) ? substitutionIndex : source;
            target = equalityComparer.Equals(target, constant) ? substitutionIndex : target;
            return new Link<TLink>(substitutionIndex, source, target);
        }

        /// <summary>
        /// <para>Creates a new link, a non-existing link.</para>
        /// <para>Создает новую связь не существующую связь.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="source">
        /// <para>The beggining of link.</para>
        /// <para>Начало связи.</para>
        /// </param>
        /// <param name="target">
        /// <para>The end of link.</para>
        /// <para>Конец связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the index of an existing relationship with the specified start and end of the relationship.</para>
        /// <para>Вовзращает индекс существующей связи с указанным началом и концом связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetOrCreate<TLink>(this ILinks<TLink> links, TLink source, TLink target)
        {
            var link = links.SearchOrDefault(source, target);
            if (EqualityComparer<TLink>.Default.Equals(link, default))
            {
                link = links.CreateAndUpdate(source, target);
            }
            return link;
        }

        /// <summary>
        /// <para>Updates a link with the specified start and end to a link with the specified new start and new end.</para>
        /// <para>Обновляет связь с указанным началом и концом на связь с указанным новым началамо и новым концом.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="source">
        /// <para>The beggining of link.</para>
        /// <para>Начало связи.</para>
        /// </param>
        /// <param name="target">
        /// <para>The end of link.</para>
        /// <para>Конец связи.</para>
        /// </param>
        /// <param name="newSource">
        /// <para>The new beggining of link.</para>
        /// <para>Новое начало связи.</para>
        /// </param>
        /// <param name="newTarget">
        /// <para>The new end of link.</para>
        /// <para>Новый конец связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the index of a specific relationship.</para>
        /// <para>Возвращает индекс определенной связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink UpdateOrCreateOrGet<TLink>(this ILinks<TLink> links, TLink source, TLink target, TLink newSource, TLink newTarget)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            var link = links.SearchOrDefault(source, target);
            if (equalityComparer.Equals(link, default))
            {
                return links.CreateAndUpdate(newSource, newTarget);
            }
            if (equalityComparer.Equals(newSource, source) && equalityComparer.Equals(newTarget, target))
            {
                return link;
            }
            return links.Update(link, newSource, newTarget);
        }

        /// <summary>
        /// <para>Removes the link to the specified start and end.</para>
        /// <para>Удаляет связь с указанным началом и концом.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="source">
        /// <para>The beggining of communication address..</para>
        /// <para>Начало адреса связи.</para>
        /// </param>
        /// <param name="target">
        /// <para>The ennd of communication address..</para>
        /// <para>Конец адреса связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the default link value.</para>
        /// <para>Возвращает стандарнтоне значение связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink DeleteIfExists<TLink>(this ILinks<TLink> links, TLink source, TLink target)
        {
            var link = links.SearchOrDefault(source, target);
            if (!EqualityComparer<TLink>.Default.Equals(link, default))
            {
                links.Delete(link);
                return link;
            }
            return default;
        }

        /// <summary>
        /// <para>Removing links.</para>
        /// <para>Удаляет связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="deletedLinks">
        /// <para>List of deleted links.</para>
        /// <para>Список удаленных связей.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteMany<TLink>(this ILinks<TLink> links, IList<TLink> deletedLinks)
        {
            for (int i = 0; i < deletedLinks.Count; i++)
            {
                links.Delete(deletedLinks[i]);
            }
        }

        /// <summary>
        /// <para>Removes used links on request.</para>
        /// <para>Удаляет использованные связи по запросу.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="linkIndex">
        /// <para>The link index.</para>
        /// <para>Индекс связи.</para>
        /// </param>
        /// <remarks>Before execution of this method ensure that deleted link is detached
        /// (all values - source and target are reset to null)
        /// or it might enter into infinite recursion.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteAllUsages<TLink>(this ILinks<TLink> links, TLink linkIndex)
        {
            var anyConstant = links.Constants.Any;
            var usagesAsSourceQuery = new Link<TLink>(anyConstant, linkIndex, anyConstant);
            links.DeleteByQuery(usagesAsSourceQuery);
            var usagesAsTargetQuery = new Link<TLink>(anyConstant, linkIndex, anyConstant);
            links.DeleteByQuery(usagesAsTargetQuery);
        }

        /// <summary>
        /// <para>Removes constrained links.</para>
        /// <para>Удаляет связи по ограничениям.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="query">
        /// <para>The limitation.</para>
        /// <para>Ограничение.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteByQuery<TLink>(this ILinks<TLink> links, Link<TLink> query)
        {
            var count = CheckedConverter<TLink, long>.Default.Convert(links.Count(query));
            if (count > 0)
            {
                var queryResult = new TLink[count];
                var queryResultFiller = new ArrayFiller<TLink, TLink>(queryResult, links.Constants.Continue);
                links.Each(queryResultFiller.AddFirstAndReturnConstant, query);
                for (var i = count - 1; i >= 0; i--)
                {
                    links.Delete(queryResult[i]);
                }
            }
        }

        // TODO: Move to Platform.Data
        /// <summary>
        /// <para>Checks if the link values are cleared.</para>
        /// <para>Проверяет сброшены ли значения связи.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="linkIndex">
        /// <para>Link Index.</para>
        /// <para>Индекс связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns <cref="true"/> if the link values are cleared, and <cref="false"/> if they are not cleared.</para>
        /// <para>Возвращает значение <cref="true"/> если сброшены значения связи, и <cref="false"/> если не сброшены.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreValuesReset<TLink>(this ILinks<TLink> links, TLink linkIndex)
        {
            var nullConstant = links.Constants.Null;
            var equalityComparer = EqualityComparer<TLink>.Default;
            var link = links.GetLink(linkIndex);
            for (int i = 1; i < link.Count; i++)
            {
                if (!equalityComparer.Equals(link[i], nullConstant))
                {
                    return false;
                }
            }
            return true;
        }

        // TODO: Create a universal version of this method in Platform.Data (with using of for loop)
        /// <summary>
        /// <para>Resets communication values without testing.</para>
        /// <para>Сбрасывает значения связи без проведения проверок.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links Storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="linkIndex">
        /// <para>Link Index.</para>
        /// <para>Индекс связи.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetValues<TLink>(this ILinks<TLink> links, TLink linkIndex)
        {
            var nullConstant = links.Constants.Null;
            var updateRequest = new Link<TLink>(linkIndex, nullConstant, nullConstant);
            links.Update(updateRequest);
        }

        // TODO: Create a universal version of this method in Platform.Data (with using of for loop)
        /// <summary>
        /// <para>Resets link values if they have not been cleared earlier.</para>
        /// <para>Сбрасывает значиения связи, если они не были сброшены ранее.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links Storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="linkIndex">
        /// <para>Link Index.</para>
        /// <para>Индекс связи.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnforceResetValues<TLink>(this ILinks<TLink> links, TLink linkIndex)
        {
            if (!links.AreValuesReset(linkIndex))
            {
                links.ResetValues(linkIndex);
            }
        }

        /// <summary>
        /// <para>Explores two equivalent relationships. Links to the old link either become links to the new link or are deleted.</para>
        /// <para>Исследует две эквивалентные связи. Ссылки на старую связь либо становяттся сслыками на новую связь либо удаляются.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="oldLinkIndex">
        /// <para>The previous link index.</para>
        /// <para>Предыдущий индекс связи.</para>
        /// </param>
        /// <param name="newLinkIndex">
        /// <para>The new link index.</para>
        /// <para>Новый индекс связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the new link index.</para>
        /// <para>Возвращает новый индекс связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink MergeUsages<TLink>(this ILinks<TLink> links, TLink oldLinkIndex, TLink newLinkIndex)
        {
            var addressToInt64Converter = CheckedConverter<TLink, long>.Default;
            var equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(oldLinkIndex, newLinkIndex))
            {
                var constants = links.Constants;
                var usagesAsSourceQuery = new Link<TLink>(constants.Any, oldLinkIndex, constants.Any);
                var usagesAsSourceCount = addressToInt64Converter.Convert(links.Count(usagesAsSourceQuery));
                var usagesAsTargetQuery = new Link<TLink>(constants.Any, constants.Any, oldLinkIndex);
                var usagesAsTargetCount = addressToInt64Converter.Convert(links.Count(usagesAsTargetQuery));
                var isStandalonePoint = Point<TLink>.IsFullPoint(links.GetLink(oldLinkIndex)) && usagesAsSourceCount == 1 && usagesAsTargetCount == 1;
                if (!isStandalonePoint)
                {
                    var totalUsages = usagesAsSourceCount + usagesAsTargetCount;
                    if (totalUsages > 0)
                    {
                        var usages = ArrayPool.Allocate<TLink>(totalUsages);
                        var usagesFiller = new ArrayFiller<TLink, TLink>(usages, links.Constants.Continue);
                        var i = 0L;
                        if (usagesAsSourceCount > 0)
                        {
                            links.Each(usagesFiller.AddFirstAndReturnConstant, usagesAsSourceQuery);
                            for (; i < usagesAsSourceCount; i++)
                            {
                                var usage = usages[i];
                                if (!equalityComparer.Equals(usage, oldLinkIndex))
                                {
                                    links.Update(usage, newLinkIndex, links.GetTarget(usage));
                                }
                            }
                        }
                        if (usagesAsTargetCount > 0)
                        {
                            links.Each(usagesFiller.AddFirstAndReturnConstant, usagesAsTargetQuery);
                            for (; i < usages.Length; i++)
                            {
                                var usage = usages[i];
                                if (!equalityComparer.Equals(usage, oldLinkIndex))
                                {
                                    links.Update(usage, links.GetSource(usage), newLinkIndex);
                                }
                            }
                        }
                        ArrayPool.Free(usages);
                    }
                }
            }
            return newLinkIndex;
        }

        /// <summary>
        /// <para>Replaces one equivalent link with another,by deleting the first one, the links are either updated or deleted.</para>
        /// <para>Заменяет одну эквивалентную связь другой, удаляя первую, ссылки либо обновляются либо удаляются.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links Storage.</para>
        /// <para>Хранилище связи.</para>
        /// </param>
        /// <param name="oldLinkIndex">
        /// <para>The previous link index.</para>
        /// <para>Предыдущий индекс связи.</para>
        /// </param>
        /// <param name="newLinkIndex">
        /// <para>The new link index.</para>
        /// <para>Новый индекс связи.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the new link index.</para>
        /// <para>Возвращает новый индекс свзяи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink MergeAndDelete<TLink>(this ILinks<TLink> links, TLink oldLinkIndex, TLink newLinkIndex)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(oldLinkIndex, newLinkIndex))
            {
                links.MergeUsages(oldLinkIndex, newLinkIndex);
                links.Delete(oldLinkIndex);
            }
            return newLinkIndex;
        }

        /// <summary>
        /// <para>Adds the following decorators to a link: a decorator that solves the problem of cascading links, removing links with empty content, and uniquely-used cascading links.</para>
        /// <para>Дабавляет следующие декораторы к связи:декоратор, разешающий проблему каскадного использования связей, удаления связей с пустым содержанием и уникально-используемых каскадных связей.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище свщяей.</para>
        /// </param>
        /// <returns>
        /// <para>Returns the links.</para>
        /// <para>Возвращает связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILinks<TLink> DecorateWithAutomaticUniquenessAndUsagesResolution<TLink>(this ILinks<TLink> links)
        {
            links = new LinksCascadeUsagesResolver<TLink>(links);
            links = new NonNullContentsLinkDeletionResolver<TLink>(links);
            links = new LinksCascadeUniquenessAndUsagesResolver<TLink>(links);
            return links;
        }

        /// <summary>
        /// <para>Gives the value of link in a specific format.</para>
        /// <para>Выдает значение связи в определенном формате.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The Links Storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="links">
        /// <para>The Link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <returns>
        /// <para>Formatted string with link values.</para>
        /// <para>Форматированная строка со значениями связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<TLink>(this ILinks<TLink> links, IList<TLink> link)
        {
            var constants = links.Constants;
            return $"({link[constants.IndexPart]}: {link[constants.SourcePart]} {link[constants.TargetPart]})";
        }

        /// <summary>
        /// <para>Gets a link by index and displays the resulting link in a specific format.</para>
        /// <para>Получает связь по индексу и выводит полученную связь в определенном формате.</para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>Communication address type.</para>
        /// <para>Тип адреса связи.</para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The Links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        /// <param name="link">
        /// <para>The Link.</para>
        /// <para>Связь.</para>
        /// </param>
        /// <returns>
        /// <para>Formatted string with link values.</para>
        /// <para>Форматированная строка со значениями связи.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<TLink>(this ILinks<TLink> links, TLink link) => links.Format(links.GetLink(link));
    }
}
