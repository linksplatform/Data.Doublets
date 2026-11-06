Confirmed for reusable workflow calls with released actionlint 1.7.12 and zizmor 1.32.0:

```yaml
jobs:
  security:
    uses: $/.github/workflows/security.yml
```

GitHub accepts this syntax, and zizmor recommends it. actionlint reports `reusable workflow call "$/.github/workflows/security.yml" ... is not following the format ...`. Our temporary wrapper normalizes only valid same-repository workflow filenames to the equivalent `./` form on stdin, preserving the original filename and working directory. actionlint still loads the actual callee and validates inputs; no workflow-call diagnostics are suppressed. Malformed paths are not rewritten. The existing queue validation and supported runner-label configuration remain separate.

PR #732's normalization approach would let us delete this compatibility transformation after a release. Reproduction, passing real actionlint validation, and malformed-path regression are in https://github.com/linksplatform/Data.Doublets/pull/557 .
