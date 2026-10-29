# Issue 524 / PR 526 work plan

Objective: implement and verify every issue requirement in this PR; keep blocked
criteria explicit, preserve history, and do not merge the PR.

- [x] Confirm prepared branch and clean initial checkout; establish native goal.
- [x] Read issue, issue comments, all three PR feedback types, required sub-issues,
  and latest merged related PR. Initial feedback/sub-issue lists are empty.
- [ ] Preserve run metadata, timestamps, SHAs, complete available logs, source
  snapshots, template trees, and online primary-source research in this folder.
- [ ] Reconstruct timeline and inventory every acceptance criterion, warning,
  false positive, false negative, and error with root causes and solution plans.
- [ ] Compare full template trees and every workflow, script, dependency/release
  configuration; adapt applicable best practices without removing features.
- [ ] Create minimal automated reproductions of observed pipeline defects before
  fixing them. Keep reusable probes under experiments/ and CI checks in scripts/.
- [ ] Repair C# test/build/package/release/docs/PDF paths and all matching defects.
- [ ] Repair C/C++ CI, dependency setup, test discovery, packaging and publishing.
- [ ] Apply relevant security, change detection, reproducible tooling, timeouts,
  artifact, formatting/static analysis, versioning and documentation practices.
- [ ] Report reproducible upstream defects, including workarounds/code suggestions.
- [ ] Run all local checks sequentially within the 2.92 GiB container; save logs.
  Use single build workers and bounded stress probes; do not repeat OOM builds.
- [ ] Commit useful atomic steps after local checks; push only prepared branch.
- [ ] Check recent CI runs against latest SHA/time, download all nonpassing logs,
  inspect actual errors with file/line references, fix, and verify fresh results.
- [ ] Include latest main, review complete PR diff, verify no feature removal,
  refresh feedback, and update PR title/body with evidence and Fixes #524.
- [ ] Verify clean working tree, all acceptance criteria and latest CI, then mark
  PR ready and native goal complete only if no required work remains.
