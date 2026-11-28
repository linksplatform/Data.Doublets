<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": [
      "# C# deletion contracts",
      "*"
    ]
  },
  "MD013": {
    "tables": false
  }
}
-->

# C# deletion contracts

The [design guidelines](design.md) keep storage and application policies separate.
Both `UnitedMemoryLinks<T>` and `SplitMemoryLinks<T>` expose unchecked deletion.
Choose a check per call or an outer decorator when addresses may be missing.
The [runnable example](../examples/Deletion/Program.cs) checks the principal
recipes below with both heap-backed stores:

```sh
dotnet run --project examples/Deletion -c Release
```

## API and overload contracts

Import `Platform.Data`, `Platform.Data.Doublets`, and
`Platform.Data.Doublets.Decorators` for the helpers and decorators.

| Call on an undecorated store | What it does | Required caller guarantees |
| --- | --- | --- |
| `Delete(new[] { id }, handler)` | Release the slot; return handler flow or `Continue` | Live address, endpoints already reset, incoming usages handled |
| `Delete(id)` | `Platform.Data` address adapter; return deleted address from callback | Same preconditions as the inner deletion; no automatic reset or missing check |
| `Delete(id, handler)` | Reset endpoints when `Exists(id)`, then call inner deletion | Address still must exist; missing addresses are forwarded; incoming usages handled separately |
| `EnsureLinkExists(id, name)` | Throw `ArgumentLinkDoesNotExistsException<T>` if missing | No deletion; caller chooses the subsequent operation |
| `DeleteIfExists(id)` | Check for a live internal address, reset endpoints, and delete. Return the deleted address or `Null` | Handle incoming usages or select a usage policy |
| `DeleteIfExists(source, target)` | Search by pair, reset the matched link's endpoints, and delete it. Return the matched address or `Null` | Handle incoming usages or select a usage policy |

`Delete(id)` and `Delete(id, null)` select different helpers. Do not infer safety
from the method name alone. A handler overload returns a flow constant, not the
deleted address. An ignored deletion produces no callback, so the address-returning
adapter returns `Null` while the low-level policy returns `Continue`.

## Caller-managed fast path

Use a valid live address, detach its endpoints, and resolve incoming usages in
your application before releasing it. An isolated point has no other users:

```csharp
var id = raw.CreatePoint();
raw.Update(id, 0u, 0u);
raw.Delete(new[] { id }, null);
```

The unchecked path deliberately trusts the caller's operation order. Resetting
through `Update` removes the link from the source and target indexes before the
allocator can overwrite or reuse its slot. Releasing an indexed slot directly
can leave stale entries and cause `SearchOrDefault` to miss other live links.
For example, create `(1, 1)`, `(2, 2)`, `(1, 2)`, `(2, 1)`, and `(2, 3)` in
that order. To delete address 4, reset it first so searches for `(2, 2)` and
`(2, 3)` still find addresses 2 and 5.

The memory backend performs no existence validation for this deletion. Never use
repeated deletion of a raw address as a membership test. Zero, service constants,
external references, freed slots and unallocated addresses do not satisfy the
live-link precondition. `AreValuesReset` and `EnforceResetValues` also require an
existing link; they are not missing-link validators.

## Checks for one call

For a missing-address exception, validate before resetting or deleting:

```csharp
raw.EnsureLinkExists(id, nameof(id));
raw.Delete(id, null); // This helper supplies endpoint reset.
```

For an address-based no-op, use the optional per-call helper:

```csharp
var deleted = raw.DeleteIfExists(id); // Returns Null for a missing address.
```

The address helper skips non-internal addresses and performs no writes when the
slot is absent. The pair-based `DeleteIfExists(source, target)` skips a failed
search. Both helpers reset the link before deleting it, including on a raw store.

## Store-wide policies

Use the existing validator to reject missing deletion requests:

```csharp
ILinks<uint> links = new LinksInnerReferenceExistenceValidator<uint>(
    raw.DecorateWithAutomaticUniquenessAndUsagesResolution());
```

It also validates internal references in `Each` and `Update`. If that broader
contract is unsuitable, choose per-call `EnsureLinkExists`, or implement a
focused `LinksDecoratorBase<T>.Delete` override for your application.

Use the optional ignore policy for idempotent deletion requests:

```csharp
ILinks<uint> links = new LinksNonExistentDeletionResolver<uint>(
    raw.DecorateWithAutomaticUniquenessAndUsagesResolution());
```

It checks existence, returns `Continue` without inner writes or callbacks for a
missing address, and forwards an existing deletion unchanged. It does not add
resetting, cascading, uniqueness or update validation. To reset without cascading,
wrap `raw` in `NonNullContentsLinkDeletionResolver<uint>` instead of the automatic
composition in these snippets.

Place the chosen missing-address policy **outside** reset and cascade layers.
Otherwise a reset resolver may read a missing link before the guard sees it.
The automatic composition itself only resolves uniqueness, contents and usages;
it does not promise missing-address validation. Adding a policy decorator updates
the C# `Facade`, so recursive cascade calls re-enter the selected outer layers.
If you compose both throw and ignore policies, the outer policy decides which
missing requests reach the inner policy.

## Incoming usages and bulk operations

Existence does not decide what to do with other links referencing the deleted
link. Choose one of these contracts:

- Manage usages yourself before raw deletion, for example by removing or
  re-pointing references according to your data model.
- Reject incoming usages with `LinksUsagesValidator<T>`. A checked reset-only
  composition, outermost first, is `LinksInnerReferenceExistenceValidator` →
  `NonNullContentsLinkDeletionResolver` → `LinksUsagesValidator` → raw store.
  Self references are excluded from the usages check.
- Cascade usages with `DecorateWithAutomaticUniquenessAndUsagesResolution()` or
  the explicit `NonNullContentsLinkDeletionResolver(LinksCascadeUsagesResolver(raw))`
  stack, and place the missing-address policy outside it. Cascading deletes other
  links too; C# cycle/recursion behavior is not strengthened by this PR.

`DeleteMany`, `DeleteByQuery`, `DeleteAllUsages`, `DeleteAll`, and `MergeAndDelete`
reset endpoints before releasing slots. Apply the chosen incoming-usage and
missing-address policies to each operation. For a snapshot that may contain links
removed by earlier cascades, use `DeleteIfExists(id)` for each address or wrap the
store in the ignore policy. `DeleteAll` accounts for holes and works down from
the highest live address. `MergeAndDelete` rewrites incoming usages first.

`NoExceptionsDecorator` suppresses exceptions; it does not undo earlier writes
and cannot make an invalid unchecked deletion safe. Callback `Break` affects
handler flow after mutation; it is not a transaction rollback. Existence checks
are not synchronization: callers must prevent conflicting concurrent writes
and must not mutate the store unexpectedly from callbacks between steps.
