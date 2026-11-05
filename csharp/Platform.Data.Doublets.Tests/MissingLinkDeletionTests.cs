using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Exceptions;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class MissingLinkDeletionTests
    {
        public static IEnumerable<object[]> MissingDeleteCases()
        {
            foreach (var split in new[] { false, true })
            {
                foreach (var decorated in new[] { false, true })
                {
                    foreach (var direct in new[] { false, true })
                    {
                        foreach (var scenario in new[] { "never-created", "deleted-middle", "deleted-last", "empty", "null-address", "out-of-range" })
                        {
                            foreach (var ignoreMissing in new[] { false, true })
                            {
                                yield return new object[] { split, decorated, direct, scenario, ignoreMissing };
                            }
                        }
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(MissingDeleteCases))]
        public static void MissingDeleteUsesSelectedPolicyWithoutChangingStore(bool split, bool decorated, bool direct, string scenario, bool ignoreMissing)
        {
            CheckMissingDelete<byte>(split, decorated, direct, scenario, ignoreMissing);
            CheckMissingDelete<ushort>(split, decorated, direct, scenario, ignoreMissing);
            CheckMissingDelete<uint>(split, decorated, direct, scenario, ignoreMissing);
            CheckMissingDelete<ulong>(split, decorated, direct, scenario, ignoreMissing);
        }

        private static void CheckMissingDelete<T>(bool split, bool decorated, bool direct, string scenario, bool ignoreMissing)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            Using<T>(split, decorated, inner =>
            {
                ILinks<T> links = ignoreMissing
                    ? new LinksNonExistentDeletionResolver<T>(inner)
                    : new LinksInnerReferenceExistenceValidator<T>(inner);
                var first = links.CreatePoint();
                var middle = links.CreatePoint();
                var last = links.CreatePoint();
                var missing = T.CreateTruncating(10);
                var next = last + T.One;
                switch (scenario)
                {
                    case "never-created":
                        break;
                    case "deleted-middle":
                        ResetAndDelete(links, middle);
                        missing = next = middle;
                        break;
                    case "deleted-last":
                        ResetAndDelete(links, last);
                        missing = next = last;
                        break;
                    case "empty":
                        ResetAndDelete(links, last);
                        ResetAndDelete(links, middle);
                        ResetAndDelete(links, first);
                        missing = next = first;
                        break;
                    case "null-address":
                        missing = links.Constants.Null;
                        break;
                    case "out-of-range":
                        missing = T.MaxValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown deletion scenario.");
                }

                Assert.False(links.Exists(missing));
                var before = Snapshot(links);
                var count = links.Count();
                var calls = 0;
                // Check both handler paths, repeating the invalid deletion each time.
                for (var attempt = 0; attempt < 2; attempt++)
                {
                    void DeleteMissing()
                    {
                        T result;
                        if (direct)
                        {
                            result = links.Delete(new[] { missing }, attempt == 0 ? null : (_, _) =>
                            {
                                calls++;
                                return links.Constants.Continue;
                            });
                        }
                        else if (attempt == 0)
                        {
                            result = links.Delete(missing, null);
                        }
                        else
                        {
                            result = links.Delete(missing, (_, _) =>
                            {
                                calls++;
                                return links.Constants.Continue;
                            });
                        }
                        Assert.Equal(links.Constants.Continue, result);
                    }
                    if (ignoreMissing)
                    {
                        DeleteMissing();
                    }
                    else
                    {
                        Assert.Throws<ArgumentLinkDoesNotExistsException<T>>(DeleteMissing);
                    }
                    Assert.Equal(0, calls);
                    Assert.Equal(count, links.Count());
                    AssertSnapshot(links, before);
                    Assert.False(links.Exists(missing));
                }

                // The no-handler adapter returns an address, rather than a flow constant.
                if (ignoreMissing)
                {
                    Assert.Equal(links.Constants.Null, links.Delete(missing));
                }
                else
                {
                    Assert.Throws<ArgumentLinkDoesNotExistsException<T>>(() => links.Delete(missing));
                }
                Assert.Equal(count, links.Count());
                AssertSnapshot(links, before);

                // A double free corrupts allocation even when enumeration still looks right.
                Assert.Equal(next, links.CreatePoint());
                var following = scenario == "deleted-middle" ? last + T.One : next + T.One;
                Assert.Equal(following, links.CreatePoint());
                Assert.Equal(count + T.CreateTruncating(2), links.Count());
                Assert.Equal(int.CreateTruncating(links.Count()), Snapshot(links).Length);
                foreach (var row in before)
                {
                    Assert.Equal(row, links.GetLink(row[0]));
                    Assert.Equal(row[0], links.SearchOrDefault(row[1], row[2]));
                }
                Assert.Equal(new[] { next, next, next }, links.GetLink(next));
                Assert.Equal(new[] { following, following, following }, links.GetLink(following));
            });
        }

        [Theory]
        [InlineData(false, false, 0)]
        [InlineData(false, false, 1)]
        [InlineData(false, false, 2)]
        [InlineData(false, true, 0)]
        [InlineData(false, true, 1)]
        [InlineData(false, true, 2)]
        [InlineData(true, false, 0)]
        [InlineData(true, false, 1)]
        [InlineData(true, false, 2)]
        [InlineData(true, true, 0)]
        [InlineData(true, true, 1)]
        [InlineData(true, true, 2)]
        public static void ExistingResetLinkCanBeDeletedWithBreakHandler(bool split, bool decorated, int policy)
        {
            CheckExistingDelete<byte>(split, decorated, policy);
            CheckExistingDelete<ushort>(split, decorated, policy);
            CheckExistingDelete<uint>(split, decorated, policy);
            CheckExistingDelete<ulong>(split, decorated, policy);
        }

        private static void CheckExistingDelete<T>(bool split, bool decorated, int policy)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            Using<T>(split, decorated, inner =>
            {
                ILinks<T> links = policy switch
                {
                    1 => new LinksInnerReferenceExistenceValidator<T>(inner),
                    2 => new LinksNonExistentDeletionResolver<T>(inner),
                    _ => inner
                };
                var index = links.CreatePoint();
                links.Update(index, T.Zero, T.Zero);
                var calls = 0;
                var result = links.Delete(new[] { index }, (before, after) =>
                {
                    calls++;
                    Assert.Equal(new[] { index, T.Zero, T.Zero }, before);
                    Assert.Null(after);
                    return links.Constants.Break;
                });
                Assert.Equal(links.Constants.Break, result);
                Assert.Equal(1, calls);
                Assert.False(links.Exists(index));
                Assert.Equal(T.Zero, links.Count());
                Assert.Equal(index, links.CreatePoint());
            });
        }

        private static void Using<T>(bool split, bool decorated, Action<ILinks<T>> action)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            using var united = split ? null : new UnitedMemoryLinks<T>(new HeapResizableDirectMemory(), 4096);
            using var splitLinks = split
                ? new SplitMemoryLinks<T>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096)
                : null;
            ILinks<T> links = split ? splitLinks! : united!;
            action(decorated ? links.DecorateWithAutomaticUniquenessAndUsagesResolution() : links);
        }

        private static T[][] Snapshot<T>(ILinks<T> links) where T : IUnsignedNumber<T>
        {
            var rows = new List<T[]>();
            links.Each(Array.Empty<T>(), link =>
            {
                rows.Add(link!.ToArray());
                return links.Constants.Continue;
            });
            return rows.ToArray();
        }

        private static void ResetAndDelete<T>(ILinks<T> links, T index) where T : IUnsignedNumber<T>
        {
            // Plain stores require endpoints to be reset before a valid deletion.
            links.Update(index, T.Zero, T.Zero);
            links.Delete(index);
        }

        private static void AssertSnapshot<T>(ILinks<T> links, T[][] expected) where T : IUnsignedNumber<T>
        {
            var actual = Snapshot(links);
            Assert.Equal(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }
    }
}
