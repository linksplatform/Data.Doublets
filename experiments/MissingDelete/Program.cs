using System.Numerics;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Exceptions;
using Platform.Memory;

// Finite diagnostic: three initial links, two allocations, four address widths.
// Run with: dotnet run --project experiments/MissingDelete -c Release
Trace<byte>();
Trace<ushort>();
Trace<uint>();
Trace<ulong>();

static void Trace<T>()
    where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
        IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
{
    foreach (var rejectMissing in new[] { false, true })
    {
        Console.WriteLine($"{typeof(T).Name}, rejected deletes: {rejectMissing}");
        using var links = new TracedUnited<T>();
        ILinks<T> deletion = rejectMissing ? new LinksInnerReferenceExistenceValidator<T>(links) : links;
        links.CreatePoint();
        var middle = links.CreatePoint();
        links.CreatePoint();
        links.Dump("initial");
        links.Update(middle, T.Zero, T.Zero);
        links.Delete(middle);
        links.Dump("deleted middle");
        if (rejectMissing)
        {
            for (var attempt = 0; attempt < 2; attempt++)
            {
                try { deletion.Delete(new[] { middle }, null); }
                catch (ArgumentLinkDoesNotExistsException<T> error) { Console.WriteLine(error.Message); }
                links.Dump("rejected missing delete");
            }
        }
        Console.WriteLine($"reused = {links.CreatePoint()}");
        links.Dump("after reuse");
        Console.WriteLine($"following = {links.CreatePoint()}");
        links.Dump("after following allocation");
    }
}

sealed class TracedUnited<T> : UnitedMemoryLinks<T>
    where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
        IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
{
    public TracedUnited() : base(new HeapResizableDirectMemory(), 4096) { }

    public void Dump(string stage)
    {
        ref var header = ref GetHeaderReference();
        Console.WriteLine($"{stage}: allocated={header.AllocatedLinks}, free={header.FreeLinks}, first={header.FirstFreeLink}, last={header.LastFreeLink}, sourceRoot={header.RootAsSource}, targetRoot={header.RootAsTarget}");
        for (var index = 1; index <= 4; index++)
        {
            ref var raw = ref GetLinkReference(T.CreateTruncating(index));
            Console.WriteLine($"  {index}: ({raw.Source}, {raw.Target}), sourceSize={raw.SizeAsSource}, targetSize={raw.SizeAsTarget}");
        }
    }
}
