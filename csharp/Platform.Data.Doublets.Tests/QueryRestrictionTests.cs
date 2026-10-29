using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class QueryRestrictionTests
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public static void NullCountRestrictionCountsAllLiveLinks(bool split, bool decorated)
        {
            CheckNullCountRestrictionCountsAllLiveLinks<byte>(split, decorated);
            CheckNullCountRestrictionCountsAllLiveLinks<ushort>(split, decorated);
            CheckNullCountRestrictionCountsAllLiveLinks<uint>(split, decorated);
            CheckNullCountRestrictionCountsAllLiveLinks<ulong>(split, decorated);
        }

        private static void CheckNullCountRestrictionCountsAllLiveLinks<T>(bool split, bool decorated)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            Using<T>(split, decorated, links =>
            {
                Assert.Equal(T.Zero, links.Count(restriction: null));
                Assert.Equal(links.Count(Array.Empty<T>()), links.Count(restriction: null));
                var first = links.CreatePoint();
                var deleted = links.CreatePoint();
                var last = links.CreatePoint();
                AssertCount(links, 3);

                ResetAndDelete(links, deleted);
                AssertCount(links, 2);
                Assert.Equal(deleted, links.CreatePoint());
                AssertCount(links, 3);

                ResetAndDelete(links, last);
                ResetAndDelete(links, deleted);
                ResetAndDelete(links, first);
                AssertCount(links, 0);
            });
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public static void NullEachRestrictionEnumeratesAllLiveLinks(bool split, bool decorated)
        {
            CheckNullEachRestrictionEnumeratesAllLiveLinks<byte>(split, decorated);
            CheckNullEachRestrictionEnumeratesAllLiveLinks<ushort>(split, decorated);
            CheckNullEachRestrictionEnumeratesAllLiveLinks<uint>(split, decorated);
            CheckNullEachRestrictionEnumeratesAllLiveLinks<ulong>(split, decorated);
        }

        private static void CheckNullEachRestrictionEnumeratesAllLiveLinks<T>(bool split, bool decorated)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            Using<T>(split, decorated, links =>
            {
                AssertScan(links);
                var first = links.CreatePoint();
                var deleted = links.CreatePoint();
                var last = links.CreatePoint();
                AssertScan(links, first, deleted, last);

                ResetAndDelete(links, deleted);
                AssertScan(links, first, last);
                Assert.Equal(deleted, links.CreatePoint());
                AssertScan(links, first, deleted, last);

                ResetAndDelete(links, last);
                ResetAndDelete(links, deleted);
                ResetAndDelete(links, first);
                AssertScan(links);
            });
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public static void NullEachRestrictionHonorsBreak(bool split, bool decorated)
        {
            CheckNullEachRestrictionHonorsBreak<byte>(split, decorated);
            CheckNullEachRestrictionHonorsBreak<ushort>(split, decorated);
            CheckNullEachRestrictionHonorsBreak<uint>(split, decorated);
            CheckNullEachRestrictionHonorsBreak<ulong>(split, decorated);
        }

        private static void CheckNullEachRestrictionHonorsBreak<T>(bool split, bool decorated)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            Using<T>(split, decorated, links =>
            {
                var first = links.CreatePoint();
                links.CreatePoint();
                foreach (var restriction in new T[]?[] { null, Array.Empty<T>() })
                {
                    var calls = 0;
                    var result = links.Each(restriction, link =>
                    {
                        calls++;
                        Assert.Equal(first, link![0]);
                        return links.Constants.Break;
                    });
                    Assert.Equal(1, calls);
                    Assert.Equal(links.Constants.Break, result);
                }
            });
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public static void UnsupportedCountRestrictionHasEnglishMessage(bool split, bool decorated)
        {
            CheckUnsupportedCountRestrictionHasEnglishMessage<byte>(split, decorated);
            CheckUnsupportedCountRestrictionHasEnglishMessage<ushort>(split, decorated);
            CheckUnsupportedCountRestrictionHasEnglishMessage<uint>(split, decorated);
            CheckUnsupportedCountRestrictionHasEnglishMessage<ulong>(split, decorated);
        }

        private static void CheckUnsupportedCountRestrictionHasEnglishMessage<T>(bool split, bool decorated)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            Using<T>(split, decorated, links =>
            {
                var any = links.Constants.Any;
                var exception = Assert.Throws<NotSupportedException>(() => links.Count(new[] { any, any, any, any }));
                Assert.Equal("Other restriction sizes and methods are not supported.", exception.Message);
            });
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public static void UnsupportedEachRestrictionHasEnglishMessage(bool split, bool decorated)
        {
            CheckUnsupportedEachRestrictionHasEnglishMessage<byte>(split, decorated);
            CheckUnsupportedEachRestrictionHasEnglishMessage<ushort>(split, decorated);
            CheckUnsupportedEachRestrictionHasEnglishMessage<uint>(split, decorated);
            CheckUnsupportedEachRestrictionHasEnglishMessage<ulong>(split, decorated);
        }

        private static void CheckUnsupportedEachRestrictionHasEnglishMessage<T>(bool split, bool decorated)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            Using<T>(split, decorated, links =>
            {
                var any = links.Constants.Any;
                var calls = 0;
                var exception = Assert.Throws<NotSupportedException>(() => links.Each(new[] { any, any, any, any }, _ =>
                {
                    calls++;
                    return links.Constants.Continue;
                }));
                Assert.Equal("Other restriction sizes and methods are not supported.", exception.Message);
                Assert.Equal(0, calls);
            });
        }

        private static void Using<T>(bool split, bool decorated, Action<ILinks<T>> action)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            // Keep these small regression stores within a single allocator page.
            using var united = split ? null : new UnitedMemoryLinks<T>(new HeapResizableDirectMemory(), 4096);
            using var splitLinks = split
                ? new SplitMemoryLinks<T>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096)
                : null;
            ILinks<T> links = split ? splitLinks! : united!;
            action(decorated ? links.DecorateWithAutomaticUniquenessAndUsagesResolution() : links);
        }

        private static void AssertCount<T>(ILinks<T> links, int expected) where T : IUnsignedNumber<T>
        {
            Assert.Equal(T.CreateTruncating(expected), links.Count(restriction: null));
            Assert.Equal(links.Count(Array.Empty<T>()), links.Count(restriction: null));
        }

        private static void AssertScan<T>(ILinks<T> links, params T[] expected) where T : IUnsignedNumber<T>
        {
            var emptyRows = new List<T[]>();
            var emptyResult = links.Each(Array.Empty<T>(), link =>
            {
                emptyRows.Add(link!.ToArray());
                return links.Constants.Continue;
            });
            var nullRows = new List<T[]>();
            var nullResult = links.Each(restriction: null, handler: link =>
            {
                nullRows.Add(link!.ToArray());
                return links.Constants.Continue;
            });
            Assert.Equal(expected, nullRows.Select(link => link[0]));
            Assert.Equal(emptyResult, nullResult);
            Assert.Equal(emptyRows.Count, nullRows.Count);
            for (var i = 0; i < nullRows.Count; i++)
            {
                Assert.Equal(new[] { expected[i], expected[i], expected[i] }, nullRows[i]);
                Assert.Equal(emptyRows[i], nullRows[i]);
            }
        }

        private static void ResetAndDelete<T>(ILinks<T> links, T index) where T : IUnsignedNumber<T>
        {
            // Raw stores require endpoint values to be reset before deletion.
            links.Update(index, links.Constants.Null, links.Constants.Null);
            links.Delete(index);
        }
    }
}
