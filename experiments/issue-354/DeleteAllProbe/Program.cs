// Issue 354 probe: compares DeleteAll loop variants on heap-backed stores.
// Every variant has a step budget, so a non-terminating loop is reported instead of hanging.
// Run: dotnet run --project experiments/issue-354/DeleteAllProbe -c Release
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

const int Budget = 100;

var variants = new (string Name, Action<ILinks<uint>> DeleteAll)[]
{
    ("main (original)", links =>
    {
        // The original `for (var i = links.Count(); i > 0; i = --i)` loop, written as a while loop.
        var steps = 0;
        var i = links.Count();
        while (i > 0)
        {
            Step(ref steps);
            links.Delete(i);
            if (links.Count() != --i)
            {
                i = links.Count();
            }
            --i;
        }
    }),
    ("issue proposal", links =>
    {
        var steps = 0;
        for (var i = links.Count(); i > 0; i = links.Count())
        {
            Step(ref steps);
            links.Delete(i);
        }
    }),
    ("ascending+exists", links =>
    {
        for (var i = 1u; links.Count() > 0 && i < Budget; ++i)
        {
            if (links.Exists(i))
            {
                links.Delete(i, null);
            }
        }
    }),
    ("descending+exists", links =>
    {
        var last = 0u;
        var remaining = links.Count();
        while (remaining > 0 && last < Budget)
        {
            ++last;
            if (links.Exists(last))
            {
                --remaining;
            }
        }
        for (var i = last; i > 0; --i)
        {
            if (links.Exists(i))
            {
                links.Delete(i, null);
            }
        }
    }),
    ("library DeleteAll", links => links.DeleteAll()),
};

var scenarios = new (string Name, Action<ILinks<uint>> Fill)[]
{
    ("3 points", links => { links.CreatePoint(); links.CreatePoint(); links.CreatePoint(); }),
    ("2 points + link", links => { var a = links.CreatePoint(); var b = links.CreatePoint(); links.GetOrCreate(a, b); }),
    ("hole at 2 of 3", links =>
    {
        links.CreatePoint(); var middle = links.CreatePoint(); links.CreatePoint();
        links.Delete(middle, null);
    }),
    ("link to higher", links =>
    {
        var a = links.CreatePoint(); var b = links.CreatePoint(); var c = links.CreatePoint();
        links.Update(a, b, c); // link 1 references 2 and 3
    }),
};

var decorators = new (string Name, Func<ILinks<uint>, ILinks<uint>> Wrap)[]
{
    ("raw", links => links),
    ("cascade", links => links.DecorateWithAutomaticUniquenessAndUsagesResolution()),
    ("usages-check", links => new LinksUsagesValidator<uint>(links)),
};

if (args.Length == 4)
{
    // Child process: run exactly one case, so a corrupted store cannot take down the whole probe.
    var (variant, deleteAll) = variants[int.Parse(args[0])];
    var (decoratorName, wrap) = decorators[int.Parse(args[1])];
    var (scenario, fill) = scenarios[int.Parse(args[2])];
    var split = args[3] == "split";
    using var memory = new HeapResizableDirectMemory();
    using var memory2 = new HeapResizableDirectMemory();
    using var store = split
        ? (IDisposable)new SplitMemoryLinks<uint>(memory, memory2, 4096)
        : new UnitedMemoryLinks<uint>(memory, 4096);
    var raw = (ILinks<uint>)store;
    fill(raw.DecorateWithAutomaticUniquenessAndUsagesResolution());
    var links = wrap(raw);
    string outcome;
    try
    {
        deleteAll(links);
        outcome = $"count={links.Count()} remaining=[{Dump(links)}] reuse={Reuse(links)}";
    }
    // Expected failures only: anything else crashes the child, which the parent reports.
    catch (Exception exception) when (exception is ArgumentException or KeyNotFoundException or InvalidOperationException)
    {
        outcome = $"{exception.GetType().Name}: {exception.Message}";
    }
    Console.WriteLine($"{variant,-18}| {decoratorName,-12}| {scenario,-16}| {args[3],-6} | {outcome}");
    return;
}

var self = Environment.ProcessPath!;
var dll = System.Reflection.Assembly.GetEntryAssembly()!.Location;
var cases =
    from v in Enumerable.Range(0, variants.Length)
    from d in Enumerable.Range(0, decorators.Length)
    from s in Enumerable.Range(0, scenarios.Length)
    from kind in new[] { "united", "split" }
    select (v, d, s, kind);
foreach (var (v, d, s, kind) in cases)
{
    var info = new System.Diagnostics.ProcessStartInfo(self) { RedirectStandardOutput = true, RedirectStandardError = true };
    if (Path.GetFileNameWithoutExtension(self) == "dotnet")
    {
        info.ArgumentList.Add(dll);
    }
    foreach (var argument in new[] { v.ToString(), d.ToString(), s.ToString(), kind })
    {
        info.ArgumentList.Add(argument);
    }
    using var child = System.Diagnostics.Process.Start(info)!;
    var stdout = child.StandardOutput.ReadToEndAsync();
    var stderr = child.StandardError.ReadToEndAsync();
    if (!child.WaitForExit(30_000))
    {
        child.Kill(true);
    }
    var line = stdout.Result.Trim();
    if (line.Length == 0)
    {
        var firstError = stderr.Result.Split('\n').FirstOrDefault()?.Trim();
        line = $"{variants[v].Name,-18}| {decorators[d].Name,-12}| {scenarios[s].Name,-16}| {kind,-6} | CRASH exit={child.ExitCode} {firstError}";
    }
    Console.WriteLine(line);
}

static string Dump(ILinks<uint> links)
{
    var remaining = new List<string>();
    links.Each(link => { remaining.Add(links.Format(link)); return links.Constants.Continue; });
    return string.Join(" ", remaining);
}

// The emptied store must allocate from 1 again and keep its source/target indexes consistent.
static string Reuse(ILinks<uint> links)
{
    var a = links.CreatePoint();
    var b = links.CreatePoint();
    var c = links.GetOrCreate(a, b);
    var ok = a == 1 && b == 2 && c == 3 && links.Count() == 3
        && links.SearchOrDefault(a, b) == c
        && links.Count(links.Constants.Any, a, links.Constants.Any) == 2
        && links.Count(links.Constants.Any, links.Constants.Any, b) == 2;
    return ok ? "ok" : $"BROKEN a={a} b={b} c={c} count={links.Count()} [{Dump(links)}]";
}

static void Step(ref int steps)
{
    if (++steps > Budget)
    {
        throw new InvalidOperationException($"no termination within {Budget} steps");
    }
}
