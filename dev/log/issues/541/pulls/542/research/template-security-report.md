The scheduled security workflow can be green without either a full dependency-tree audit or a fatal verdict on CodeQL findings. This leaves old pinned vulnerable dependencies and source findings outside the workflow's terminal result.

Reproduction from the current full template tree:

1. Trigger `security.yml` on its weekly schedule. The dependency-review job is PR-only, so it skips; it would only inspect changed dependencies even on a PR. There is no full-tree ecosystem audit in this workflow.
2. Introduce a source issue detected by the selected CodeQL suite. `github/codeql-action/analyze` can successfully produce/upload a SARIF report containing findings while its step succeeds. The separate code-scanning check may report findings, but the workflow/aggregate job observes successful analysis execution.

Concrete downstream evidence: https://github.com/linksplatform/Data.Doublets/actions/runs/40931698011/job/123652306137 has a successful Analyze step followed by a failing explicit SARIF gate on five findings. The original full SARIF and logs are retained in https://github.com/linksplatform/Data.Doublets/pull/542 under `dev/log/issues/541/pulls/542/`. That PR also found two previously unaudited pinned fpdf2 advisories using a full Python dependency audit, illustrating the distinction between source scanning, dependency review and full-tree audits.

A small gate regression can use this analysis-success SARIF fixture and must fail on its finding:

```json
{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"CodeQL"}},"invocations":[{"executionSuccessful":true}],"results":[{"ruleId":"example/finding","level":"warning","message":{"text":"reproducing finding"}}]}]}
```

Workaround/suggested implementation: retain SARIF with `analyze`'s output input and add a mandatory validator that rejects every desired finding level, failed tool diagnostic, missing/empty invocation list, unknown execution state, or missing report. For NuGet, run locked restore with `NuGetAuditMode=all` and warnings as errors, then audit all transitive packages and validate its JSON response. For native dependencies, use an ecosystem-supported provider, retain the resolved dependency coordinates, and explicitly report missing provider authorization or unknown packages rather than implying a clean audit. Run the audit on PRs, main and schedules, independently of dependency-review eligibility. Add bounded regressions with a finding and an incomplete scan, and verify that the final workflow gate fails.

Related reports already exist for the other templates: https://github.com/link-foundation/python-ai-driven-development-pipeline-template/issues/122 and https://github.com/link-foundation/rust-ai-driven-development-pipeline-template/issues/221. This report concerns this template's own security workflow; it does not request copying unrelated language distributions.
