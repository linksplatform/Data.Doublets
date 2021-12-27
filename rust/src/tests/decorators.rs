use crate::doublets::decorators::{
    CascadeUniqueResolver, LogAllChanges, NonNullDeletionResolver, UniqueResolver, UniqueValidator,
};
use crate::doublets::Flow::{Break, Continue};
use crate::doublets::{Doublet, ILinks, Link, LinksError};
use crate::num::LinkType;
use crate::tests::make_links;
use crate::tests::make_mem;
use std::error::Error;
use std::marker::PhantomData;
use std::ops::Try;

// Maybe later add to library
#[derive(Debug, Clone)]
enum ChangesKind {
    Create,
    Update,
    Delete,
}

fn what_operation<T: LinkType>(before: &Link<T>, after: &Link<T>) -> ChangesKind {
    if before == &Link::nothing() {
        ChangesKind::Create
    } else if after == &Link::nothing() {
        ChangesKind::Delete
    } else {
        ChangesKind::Update
    }
}

#[test]
fn non_null_deletions() -> Result<(), Box<dyn Error>> {
    let mem = make_mem();
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
    let mem = make_mem();
    let links = make_links(mem).unwrap();
    let mut links = UniqueResolver::new(links);

    let point_a = links.create_point()?;
    let point_b = links.create_point()?;

    let one = links.create_link(point_a, point_b)?;
    let two = links.create_link(point_a, point_b)?;

    assert_ne!(links.get_link(one), links.get_link(two));
    Ok(())
}

#[test]
fn unique_validator() -> Result<(), Box<dyn Error>> {
    let mem = make_mem();
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
    let mem = make_mem();
    let links = make_links(mem).unwrap();
    let mut links = CascadeUniqueResolver::new(links);

    let point_a = links.create_point()?;
    let point_b = links.create_point()?;

    let linker = links.create_link(point_a, point_b)?;

    links.create_link(point_a, linker)?;
    links.create_link(point_b, linker)?;

    let linker = links.create_link(point_a, point_b)?;

    let any = links.constants().any;
    let query = [any, any, linker];
    assert_eq!(links.count_by(query), 2);

    Ok(())
}
