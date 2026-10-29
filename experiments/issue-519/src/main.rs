//! Bounded issue 519 parity audit against the pinned current Rust implementation.
use doublets::{
    data::Flow,
    decorators::{DecoratorsExt, LoggingDecorator},
    mem::Global,
    split, unit, Doublets, Error, Link,
};

fn audit(mut links: impl Doublets<usize>, automatic: bool) -> Result<(), Error<usize>> {
    let first = links.create_point()?;
    let second = links.create_point()?;
    let created = links.create_link(first, first)?;
    assert_eq!(created, if automatic { first } else { 3 });
    assert!(links.exist(created));
    // Rust has no UpdateOrCreateOrGet; exercise its missing-pair creation path.
    assert_eq!(links.search(first, second), None);
    let updated = links.create_link(second, second)?;
    assert_eq!(updated, if automatic { second } else { 4 });
    assert!(links.exist(updated));
    let found = links.search(second, second).expect("existing pair");
    assert_eq!(links.get_or_create(second, second)?, found);
    for collide in [false, true] {
        for stop_at in [1, 2] {
            let mut calls = 0;
            let result =
                links.create_link_with(first, if collide { first } else { second }, |_, _| {
                    calls += 1;
                    if calls == stop_at {
                        Flow::Break
                    } else {
                        Flow::Continue
                    }
                })?;
            assert_eq!(result, Flow::Break);
            assert_eq!(calls, stop_at);
        }
    }
    if automatic {
        assert_eq!(links.update(second, first, first)?, first);
        assert!(!links.exist(second));
        assert_eq!(links.try_get_link(first)?, Link::point(first));
        assert_eq!(links.update(first, first, first)?, first);
    }
    Ok(())
}

fn main() -> Result<(), Error<usize>> {
    if std::env::args().any(|arg| arg == "--logging-break") {
        let mut links = LoggingDecorator::new(
            unit::Store::<usize, _>::new(Global::new())?,
            Vec::<u8>::new(),
        );
        let mut calls = 0;
        let result = links.create_link_with(0, 0, |_, _| {
            calls += 1;
            if calls == 2 {
                Flow::Break
            } else {
                Flow::Continue
            }
        })?;
        println!("logging create_link_with: {result:?}; callback calls: {calls}; expected Break");
        assert_eq!(result, Flow::Break);
        return Ok(());
    }
    audit(unit::Store::<usize, _>::new(Global::new())?, false)?;
    println!("unit/raw: live addresses and Break propagation passed");
    audit(
        split::Store::<usize, _, _>::new(Global::new(), Global::new())?,
        false,
    )?;
    println!("split/raw: live addresses and Break propagation passed");
    audit(
        unit::Store::<usize, _>::new(Global::new())?
            .with_automatic_uniqueness_and_usages_resolution(),
        true,
    )?;
    println!("unit/automatic: live addresses, merge, no-op and Break propagation passed");
    audit(
        split::Store::<usize, _, _>::new(Global::new(), Global::new())?
            .with_automatic_uniqueness_and_usages_resolution(),
        true,
    )?;
    println!("split/automatic: live addresses, merge, no-op and Break propagation passed");
    Ok(())
}
