using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Exceptions;
using Platform.Delegates;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class UpdateContractTests
    {
        public static IEnumerable<object[]> MissingUpdateCases()
        {
            foreach (var split in new[] { false, true })
                foreach (var automatic in new[] { false, true })
                    foreach (var policy in new[] { "validator", "per-call", "ignore" })
                        foreach (var scenario in new[] { "never-created", "deleted-middle", "deleted-last", "empty" })
                            foreach (var collision in new[] { false, true })
                                yield return new object[] { split, automatic, policy, scenario, collision };
        }

        [Theory]
        [MemberData(nameof(MissingUpdateCases))]
        public static void SelectedMissingUpdatePolicyPreservesStore(bool split, bool automatic, string policy, string scenario, bool collision)
        {
            CheckMissingUpdate<byte>(split, automatic, policy, scenario, collision);
            CheckMissingUpdate<ushort>(split, automatic, policy, scenario, collision);
            CheckMissingUpdate<uint>(split, automatic, policy, scenario, collision);
            CheckMissingUpdate<ulong>(split, automatic, policy, scenario, collision);
        }

        private static void CheckMissingUpdate<T>(bool split, bool automatic, string policy, string scenario, bool collision)
            where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
                IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
        {
            using var united = split ? null : new UnitedMemoryLinks<T>(new HeapResizableDirectMemory(), 4096);
            using var separated = split ? new SplitMemoryLinks<T>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) : null;
            ILinks<T> raw = split ? separated! : united!;
            ILinks<T> inner = automatic ? raw.DecorateWithAutomaticUniquenessAndUsagesResolution() : raw;
            ILinks<T> links = policy == "validator" ? new LinksInnerReferenceExistenceValidator<T>(inner) : inner;
            var first = links.CreatePoint();
            var middle = links.CreatePoint();
            var last = links.CreatePoint();
            var missing = T.CreateTruncating(10);
            var next = last + T.One;
            if (scenario == "deleted-middle")
            {
                links.Delete(middle, null);
                missing = next = middle;
            }
            else if (scenario == "deleted-last")
            {
                links.Delete(last, null);
                missing = next = last;
            }
            else if (scenario == "empty")
            {
                links.Delete(last, null);
                links.Delete(middle, null);
                links.Delete(first, null);
                missing = next = first;
            }

            var source = scenario == "empty" ? T.Zero : first;
            var target = scenario == "empty" ? T.Zero : collision ? first : last;
            var before = Snapshot(raw);
            var count = raw.Count();
            var calls = 0;
            Assert.False(raw.Exists(missing));
            // All invalid addresses stop at a selected policy, never at raw memory.
            for (var shape = 0; shape < 3; shape++)
            {
                var api = shape;
                void UpdateMissing()
                {
                    if (policy == "per-call") links.EnsureLinkExists(missing, nameof(missing));
                    if (policy == "ignore" && !links.Exists(missing)) return;
                    WriteHandler<T> handler = (_, _) => { calls++; return links.Constants.Break; };
                    if (api == 0) links.Update(missing, source, target);
                    else if (api == 1) links.Update(missing, source, target, handler);
                    else links.Update(new[] { missing }, new[] { missing, source, target }, null);
                }
                if (policy == "ignore") UpdateMissing();
                else Assert.Throws<ArgumentLinkDoesNotExistsException<T>>(UpdateMissing);
                Assert.Equal(0, calls);
                Assert.Equal(count, raw.Count());
                AssertRows(before, Snapshot(raw));
                Assert.False(raw.Exists(missing));
                Assert.Equal(collision && scenario != "empty" ? first : T.Zero, raw.SearchOrDefault(source, target));
                foreach (var row in before)
                {
                    Assert.Equal(row[0], raw.SearchOrDefault(row[1], row[2]));
                    AssertRows(before.Where(x => x[1] == row[1]).ToArray(), Snapshot(raw, new[] { raw.Constants.Any, row[1], raw.Constants.Any }));
                    AssertRows(before.Where(x => x[2] == row[2]).ToArray(), Snapshot(raw, new[] { raw.Constants.Any, raw.Constants.Any, row[2] }));
                }
            }
            // Both the phantom-pair and free-list failure from #517 stay observable.
            if (scenario != "empty" && !collision)
            {
                Assert.Equal(next, links.CreateAndUpdate(source, target));
                Assert.Equal(next, raw.SearchOrDefault(source, target));
            }
            else Assert.Equal(next, links.CreatePoint());
            var following = scenario == "deleted-middle" ? last + T.One : next + T.One;
            Assert.Equal(following, links.CreatePoint());
            Assert.Equal(count + T.CreateTruncating(2), raw.Count());
            Assert.Equal(int.CreateTruncating(raw.Count()), Snapshot(raw).Length);
            foreach (var row in before) Assert.Equal(row, raw.GetLink(row[0]));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void ValidRawUpdateOmitsExistenceChecksAndPreservesCallbackFlow(bool split)
        {
            using var united = split ? null : new UnitedWithoutChecks();
            using var separated = split ? new SplitWithoutChecks() : null;
            ILinks<uint> links = split ? separated! : united!;
            var first = links.CreatePoint();
            var second = links.CreatePoint();
            if (split) separated!.RejectChecks = true;
            else united!.RejectChecks = true;
            var calls = 0;
            var result = links.Update(new[] { second }, new[] { second, first, second }, (before, after) =>
            {
                calls++;
                Assert.Equal(new[] { second, second, second }, before);
                Assert.Equal(new[] { second, first, second }, after);
                return links.Constants.Break;
            });
            if (split) separated!.RejectChecks = false;
            else united!.RejectChecks = false;
            Assert.Equal(links.Constants.Break, result);
            Assert.Equal(1, calls);
            Assert.Equal(2u, links.Count());
            Assert.Equal(second, links.SearchOrDefault(first, second));
            Assert.Equal(second, links.Update(second, second, first));
            Assert.Equal(links.Constants.Continue, links.Update(new[] { second }, new[] { second, first, second }, null));
        }

        [Fact]
        public static void OuterValidatorRejectsBeforeAnyResolverOperation()
        {
            using var raw = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
            var first = raw.CreatePoint();
            var spy = new TerminalSpy(raw);
            var links = new LinksInnerReferenceExistenceValidator<uint>(spy.DecorateWithAutomaticUniquenessAndUsagesResolution());
            Assert.Throws<ArgumentLinkDoesNotExistsException<uint>>(() => links.Update(10u, first, first));
            Assert.Equal(0, spy.Operations);
            // Valid id but missing endpoint is also rejected by this broader policy.
            Assert.Throws<ArgumentLinkDoesNotExistsException<uint>>(() => links.Update(first, first, 10u));
            Assert.Equal(0, spy.Operations);
        }

        [Fact]
        public static void AutomaticCompositionDoesNotSelectExistenceValidation()
        {
            using var raw = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
            var first = raw.CreatePoint();
            var spy = new TerminalSpy(raw);
            var links = spy.DecorateWithAutomaticUniquenessAndUsagesResolution();
            // The terminal spy absorbs the request: no invalid raw access is performed.
            Assert.Equal(links.Constants.Continue, links.Update(new[] { 10u }, new[] { 10u, first, first }, null));
            Assert.Equal(2, spy.Operations); // Search, then update; no implicit validator.
            Assert.Equal(1u, raw.Count());
            Assert.Equal(new[] { first, first, first }, raw.GetLink(first));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public static void CheckedValidUpdatePreservesResultAndCallback(bool split, bool automatic)
        {
            using var united = split ? null : new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
            using var separated = split ? new SplitMemoryLinks<uint>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) : null;
            ILinks<uint> raw = split ? separated! : united!;
            var links = new LinksInnerReferenceExistenceValidator<uint>(automatic ? raw.DecorateWithAutomaticUniquenessAndUsagesResolution() : raw);
            var first = links.CreatePoint();
            var second = links.CreatePoint();
            var calls = 0;
            Assert.Equal(links.Constants.Break, links.Update(second, first, second, (before, after) =>
            {
                calls++;
                Assert.Equal(new[] { second, second, second }, before);
                Assert.Equal(new[] { second, first, second }, after);
                return links.Constants.Break;
            }));
            Assert.Equal(1, calls);
            Assert.Equal(second, links.Update(second, second, first));
            Assert.Equal(second, raw.SearchOrDefault(second, first));
            Assert.Equal(2u, raw.Count());
        }

        [Fact]
        public static void InnerReferenceValidatorDoesNotValidateServiceConstantsAsAddresses()
        {
            using var raw = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
            var first = raw.CreatePoint();
            var spy = new TerminalSpy(raw);
            var links = new LinksInnerReferenceExistenceValidator<uint>(spy);
            // These are not live link addresses. The spy prevents raw memory access.
            foreach (var address in new[] { links.Constants.Null, links.Constants.Any })
            {
                Assert.False(links.Constants.IsInternalReference(address));
                Assert.Equal(links.Constants.Continue, links.Update(new[] { address }, new[] { address, first, first }, null));
            }
            Assert.Equal(2, spy.Operations);
        }

        private static T[][] Snapshot<T>(ILinks<T> links, IList<T>? restriction = null) where T : IUnsignedNumber<T>
        {
            var rows = new List<T[]>();
            links.Each(restriction, row => { rows.Add(row!.ToArray()); return links.Constants.Continue; });
            return rows.OrderBy(row => row[0]).ToArray();
        }

        private static void AssertRows<T>(T[][] expected, T[][] actual)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++) Assert.Equal(expected[i], actual[i]);
        }

        private sealed class UnitedWithoutChecks : UnitedMemoryLinks<uint>
        {
            public bool RejectChecks { get; set; }
            public UnitedWithoutChecks() : base(new HeapResizableDirectMemory(), 4096) { }
            protected override bool Exists(uint link) => RejectChecks ? throw new InvalidOperationException("Unexpected core validation") : base.Exists(link);
        }

        private sealed class SplitWithoutChecks : SplitMemoryLinks<uint>
        {
            public bool RejectChecks { get; set; }
            public SplitWithoutChecks() : base(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) { }
            protected override bool Exists(uint link) => RejectChecks ? throw new InvalidOperationException("Unexpected core validation") : base.Exists(link);
        }

        private sealed class TerminalSpy : LinksDecoratorBase<uint>
        {
            public int Operations { get; private set; }
            public TerminalSpy(ILinks<uint> links) : base(links) { }
            public override uint Each(IList<uint>? restriction, ReadHandler<uint>? handler) { Operations++; return Constants.Continue; }
            public override uint Update(IList<uint>? restriction, IList<uint>? substitution, WriteHandler<uint>? handler) { Operations++; return Constants.Continue; }
            public override uint Delete(IList<uint>? restriction, WriteHandler<uint>? handler) { Operations++; return Constants.Continue; }
        }
    }
}
