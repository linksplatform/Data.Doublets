Additional reproduction from Data.Doublets: https://github.com/linksplatform/Data.Doublets/actions/runs/40917637619 (all three artifact downloads in `publish`). The pinned v8.0.2 bundle is `9000827ccba6bdab643e8b6fd33ac0654aef8333`.

A bounded, credential-free reproduction exercises the exact vendored matcher with a two-byte ZIP signature:

```sh
git clone https://github.com/actions/download-artifact /tmp/download-artifact-repro
git -C /tmp/download-artifact-repro checkout 9000827ccba6bdab643e8b6fd33ac0654aef8333
node --trace-deprecation experiments/reproduce_artifact_buffer.mjs /tmp/download-artifact-repro
```

The experiment source is in https://github.com/linksplatform/Data.Doublets/pull/542 (`experiments/reproduce_artifact_buffer.mjs`). It reads `dist/index.js`, isolates `MatcherStream`, and sends `Buffer.from('PK')` through `new MatcherStream('PK')`. `new Buffer` in that function produces DEP0005. `--throw-deprecation` turns it into a failing regression without any network/token requirements.

Workaround: set `skip-decompress: true`, keep the default SHA-256 integrity check, and extract the downloaded ZIP using Python's `zipfile` after validating member paths. Our PR tests a real upload/download round trip and validates extraction bytes. Do not suppress Node warnings or disable digest checking.

Suggested code fix: replace string/data constructor calls with `Buffer.from(...)`, length allocations with `Buffer.alloc(...)`, or replace the obsolete extraction dependency chain; rebuild the shipped `dist/` bundle and exercise actual extraction under `--throw-deprecation`. The existing PR https://github.com/actions/download-artifact/pull/490 is relevant and remains open at the time of this investigation.
