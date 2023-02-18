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
    public void ClearGarbagePartialPointDependency()
    {
        TLinkAddress link = Links.GetOrCreate(TLinkAddress.CreateTruncating(1), TLinkAddress.CreateTruncating(2));
        Links.ClearGarbage(link);
        Assert.True(Links.Exists(link));
    }
    
    [Fact]
    public void ClearGarbageFullPointDependency()
    {
        TLinkAddress link = Links.GetOrCreate(TLinkAddress.CreateTruncating(1), TLinkAddress.CreateTruncating(1));
        Links.ClearGarbage(link);
        Assert.True(Links.Exists(link));
    }
    
    [Fact]
    public void ClearGarbageWithInDependency()
    {
        TLinkAddress link = Links.GetOrCreate(TLinkAddress.CreateTruncating(2), TLinkAddress.CreateTruncating(3));
        TLinkAddress dependant = Links.GetOrCreate(TLinkAddress.CreateTruncating(10), link);
        Links.ClearGarbage(link);
        Assert.True(Links.Exists(link));
    }
    
    [Fact]
    public void ClearGarbageWithOutDependency()
    {
        TLinkAddress link = Links.GetOrCreate(TLinkAddress.CreateTruncating(2), TLinkAddress.CreateTruncating(3));
        TLinkAddress dependant = Links.GetOrCreate(link, TLinkAddress.CreateTruncating(10));
        Links.ClearGarbage(link);
        Assert.True(Links.Exists(link));
    }
    
    [Fact]
    public void ClearGarbageWithoutDependency()
    {
        TLinkAddress link = Links.GetOrCreate(TLinkAddress.CreateTruncating(2), TLinkAddress.CreateTruncating(3));
        Links.ClearGarbage(link);
        Assert.False(Links.Exists(link));
    }
}
