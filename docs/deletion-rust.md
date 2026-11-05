<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": [
      "# Choosing a deletion contract in Rust",
      "*"
    ]
  },
  "MD013": {
    "tables": false
  }
}
-->

# Choosing a deletion contract in Rust

This Rust guide and the [shared design guidelines](design.md) live in the same
PR as the [C# guide](deletion-csharp.md), so the intended architecture is explicit
for both implementations. Rust source was audited at doublets-rs main
[`31b38458efbd277b399e46b22474b85a85bda24b`](https://github.com/linksplatform/doublets-rs/commit/31b38458efbd277b399e46b22474b85a85bda24b)
(`doublets` 0.13.0). Current implementation differences are documented below;
C# and Rust's raw deletion contracts are not currently identical.

## Current APIs

Import `doublets::{Doublets, Links}` for high-level helpers and low-level CRUD,
and `doublets::decorators::DecoratorsExt` for compositions.

| Operation | Current behavior |
| --- | --- |
| `delete_links(&[id], &mut handler)` | Both backends verify existence, reset endpoints internally, release the slot, and return `Result<Flow, Error<T>>` |
| `delete_with(id, handler)` / `delete_by_with([id], handler)` | Forward to low-level deletion; callback result is flow |
| `delete(id)` / `delete_by([id])` | Return the former link address on success; propagate errors |
| `try_get_link(id)` | Return the link or `Error::NotExists(id)` without deleting |
| `get_link(id)` / `exist(id)` | Return `Option<Link<T>>` / membership for a per-call decision |
| `delete_all()` | Snapshot actual addresses and skip entries already removed by cascades |
| `delete_query_with`, `delete_usages_with`, `rebase_and_delete` | Compound helpers; their writes still use the selected inner stack |

Missing addresses currently return `Error::NotExists` even when all validators
are omitted. This prevents the C# missing-delete corruption in the original
report, but does not satisfy the design goal of an optional contract check.
Do not rely on a backend check as the architectural substitute for a configured
boundary.

## Explicit error policy

For a store-wide checked boundary, use the existing existence validator:

```rust
use doublets::{decorators::DecoratorsExt, mem::Global, unit, Doublets};

let mut links = unit::Store::<usize, _>::new(Global::new())?
    .with_automatic_uniqueness_and_usages_resolution()
    .with_inner_reference_existence_validation();
let id = links.create_point()?;
links.delete(id)?;
assert!(matches!(links.delete(id), Err(doublets::Error::NotExists(x)) if x == id));
```

It checks deletion before forwarding, and validates internal references in
updates/reads too. This makes the requested policy visible even though the
current backend repeats the existence check. For one checked operation, use
`try_get_link(id)?` before resetting or deleting instead.

## Ignore only a missing requested address

Rust currently has no dedicated ignore-missing-deletion builder. Use a per-call
helper, or a custom decorator that guards before any reset or cascade:

```rust
use doublets::{Doublets, Error};

fn delete_if_exists<L: Doublets<usize>>(
    links: &mut L,
    id: usize,
) -> Result<usize, Error<usize>> {
    if links.exist(id) {
        links.delete(id)
    } else {
        Ok(0) // No writes or callbacks; zero is the default null address.
    }
}
```

A helper with a handler should return `Ok(Flow::Continue)` without calling that
handler when absent. Check before the operation instead of suppressing every
error afterward: an error from a later cascaded write is different from the
requested address being absent. `with_no_exceptions()` maps all mutation errors
to `Flow::Break`; it neither selects only missing deletions nor rolls back
previous writes.

For deletion by pair, call `search(source, target)` and delete only its `Some(id)`
result. The endpoint-reset and usages policies still apply to the successful
operation. The existence check and deletion require exclusive operation ordering;
a check is not a transaction or a concurrency primitive.

## Contents, usages and decorator order

Use `.with_non_null_contents_deletion_resolution()` when the selected stack
should explicitly reset contents. The automatic uniqueness/usages composition
includes that layer and cascading. Select `.with_usages_validation()` to reject
incoming usages, or `.with_cascade_usages_resolution()` together with the reset
layer to remove them. A caller-managed sequence can remove or re-point usages
itself before deletion. Uniqueness is another independent choice.

Rust's decorators own the layers below them and do not propagate C#'s `Facade`.
An outer validator guards the entry operation, but is not automatically re-entered
by every inner cascade. Policies needed by recursive calls must also be available
below the cascading layer; inspect the composition's ordering. The automatic
cascade handles its recursive resets explicitly. A user-supplied ignore wrapper
should skip a missing entry before handing it to any inner resolver.

Callbacks occur after the corresponding write. `Flow::Break` does not reverse
that write or make a compound operation atomic. For bulk work, use actual
addresses and account for links already removed by cascades; do not assume that
`count()` is the largest live address. A missing-request guard does not repair
an invalid mutation that has already happened.

## Caller-managed unchecked route: current gap

The intended architecture allows a caller to guarantee a live address, detached
contents and resolved usages, then call a minimal release operation without
redundant contract verification or automatic resetting. Current Rust stores do
not expose that route:

- Both `update_links` and `delete_links` call `try_get_link` in the backend.
  The missing bypass is reported in [doublets-rs #80](https://github.com/linksplatform/doublets-rs/issues/80).
- Both backends call `self.update(index, 0, 0)` during deletion even if contents
  are already reset. Separating that policy is reported in
  [doublets-rs #81](https://github.com/linksplatform/doublets-rs/issues/81).

Removing a wrapper with `into_inner()` still reaches those checked/resetting
backends. There is no supported `delete_unchecked` recipe to demonstrate at this
revision. Do not manufacture one using raw pointers or claim that omitting
`InnerReferenceExistenceValidator` removes the backend cost. Future unchecked
primitives must use an appropriate Rust `unsafe` contract; safe public methods
must preserve memory safety. These separate implementation reports remain open.
Optional source/target indexing is also tracked in [#82](https://github.com/linksplatform/doublets-rs/issues/82);
it does not change the deletion recipes available at this revision.

## Reproducible examples

The [Rust policy probe](../experiments/deletion_policies.rs) compiles the address
helper and explicit validator compositions for both backends and automatic
stacks. The [missing-delete probe](../experiments/missing_delete.rs) records the
current embedded-check behavior. Copy them to a checkout of the audited revision
and run the [bounded commands](../experiments/README.md). The assertions about
current backend checks are observations, not a recommendation to keep that
policy in the storage root.
