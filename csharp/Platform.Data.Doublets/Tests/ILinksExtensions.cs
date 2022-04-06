using System;
using System.Runtime.CompilerServices;
using Platform.Ranges;
using Platform.Random;
using Platform.Converters;
using System.Collections.Generic;
using Platform.Numbers;
using Platform.Setters;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// <para>
    /// Represents the links extensions.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class ILinksExtensions
    {
        private static void EnsureTrue(bool boolean) => EnsureTrue(boolean, default);

        private static void EnsureTrue(bool boolean, string message)
        {
            if (boolean)
            {
                return;
            }
            string messageBuilder() => message;
            throw new ArgumentException("EnsureTrue Failed. The value is not a true. " + messageBuilder());
        }

        private static void EnsureEqual<T>(T expected, T actual) => EnsureEqual(expected, actual, default);

        private static void EnsureEqual<T>(T expected, T actual, string message) => EnsureEqual(expected, actual, message, EqualityComparer<T>.Default);

        private static void EnsureEqual<T>(T expected, T actual, string message, IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer.Equals(expected, actual))
            {
                return;
            }
            string messageBuilder() => message;
            throw new ArgumentException("EnsureEqual Failed. The values are not equal. " + messageBuilder());
        }

        /// <summary>
        /// <para>
        /// Tests the crud operations using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="T">
        /// <para>The .</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        public static void TestCRUDOperations<T> (this ILinks<T> links)
        {
            var constants = links.Constants;

            var equalityComparer = EqualityComparer<T>.Default;

            var zero = default(T);
            var one = Arithmetic.Increment(zero);

            // Create Link
            EnsureTrue(equalityComparer.Equals(links.Count(), zero));

            var setter = new Setter<T, T>(constants.Continue, constants.Break, constants.Null);
            links.Each(setter.SetFirstFromNonNullListAndReturnTrue, constants.Any, constants.Any, constants.Any);

            EnsureTrue(equalityComparer.Equals(setter.Result, constants.Null));

            var linkAddress = links.Create();

            var link = new Link<T>(links.GetLink(linkAddress));

            EnsureTrue(link.Count == 3);
            EnsureTrue(equalityComparer.Equals(link.Index, linkAddress));
            EnsureTrue(equalityComparer.Equals(link.Source, constants.Null));
            EnsureTrue(equalityComparer.Equals(link.Target, constants.Null));

            EnsureTrue(equalityComparer.Equals(links.Count(), one));

            // Get first link
            setter = new Setter<T, T>(constants.Continue, constants.Break, constants.Null);
            links.Each(setter.SetFirstFromNonNullListAndReturnTrue, constants.Any, constants.Any, constants.Any);

            EnsureTrue(equalityComparer.Equals(setter.Result, linkAddress));

            // Update link to reference itself
            links.Update(linkAddress, linkAddress, linkAddress);

            link = new Link<T>(links.GetLink(linkAddress));

            EnsureTrue(equalityComparer.Equals(link.Source, linkAddress));
            EnsureTrue(equalityComparer.Equals(link.Target, linkAddress));

            // Update link to reference null (prepare for delete)
            var updated = links.Update(linkAddress, constants.Null, constants.Null);

            EnsureTrue(equalityComparer.Equals(updated, linkAddress));

            link = new Link<T>(links.GetLink(linkAddress));

            EnsureTrue(equalityComparer.Equals(link.Source, constants.Null));
            EnsureTrue(equalityComparer.Equals(link.Target, constants.Null));

            // Delete link
            links.Delete(linkAddress);

            EnsureTrue(equalityComparer.Equals(links.Count(), zero));

            setter = new Setter<T, T>(constants.Continue, constants.Break, constants.Null);
            links.Each(setter.SetFirstFromNonNullListAndReturnTrue, constants.Any, constants.Any, constants.Any);

            EnsureTrue(equalityComparer.Equals(setter.Result, constants.Null));
        }

        /// <summary>
        /// <para>
        /// Tests the raw numbers crud operations using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="T">
        /// <para>The .</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        public static void TestRawNumbersCRUDOperations<T>(this ILinks<T> links)
        {
            // Constants
            var constants = links.Constants;
            var equalityComparer = EqualityComparer<T>.Default;

            var zero = default(T);
            var one = Arithmetic.Increment(zero);
            var two = Arithmetic.Increment(one);

            var h106E = new Hybrid<T>(106L, isExternal: true);
            var h107E = new Hybrid<T>(-char.ConvertFromUtf32(107)[0]);
            var h108E = new Hybrid<T>(-108L);

            EnsureEqual(106L, h106E.AbsoluteValue);
            EnsureEqual(107L, h107E.AbsoluteValue);
            EnsureEqual(108L, h108E.AbsoluteValue);

            // Create Link (External -> External)
            var linkAddress1 = links.Create();

            links.Update(linkAddress1, h106E, h108E);

            var link1 = new Link<T>(links.GetLink(linkAddress1));

            EnsureTrue(equalityComparer.Equals(link1.Source, h106E));
            EnsureTrue(equalityComparer.Equals(link1.Target, h108E));

            // Create Link (Internal -> External)
            var linkAddress2 = links.Create();

            links.Update(linkAddress2, linkAddress1, h108E);

            var link2 = new Link<T>(links.GetLink(linkAddress2));

            EnsureTrue(equalityComparer.Equals(link2.Source, linkAddress1));
            EnsureTrue(equalityComparer.Equals(link2.Target, h108E));

            // Create Link (Internal -> Internal)
            var linkAddress3 = links.Create();

            links.Update(linkAddress3, linkAddress1, linkAddress2);

            var link3 = new Link<T>(links.GetLink(linkAddress3));

            EnsureTrue(equalityComparer.Equals(link3.Source, linkAddress1));
            EnsureTrue(equalityComparer.Equals(link3.Target, linkAddress2));

            // Search for created link
            var setter1 = new Setter<T, T>(constants.Continue, constants.Break, constants.Null);
            links.Each(setter1.SetFirstAndReturnFalse, constants.Any, h106E, h108E);

            EnsureTrue(equalityComparer.Equals(setter1.Result, linkAddress1));

            // Search for nonexistent link
            var setter2 = new Setter<T, T>(constants.Continue, constants.Break, constants.Null);
            links.Each(setter2.SetFirstAndReturnFalse, constants.Any, h106E, h107E);

            EnsureTrue(equalityComparer.Equals(setter2.Result, constants.Null));

            // Update link to reference null (prepare for delete)
            var updated = links.Update(linkAddress3, constants.Null, constants.Null);

            EnsureTrue(equalityComparer.Equals(updated, linkAddress3));

            link3 = new Link<T>(links.GetLink(linkAddress3));

            EnsureTrue(equalityComparer.Equals(link3.Source, constants.Null));
            EnsureTrue(equalityComparer.Equals(link3.Target, constants.Null));

            // Delete link
            links.Delete(linkAddress3);

            EnsureTrue(equalityComparer.Equals(links.Count(), two));

            var setter3 = new Setter<T, T>(constants.Continue, constants.Break, constants.Null);
            links.Each(setter3.SetFirstAndReturnFalse, constants.Any, constants.Any, constants.Any);

            EnsureTrue(equalityComparer.Equals(setter3.Result, linkAddress2));
        }

        /// <summary>
        /// <para>
        /// Tests the multiple random creations and deletions using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>The link.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="maximumOperationsPerCycle">
        /// <para>The maximum operations per cycle.</para>
        /// <para></para>
        /// </param>
        public static void TestMultipleRandomCreationsAndDeletions<TLink>(this ILinks<TLink> links, int maximumOperationsPerCycle)
        {
            var comparer = Comparer<TLink>.Default;
            var addressToUInt64Converter = CheckedConverter<TLink, ulong>.Default;
            var uInt64ToAddressConverter = CheckedConverter<ulong, TLink>.Default;
            for (var N = 1; N < maximumOperationsPerCycle; N++)
            {
                var random = new System.Random(N);
                var createdAddresses = new List<TLink>();
                var created = 0UL;
                var deleted = 0UL;
                for (var i = 0; i < N; i++)
                {
                    var createPoint = random.NextBoolean();
                    if (created >= 2 && createPoint)
                    {
                        var linksAddressRange = new Range<ulong>(0, created);
                        var source = uInt64ToAddressConverter.Convert(createdAddresses[random.NextUInt64(linksAddressRange)]);
                        var target = uInt64ToAddressConverter.Convert(createdAddresses[random.NextUInt64(linksAddressRange)]); //-V3086
                        var resultLink = links.GetOrCreate(source, target);
                        if (comparer.Compare(resultLink, default) > 0)
                        {
                            createdAddresses.Add(resultLink);
                            created++;
                        }
                    }
                    else
                    {
                        createdAddresses.Add(links.Create());
                        created++;
                    }
                }
                EnsureTrue(created == addressToUInt64Converter.Convert(links.Count()));
                var allLinks = links.All();
                var linksAddressRange = new Range<ulong>(0, created);
                // Random deletions
                for (var i = 0; i < N; i++)
                {
                    var id = uInt64ToAddressConverter.Convert(createdAddresses[random.NextUInt64(linksAddressRange)]);
                    if (links.Exists(id))
                    {
                        links.Delete(id);
                    }
                }
                // Delete all remaining links
                foreach (var linkToDelete in createdAddresses)
                {
                    if (links.Exists(id))
                    {
                        links.Delete(id);
                    }
                }
                EnsureTrue(addressToUInt64Converter.Convert(links.Count()) == 0L);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="links"></param>
        /// <param name="numberOfOperations"></param>
        /// <typeparam name="TLinkAddress"></typeparam>
        public static void TestMultipleCreationsAndDeletions<TLinkAddress>(this ILinks<TLinkAddress> links, int numberOfOperations)
        {
            for (int i = 0; i < numberOfOperations; i++)
            {
                links.Create();
            }
            for (int i = 0; i < numberOfOperations; i++)
            {
                links.Delete(links.Count());
            }
        }

        /// <summary>
        /// <para>
        /// Runs the random creations using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLink">
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
        /// <para>
        /// Runs the random searches using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLink">
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
        /// <para>
        /// Runs the random deletions using the specified links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLink">
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
    }
}
