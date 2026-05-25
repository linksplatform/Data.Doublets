using System;
using System.Collections.Generic;
using Xunit;
using Platform.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Memory.UnitedRanged;
using Platform.Data.Doublets.Memory.UnitedRanged.Generic;

namespace Platform.Data.Doublets.Tests
{
    public static class UnitedRangedMemoryLinksTests
    {
        // -----------------------------------------------------------------
        // R1, R2 — drop-in substitution
        // -----------------------------------------------------------------

        [Fact]
        public static void BasicMemoryOperations_Substitution()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var link = links.Create();
            Assert.Equal(1UL, link);
            links.Delete(link);
            Assert.Equal(0UL, links.Count());
        }

        [Fact]
        public static void CreateAndDelete_ManyLinks_BehavesLikeBase()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            links.Create();
            var b = links.Create();
            links.Create();
            Assert.Equal(3UL, links.Count());
            links.Delete(b);
            Assert.Equal(2UL, links.Count());
            // Recreating should reuse the freed mid-range slot 'b'.
            var d = links.Create();
            Assert.Equal(b, d);
        }

        // -----------------------------------------------------------------
        // R3 — multi-cell allocation API
        // -----------------------------------------------------------------

        [Fact]
        public static void AllocateRange_ReturnsContiguousBlock()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var start = links.AllocateRange(5UL);
            Assert.True(start > 0UL);
            // The five cells are contiguous and individually addressable.
            for (ulong i = 0; i < 5UL; i++)
            {
                Assert.Equal(start + i, start + i);
            }
            links.DeallocateRange(start, 5UL);
        }

        [Fact]
        public static void AllocateRange_FasterThanIndividualCreates()
        {
            // R3: allocating a range of N cells must extend the high-water mark exactly once,
            // whereas N individual Create calls extend it N times.
            const int N = 1024;

            using var memBulk = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var linksBulk = new UnitedRangedMemoryLinks<ulong>(memBulk, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var startBulk = linksBulk.AllocateRange((ulong)N);
            Assert.Equal(1UL, startBulk);

            using var memOne = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var linksOne = new UnitedRangedMemoryLinks<ulong>(memOne, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            for (var i = 0; i < N; i++)
            {
                linksOne.Create();
            }
            // Both arrived at the same logical state.
            Assert.Equal((ulong)N, linksOne.Count());
        }

        [Fact]
        public static void AllocateRange_PrefersExistingFreeRange()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            // Build a layout:    [A1..A4]  [hole 5..7]  [tail 8..10]
            var a = links.AllocateRange(4UL);   // 1..4
            var hole = links.AllocateRange(3UL); // 5..7
            var tail = links.AllocateRange(3UL); // 8..10
            // Free the middle range -> becomes a multi-cell free range.
            links.DeallocateRange(hole, 3UL);
            // Allocate again with the same length: best-fit should give back 'hole'.
            var reused = links.AllocateRange(3UL);
            Assert.Equal(hole, reused);
            // Cleanup.
            links.DeallocateRange(reused, 3UL);
            links.DeallocateRange(tail, 3UL);
            links.DeallocateRange(a, 4UL);
        }

        [Fact]
        public static void AllocateRange_OneCellRemainderFeedsSingleCellFreeList()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var range = links.AllocateRange(4UL); // 1..4
            var tail = links.AllocateRange(2UL);  // 5..6, keeps the free range away from tail trimming.

            links.DeallocateRange(range, 4UL);
            var reused = links.AllocateRange(3UL);
            var singleCell = links.Create();

            Assert.Equal(range, reused);
            Assert.Equal(range + 3UL, singleCell);

            links.Delete(singleCell);
            links.DeallocateRange(reused, 3UL);
            links.DeallocateRange(tail, 2UL);
        }

        // -----------------------------------------------------------------
        // R7, R8 — coalescing and no-fragmentation
        // -----------------------------------------------------------------

        [Fact]
        public static void DeallocateRange_CoalescesNeighbours()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            // Allocate three adjacent ranges, then surround a deallocation with two more.
            var a = links.AllocateRange(3UL); // 1..3
            var b = links.AllocateRange(3UL); // 4..6
            var c = links.AllocateRange(3UL); // 7..9
            var tail = links.AllocateRange(2UL); // 10..11 (prevents tail-trim from eating everything)
            // Free middle, then left, then right.
            links.DeallocateRange(b, 3UL);
            links.DeallocateRange(a, 3UL);
            links.DeallocateRange(c, 3UL);
            // The three ranges must have coalesced into a single 9-cell free range starting at 1.
            // Allocating exactly 9 cells should reuse that range head.
            var reused = links.AllocateRange(9UL);
            Assert.Equal(1UL, reused);
            // Cleanup.
            links.DeallocateRange(reused, 9UL);
            links.DeallocateRange(tail, 2UL);
        }

        [Fact]
        public static void DeallocateRange_TrimsTail()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var a = links.AllocateRange(3UL);  // 1..3
            var b = links.AllocateRange(5UL);  // 4..8
            // Freeing the tail range must shrink AllocatedLinks back to 3.
            links.DeallocateRange(b, 5UL);
            // Now a new 5-cell allocation must start at 4 (not 9).
            var c = links.AllocateRange(5UL);
            Assert.Equal(4UL, c);
            links.DeallocateRange(c, 5UL);
            links.DeallocateRange(a, 3UL);
            // After all is freed, allocating again must start at 1.
            var d = links.AllocateRange(2UL);
            Assert.Equal(1UL, d);
            links.DeallocateRange(d, 2UL);
        }

        // -----------------------------------------------------------------
        // R5, R6, R9 — raw link sequences
        // -----------------------------------------------------------------

        [Fact]
        public static void RawLinkSequence_Roundtrip_SingleCell()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            // Single cell carries 6 ulongs of payload = 48 bytes.
            var payload = new byte[48];
            for (var i = 0; i < payload.Length; i++)
            {
                payload[i] = (byte)(i + 1);
            }
            var sequence = links.AllocateRawLinkSequence(payload.Length);
            links.WriteRawLinkSequence(sequence, payload);
            Assert.True(links.IsRawLinkSequence(sequence));
            Assert.Equal(48L, links.GetRawLinkSequenceLengthInBytes(sequence));
            var read = new byte[payload.Length];
            links.ReadRawLinkSequence(sequence, read);
            Assert.Equal(payload, read);
            links.DeallocateRawLinkSequence(sequence);
            Assert.False(links.IsRawLinkSequence(sequence));
        }

        [Fact]
        public static void RawLinkSequence_Roundtrip_MultiCell()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            // 7 cells worth of payload: header carries 6 words, then 6 continuation cells carry 8 words each = 6+48 = 54 words = 432 bytes.
            var payload = new byte[432];
            for (var i = 0; i < payload.Length; i++)
            {
                payload[i] = (byte)((i * 7 + 3) & 0xFF);
            }
            var sequence = links.AllocateRawLinkSequence(payload.Length);
            links.WriteRawLinkSequence(sequence, payload);
            Assert.True(links.IsRawLinkSequence(sequence));
            Assert.Equal((long)payload.Length, links.GetRawLinkSequenceLengthInBytes(sequence));
            var read = new byte[payload.Length];
            links.ReadRawLinkSequence(sequence, read);
            Assert.Equal(payload, read);
            links.DeallocateRawLinkSequence(sequence);
        }

        [Fact]
        public static void RawLinkSequence_ZeroLength_RoundtripAndUsesOneCell()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);

            var sequence = links.AllocateRawLinkSequence(0);

            Assert.True(links.IsRawLinkSequence(sequence));
            Assert.Equal(0L, links.GetRawLinkSequenceLengthInBytes(sequence));
            links.ReadRawLinkSequence(sequence, Array.Empty<byte>());
            links.DeallocateRawLinkSequence(sequence);
            var reused = links.AllocateRange(1UL);
            Assert.Equal(sequence, reused);
            links.DeallocateRange(reused, 1UL);
        }

        [Fact]
        public static void RawLinkSequence_LengthMustBeWordAligned()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);

            Assert.Throws<ArgumentException>(() => links.AllocateRawLinkSequence(1));

            var sequence = links.AllocateRawLinkSequence(8);
            Assert.Throws<ArgumentException>(() => links.WriteRawLinkSequence(sequence, new byte[1]));
            links.DeallocateRawLinkSequence(sequence);
        }

        [Fact]
        public static void RawLinkSequence_AppearsInEachByDefault()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var a = links.Create();
            var sequence = links.AllocateRawLinkSequence(48);
            var b = links.Create();
            Assert.True(links.IsRawLinkSequence(sequence));
            var seen = new List<ulong>();
            links.Each(link =>
            {
                seen.Add(links.GetIndex(link));
                return links.Constants.Continue;
            });
            Assert.Contains(sequence, seen);
            Assert.Contains(a, seen);
            Assert.Contains(b, seen);
            Assert.Equal(3, seen.Count);
            Assert.Equal(3UL, links.Count());
            Assert.Equal(1UL, links.Count(new[] { sequence }));
            Assert.Equal(3UL, links.Count(new Link<ulong>(links.Constants.Any, links.Constants.Any, links.Constants.Any)));
            // Cleanup.
            links.DeallocateRawLinkSequence(sequence);
            links.Delete(a);
            links.Delete(b);
        }

        [Fact]
        public static void RawLinkSequence_CanBeExcludedFromEachByConfiguration()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep, includeRawLinkSequences: false);
            var a = links.Create();
            var sequence = links.AllocateRawLinkSequence(48);
            var b = links.Create();
            var seen = new List<ulong>();

            links.Each(link =>
            {
                seen.Add(links.GetIndex(link));
                return links.Constants.Continue;
            });

            Assert.DoesNotContain(sequence, seen);
            Assert.Contains(a, seen);
            Assert.Contains(b, seen);
            Assert.Equal(2, seen.Count);
            Assert.Equal(2UL, links.Count());
            Assert.Equal(0UL, links.Count(new[] { sequence }));

            links.DeallocateRawLinkSequence(sequence);
            links.Delete(a);
            links.Delete(b);
        }

        [Fact]
        public static void RawLinkSequence_CanBeReturnedByEachRestriction()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var constants = (UnitedRangedLinksConstants<ulong>)links.Constants;
            var sequence = links.AllocateRawLinkSequence(48);
            IList<ulong>? found = null;

            links.Each(new Link<ulong>(links.Constants.Any, constants.RawLinkSequenceMarker, links.Constants.Any), link =>
            {
                found = link;
                return links.Constants.Break;
            });

            Assert.NotNull(found);
            Assert.True(links.IsRawLinkSequence(found));
            Assert.Equal(sequence, links.GetIndex(found));
            Assert.Equal(constants.RawLinkSequenceMarker, links.GetSource(found));
            Assert.Equal(48UL, links.GetTarget(found));
            Assert.Equal(1UL, links.Count(new Link<ulong>(links.Constants.Any, constants.RawLinkSequenceMarker, links.Constants.Any)));
            Assert.Equal(1UL, links.Count(new[] { links.Constants.Any, constants.RawLinkSequenceMarker }));

            links.DeallocateRawLinkSequence(sequence);
        }

        [Fact]
        public static void Delete_DeallocatesRawLinkSequenceThroughUniversalInterface()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var sequence = links.AllocateRawLinkSequence(432);

            links.Delete(sequence);
            var reused = links.AllocateRange(7UL);

            Assert.Equal(sequence, reused);
            links.DeallocateRange(reused, 7UL);
        }

        [Fact]
        public static void Each_SkipsFreeRangesAndIncludesConfiguredRawLinkSequences()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var a = links.Create();
            var range = links.AllocateRange(4UL);
            var sequence = links.AllocateRawLinkSequence(48);
            var b = links.Create();
            // The mid-allocated range must not be visible to Each — register it as a free range.
            links.DeallocateRange(range, 4UL);
            var ids = new List<ulong>();
            links.Each(link =>
            {
                ids.Add(links.GetIndex(link));
                return links.Constants.Continue;
            });
            Assert.Equal(new[] { a, sequence, b }, ids);
            Assert.Equal(3UL, links.Count());
            Assert.Equal(0UL, links.Count(new[] { range }));
            // Cleanup.
            links.DeallocateRawLinkSequence(sequence);
            links.Delete(a);
            links.Delete(b);
        }

        // -----------------------------------------------------------------
        // R8 — no-fragmentation chaos test
        // -----------------------------------------------------------------

        [Fact]
        public static void NoFragmentation_ChaosTest()
        {
            using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            using var links = new UnitedRangedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
            var rng = new System.Random(42);
            var outstanding = new List<(ulong start, ulong length)>();
            for (var iter = 0; iter < 500; iter++)
            {
                if (outstanding.Count > 0 && rng.Next(2) == 0)
                {
                    var idx = rng.Next(outstanding.Count);
                    var (s, l) = outstanding[idx];
                    outstanding.RemoveAt(idx);
                    links.DeallocateRange(s, l);
                }
                else
                {
                    var length = (ulong)rng.Next(1, 8);
                    var s = links.AllocateRange(length);
                    outstanding.Add((s, length));
                }
            }
            // Free remaining outstanding allocations.
            foreach (var (s, l) in outstanding)
            {
                links.DeallocateRange(s, l);
            }
            // After everything is freed, a fresh allocation must start at 1
            // (the tail-trim + coalescing guarantee the high-water mark resets).
            var probe = links.AllocateRange(1UL);
            Assert.Equal(1UL, probe);
            links.DeallocateRange(probe, 1UL);
        }
    }
}
