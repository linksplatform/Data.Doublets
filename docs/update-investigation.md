<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# Update contract investigation for issue 517", "*"]
  },
  "MD013": { "tables": false }
}
-->

# Update contract investigation for issue 517

Investigated at main `8125301e57357bd7ed50d080eaaeb3f6fd319599`, after PR #527.
Read the complete [issue](https://github.com/linksplatform/Data.Doublets/issues/517),
its two comments, all PR #537 comment/review endpoints, and the
[architecture decision](https://github.com/linksplatform/Data.Doublets/pull/527#issuecomment-6492081425).
No required sub-issues were listed. The later issue comments supersede the
original suggestion to add mandatory checks to raw updates.

## Decision

Keep runtime behavior unchanged. Existing composition already allows users to
reject missing internal addresses, check one operation, skip missing operations,
or trust a caller-managed sequence. Adding compulsory backend checks would
remove an intentional option. A new default or convenience wrapper would not
add configurability to these cases. The missing pieces were update-specific
documentation, discoverability, executable examples and regression evidence.

The existing validator has a broader contract than checking just the address:
it validates every internal value in restriction and substitution, and also
participates in reads and deletions. Non-internal values are outside that
validator's scope. The [recipes](update-csharp.md) explain range checking for
untrusted addresses and how to choose a narrower per-call policy.

## Traced paths

- Both memory `Update` implementations obtain the slot and detach/attach indexes
  without first checking liveness. Unallocated and freed addresses are outside
  their caller contract; inserting a check here is deliberately rejected.
- The address overload constructs restriction/substitution lists and forwards
  to the selected store. The Platform.Data 0.16.1 adapter derives an address
  from callbacks; it does not establish liveness.
- The automatic composition adds uniqueness, contents reset and usages handling.
  `LinksUniquenessResolver.Update` searches for the requested pair before deciding
  whether to update or resolve a conflict. An inner validator is too late for an
  entry-boundary guarantee.
- `LinksInnerReferenceExistenceValidator.Update` validates both argument lists
  before forwarding. `EnsureInnerReferenceExists` only checks the configured
  internal range. Platform.Data's `Exists` accepts configured external references,
  so a range guard is needed to establish that an untrusted update id is internal.
- Per-call `EnsureLinkExists` or an `Exists` branch selects the same missing-slot
  decision without changing other operations. Endpoint validity remains a separate
  caller choice. Existence does not provide locking or stale-handle detection.

## Requirement and verification inventory

| Requirement from issue/comments | Evidence |
| --- | --- |
| Research whether more configurability requires runtime changes | Traced paths above; all initial 100 contract cases pass before library/documentation changes |
| Preserve raw update without compulsory existence validation | Two valid raw-store tests override existence checking to fail if called; verify callbacks, results and indexes |
| Reject never-created and freed addresses by explicit policy | Both backends, plain/automatic compositions, outer validator and per-call checks |
| Validate before resolver searches, resets or merges | Terminal spy sees no read/update/delete after outer validator rejection, including a missing endpoint |
| Do not silently add validation to automatic resolution | Terminal spy absorbs a missing request safely and records search/update forwarding; raw memory never receives it |
| Preserve contents, counts, source/target indexes and allocator | Snapshot, pair-search, source/target enumeration, absent address, callback and subsequent two-allocation assertions |
| Preserve valid checked updates | Four backend/composition cases check address results, callback data, `Break`, count and final search |
| Explain validator scope and alternatives | Service-constant test uses a terminal spy; recipes distinguish internal address, endpoint validation, no-op and raw policies |
| Make guidance usable and discoverable | Documentation navigation, README, XML remarks, executable `examples/Update`, automatic C# CI execution |

The missing-update matrix has 96 cases, each running byte, ushort, uint and ulong:
two backends × two compositions × three policies × four missing-slot scenarios
× two pair choices. Each invokes the address adapter, handler overload and primitive
null-handler form. Existing-pair cases exercise the boundary before a possible
uniqueness merge; unused-pair cases verify a later `CreateAndUpdate` is retained
and searchable. All invalid requests stop at an explicit policy or terminal spy.
The suite never intentionally corrupts raw memory or relies on a hang to pass.

The reported raw corruption was not replayed because the issue's architecture
feedback explicitly excludes invalid raw writes as in-process regression probes.
Tests establish the selected contract on current main; they are not evidence of
a newly repaired backend bug. See the PR for final full-suite and hosted results.

The [negative experiment](../experiments/verify_update_contract_guards.py)
temporarily inserts mandatory raw checks and removes the outer validator only
in the terminal-spy test. Exactly three contract tests fail, then sources are
restored. This verifies the regression assertions without unsafe raw requests.
See the [experiment commands](../experiments/README.md) for reproduction.

## Scope of related work

The shared design already covers C# and Rust. PR #527's
[audit](deletion-audit.md) separately tracks mandatory Rust mutation checks,
index selection and allocation proposals. Those are not dependencies of this
C# update-contract investigation, and no Rust implementation changes are claimed.
