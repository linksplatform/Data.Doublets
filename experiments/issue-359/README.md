<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# Positional query regression", "*"]
  }
}
-->

# Positional query regression

Issue: <https://github.com/linksplatform/Data.Doublets/issues/359>

Pull request: <https://github.com/linksplatform/Data.Doublets/pull/413>

## Plan and requirements

- [x] Read the complete issue and all issue/PR comments, inline comments and reviews.
  There are no required sub-issues. The additional PR request is to validate the
  critique, cover both operations and compare with `linksplatform/doublets-rs`.
- [x] Merge current `main`, resolve conflicts and preserve existing features.
- [x] Reproduce endpoint confusion in both layouts and languages with real stores.
- [x] Fix three-part concrete-index `Count` and `Each`: source restrictions match
  only the source; target restrictions match only the target; `Any` leaves its
  own position unrestricted.
- [x] Preserve two-part either-endpoint queries, double wildcards, exact matches,
  missing/deleted indices, callback return values, and all supported index modes.
- [x] Verify byte/ushort/uint/ulong addresses, raw/decorated stores, null endpoints,
  self references, internal/external endpoints and slot reuse.
- [x] Compare the same cases with pinned current Rust united/split implementations.
- [x] Replace the misleading mock example and overwrite explanation with evidence.
- [ ] Add a patch changeset for both packages; run focused local verification and
  current-commit CI; investigate and archive any non-passing runs.
- [ ] Review the final diff and description, push all work, confirm a clean tree,
  and mark PR #413 ready with `Fixes #359` retained.

## Initial finding

The critique is valid. The old three-part concrete-index path selects the
non-wildcard value, then checks **both** stored endpoints. For `(id: a b)` where
`a != b`, `[id, Any, a]` and `[id, b, Any]` incorrectly match. They should yield
count zero and no callbacks. `[id, Any, b]` and `[id, a, Any]` should match once.
This affects both `Count` and `Each` in united and split C#/C++ stores.

The earlier PR fixed only split Count and C++ split Each. Its overwrite theory
is incorrect: a preceding double-wildcard branch already returns before this
code, so both assignments cannot run here. The mock example also used `0` as
`Any`, whereas the real constants distinguish null from wildcard.

Two-part `[id, value]` deliberately means either endpoint and must stay that way.
The Rust united and split implementations use position-specific predicates for
three-part queries, agreeing with the issue's suggested behavior.

## Verification

The baseline is `80172fcd0` (current main at the start of this work).

- All **32** new C# cases fail against the baseline; all pass after the fix.
  Each case also runs byte, ushort, uint and ulong stores. The original PR's
  partial patch still failed 20 cases.
- Both new native regressions fail against the baseline and pass after the fix.
  They exercise uint32/uint64 united/split, raw/decorated stores and both mutable
  and const Count overloads. The single GCC compiler peaked at 563,952 KiB RSS,
  with a 1,900 MiB virtual-memory cap and `-O0 -g0`.
- Both languages compare Count/Each with a snapshot oracle over every live row
  and all 36 source/target combinations from wildcard, null, internal, external
  and missing values. Each checks Continue and Break, zero/one callbacks and
  full returned records. Repeated checks cover updates and free-slot reuse.
- Existing either-endpoint tests and full CI protect unrestricted/global queries.
- `examples/Queries` is a real-store trace with assertions, now executed by CI.
  It replaces the standalone mock example; normal library operations stay silent.

Run the focused checks with:

```sh
dotnet test csharp/Platform.Data.Doublets.Tests -c Release -m:1 \
  -p:UseSharedCompilation=false --filter FullyQualifiedName~PositionalQueryTests \
  --blame-hang-timeout 3m
dotnet run --project examples/Queries -c Release
# After normal cpp/setup.sh and CMake configuration with compile_commands.json:
python3 experiments/issue-359/focused-native.py
```

`reproduce-before.py` temporarily loads pinned baseline library sources, runs
its command and restores the working sources even when a test fails. Do not run
other source builds concurrently with this probe. For example:

```sh
python3 experiments/issue-359/reproduce-before.py \
  dotnet test csharp/Platform.Data.Doublets.Tests -c Release -m:1 \
  -p:UseSharedCompilation=false --filter FullyQualifiedName~PositionalQueryTests
```

## Rust comparison and compatibility decision

The standalone `RustParity` project pins upstream commit
`e58169a34f02d506d006c4399535ba1a097e7db2` and its dependency lockfile. It verifies
**1,116 three-part queries per layout**, including Count, Each, callback flow,
null/external endpoints, holes, updates and slot reuse. Both layouts pass.

```sh
CARGO_BUILD_JOBS=1 CARGO_PROFILE_DEV_DEBUG=0 cargo run \
  --manifest-path experiments/issue-359/RustParity/Cargo.toml --locked -j 1
```

The Rust [united predicates][rust-unit] and [split predicates][rust-split]
restrict each endpoint independently, as this fix does. An optional wider audit
also found an existing Rust split **two-part** Count inconsistency: for
`(3: 1 2)`, `[3, 1]` yields Count 0 while Each invokes its callback once. The
split Count code requires both endpoints to equal the value; its Each and both
united methods accept either endpoint. The audit finds 25 such differences in
the finite dataset and exits 1 to make the discrepancy visible:

```sh
CARGO_BUILD_JOBS=1 CARGO_PROFILE_DEV_DEBUG=0 cargo run \
  --manifest-path experiments/issue-359/RustParity/Cargo.toml --locked -j 1 \
  -- --audit-two-part
```

The compatibility decision is to preserve C#/C++'s established either-endpoint
meaning for two-part queries. It agrees with Each and Rust united Count, avoids
changing users' existing short queries, and keeps Count equal to the number of
callbacks. The three-part behavior requested by #359 agrees across all ports.
This comparison does not claim equivalence for unrelated APIs.

[rust-unit]: https://github.com/linksplatform/doublets-rs/blob/e58169a34f02d506d006c4399535ba1a097e7db2/doublets/src/mem/unit/store.rs#L439
[rust-split]: https://github.com/linksplatform/doublets-rs/blob/e58169a34f02d506d006c4399535ba1a097e7db2/doublets/src/mem/split/store.rs#L733

## CI investigation

The first run on `b60b11901`, created after that commit at 19:58:34 UTC on
2026-11-05, failed in quality before builds. Its preserved log reports:

- `ci-42098974375.log:1027-1028`: Zizmor rejects the two CodeQL `# v4` comments
  because that moving tag now points at `33d7b85da1a0...`.
- `ci-42098974375.log:1029`: quality exits 13.
- `ci-42098974375.log:1255`: required correctly rejects failed quality.

The pinned action commit is actually release **v4.39.1**, verified through the
upstream tag object. Correcting the two comments retains the exact action code
and all security checks. This inherited main failure is addressed in this PR.

Before/after test logs and the complete failed CI log are retained under
`dev/log/issues/359/pulls/413/`.
