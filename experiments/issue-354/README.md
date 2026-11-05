<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# DeleteAll regression", "*"]
  }
}
-->

# DeleteAll regression

Issue: <https://github.com/linksplatform/Data.Doublets/issues/354>

Pull request: <https://github.com/linksplatform/Data.Doublets/pull/510>

The issue calls the C# `DeleteAll` loop confusing and suggests deleting
`links.Count()` until the count reaches zero. The original loop is worse than
confusing: it doesn't delete every link. The suggested loop is clearer, but it
is correct only when the store has never had a deletion.

## Root cause

The original C# loop decremented `i` twice per pass (`--i` in the condition
check and `i = --i` in the iterator). It deleted 3, skipped 2 and deleted 1,
leaving `(2: 2 2)` behind. It also relied on two assumptions that the memory
stores don't hold:

- `Count()` is the number of links, not the highest address. After a deletion
  in the middle, the highest address is above `Count()`. With points 1 to 5
  and points 2 and 3 deleted, `Count()` is 3 while link 5 still exists.
- The raw store `Delete(address)` doesn't check that the link exists. It
  frees any address below `AllocatedLinks`, so deleting a freed address frees
  it twice and corrupts the free list. It also doesn't reset the endpoints, so
  the source and target indexes keep entries for the deleted link.
  `GetOrCreate` then finds a stale entry instead of creating a new link.

The native `DeleteAll` was already the issue's loop: it deleted `Count()`
through the store's raw `Delete` until the count reached zero. It had the
same problems with freed addresses and stale index entries.

## Fix

Both ports now:

1. Walk up from address 1, counting existing links, until `Count()` links
   have been seen. The last one is the highest existing address.
2. Walk down from that address and delete each existing link with the
   handler-taking `Delete`, which resets the endpoints before deleting.

Deleting from the top means links are usually deleted before the links they
use, and the last deletion trims the store back to empty, so new links start
at address 1 again. Both passes are linear in the highest address and use
constant memory.

## Native `CreateAndUpdate`

The native regression test exposed one more defect on the reuse path. The
handler-less `CreateAndUpdate(storage, source, target)` recorded the new
address but returned the result of the handler-taking overload, which is
`Continue`. `GetOrCreate` therefore returned `Continue` whenever it created a
link. It now returns the recorded address, as the C# overload does.

## Probe

`DeleteAllProbe` runs every combination of loop variant, decorator and fill
scenario on united and split `uint` stores. Each case runs in a child process
with a step budget and a 30 second limit, because the original loop can
overflow the stack. A case reports the remaining links and whether a fresh
`GetOrCreate(1, 2)` returns a new link 3 that the indexes can find.

```sh
probe=experiments/issue-354/DeleteAllProbe
dotnet build "$probe" -c Release
dotnet "$probe/bin/Release/net8.0/DeleteAllProbe.dll"
```

Results are in
[probe.txt](../../dev/log/issues/354/pulls/510/validation/probe.txt):

- `main (original)` leaves links behind or breaks reuse in every raw and
  cascade scenario, and overflows the stack for a link to a higher address.
- `issue proposal` works for points only. Links between points leave stale
  index entries, a freed address leaves link 3 behind, the cascade
  decorator throws `Link 2 does not exist`, and the usages validator rejects
  deleting a point before the link that uses it.
- `ascending+exists` is correct for raw and cascade stores. With
  `LinksUsagesValidator` it fails as soon as it reaches a point that a link
  still uses.
- `descending+exists` and `library DeleteAll` give identical results: every
  raw and cascade scenario passes.

## Known limitation

`LinksUsagesValidator` rejects deleting a link that another link still uses.
Deleting from the top handles the usual order, where links use lower
addresses. A link that uses higher addresses, such as `(1: 2 3)` made by
updating point 1, still makes `DeleteAll` throw through the validator, because
no order is valid for every store without resolving usages. Use the cascade
decorator, `DecorateWithAutomaticUniquenessAndUsagesResolution`, for such
stores. The probe records this as the `usages-check | link to higher` case.

## Requirement inventory

- Simplify the confusing loop (issue): both ports replace it with two plain
  loops. The issue's single-loop version was rejected because it is wrong for
  stores with freed addresses or links, as the probe and the tests show.
- Check that the issue is still applicable (pull request comment): `main` still
  had the original loop in C#, and the issue's loop in C++.
- Use the simpler correct version across the codebase (pull request comment):
  both ports use the same algorithm. No other code deletes links by `Count()`.
  The random-deletion benchmark helpers pick random addresses below `Count()`,
  but they are benchmark helpers and are left unchanged.
- Regression tests: `DeleteAllTests.cs` (14 cases) and `DeleteAllTests.h`
  (united `uint64` and split `uint32` stores).

## Commands

```sh
dotnet test csharp/Platform.Data.Doublets.Tests -c Release \
  --filter FullyQualifiedName~DeleteAllTests
```

The two native tests are part of the normal CMake/CTest suite
(`bash cpp/setup.sh`, then `bash cpp/run_tests.sh`).
