# Issue 517 investigation checklist

Objective: implement and verify the complete selected update contract in PR 537,
preserving caller-selected validation and the unchecked raw route.

- [x] Read the complete issue and comments, PR conversation/inline comments and
  reviews, related PR 527's architecture review, and contributing guidance.
- [x] Check the prepared branch and working tree; fetch main; check sub-issues
  (none listed) and inspect recent merged work and CI timestamps/SHAs.
- [x] Trace both raw updates, Platform.Data address adapters, the automatic
  resolver composition, validator, reference classification and callback flow.
- [x] Establish a safe baseline with finite checked missing-address cases and
  valid unchecked calls; never pass an invalid address into raw memory update.
- [x] Test both backends, plain/automatic stacks, missing/deleted addresses,
  extension/primitive overloads, callbacks, unchanged queries and allocation.
- [x] Verify validation happens before resolver search/reset/merge; verify
  the raw route omits existence checking using only valid live links.
- [x] Decide from evidence whether runtime changes improve configurability;
  document the decision, exact contracts, limitations and executable recipes.
- [x] Publish update guidance in the documentation navigation and README;
  ensure examples run automatically and add any required release changeset.
- [ ] Run the full C# suite, examples, formatting, package smoke test,
  repository/pipeline checks, docs/link validation and applicable local CI.
  Run memory-heavy builds sequentially within the container budget.
- [ ] Commit each useful atomic step after local checks; preserve history;
  push only issue-517-8bdec171952f and update the existing PR's title/body.
- [ ] Inspect the final PR diff, merge current main if necessary, recheck
  all comments, and verify the working tree is clean.
- [ ] Verify hosted checks match the final SHA and finish successfully. For any
  failure, download logs to ci-logs/, read chunks of at most 1500 lines, identify
  exact errors/lines, fix and verify; do not rely on stale passing runs.
- [ ] Mark PR 537 ready, leave it unmerged and retain Fixes #517 in its body.

Logs and local build output live in ignored artifacts/issue-517/. Any bounded
investigation scripts stay in experiments/; runnable usage goes in examples/.
