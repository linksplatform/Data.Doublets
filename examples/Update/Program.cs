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
        // Caller guarantees live addresses and valid endpoint/index state.
        var first = raw.CreatePoint();
        var second = raw.CreatePoint();
        Check(raw.Update(second, first, second) == second, "valid unchecked update");
        Check(raw.SearchOrDefault(first, second) == second, "unchecked index maintenance");
    });
    Run(split, raw =>
    {
        var first = raw.CreatePoint();
        var second = raw.CreatePoint();
        raw.EnsureLinkExists(second, nameof(second));
        raw.Update(second, first, second);
        var missing = 10u;
        ExpectMissing(() => raw.EnsureLinkExists(missing, nameof(missing)));
        var callbacks = 0;
        if (raw.Constants.IsInternalReference(missing) && raw.Exists(missing))
        {
            raw.Update(missing, first, second, (_, _) => { callbacks++; return raw.Constants.Continue; });
        }
        Check(callbacks == 0 && raw.Count() == 2, "explicit missing-address no-op");
    });
    foreach (var automatic in new[] { false, true })
    {
        Run(split, raw =>
        {
            ILinks<uint> links = new LinksInnerReferenceExistenceValidator<uint>(
                automatic ? raw.DecorateWithAutomaticUniquenessAndUsagesResolution() : raw);
            var first = links.CreatePoint();
            var second = links.CreatePoint();
            // Issue #517, repro 1: no phantom pair or uniqueness merge.
            ExpectMissing(() => links.Update(10u, first, second));
            Check(links.Count() == 2 && links.SearchOrDefault(first, second) == 0, "no phantom update");
            var pair = links.CreateAndUpdate(first, second);
            Check(pair == 3 && links.Count() == 3 && links.SearchOrDefault(first, second) == pair,
                "following creation retained");
            var callbacks = 0;
            Check(links.Update(pair, second, first, (_, _) => { callbacks++; return links.Constants.Break; })
                == links.Constants.Break, "checked callback flow");
            Check(callbacks == 1 && links.SearchOrDefault(second, first) == pair, "checked valid update");
        });
        Run(split, raw =>
        {
            ILinks<uint> links = new LinksInnerReferenceExistenceValidator<uint>(
                automatic ? raw.DecorateWithAutomaticUniquenessAndUsagesResolution() : raw);
            var first = links.CreatePoint();
            var middle = links.CreatePoint();
            var last = links.CreatePoint();
            links.Delete(middle, null); // Supplies reset, including on a plain store.
            // Issue #517, repro 2: reject before touching the unused-list entry.
            ExpectMissing(() => links.Update(middle, first, last));
            ExpectMissing(() => links.Update(middle, first, first)); // Existing pair: no merge.
            Check(links.Count() == 2 && links.SearchOrDefault(first, last) == 0, "deleted-slot rejection");
            Check(links.CreatePoint() == middle && links.CreatePoint() == 4, "free-list preservation");
            Check(links.GetSource(first) == first && links.GetTarget(last) == last, "original links preserved");
        });
    }
    Console.WriteLine($"{(split ? "Split" : "United")}: all update examples passed.");
}

static void Run(bool split, Action<ILinks<uint>> action)
{
    using var united = split ? null : new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
    using var separated = split
        ? new SplitMemoryLinks<uint>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096)
        : null;
    action(split ? separated! : united!);
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
