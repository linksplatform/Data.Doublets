<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# C# update contracts", "*"]
  },
  "MD013": { "tables": false }
}
-->

# C# update contracts

`UnitedMemoryLinks<T>` and `SplitMemoryLinks<T>` update a live address under
caller-managed preconditions. Missing-address validation is an optional policy,
as described in the [shared design](design.md). The automatic uniqueness/usages
composition does not select that policy. A returned address does not establish
that an unchecked request was valid.

For [issue #517](https://github.com/linksplatform/Data.Doublets/issues/517), the
existing validator and per-call helpers provide the requested checked boundary.
No new decorator or compulsory backend check is needed. See the
[investigation and evidence](update-investigation.md) and
[runnable recipes](../examples/Update/README.md).

## Choose an update contract

Import `Platform.Data`, `Platform.Data.Doublets`, and
`Platform.Data.Doublets.Decorators` for these APIs.

| Choice | Missing internal address | Other responsibilities |
| --- | --- | --- |
| Raw store or automatic uniqueness/usages composition | Outside the caller contract; no guaranteed exception or result | Caller guarantees a live update address and valid endpoints/index state |
| Outer `LinksInnerReferenceExistenceValidator<T>` | Throws `ArgumentLinkDoesNotExistsException<T>` before forwarding | Validates every internal reference in both restriction and substitution, and also affects reads/deletion |
| `EnsureLinkExists(id, nameof(id))` before updating | Throws before updating | Checks one reference; endpoint policy remains separate |
| `if (links.Exists(id))` before updating | Skips the complete update | Caller chooses how to represent the skipped result |
| Application-specific `LinksDecoratorBase<T>.Update` override | Whatever the application explicitly selects | Guard before forwarding; use a focused wrapper when the broader validator contract is unsuitable |

The update **address** must be a live internal link. Null, `Any`, `Itself`, other
service constants and external values cannot identify a raw slot to update.
`EnsureInnerReferenceExists` and its validator only check values classified as
internal references: they do not turn arbitrary numeric inputs into valid update
addresses. `Exists` treats configured external references as existing values,
so `EnsureLinkExists` alone also does not prove an external value is a live slot.
For untrusted address input, first require `Constants.IsInternalReference(id)`:

```csharp
if (!links.Constants.IsInternalReference(id))
    throw new ArgumentOutOfRangeException(nameof(id));
links.EnsureLinkExists(id, nameof(id));
links.Update(id, source, target);
```

That range check concerns the update address. Null endpoints can be useful for
resetting; supported external endpoints depend on the configured backend.
The inner-reference validator permits such non-internal endpoint values; it
does not enforce a particular application's external-reference rules.

## Store-wide rejection before resolver work

Put the validator outside the complete resolver stack:

```csharp
ILinks<uint> links = new LinksInnerReferenceExistenceValidator<uint>(
    raw.DecorateWithAutomaticUniquenessAndUsagesResolution());
links.CreatePoint();
links.CreatePoint();
links.Update(10u, 1u, 2u); // Throws before search, reset, merge or memory update.
```

For a plain store, construct `new LinksInnerReferenceExistenceValidator<uint>(raw)`.
The same boundary rejects a freed middle slot before its free-list metadata can
be interpreted as a live link. It also rejects missing internal source/target
references and internal references in a low-level substitution. If only address
validation is wanted, use the per-call recipe or a focused application wrapper.

Wrapping `raw` with the validator and then adding resolvers outside it does not
provide this ordering: uniqueness can search or merge before the check is reached.
C# decorators propagate the outer `Facade` for recursive resolver operations;
validate at the entry boundary, and use that boundary consistently.

`DecorateWithAutomaticUniquenessAndUsagesResolution()` composes, outermost first,
`LinksCascadeUniquenessAndUsagesResolver`, `NonNullContentsLinkDeletionResolver`,
and `LinksCascadeUsagesResolver`. Their names describe resolution policies,
not existence guarantees. A deletion ignore policy likewise does not guard updates.

## Per-call checks and missing-address no-ops

When `id` is already known to be an internal address, choose a throwing check:

```csharp
links.EnsureLinkExists(id, nameof(id));
links.Update(id, source, target);
```

Or skip a missing update entirely:

```csharp
if (links.Constants.IsInternalReference(id) && links.Exists(id))
{
    links.Update(id, source, target);
}
```

The skipped branch performs no update, resolver search, mutation or callback.
Return a separate status or a chosen application value if a result is required.
Do not fabricate a successful updated address. `NoExceptionsDecorator` suppresses
exceptions but does not undo earlier writes or validate unsafe requests.

## Caller-managed updates and overload results

For a valid live link, raw updates retain the route without existence validation:

```csharp
var first = raw.CreatePoint();
var second = raw.CreatePoint();
raw.Update(second, first, second);
```

The current layout requires coherent index/free-list metadata, a live internal
update address, and endpoints that the selected indexing strategy supports.
For split internal indexing, use live internal endpoint links (or null to reset);
external endpoints require the configured external range. Updating a freed slot
or an unallocated address violates these preconditions and may corrupt indexes,
allocation or memory, or hang. The observed outcomes in the issue are not a
supported missing-address policy. Do not probe invalid raw writes in-process.

| Overload | Result |
| --- | --- |
| `Update(id, source, target)` | Address adapter: derives its result from the write callback |
| `Update(id, source, target, handler)` | Inner flow result, such as `Continue` or `Break` |
| `Update(restriction, substitution, handler)` | Primitive flow result; null handler does not invoke a callback |
| `Update(restriction, substitution)` | Address adapter around the primitive |

Use a single concrete address in primitive restrictions and keep the substitution
index consistent with that address. The raw implementation does not implement a
general query/update language. Convenience `Update` overloads taking two or four
values can select compound merge or update/create/get operations; they are not
missing-address validators. Uniqueness resolution can merge/delete links, so
the result and callbacks follow the selected stack's behavior.

`Break` is callback flow after a write; it does not roll the write back. Guarding
existence is not synchronization. Prevent concurrent deletion/reuse and unexpected
callback mutation between checking and updating. A reused numeric address names
its new live link; these checks do not supply generation/version identity.
