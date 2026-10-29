<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# Issue 519 / PR 544 evidence", "*"]
  }
}
-->

# Issue 519 / PR 544 evidence

The implementation is in [PR 544](https://github.com/linksplatform/Data.Doublets/pull/544).

- [Analysis](ANALYSIS.md): acceptance criteria, root causes, implementation choices,
  C# regression coverage and the Rust audit/report.
- [Plan](PLAN.md): tracked implementation steps and final verification procedure.
- `tests/`: local reproductions and verification output. The original C# run
  intentionally failed before library changes; the optional Rust logging probe
  intentionally fails on the pinned upstream revision. These are negative
  regression evidence, distinct from the passing final C# suite/default audit.
- [Rust upstream report](upstream/rust-logging-break.md) and its
  [issue URL](upstream/rust-logging-break-url.txt).
- [Reusable Rust audit](../../../../../../experiments/issue-519/README.md).

Hosted CI results and final readiness are recorded in the PR description after
the evidence commit, so they can identify the exact validated head without
creating another commit solely to record its own hash.
