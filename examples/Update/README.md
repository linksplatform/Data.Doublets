<!-- markdownlint-configure-file
{
  "MD043": { "headings": ["# Update choices", "*"] }
}
-->

# Update choices

Run `dotnet run --project examples/Update -c Release` with .NET 8.

The program checks valid caller-managed updates, per-call rejection and skipping,
and an outer existence validator around plain and automatic stacks. Both issue
517 cases are submitted only through the selected checked boundary. It verifies
no phantom pair, no lost following creation, preserved free-slot reuse and valid
callback flow on both heap-backed backends. Each store creates at most four links.
Invalid raw writes are never performed. See the
[C# update guide](../../docs/update-csharp.md) and
[investigation](../../docs/update-investigation.md).
