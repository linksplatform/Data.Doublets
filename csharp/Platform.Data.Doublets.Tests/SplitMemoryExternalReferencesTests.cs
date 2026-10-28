using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class SplitMemoryExternalReferencesTests
    {
        public static IEnumerable<object[]> Configurations()
        {
            yield return new object[] { IndexTreeType.Default, true };
            yield return new object[] { IndexTreeType.SizeBalancedTree, true };
            yield return new object[] { IndexTreeType.SizeBalancedTree, false };
            yield return new object[] { IndexTreeType.RecursionlessSizeBalancedTree, true };
            yield return new object[] { IndexTreeType.RecursionlessSizeBalancedTree, false };
        }

        public static IEnumerable<object[]> ExactQueries()
        {
            foreach (var configuration in Configurations())
            {
                foreach (var operation in new[] { "Each", "Count", "SearchOrDefault" })
                {
                    yield return new[] { configuration[0], configuration[1], operation };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ExactQueries))]
        public static void InternalSourceAndExternalTargetAreFound(IndexTreeType tree, bool useLinkedList, string operation)
        {
            CheckExactQuery<byte>(tree, useLinkedList, operation);
            CheckExactQuery<ushort>(tree, useLinkedList, operation);
            CheckExactQuery<uint>(tree, useLinkedList, operation);
            CheckExactQuery<ulong>(tree, useLinkedList, operation);
        }

        private static void CheckExactQuery<T>(IndexTreeType tree, bool useLinkedList, string operation)
            where T : IUnsignedNumber<T>, IComparisonOperators<T, T, bool>
        {
            using var links = Create<T>(tree, useLinkedList);
            var source = links.CreatePoint();
            var target = new Hybrid<T>(T.CreateTruncating(20), isExternal: true).Value;
            var link = links.CreateAndUpdate(source, target);
            Assert.Equal(source, links.GetSource(link));
            Assert.Equal(target, links.GetTarget(link));
            switch (operation)
            {
                case "Each":
                    Assert.Equal(new[] { link }, Find(links, source, target));
                    break;
                case "Count":
                    Assert.Equal(T.One, links.Count(links.Constants.Any, source, target));
                    break;
                case "SearchOrDefault":
                    Assert.Equal(link, links.SearchOrDefault(source, target));
                    break;
            }
        }

        [Theory]
        [MemberData(nameof(Configurations))]
        public static void QueriesMatchScanAfterEndpointChanges(IndexTreeType tree, bool useLinkedList)
        {
            using var links = Create<ulong>(tree, useLinkedList);
            var a = links.CreatePoint();
            var b = links.CreatePoint();
            var x = new Hybrid<ulong>(20, isExternal: true).Value;
            var y = new Hybrid<ulong>(21, isExternal: true).Value;
            var values = new[] { a, b, x, y };
            var mixed = links.CreateAndUpdate(a, x);
            links.CreateAndUpdate(a, y); // Same source, different external target.
            links.CreateAndUpdate(b, x); // Same external target, different source.
            links.CreateAndUpdate(x, a);
            links.CreateAndUpdate(x, y);
            AssertQueriesMatchScan(links, values);

            // Exercise detachment and attachment across all endpoint categories.
            foreach (var pair in new[] { (b, y), (y, b), (y, x), (a, b), (a, x) })
            {
                links.Update(mixed, pair.Item1, pair.Item2);
                AssertQueriesMatchScan(links, values);
            }
            Assert.Equal(mixed, links.GetOrCreate(a, x));
            AssertQueriesMatchScan(links, values);

            // Raw stores require endpoints to be reset before deletion.
            links.Update(mixed, links.Constants.Null, links.Constants.Null);
            links.Delete(mixed);
            AssertQueriesMatchScan(links, values);
            Assert.Equal(mixed, links.CreateAndUpdate(a, x)); // Reuse the free slot.
            AssertQueriesMatchScan(links, values);
        }

        private static SplitMemoryLinks<T> Create<T>(IndexTreeType tree, bool useLinkedList)
            where T : IUnsignedNumber<T>, IComparisonOperators<T, T, bool>
        {
            // Reserve at least one page for the memory allocator, without the default million-link allocation.
            return new SplitMemoryLinks<T>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(),
                4096, new LinksConstants<T>(enableExternalReferencesSupport: true), tree, useLinkedList);
        }

        private static T[] Find<T>(ILinks<T> links, T source, T target) where T : IUnsignedNumber<T>
        {
            var found = new List<T>();
            links.Each(link =>
            {
                found.Add(link![0]);
                return links.Constants.Continue;
            }, links.Constants.Any, source, target);
            return found.OrderBy(index => index).ToArray();
        }

        private static void AssertQueriesMatchScan(ILinks<ulong> links, ulong[] values)
        {
            var all = new List<ulong[]>();
            links.Each(link =>
            {
                all.Add(link!.ToArray());
                return links.Constants.Continue;
            });
            var any = links.Constants.Any;
            foreach (var source in values.Append(any))
            {
                foreach (var target in values.Append(any))
                {
                    var expected = all.Where(link => (source == any || link[1] == source)
                        && (target == any || link[2] == target)).Select(link => link[0]).OrderBy(index => index).ToArray();
                    Assert.Equal(expected, Find(links, source, target));
                    Assert.Equal((ulong)expected.Length, links.Count(any, source, target));
                    if (source != any && target != any)
                    {
                        Assert.Equal(expected.SingleOrDefault(), links.SearchOrDefault(source, target));
                        var calls = 0;
                        var result = links.Each(link =>
                        {
                            calls++;
                            Assert.Equal(expected.Single(), link![0]);
                            return links.Constants.Break;
                        }, any, source, target);
                        Assert.Equal(expected.Length, calls);
                        Assert.Equal(expected.Length == 0 ? links.Constants.Continue : links.Constants.Break, result);
                    }
                }
            }
        }
    }
}
