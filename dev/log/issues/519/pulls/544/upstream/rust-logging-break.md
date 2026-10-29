While auditing parity for https://github.com/linksplatform/Data.Doublets/issues/519,
I reproduced a related callback-control issue on current doublets-rs main
`645dd2b8e5c9bff1e38335da42e3c008b5d2769e` (doublets 0.15.0, stable rustc 1.99.0).

Ordinary unit/split stores and automatic uniqueness stacks return live surviving
addresses and preserve Break for the original scenarios. However, adding the
public LoggingDecorator makes create_link_with lose a Break returned during update:

```rust
use doublets::{
    data::Flow, decorators::LoggingDecorator, mem::Global, unit, Doublets,
};

fn main() -> Result<(), doublets::Error<usize>> {
    let mut links = LoggingDecorator::new(
        unit::Store::<usize, _>::new(Global::new())?, Vec::<u8>::new(),
    );
    let mut calls = 0;
    let result = links.create_link_with(0, 0, |_, _| {
        calls += 1;
        if calls == 2 { Flow::Break } else { Flow::Continue }
    })?;
    assert_eq!(calls, 2);
    assert_eq!(result, Flow::Break); // fails: actual Continue
    Ok(())
}
```

Expected: callback control is Break; the created link remains valid. Actual:
`Ok(Continue)`, despite the update callback returning Break. The same operation
without the logging layer returns Break.

`create_link_with` in `doublets/src/data/traits.rs` wraps callbacks in Fuse but
returns only the primitive update_links result. The logged! wrapper in
`doublets/src/decorators/logging.rs` invokes Fuse::call and discards its flow,
returning Continue to the underlying primitive. Therefore the outer helper loses
its recorded Break. The C# fix records callback flow independently of the
primitive return value. Keeping logging of completed mutations need not discard
callback control returned to the caller.

A bounded, pinned standalone reproduction and four-stack positive audit are
retained in https://github.com/linksplatform/Data.Doublets/pull/544 under
`experiments/issue-519`. Run `cargo run --manifest-path
experiments/issue-519/Cargo.toml -j 1 -- --logging-break` to reproduce the assertion
failure; the default invocation verifies the original address/control scenarios.
