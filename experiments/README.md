---
title: Deletion contract investigation
---

These finite probes support [issue #518](https://github.com/linksplatform/Data.Doublets/issues/518)
and the [architecture review](https://github.com/linksplatform/Data.Doublets/pull/527#issuecomment-6492081425).
Read the [C# guide](../docs/deletion-csharp.md), [Rust guide](../docs/deletion-rust.md),
and [design/audit evidence](../docs/deletion-audit.md) for the selected contracts.

Run from this repository root with .NET 8:

```sh
dotnet test csharp/Platform.Data.Doublets.Tests -c Release \
  --filter 'FullyQualifiedName~MissingLinkDeletionTests|FullyQualifiedName~DeletionContractTests' \
  --blame-hang-timeout 3m -m:1 -p:UseSharedCompilation=false
dotnet run --project examples/Deletion -c Release
dotnet run --project experiments/MissingDelete -c Release
```

The 96 missing-delete theory cases select throw or ignore explicitly, for both
stores and plain/automatic stacks. Each runs byte, ushort, uint and ulong
addresses. Twelve valid-delete cases check callback `Break` with bare, throw and
ignore policies. Three tests prove that the core and reset resolver omit
existence validation; they only pass valid reset links. Missing/no-op checks
verify contents, count, callbacks, search results and subsequent slot reuse.

The trace prints allocator headers and link metadata for a valid middle deletion,
optional rejection by the existing validator, reuse and following allocation.
It creates at most five links per store. Raw deletions reset endpoints first.
Tracing lives only in this explicit diagnostic program; library calls do not log.

To reproduce the unwanted compulsory checks from the previous PR head:

```sh
python3 experiments/reproduce_deletion_contract.py /path/to/dotnet
```

The script temporarily substitutes three implementations from `136ada5ad` and
restores source files in `finally`. Run with no concurrent builds/source edits.
Expected result: three failing contract tests. Only valid live/reset addresses
are deleted; the probe never deliberately corrupts raw memory. Rebuild afterward
to restore current binaries. The old baseline runner that deliberately submitted
invalid raw addresses has been replaced.

## Rust observations and recipes

Copy both Rust probes into a checkout of doublets-rs at the audited main revision:

```sh
cp /path/to/Data.Doublets/experiments/missing_delete.rs \
  doublets/tests/issue_518_missing_delete.rs
cp /path/to/Data.Doublets/experiments/deletion_policies.rs \
  doublets/tests/issue_518_deletion_policies.rs
```

Build sequentially with one job, debug info disabled, a 1.75 GiB per-process
address-space limit and an 8 MiB stack limit:

```sh
(
  ulimit -v 1835008 -s 8192
  export CARGO_BUILD_JOBS=1 CARGO_PROFILE_DEV_DEBUG=0 CARGO_PROFILE_TEST_DEBUG=0
  cargo test -j 1 -p doublets --test issue_518_missing_delete -- --test-threads=1
  cargo test -j 1 -p doublets --test issue_518_deletion_policies -- --test-threads=1
)
```

Verified at `31b38458efbd277b399e46b22474b85a85bda24b` (0.13.0): all four
missing-delete tests and both policy-example tests pass. Missing-delete coverage
is 48 combinations and 96 rejected operations, with no callbacks, count/content
changes or allocator corruption. The two policy tests exercise per-call ignoring
and explicit outer validation with both backends and automatic stacks.

These passing observations do not endorse embedded checks: Rust currently cannot
omit backend existence validation or endpoint reset. Those separate design gaps
are reported in [#80](https://github.com/linksplatform/doublets-rs/issues/80) and
[#81](https://github.com/linksplatform/doublets-rs/issues/81). No Rust backend code
is changed by this PR. Rust recipe code is included and verified here alongside
the C# documentation, as requested by the review.
