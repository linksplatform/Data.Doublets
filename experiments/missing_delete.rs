//! Probe for Data.Doublets issue #518 against doublets-rs main.
//!
//! Copy this file to doublets/tests/issue_518_missing_delete.rs in a doublets-rs
//! checkout, then run:
//! CARGO_BUILD_JOBS=1 CARGO_PROFILE_DEV_DEBUG=0 cargo test -p doublets \
//!   --test issue_518_missing_delete -- --test-threads=1
//! The probe creates at most five links per store and runs no unbounded loops.

use doublets::{
    data::Flow, decorators::DecoratorsExt, mem::Global, split, unit, Doublets, DoubletsExt, Error,
};

fn check<L: Doublets<usize>>(factory: impl Fn() -> L) {
    for scenario in [
        "never-created",
        "deleted-middle",
        "deleted-last",
        "empty",
        "null",
        "out-of-range",
    ] {
        for direct in [false, true] {
            let mut store = factory();
            let first = store.create_point().unwrap();
            let middle = store.create_point().unwrap();
            let last = store.create_point().unwrap();
            let (missing, next) = match scenario {
                "deleted-middle" => {
                    store.delete(middle).unwrap();
                    (middle, middle)
                }
                "deleted-last" => {
                    store.delete(last).unwrap();
                    (last, last)
                }
                "empty" => {
                    store.delete(last).unwrap();
                    store.delete(middle).unwrap();
                    store.delete(first).unwrap();
                    (first, first)
                }
                "null" => (0, last + 1),
                "out-of-range" => (usize::MAX, last + 1),
                _ => (10, last + 1),
            };
            let before: Vec<_> = store.iter().collect();
            let count = store.count();
            let mut calls = 0;
            for _ in 0..2 {
                let mut handler = |_, _| {
                    calls += 1;
                    Flow::Continue
                };
                let result = if direct {
                    store.delete_links(&[missing], &mut handler)
                } else {
                    store.delete_with(missing, &mut handler)
                };
                assert!(
                    matches!(result, Err(Error::NotExists(id)) if id == missing),
                    "{scenario}, direct={direct}: {result:?}"
                );
                assert_eq!(calls, 0);
                assert_eq!(store.count(), count);
                assert_eq!(store.iter().collect::<Vec<_>>(), before);
            }
            assert_eq!(store.create_point().unwrap(), next);
            let following = if scenario == "deleted-middle" {
                last + 1
            } else {
                next + 1
            };
            assert_eq!(store.create_point().unwrap(), following);
            assert_eq!(store.count(), count + 2);
            assert_eq!(store.iter().count(), count + 2);
        }
    }
}

#[test]
fn united_plain() {
    check(|| unit::Store::<usize, _>::new(Global::new()).unwrap());
}

#[test]
fn united_decorated() {
    check(|| {
        unit::Store::<usize, _>::new(Global::new())
            .unwrap()
            .with_automatic_uniqueness_and_usages_resolution()
    });
}

#[test]
fn split_plain() {
    check(|| split::Store::<usize, _, _>::new(Global::new(), Global::new()).unwrap());
}

#[test]
fn split_decorated() {
    check(|| {
        split::Store::<usize, _, _>::new(Global::new(), Global::new())
            .unwrap()
            .with_automatic_uniqueness_and_usages_resolution()
    });
}
