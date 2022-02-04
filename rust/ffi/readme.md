

## Build dynamic or static library

### Basic build library

Install [rustup](https://rustup.rs/) and setup Rust language tools.

on linux:
```shell
rustup toolchain install nightly
```
on windows:
```shell
rustup toolchain install nightly-[gnu|msvc]
```

Run cargo build in this folder:
```shell
cargo +nightly build --release
```

Great! Your libray is located in the `target/release` folder.

### Advanced build library
You can configure your build in the __`Cargo.toml`__ file:
Try write the following code:
```toml
[profile.release]
debug = true
overflow-checks = true
```
And rerun build 


What is it?\
`debug` - controls the amount of debug information included in the compiled binary.\
`overflow-checks` - controls the behavior of runtime [integer overflow](https://doc.rust-lang.org/reference/expressions/operator-expr.html#overflow).

[More options](https://doc.rust-lang.org/cargo/reference/profiles.html)

Also you can configure log level.\
Try replace
```toml
[package.log]
features = ["release_max_level_error"]
```
To
```toml
[package.log]
features = ["release_max_level_info"]
```