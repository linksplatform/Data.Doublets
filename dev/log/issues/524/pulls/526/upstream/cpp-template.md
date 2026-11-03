Found while comparing the entire template tree for linksplatform/Data.Doublets#524 / https://github.com/linksplatform/Data.Doublets/pull/526.

At 7ac75134c697e9468f1242c20f1d229594decbc3, `.github/workflows/release.yml` correctly uses job-level concurrency but its comment explicitly assumes that at most one pending writer is retained. The writer groups omit GitHub's new `queue: max`. Consequently a third main run replaces a second pending writer, dropping a queued release. This is observable without publishing: have the first writer wait, then push two more independent changesets and inspect the cancellation of the second run.

Suggested fix: all repository/main writers should share one group, `cancel-in-progress: false`, and `queue: max`; read-only jobs should retain distinct groups. Keep bounded non-fast-forward handling and never force push. The public documentation is https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/control-workflow-concurrency .

Additionally workflows use mutable action tags such as `actions/checkout@v7`. Enumerate every remote `uses:` and pin it to a reviewed full commit SHA; configure Dependabot for updates. Reproduction: a check requiring `uses: owner/name@[0-9a-f]{40}` fails on the current workflow tree.

Workaround: override concurrency and action pins in consuming repositories, as in PR #526. actionlint 1.7.12 needs an independently validated workaround for the new queue field (existing report https://github.com/rhysd/actionlint/issues/657); do not disable unrelated lint checks.
