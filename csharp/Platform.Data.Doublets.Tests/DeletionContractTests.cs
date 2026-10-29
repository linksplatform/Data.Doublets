using System;
using System.Collections.Generic;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Delegates;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class DeletionContractTests
    {
        [Fact]
        public static void UnitedCoreDeletionDoesNotValidateCallerContract()
        {
            using var links = new UnitedWithoutExistenceChecks();
            var index = links.CreatePoint();
            links.Update(index, 0u, 0u);
            links.RejectExistenceChecks = true;
            links.Delete(new[] { index }, null);
            links.RejectExistenceChecks = false;
            Assert.Equal(0u, links.Count());
            Assert.Equal(index, links.CreatePoint());
        }

        [Fact]
        public static void SplitCoreDeletionDoesNotValidateCallerContract()
        {
            using var links = new SplitWithoutExistenceChecks();
            var index = links.CreatePoint();
            links.Update(index, 0u, 0u);
            links.RejectExistenceChecks = true;
            links.Delete(new[] { index }, null);
            links.RejectExistenceChecks = false;
            Assert.Equal(0u, links.Count());
            Assert.Equal(index, links.CreatePoint());
        }

        [Fact]
        public static void ResetResolverDoesNotAlsoValidateExistence()
        {
            using var raw = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
            var inner = new ResetLinkStub(raw);
            var links = new NonNullContentsLinkDeletionResolver<uint>(inner);
            Assert.Equal(links.Constants.Continue, links.Delete(new[] { 1u }, null));
            Assert.True(inner.Deleted);
        }

        private sealed class UnitedWithoutExistenceChecks : UnitedMemoryLinks<uint>
        {
            public bool RejectExistenceChecks { get; set; }
            public UnitedWithoutExistenceChecks() : base(new HeapResizableDirectMemory(), 4096) { }
            protected override bool Exists(uint link) => RejectExistenceChecks
                ? throw new InvalidOperationException("Core deletion must trust its caller.")
                : base.Exists(link);
        }

        private sealed class SplitWithoutExistenceChecks : SplitMemoryLinks<uint>
        {
            public bool RejectExistenceChecks { get; set; }
            public SplitWithoutExistenceChecks()
                : base(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) { }
            protected override bool Exists(uint link) => RejectExistenceChecks
                ? throw new InvalidOperationException("Core deletion must trust its caller.")
                : base.Exists(link);
        }

        private sealed class ResetLinkStub : LinksDecoratorBase<uint>
        {
            public bool Deleted { get; private set; }
            public ResetLinkStub(ILinks<uint> links) : base(links) { }
            public override uint Count(IList<uint>? restriction)
                => throw new InvalidOperationException("Reset resolution must not impose existence validation.");
            public override uint Each(IList<uint>? restriction, ReadHandler<uint>? handler)
                => handler!(new[] { 1u, 0u, 0u });
            public override uint Delete(IList<uint>? restriction, WriteHandler<uint>? handler)
            {
                Deleted = true;
                return Constants.Continue;
            }
        }
    }
}
