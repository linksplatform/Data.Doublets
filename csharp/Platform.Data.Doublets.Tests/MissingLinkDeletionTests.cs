using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            foreach (var decorated in new[] { false, true })
            foreach (var direct in new[] { false, true })
            foreach (var scenario in new[] { "never-created", "deleted-middle", "deleted-last", "empty", "null-address", "out-of-range" })
            {
                yield return new object[] { split, decorated, direct, scenario };
            }
        }

        [Theory]
        [MemberData(nameof(MissingDeleteCases))]
        public static void MissingDeleteThrowsWithoutChangingStore(bool split, bool decorated, bool direct, string scenario)
        {
            CheckMissingDelete<byte>(split, decorated, direct, scenario);
            CheckMissingDelete<ushort>(split, decorated, direct, scenario);
            CheckMissingDelete<uint>(split, decorated, direct, scenario);
            CheckMissingDelete<ulong>(split, decorated, direct, scenario);
        }

        private static void CheckMissingDelete<T>(bool split, bool decorated, bool direct, string scenario)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            Using<T>(split, decorated, links =>
            {
                var first = links.CreatePoint();
                var middle = links.CreatePoint();
                var last = links.CreatePoint();
                var missing = T.CreateTruncating(10);
                var next = last + T.One;
                switch (scenario)
                {
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
                }

                Assert.False(links.Exists(missing));
                var before = Snapshot(links);
                var count = links.Count();
                var calls = 0;
                // Check both handler paths, repeating the invalid deletion each time.
                for (var attempt = 0; attempt < 2; attempt++)
                {
                    Assert.Throws<ArgumentLinkDoesNotExistsException<T>>(() =>
                    {
                        if (direct)
                        {
                            links.Delete(new[] { missing }, attempt == 0 ? null : (_, _) =>
                            {
                                calls++;
                                return links.Constants.Continue;
                            });
                        }
                        else if (attempt == 0)
                        {
                            links.Delete(missing);
                        }
                        else
                        {
                            links.Delete(missing, (_, _) =>
                            {
                                calls++;
                                return links.Constants.Continue;
                            });
                        }
                    });
                    Assert.Equal(0, calls);
                    Assert.Equal(count, links.Count());
                    AssertSnapshot(links, before);
                    Assert.False(links.Exists(missing));
                }

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
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public static void ExistingResetLinkCanBeDeletedWithBreakHandler(bool split, bool decorated)
        {
            CheckExistingDelete<byte>(split, decorated);
            CheckExistingDelete<ushort>(split, decorated);
            CheckExistingDelete<uint>(split, decorated);
            CheckExistingDelete<ulong>(split, decorated);
        }

        private static void CheckExistingDelete<T>(bool split, bool decorated)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            Using<T>(split, decorated, links =>
            {
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
