use std::error::Error;

use crate::doublets::decorators::{
    CascadeUniqueResolver, CascadeUsagesResolver, NonNullDeletionResolver, UniqueResolver,
    UniqueValidator, UsagesValidator,
};
use crate::doublets::Flow::Continue;
use crate::doublets::{Doublet, Link, Links, LinksError};
use crate::tests::{make_links, make_mem};

#[test]
fn non_null_deletions() -> Result<(), Box<dyn Error>> {
    let mem = make_mem().unwrap();
    let links = make_links(mem).unwrap();
    let mut links = NonNullDeletionResolver::new(links);

    let point = links.create_point()?;

    let mut changes = Vec::new();
    links.delete_with(point, |before, after| {
        changes.push((before, after));
        Continue
    });

    assert_eq!(
        changes,
        vec![
            (Link::new(1, 1, 1), Link::new(1, 0, 0)),
            (Link::new(1, 0, 0), Link::new(0, 0, 0)),
        ]
    );

    // revert deleteion

    Ok(())
}

#[test]
fn unique_resolver() -> Result<(), Box<dyn Error>> {
    let mem = make_mem().unwrap();
    let links = make_links(mem).unwrap();
    let mut links = UniqueResolver::new(links);

    let point_a = links.create_point()?;
    let point_b = links.create_point()?;

    let one = links.create_link(point_a, point_b)?;
    let two = links.create_link(point_a, point_b)?;
    assert_eq!(one, two);
    Ok(())
}

#[test]
fn unique_validator() -> Result<(), Box<dyn Error>> {
    let mem = make_mem().unwrap();
    let links = make_links(mem).unwrap();
    let mut links = UniqueValidator::new(links);

    let point_a = links.create_point()?;
    let point_b = links.create_point()?;

    links.create_link(point_a, point_b)?;
    match links.create_link(point_a, point_b).unwrap_err() {
        LinksError::AlreadyExists(doublet) => {
            assert_eq!(doublet, Doublet::new(point_a, point_b));
        }
        _ => panic!("test not passed"),
    }
    Ok(())
}

#[test]
fn cascade_resolver() -> Result<(), Box<dyn Error>> {
    let mem = make_mem().unwrap();
    let links = make_links(mem).unwrap();
    let mut links = CascadeUniqueResolver::new(links);

    let point_a = links.create_point()?;
    let point_b = links.create_point()?;

    let linker = links.create_link(point_a, point_b)?;

    let one = links.create_link(point_a, linker)?;
    let two = links.create_link(point_b, linker)?;

    assert_eq!(linker, links.create_link(point_a, point_b)?);

    let any = links.constants().any;
    let query = [any, any, linker];
    assert_eq!(links.count_by(query), 2);

    Ok(())
}

#[test]
// TODO: rename to `BorrowingValidator` or other name
fn usages_validator() -> Result<(), Box<dyn Error>> {
    let mem = make_mem().unwrap();
    let links = make_links(mem).unwrap();
    let mut links = UsagesValidator::new(links);

    let point = links.create_point()?;
    let anchor = links.create_point()?;

    links.create_link(anchor, point);

    match links.update(point, 0, 0).unwrap_err() {
        LinksError::HasDeps(reference) => {
            assert_eq!(reference, links.try_get_link(point)?);
        }
        _ => panic!("test not passed"),
    }

    match links.delete(point).unwrap_err() {
        LinksError::HasDeps(reference) => {
            assert_eq!(reference, links.try_get_link(point)?);
        }
        _ => panic!("test not passed"),
    }

    Ok(())
}

#[test]
// TODO: rename to less stupid name
fn cascade_usages_resolver() -> Result<(), Box<dyn Error>> {
    let mem = make_mem().unwrap();
    let links = make_links(mem).unwrap();
    let mut links = CascadeUsagesResolver::new(links);

    let konard = links.create_point()?;

    for _ in 0..1000 {
        let fan = links.create_point()?;
        links.create_link(fan, konard)?;
    }

    assert_eq!(links.count(), 1 + 1000 + 1000);

    links.delete(konard)?;

    assert_eq!(links.count(), 1000);

    Ok(())
}
