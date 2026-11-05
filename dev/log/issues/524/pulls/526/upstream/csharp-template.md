Found while comparing the entire template tree for linksplatform/Data.Doublets#524 / https://github.com/linksplatform/Data.Doublets/pull/526.

At 4cc372ebf2075a0c948b46881143f7d66f0f0e7e, `.github/workflows/release.yml` has workflow-level concurrency with `cancel-in-progress: false` on main, but no `queue: max`. GitHub's default retains only one pending run. Thus a third push can replace a second pending release, even though running releases are not cancelled. Reader jobs also share the writer's workflow group.

Reproduce: start a main release and keep its writer running; push two more independent changesets. Observe the second pending run being cancelled rather than all three writers completing. No production packages need to be published: a test writer can simply wait and print its SHA.

Suggested fix: use distinct job-level groups for read-only jobs and one repository/main group for all mutating jobs, with `cancel-in-progress: false` and `queue: max`. Readers may cancel superseded checks on PR refs. Use bounded non-fast-forward retry/rebase and explicit GH006/GH013 handling; never force push.

The same revision also uses mutable action tags (`actions/checkout@v6`, `oven-sh/setup-bun@v2`, etc.). Pin each reviewed action to its full commit SHA and let Dependabot update it. A minimal check is to enumerate every `uses:` in all workflow files and require a 40-character SHA for remote actions.

Workaround: adapt those policies in consuming repositories. We have done so in PR #526. GitHub documents the queue setting at https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/control-workflow-concurrency . actionlint 1.7.12 does not yet understand this field (https://github.com/rhysd/actionlint/issues/657); independently validate its schema and exclude only that exact obsolete-schema diagnostic, preserving all other lint errors.
