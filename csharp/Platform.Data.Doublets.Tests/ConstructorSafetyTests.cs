using System;
using Platform.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class ConstructorSafetyTests
{
    [Fact]
    public void UnitedConstructionDoesNotCallOverridesBeforeDerivedFieldsAreReady()
    {
        using var memory = new HeapResizableDirectMemory();
        using var links = new UnitedProbe(memory);
        Assert.Equal(1UL, links.Create());
    }

    [Fact]
    public void SplitConstructionDoesNotCallOverridesBeforeDerivedFieldsAreReady()
    {
        using var data = new HeapResizableDirectMemory();
        using var index = new HeapResizableDirectMemory();
        using var links = new SplitProbe(data, index);
        Assert.Equal(1UL, links.Create());
    }

    private class UnitedProbe : UnitedMemoryLinks<ulong>
    {
        private readonly bool _ready;
        public UnitedProbe(IResizableDirectMemory memory) : base(memory, 4096) => _ready = true;
        protected override void Init(IResizableDirectMemory memory, long memoryReservationStep)
        {
            if (!_ready) throw new InvalidOperationException("Derived fields are not initialized");
            base.Init(memory, memoryReservationStep);
        }
        protected override void SetPointers(IResizableDirectMemory memory)
        {
            if (!_ready) throw new InvalidOperationException("Derived fields are not initialized");
            base.SetPointers(memory);
        }
    }

    private class SplitProbe : SplitMemoryLinks<ulong>
    {
        private readonly bool _ready;
        public SplitProbe(IResizableDirectMemory data, IResizableDirectMemory index) : base(data, index, 64) => _ready = true;
        protected override void Init(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
        {
            if (!_ready) throw new InvalidOperationException("Derived fields are not initialized");
            base.Init(dataMemory, indexMemory);
        }
        protected override void SetPointers(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
        {
            if (!_ready) throw new InvalidOperationException("Derived fields are not initialized");
            base.SetPointers(dataMemory, indexMemory);
        }
    }
}
