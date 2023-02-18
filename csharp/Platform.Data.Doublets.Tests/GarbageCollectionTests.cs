using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;
using TLinkAddress = System.UInt64;

namespace Platform.Data.Doublets.Tests;

public class GarbageCollectionTests
{
    public IResizableDirectMemory LinksMemory;
    public ILinks<TLinkAddress> Links;
    
    
    public GarbageCollectionTests()
    {
        LinksMemory = new HeapResizableDirectMemory();
        Links = new UnitedMemoryLinks<TLinkAddress>(LinksMemory);
    }
    [Fact]
    public void Test()
    {
        TLinkAddress link1 = Links.GetOrCreate(TLinkAddress.CreateTruncating(1), TLinkAddress.CreateTruncating(1));
        TLinkAddress link2 = Links.GetOrCreate(TLinkAddress.CreateTruncating(1), TLinkAddress.CreateTruncating(1));
        TLinkAddress dependantOfLink1 = Links.GetOrCreate(TLinkAddress.CreateTruncating(1), TLinkAddress.CreateTruncating(1));
        Links.ClearGarbage(link1);
        Links.ClearGarbage(link2);
        Assert.True(Links.Exists(link1));
        Assert.False(Links.Exists(link1));
    }
}
