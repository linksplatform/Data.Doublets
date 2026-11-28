using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Exceptions;
using Platform.Memory;

foreach (var split in new[] { false, true })
{
    Run(split, raw =>
    {
        // The caller guarantees a live link, detached contents, and resolved usages.
        var id = raw.CreatePoint();
        raw.Update(id, 0u, 0u);
        raw.Delete(new[] { id }, null);
        Check(raw.Count() == 0, "unchecked deletion");
    });
    Run(split, raw =>
    {
        var id = raw.CreatePoint();
        raw.EnsureLinkExists(id, nameof(id));
        raw.Delete(id, null); // This overload resets endpoints before deleting.
        ExpectMissing(() => raw.EnsureLinkExists(id, nameof(id)));
        Check(raw.DeleteIfExists(id) == raw.Constants.Null, "ignore a missing address");
        Check(raw.Count() == 0, "per-call policies");
    });
    Run(split, raw =>
    {
        ILinks<uint> links = new LinksInnerReferenceExistenceValidator<uint>(
            raw.DecorateWithAutomaticUniquenessAndUsagesResolution());
        var first = links.CreatePoint();
        var middle = links.CreatePoint();
        links.CreatePoint();
        links.Delete(middle);
        ExpectMissing(() => links.Delete(middle));
        ExpectMissing(() => links.Delete(10u));
        Check(links.Count() == 2 && links.Exists(first), "throw policy preserves contents");
        Check(links.CreatePoint() == middle && links.CreatePoint() == 4u, "allocator reuse");
    });
    Run(split, raw =>
    {
        ILinks<uint> links = new LinksNonExistentDeletionResolver<uint>(
            raw.DecorateWithAutomaticUniquenessAndUsagesResolution());
        var id = links.CreatePoint();
        links.Delete(id);
        Check(links.Delete(id, (_, _) => throw new InvalidOperationException("Missing callback"))
            == links.Constants.Continue, "ignore policy");
        Check(links.Delete(id) == links.Constants.Null, "address adapter returns null on no-op");
        Check(links.Count() == 0 && links.CreatePoint() == id, "ignore preserves allocator");
    });
    Run(split, raw =>
    {
        // The two per-call helpers reset endpoints on a raw store.
        var first = raw.CreatePoint();
        var second = raw.CreatePoint();
        var third = raw.CreateAndUpdate(first, second);
        var victim = raw.CreateAndUpdate(second, first);
        var survivor = raw.CreateAndUpdate(second, third);
        Check(raw.DeleteIfExists(victim) == victim, "delete by address");
        Check(raw.SearchOrDefault(second, second) == second, "find the second point after deletion");
        Check(raw.SearchOrDefault(second, third) == survivor, "find the surviving pair after deletion");
        Check(raw.DeleteIfExists(second, third) == survivor, "delete by pair");
        Check(raw.DeleteIfExists(second, third) == raw.Constants.Null, "missing pair");
    });
    Run(split, raw =>
    {
        ILinks<uint> links = new LinksInnerReferenceExistenceValidator<uint>(
            new NonNullContentsLinkDeletionResolver<uint>(new LinksUsagesValidator<uint>(raw)));
        var first = links.CreatePoint();
        var second = links.CreatePoint();
        var usage = links.CreateAndUpdate(first, second);
        var rejected = false;
        try
        {
            links.Delete(first);
        }
        catch (ArgumentLinkHasDependenciesException<uint>)
        {
            rejected = true;
        }
        Check(rejected && links.Exists(first) && links.Exists(usage), "reject incoming usages");
        links.Delete(usage);
        links.Delete(first);
        Check(!links.Exists(first) && links.Exists(second), "delete after usage removal");
    });
    Run(split, raw =>
    {
        ILinks<uint> links = new LinksNonExistentDeletionResolver<uint>(
            raw.DecorateWithAutomaticUniquenessAndUsagesResolution());
        var first = links.CreatePoint();
        var second = links.CreatePoint();
        var usage = links.CreateAndUpdate(first, second);
        links.Delete(first);
        Check(!links.Exists(first) && !links.Exists(usage) && links.Exists(second), "cascade usages");
        // Snapshot actual addresses, including holes; cascades may delete later entries.
        var addresses = new List<uint>();
        links.Each(Array.Empty<uint>(), link =>
        {
            addresses.Add(link![0]);
            return links.Constants.Continue;
        });
        foreach (var address in addresses)
        {
            links.Delete(address);
        }
        Check(links.Count() == 0, "bulk deletion through a selected policy");
    });
    Console.WriteLine($"{(split ? "Split" : "United")}: all deletion examples passed.");
}

static void Run(bool split, Action<ILinks<uint>> action)
{
    using var united = split ? null : new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
    using var splitLinks = split
        ? new SplitMemoryLinks<uint>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096)
        : null;
    action(split ? splitLinks! : united!);
}

static void Check(bool condition, string operation)
{
    if (!condition)
    {
        throw new InvalidOperationException($"Failed: {operation}");
    }
}

static void ExpectMissing(Action action)
{
    try { action(); }
    catch (ArgumentLinkDoesNotExistsException<uint>) { return; }
    throw new InvalidOperationException("Expected a missing-link exception.");
}
