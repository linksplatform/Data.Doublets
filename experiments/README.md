---
title: Missing-link deletion investigation
---

These finite probes support [Data.Doublets issue #518](https://github.com/linksplatform/Data.Doublets/issues/518).

Run from the repository root with .NET 8:

```sh
dotnet test csharp/Platform.Data.Doublets.Tests -c Release \
  --filter FullyQualifiedName~MissingLinkDeletionTests --blame-hang-timeout 3m
dotnet run --project experiments/MissingDelete -c Release
```

The 52 theory cases cover united/split stores, plain/automatic decorators,
direct/convenience deletion, six missing-address scenarios, and successful
deletion callbacks. Each case checks byte, ushort, uint, and ulong addresses.
The tracing program prints allocator headers and link metadata for a valid
middle deletion, rejected repeated deletions, and reuse followed by allocation.
It creates at most five links per store. Valid plain-store deletions reset
endpoints first, as required by the low-level storage API.

To reproduce the original failures, run the following with no concurrent builds
or edits to the three deletion implementations or the regression test file:

```sh
python experiments/reproduce_missing_delete.py /path/to/dotnet
```

The script temporarily substitutes the original implementations from commit
`8d5c7541371957739de8bfa382e04b0c7d84534f` and restores the working files in a
`finally` block. It excludes extreme addresses because the original code reads
unchecked memory. Expected result: 40 failing missing-delete cases and four
passing valid-delete controls. Rebuild after running this script to restore the
fixed binaries.

To compare with Rust, copy `missing_delete.rs` into a doublets-rs checkout as
`doublets/tests/issue_518_missing_delete.rs`, then run in that checkout:

```sh
ulimit -v 1572864 -s 8192
CARGO_BUILD_JOBS=1 CARGO_PROFILE_DEV_DEBUG=0 CARGO_PROFILE_TEST_DEBUG=0 \
  cargo test -p doublets --test issue_518_missing_delete -- --test-threads=1
```

Verified against doublets-rs main at
`f08c386f29646c75511671dbdbe54ca0d179af83` (0.11.0). All four tests pass,
covering 48 store/scenario/API combinations and 96 rejected deletions.
Missing addresses return `Error::NotExists` without callbacks or changes to
count, contents, or subsequent allocation. Both backends validate with
`try_get_link` before mutation; the reset helper does so too. The reported C#
bug does not reproduce there, so no upstream bug report is needed.
