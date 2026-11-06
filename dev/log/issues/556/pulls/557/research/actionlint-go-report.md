The current `kjanat/actionlint:1.17.0` image at digest `sha256:d484cccadf56b6a356466ade9af4c8e05c5dce1badaf0a9ecd0d7639d68d9c92` includes an actionlint binary built with Go 1.27.0. OSV Scanner 2.7.0 reports 14 Go standard-library advisories in that binary on 2026-11-06.

Minimal reproduction:

```sh
docker pull kjanat/actionlint:1.17.0@sha256:d484cccadf56b6a356466ade9af4c8e05c5dce1badaf0a9ecd0d7639d68d9c92
osv-scanner scan image --format json kjanat/actionlint:1.17.0@sha256:d484cccadf56b6a356466ade9af4c8e05c5dce1badaf0a9ecd0d7639d68d9c92
```

Reported IDs: GO-2026-6599, GO-2026-6600, GO-2026-6603, GO-2026-6604, GO-2026-6605, GO-2026-6607, GO-2026-6608, GO-2026-6609, GO-2026-6610, GO-2026-6611, GO-2026-6612, GO-2026-6613, GO-2026-6617 and GO-2026-6809. This is a package-level audit; it does not establish that every affected function is reachable through actionlint.

Our workaround in https://github.com/linksplatform/Data.Doublets/pull/557 rebuilds the unchanged v1.17.0 source archive (SHA-256 `78def64642f4c85bf32a1a9ffc73b67fd5378a022bb8e3bd918d8fe42282729d`) using Go 1.27.3, while retaining the bundled ShellCheck. OSV's resulting image scan exits successfully with no reported vulnerabilities. We also refresh the Python/Pyflakes layer to Python 3.15.2 / Pyflakes 4.1.0, and preserve embedded-shell/Python validation tests.

Suggested upstream change: rebuild the release binaries and container with current patched Go 1.27.3, refresh the embedded Python tools, and run a package-level image audit during releases so consumers do not need to maintain this rebuild. Thank you for maintaining support for current GitHub workflow syntax.
