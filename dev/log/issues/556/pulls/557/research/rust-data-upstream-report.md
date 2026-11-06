Current doublets-rs HEAD 18d6a81da69ee661c6fa7563abca4c8b20c3404d (doublets 0.20.0) requires platform-data 2.0.0, while the crates.io sparse index now publishes platform-data 3.0.0.

Minimal dependency-resolution reproduction:

```toml
[package]
name = "current-doublets-resolution"
version = "0.0.0"
edition = "2024"

[dependencies]
doublets = { git = "https://github.com/linksplatform/doublets-rs", rev = "18d6a81da69ee661c6fa7563abca4c8b20c3404d" }
```

Run `cargo generate-lockfile`, then `cargo tree -i platform-data`. It selects platform-data 2.0.0 through doublets' `version = "2.0.0"` requirement. A downstream lockfile update cannot migrate the upstream trait/API implementation across this major version. Adding an unused direct 3.0 dependency would only create two copies.

Please migrate doublets' platform-data requirement to 3.0 and adapt its implementation to the current API, with the upstream store and decorator tests. Our two parity probes and submodule already use the same current doublets revision and current Rust 1.99.1; they retain the upstream v2 requirement until that migration is available. Registry evidence: https://index.crates.io/pl/at/platform-data . Related earlier type-unification work: PR #48.
