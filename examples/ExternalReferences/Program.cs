using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

foreach (var name in new[] { "united", "split" })
{
    var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: true);
    using var data = new HeapResizableDirectMemory();
    using var index = new HeapResizableDirectMemory();
    ILinks<ulong> links = name == "united"
        ? new UnitedMemoryLinks<ulong>(data, 4096, constants, IndexTreeType.Default)
        : new SplitMemoryLinks<ulong>(data, index, 4096, constants);
    using var store = (IDisposable)links;
    var a = links.CreatePoint();
    var x = new Hybrid<ulong>(20, isExternal: true).Value;
    var b = links.CreateAndUpdate(a, x);
    var found = new List<ulong>();
    links.Each(link =>
    {
        found.Add(link![0]);
        return constants.Continue;
    }, constants.Any, a, x);
    var count = links.Count(constants.Any, a, x);
    var search = links.SearchOrDefault(a, x);
    Console.WriteLine($"{name}: created {b} = ({a}, {x}); Each -> [{string.Join(",", found)}], Count -> {count}, SearchOrDefault -> {search}");
    if (!found.SequenceEqual(new[] { b }) || count != 1 || search != b)
    {
        throw new InvalidOperationException($"{name} failed to find the stored link.");
    }
}
