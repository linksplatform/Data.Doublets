using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Memory;

using var raw = new SplitMemoryLinks<byte>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096,
    new LinksConstants<byte>(enableExternalReferencesSupport: true), IndexTreeType.Default, false);
ILinks<byte> links = raw;
var a = links.CreatePoint();
var b = links.CreatePoint();
var sourceOnly = links.CreateAndUpdate(a, b);
var targetOnly = links.CreateAndUpdate(b, a);
var external = new Hybrid<byte>(20, isExternal: true).Value;
links.CreateAndUpdate(external, a);
links.CreateAndUpdate(b, external);

void Query(string stage)
{
    foreach (var v in new[] { a, b, (byte)0, (byte)50, external })
    {
        Console.WriteLine($"{stage}: value={v}");
        Console.WriteLine($"Count={links.Count(new[] { links.Constants.Any, v })}");
        links.Each(new[] { links.Constants.Any, v }, link =>
        {
            Console.WriteLine($"  {link[0]}: {link[1]}, {link[2]}");
            return links.Constants.Continue;
        });
    }
}

Query("initial");
var empty = links.Create();
Query("created empty");
links.Delete(empty);
Query("deleted empty");
links.Update(sourceOnly, b, sourceOnly);
Query("updated sourceOnly");
links.Update(targetOnly, (byte)0, (byte)0);
Query("reset targetOnly");
links.Delete(targetOnly);
Query("deleted targetOnly");
links.CreateAndUpdate(b, a);
Query("reused targetOnly");
