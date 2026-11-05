# Issue 524 / PR 526 work plan

Objective: implement and verify every issue requirement in this PR; keep blocked
criteria explicit, preserve history, and do not merge the PR.

- [x] Confirm prepared branch and clean initial checkout; establish native goal.
- [x] Read issue, issue comments, all three PR feedback types, required sub-issues,
  and latest merged related PR. Initial feedback/sub-issue lists are empty.
- [x] Preserve run metadata, timestamps, SHAs, complete available logs, source
  snapshots, template trees, and online primary-source research in this folder.
- [x] Reconstruct timeline and inventory every acceptance criterion, warning,
  false positive, false negative, and error with root causes and solution plans.
- [x] Compare full template trees and every workflow, script, dependency/release
  configuration; adapt applicable best practices without removing features.
- [x] Create minimal automated reproductions of observed pipeline defects before
  fixing them. Keep reusable probes under experiments/ and CI checks in scripts/.
- [x] Repair C# test/build/package/release/docs/PDF paths and all matching defects.
- [x] Repair C/C++ CI, dependency setup, test discovery, packaging and publishing.
- [x] Apply relevant security, change detection, reproducible tooling, timeouts,
  artifact, formatting/static analysis, versioning and documentation practices.
- [x] Report reproducible upstream defects, including workarounds/code suggestions.
- [x] Run all local checks sequentially within the 2.92 GiB container; save logs.
  Use single build workers and bounded stress probes; do not repeat OOM builds.
- [x] Commit useful atomic steps after local checks; push only prepared branch.
- [x] Check recent CI runs against latest SHA/time, download all nonpassing logs,
  inspect actual errors with file/line references, fix, and verify fresh results.
- [x] Include latest main, review complete PR diff, verify no feature removal,
  refresh feedback, and update PR title/body with evidence and Fixes #524.
- [x] Verify all implementation acceptance criteria in full CI and assemble the
  final evidence, upstream verification updates and PR description.

Seventh run 40842727244 at 5cf40c5a338a38978221aab65a8e017005c95303 is
fully green: 82 C# cases per platform, 46 native cases per configuration, 44
pipeline regressions without skips, zero findings in three complete CodeQL
reports, zero compiler/runtime/action warnings, all installed consumers and
538 HTML / 277 PDF pages. Every artifact is archived and hash-verified.
Latest main is included; all nine automated inline findings are resolved and
there are no human requests or required child issues. No implementation is
left unfinished. Public publication still requires owner-configured credentials;
main fails explicitly with instructions, while PR checks are secret-free.

Final delivery requires the same green CI on the evidence-only commit, a clean
working tree, a final feedback/main refresh, and removal of draft status. Those
operations are recorded by GitHub and the final local run metadata, followed by
completion of the native goal. Do not merge this PR.
