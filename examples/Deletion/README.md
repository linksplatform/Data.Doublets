# Deletion choices

Run `dotnet run --project examples/Deletion -c Release` with .NET 8.

The program checks caller-managed deletion, per-call checks, the pair-based
helper, outer throw/ignore decorators, incoming-usage rejection, cascading and
snapshot bulk deletion. It runs each recipe for both united and split heap-backed
stores, creates at most five links per scenario, and fails if results differ from
the documented contract. See the [C# guide](../../docs/deletion-csharp.md) and
[shared design guidelines](../../docs/design.md).
