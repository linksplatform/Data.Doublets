//! Executable examples for docs/deletion-rust.md at the audited Rust revision.
//! Copy to doublets/tests/issue_518_deletion_policies.rs and run that test target.
//! Creates at most five links per store; never calls an unchecked invalid address.

use doublets::{data::Flow, decorators::DecoratorsExt, mem::Global, split, unit, Doublets, Error};

fn delete_if_exists<L: Doublets<usize>>(
    links: &mut L,
    index: usize,
) -> Result<usize, Error<usize>> {
    if links.exist(index) {
        links.delete(index)
    } else {
        Ok(0)
    }
}

fn check<L: Doublets<usize>>(mut links: L) {
    let first = links.create_point().unwrap();
    let middle = links.create_point().unwrap();
    links.create_point().unwrap();
    assert_eq!(delete_if_exists(&mut links, middle).unwrap(), middle);
    assert_eq!(delete_if_exists(&mut links, middle).unwrap(), 0);
    assert_eq!(delete_if_exists(&mut links, usize::MAX).unwrap(), 0);
    assert_eq!(links.count(), 2);
    assert!(links.exist(first));
    assert_eq!(links.create_point().unwrap(), middle);
    assert_eq!(links.create_point().unwrap(), 4);

    // This is an observation of the CURRENT backend, not the target core contract.
    let mut calls = 0;
    assert!(matches!(
        links.delete_links(&[10], &mut |_, _| {
            calls += 1;
            Flow::Continue
        }),
        Err(Error::NotExists(10))
    ));
    assert_eq!(calls, 0);
}

#[test]
fn united_choices() {
    check(unit::Store::<usize, _>::new(Global::new()).unwrap());
    check(
        unit::Store::<usize, _>::new(Global::new())
            .unwrap()
            .with_automatic_uniqueness_and_usages_resolution(),
    );
    check(
        unit::Store::<usize, _>::new(Global::new())
            .unwrap()
            .with_automatic_uniqueness_and_usages_resolution()
            .with_inner_reference_existence_validation(),
    );
}

#[test]
fn split_choices() {
    check(split::Store::<usize, _, _>::new(Global::new(), Global::new()).unwrap());
    check(
        split::Store::<usize, _, _>::new(Global::new(), Global::new())
            .unwrap()
            .with_automatic_uniqueness_and_usages_resolution(),
    );
    check(
        split::Store::<usize, _, _>::new(Global::new(), Global::new())
            .unwrap()
            .with_automatic_uniqueness_and_usages_resolution()
            .with_inner_reference_existence_validation(),
    );
}
