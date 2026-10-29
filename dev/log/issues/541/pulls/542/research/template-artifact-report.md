The template uses `actions/download-artifact@v8` with its default ZIP extraction. The current v8.0.2 bundle calls deprecated Buffer constructors through `unzip-stream`, producing DEP0005 on Node 24. This is the same issue reproduced in downstream Data.Doublets: https://github.com/linksplatform/Data.Doublets/actions/runs/40917637619 and investigated in https://github.com/linksplatform/Data.Doublets/pull/542.

Reproduce with an ordinary artifact round trip:

```yaml
- run: printf 'check\n' > check.txt
- uses: actions/upload-artifact@v7
  with:
    name: check
    path: check.txt
- uses: actions/download-artifact@v8
  with:
    name: check
    path: downloaded
```

The download emits `[DEP0005] DeprecationWarning: Buffer() is deprecated`. A bounded offline reproduction and its stack trace are provided in the downstream PR (`experiments/reproduce_artifact_buffer.mjs`); it needs only two bytes and no credentials.

Workaround: use `skip-decompress: true` with digest verification left enabled, then extract with a maintained ZIP component such as Python `zipfile`, validating paths and symlinks first. Verify bytes with an upload/download regression on pull requests. Once an upstream release incorporates the fix, prefer that release and remove the workaround.

Upstream root cause and suggested code changes are tracked at https://github.com/actions/download-artifact/issues/484 and https://github.com/actions/download-artifact/pull/490: replace obsolete Buffer constructors throughout the transitive extraction dependencies, rebuild `dist/`, and test extraction with `NODE_OPTIONS=--throw-deprecation`. Pin reviewed action revisions so the behavior is reproducible.
