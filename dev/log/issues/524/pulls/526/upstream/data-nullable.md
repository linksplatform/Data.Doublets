Found while fixing all nullable diagnostics in https://github.com/linksplatform/Data.Doublets/issues/524 / https://github.com/linksplatform/Data.Doublets/pull/526.

Platform.Data 0.16.1 exposes `WriteHandlerState<TLinkAddress>.Handle(IList<TLinkAddress> before, IList<TLinkAddress> after)` as non-nullable. Creation legitimately has no before snapshot, and deletion legitimately has no after snapshot; the write-handler delegate already models these optional snapshots. Calling Handle with the correctly annotated values therefore produces CS8604 in a nullable-enabled consumer (and breaks warnings-as-errors).

Minimal reproduction: in a net8.0 project with Nullable=enable and Platform.Data 0.16.1, create a WriteHandlerState<ulong> and call `state.Handle(null, new ulong[] { 1, 0, 0 })` for creation, then `state.Handle(new ulong[] { 1, 0, 0 }, null)` for deletion. Build with `dotnet build -warnaserror`.

Workaround: downstream PR #526 adds a small nullable-aware adapter which invokes the optional delegate with nullable snapshots and preserves the existing Apply/result semantics. It does not replace null snapshots with invented data or suppress warnings globally. The upstream source used for comparison is archived in the PR evidence directory.

Suggested fix: annotate both Handle parameters as `IList<TLinkAddress>?`, retain the existing runtime behavior, and add nullable-enabled creation/deletion consumer tests. Also check matching overloads and delegate declarations throughout Platform.Data for the same contract mismatch.
