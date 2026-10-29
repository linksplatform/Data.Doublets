<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": [
      "# Deletion and policy audit for issue 518",
      "*"
    ]
  },
  "MD013": {
    "tables": false
  }
}
-->

# Deletion and policy audit for issue 518

Scope: all issue #518 and PR #527 comments/reviews, all open and closed issue
listings in both repositories, and the C#/Rust mutation paths, helpers,
decorator bases, existence, reset, usages, uniqueness, suppression and builder
implementations. C# base: `799502ce9` (`main`); previous PR head: `136ada5ad`.
Rust main: `31b38458efbd277b399e46b22474b85a85bda24b` (0.13.0).
No required sub-issues were listed. Other design findings are explicitly separate
work per the [review](https://github.com/linksplatform/Data.Doublets/pull/527#issuecomment-6492081425).

## Requirement and evidence inventory

| Requirement | Implementation / verification |
| --- | --- |
| Keep C# raw delete unchecked | Removed the previous PR's `Exists` branches in both memory backends; valid raw calls in `DeletionContractTests` fail if the core validates existence |
| Keep a reset resolver focused on reset | Removed its added `EnsureLinkExists`; a stub regression rejects unexpected existence validation |
| Configurable missing deletion: error | Existing `LinksInnerReferenceExistenceValidator<T>`; outer-policy tests for both backends, plain and automatic stacks |
| Configurable missing deletion: no-op | Optional `LinksNonExistentDeletionResolver<T>`; no inner mutation or callback when absent |
| Preserve store on selected checked failures/no-ops | 96 theory cases × four address widths, three API shapes, missing/repeated/last/empty/zero/extreme addresses; count, full contents, callback, search and allocator reuse assertions |
| Preserve valid deletion/results | 12 cases × four widths, bare/throw/ignore policies, both backends and optional automatic stack; `Break` callback, count and subsequent reuse |
| Document C# use cases and overloads | [C# guide](deletion-csharp.md), XML contracts and executable `examples/Deletion` for both backends |
| Document Rust use cases and vision | [Rust guide](deletion-rust.md) and [shared design](design.md); actual limitations distinguished from intended architecture |
| Check Rust for the original symptom | Four bounded missing-delete tests: 48 combinations / 96 rejected deletions; no corruption or callbacks |
| Audit and report other design errors separately | Findings below, with existing issue references or new reports; no unrelated code fixes included |

## Findings and separate reports

| Finding | Evidence and disposition |
| --- | --- |
| C# mandatory delete validation added by the old PR | Reproduced by three failing tests at `136ada5ad`, corrected in this PR; no invalid raw address needed |
| C# missing-update issue proposes mandatory base checking | [#517](https://github.com/linksplatform/Data.Doublets/issues/517); added [architecture guidance](https://github.com/linksplatform/Data.Doublets/issues/517#issuecomment-6492359137) to use an explicit validator or per-call check instead |
| Rust core existence validation cannot be bypassed | `mem/unit/store.rs` lines 504/554 and `mem/split/store.rs` lines 796/868; reported as [#80](https://github.com/linksplatform/doublets-rs/issues/80) |
| Rust core deletion resets even without the reset layer | Same files, lines 555/872; reported separately as [#81](https://github.com/linksplatform/doublets-rs/issues/81) |
| Rust uniqueness issue proposes embedding existence validation in a resolver | [#66](https://github.com/linksplatform/doublets-rs/issues/66); added [architecture guidance](https://github.com/linksplatform/doublets-rs/issues/66#issuecomment-6492359643) about an outer validator |
| C# indexes are mandatory in the current memory layout | Already tracked in [#239](https://github.com/linksplatform/Data.Doublets/issues/239) and [#395](https://github.com/linksplatform/Data.Doublets/issues/395); no duplicate report |
| Rust indexing is also inseparable from the current store/liveness representation | Tree fields and `is_unused` metadata inspected in both layouts; reported as [#82](https://github.com/linksplatform/doublets-rs/issues/82) |
| C# reuse of free slots has no append-only selection | Already tracked in [#172](https://github.com/linksplatform/Data.Doublets/issues/172); no duplicate report |
| Earlier C# unchecked-decorator request | [#51](https://github.com/linksplatform/Data.Doublets/issues/51) explicitly requested omitting Ensure checks; consistent with this PR, not a new error |
| Earlier proposal to make uniqueness checking the default | [#514](https://github.com/linksplatform/Data.Doublets/issues/514) was closed after recognizing the optional decorator design; no new default introduced |
| Numeric/dangling references are application-dependent | [#508](https://github.com/linksplatform/Data.Doublets/issues/508); no global reference validation added |

`LinksDecoratorBase` forwards calls; the automatic stack does not require a
validator. Explicit existence/usages/uniqueness validators, exception suppression,
logging, and transactions are selectable layers, so their named behavior is not
a compulsory-core-contract violation. Rust `try_get_link` in an explicitly chosen
helper is likewise appropriate; the issue is its unavoidable use in root writes.

Index detachment and split internal/external reclassification are necessary for
the selected representation. Address-capacity limits are structural bounds.
They were not removed or misreported as optional application validation. The audit
does not claim an index-free backend or fully optional allocator is already available.

## Verification

The three new contract tests fail against the previous PR implementation and pass
after removing its mandatory checks. The [baseline runner](../experiments/reproduce_deletion_contract.py)
reproduces this safely with valid reset links and restores source files afterward.
[Experiment instructions](../experiments/README.md) preserve commands, finite
inputs and Rust build limits. The example exercises raw, per-call, pair-based,
throw, ignore, usages-reject, cascade and snapshot bulk deletion in both stores.

The updated PR records final local and CI results at its final commit. Other
reports above remain separate and are not acceptance criteria to implement here,
as explicitly requested by the reviewer.
