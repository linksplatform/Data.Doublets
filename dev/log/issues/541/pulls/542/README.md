# Issue 541 / pull request 542 evidence

Investigation and work are confined to
[PR 542](https://github.com/linksplatform/Data.Doublets/pull/542).

- [Analysis](ANALYSIS.md): requirement inventory, timeline, causes, alternatives,
  implementation plans, all 17 linked best practices, primary sources and blockers.
- [Verification](VERIFICATION.md): before/after evidence, local checks, hosted
  results and explicit unfinished criteria.
- [Plan](PLAN.md): completed work and outstanding acceptance criteria.
- [Template comparison](research/TEMPLATE-COMPARISON.md): every tracked file in
  the complete C#, C++, Python and Rust templates, with transfer decisions.
- `github/`: exact issue/PR text, all comment types/reviews, run/job/artifact data,
  related PRs, public advisories and repository policy snapshots.
- `ci-logs/`: complete cited, related and implementation run logs, including
  recovered cancelled-run output; line numbers in the analysis refer to these
  unmodified downloads.
- `research/`: original full-tree template archives with exact revisions and
  SHA-256 file hashes, reports to upstream projects, real SARIF/CI artifacts,
  offline source probes and semantic review evidence.
- `tests/`: complete local experiment/check output, before/after audit JSON and
  the bounded native memory-stop record. Expected errors in baseline/fault
  injection logs are classified in the verification record.
- `SHA256SUMS`: integrity inventory of this committed evidence snapshot, excluding
  itself. Later CI downloads have their own GitHub artifact digest/metadata.

No screenshot is required: this issue changes build and release tooling, with no
product UI changes. No package publication, main write or branch-policy change is
represented as successful. Release authorization and native advisory-service
coverage remain unfinished as documented.
