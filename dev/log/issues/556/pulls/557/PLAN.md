# Issue 556 / PR 557 work plan

Objective: fully update every live dependency, repair the merged updates without reverting them, and verify the resulting repository in PR 557. The native goal remains active until every acceptance criterion is verified.

## Requirements and verification

- [x] Read the entire issue, comments, required sub-issues, PR conversation, inline comments and reviews; save snapshots under github/. No sub-issues or review feedback were present initially.
- [x] Preserve and investigate latest failing default-branch and prepared-branch runs by timestamp and SHA; reproduce root causes with minimal regression tests.
- [x] Inventory every live manifest, lockfile, toolchain, action, image and native dependency. Record registry-resolved previous/latest/selected versions and explicit reasons for exceptions.
- [x] Update C#/.NET first, including every example/experiment, SDK, framework, packages and transitive lock entries; examine major migration guides and run build/test/format/package/consumer checks.
- [x] Update Python requirements, runtime floor, lint/type tools and all transitive resolutions; run all pipeline regression tests, lint, formatting, typing, docs and security audit.
- [x] Update Rust experimental manifests, locks, edition/toolchain and submodule consistently; run their build/test/lint and security audits within memory limits or in CI.
- [x] Investigate detected JavaScript: distinguish inline Actions scripts from a missing application manifest and document findings.
- [x] Update all Actions, reusable workflows, pre-commit, base images, native packages and build/analyzer tools. Inspect changelogs and remove workarounds replaced by upstream features.
- [x] Apply outstanding updates in PRs 397, 534, 549, 552 and 555, accounting for dependencies no longer used; repair actual workflow/settings faults without bypassing protection.
- [x] Expand Dependabot coverage to all real supported manifests and directories without configuring nonexistent manifests.
- [x] Search existing upstream reports for retained/new workarounds; report reproducible general issues upstream and link reports in analysis and PR.
- [x] Add release changesets as required; preserve all public behavior unless migration requires a documented change.
- [x] Commit useful atomic changes and push only issue-556-7e49ed94b898. Merge current main into this branch without rewriting history.
- [x] Read complete PR diff, refresh all feedback, confirm clean tree and current-SHA passing full CI, update title/body including Fixes #556, and mark PR 557 ready without merging.
- [x] Confirm default-branch CI recovery where authorized and possible. The original upload-only failure was transient; its full rerun passed. Full current-main CI was also dispatched as 42136956911. This PR stays unmerged.

## Investigation protocol

List five latest branch runs with createdAt/headSha; compare with latest commit; download every nonpassing run to ci-logs/; inspect logs in chunks no larger than 1500 lines; record actionable error line numbers and reproduction. Expected negative-test stderr is not itself a CI failure. Run memory-heavy validation sequentially with one worker. Save experiments in experiments/issue-556 and concise research/evidence here.

## Candidate solutions

Prefer official package upgrades and upstream functionality over custom compatibility code. Use existing regression and integration frameworks; add targeted cases for observed failures. Keep necessary upstream workarounds until reproduced fixes are available. Use existing CI for native/compiler matrices and CodeQL; add missing Rust coverage because this issue explicitly requires complete ecosystem validation. No npm project is needed solely for github-script/Actions JavaScript; establish actual ownership before adding manifests.

Finalization is enforced against the live latest-head PR checks before marking the PR ready. Hosted checks, rather than superseded run snapshots, determine completion.
