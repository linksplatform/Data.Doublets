# Issue 519 work plan

Objective: implement and verify all issue 519 requirements in PR 544 on
`issue-519-99675477c12d`, keeping any externally blocked criteria explicit.

- [x] Read the complete issue and all comments; inspect PR conversation, inline
  comments and reviews. No required sub-issues are listed.
- [x] Verify the prepared branch and clean starting tree; read CONTRIBUTING.md.
- [x] Trace result selection and callback control through plain and decorated
  stores, including uniqueness merges and UpdateOrCreateOrGet branches.
- [x] Add minimal failing regressions before changing the library, covering
  both united/split backends, raw/decorated behavior, surviving addresses,
  callback events, null handlers and Break propagation.
- [x] Implement the root-cause fix without changing caller-selected raw policies.
- [x] Audit current linksplatform/doublets-rs source for the same address and
  callback issues; retain bounded reproducible experiments and report confirmed
  analogous defects upstream, as requested in the issue comment.
- [x] Add the C# patch release fragment required by the automatic release workflow.
- [x] Run focused then full C# checks and CONTRIBUTING.md checks; save large logs.
  Keep memory-heavy commands sequential within the 2.92 GiB container budget.
- [x] Commit the implementation and regressions as a useful atomic step after
  local checks. Preserve history.

Finalization gates after the evidence commit (record their outcomes against the
final head in the live PR description):

- [ ] Push only the prepared branch. Merge current main if necessary.
- [ ] Update PR title/description with reproduction, tests, Rust audit evidence,
  requirement mapping and a positive `Fixes #519` closing reference.
- [ ] Review the complete PR diff and ensure no unrelated feature was removed.
- [ ] Verify fresh CI runs against the final commit and timestamps. Download any
  nonpassing run logs to ci-logs/, read errors with line numbers, fix and recheck.
- [ ] Refresh issue/PR comments and reviews, verify clean git status and mark
  PR 544 ready only after all criteria are met. Leave the PR unmerged.
