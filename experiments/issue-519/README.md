<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# Issue 519 Rust parity audit", "*"]
  }
}
-->

# Issue 519 Rust parity audit

The C# regression suite is `UniquenessResultTests`. This standalone Rust program
pins doublets-rs main at `645dd2b8e5c9bff1e38335da42e3c008b5d2769e`
(doublets 0.15.0) and checks the corresponding operations through both memory
backends, plain and automatic uniqueness/usage resolution stacks.

```sh
CARGO_BUILD_JOBS=1 CARGO_PROFILE_DEV_DEBUG=0 CARGO_INCREMENTAL=0 \
  cargo run --locked --manifest-path experiments/issue-519/Cargo.toml -j 1
```

It checks duplicate creation, live returned addresses, the absent-pair creation
path, get-or-create lookup, direct uniqueness merge, unchanged update and Break
from creation/update callbacks. Rust has no direct `UpdateOrCreateOrGet` API;
the test explicitly exercises the corresponding creation path. All four stacks
pass. Inputs are fixed and small (fewer than twenty links); there is no memory or
stack stress. Use one build worker and omit compiler debug data for this container.

The optional probe intentionally fails its assertion on the pinned Rust revision:

```sh
CARGO_BUILD_JOBS=1 CARGO_PROFILE_DEV_DEBUG=0 CARGO_INCREMENTAL=0 \
  cargo run --locked --manifest-path experiments/issue-519/Cargo.toml -j 1 -- --logging-break
```

`LoggingDecorator` discards callback flow, so `create_link_with` returns Continue
although its second callback returned Break. The default audit passes the same
control checks without this layer. This analogous defect was reported as
[doublets-rs issue 86](https://github.com/linksplatform/doublets-rs/issues/86).
That report satisfies the Rust reporting requirement of the C# issue; changing
Rust is outside this PR. The existing Rust uniqueness resolver already reports
the survivor after deletion and its usage merge already preserves both endpoints.

Logs and the requirement inventory are under `dev/log/issues/519/pulls/544/`.
