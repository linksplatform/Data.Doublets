using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Memory;

// Run this small trace explicitly; normal storage operations do not print logs.
using var links = new SplitMemoryLinks<uint>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096);
var a = links.CreatePoint();
var b = links.CreatePoint();
var id = links.CreateAndUpdate(a, b);
var any = links.Constants.Any;
Console.WriteLine($"Stored record: ({id}: {a} {b}); Any = {any}");
foreach (var (query, expected) in new (uint[], uint)[]
{
    (new[] { id, any, a }, 0), // a is the source, so it cannot satisfy a target restriction.
    (new[] { id, b, any }, 0),
    (new[] { id, any, b }, 1),
    (new[] { id, a, any }, 1),
    (new[] { id, any, any }, 1),
    (new[] { id, a, b }, 1),
    (new[] { id, b, a }, 0),
    (new[] { id, a }, 1), // Two-part queries match either endpoint.
    (new[] { id, b }, 1)
})
{
    var visited = new List<uint>();
    var flow = links.Each(query, link =>
    {
        visited.Add(link![0]);
        return links.Constants.Continue;
    });
    var count = links.Count(query);
    Console.WriteLine($"[{string.Join(", ", query)}]: Count={count}, visits=[{string.Join(", ", visited)}]");
    if (count != expected || visited.Count != expected || flow != links.Constants.Continue)
    {
        throw new InvalidOperationException("The positional query contract was violated.");
    }
}
