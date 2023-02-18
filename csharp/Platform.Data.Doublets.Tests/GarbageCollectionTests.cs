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
        TLinkAddress link = Links.CreatePoint();
        Links.ClearGarbage(link);
        Assert.True(Links.Exists(link));
    }
    
    [Fact]
    public void ClearGarbageFullPointDependency()
    {
        TLinkAddress link = Links.CreatePoint();
        Links.ClearGarbage(link);
        Assert.True(Links.Exists(link));
    }
    
    [Fact]
    public void ClearGarbageWithInDependency()
    {
        TLinkAddress link = Links.CreatePoint();
        TLinkAddress dependant = Links.GetOrCreate(TLinkAddress.CreateTruncating(10), link);
        Links.ClearGarbage(link);
        Assert.True(Links.Exists(link));
        Assert.True(Links.Exists(dependant));
    }
    
    [Fact]
    public void ClearGarbageWithOutDependency()
    {
        TLinkAddress link = Links.CreatePoint();
        TLinkAddress dependant = Links.GetOrCreate(link, TLinkAddress.CreateTruncating(10));
        Links.ClearGarbage(link);
        Assert.True(Links.Exists(link));
        Assert.True(Links.Exists(dependant));
    }
    
    [Fact]
    public void ClearGarbageWithoutDependency()
    {
        TLinkAddress link1 = Links.CreatePoint();
        TLinkAddress link2 = Links.CreatePoint();
        TLinkAddress link3 = Links.GetOrCreate(link1, link2);
        Links.ClearGarbage(link3);
        Assert.False(Links.Exists(link3));
        Assert.True(Links.Exists(link2));
        Assert.True(Links.Exists(link1));
    }

    [Fact]
    public void ComplexgarbageCollectionTest()
    {
        /*
           (1: 1 1)
           (2: 2 2)
           (3: 1 2)
           (4: 3 2)
         */
        TLinkAddress link1 = Links.CreatePoint();
        TLinkAddress link2 = Links.CreatePoint();
        TLinkAddress link3 = Links.GetOrCreate(link1, link2);
        TLinkAddress link4 = Links.GetOrCreate(link3, link2);
        Links.ClearGarbage(link4);
        Assert.False(Links.Exists(link4));
        Assert.False(Links.Exists(link3));
        Assert.True(Links.Exists(link2));
        Assert.True(Links.Exists(link1));
        
    }
}
