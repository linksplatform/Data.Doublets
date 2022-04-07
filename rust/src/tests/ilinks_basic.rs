use num_traits::ToPrimitive;
use std::ops::ControlFlow;

use crate::data::{AddrToRaw, Hybrid, Links, LinksConstants, Query, RawToAddr};
use crate::doublets::{Doublets, ILinksExtensions, Link, LinksError};
use crate::mem::GlobalMem;
use crate::num::ToSigned;
use crate::tests::make_links;
use crate::tests::make_mem;
use crate::{query, Store};

#[test]
fn create() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    links.create().unwrap();

    assert_eq!(1, links.count());
}

#[test]
fn create_point() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    let point = links.create_point().unwrap();

    assert_eq!(1, links.count());
    // TODO: expect
    assert_eq!(links.get_link(point).unwrap(), Link::point(point));
}

#[test]
fn each_eq_count() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    let root = links.create_point().unwrap();

    for _ in 0..10 {
        let new = links.create_point().unwrap();
        links.create_and_update(new, root).unwrap();
    }

    let any = links.constants.any;
    let query = [any, any, root];

    let mut count = 0;
    links.each_by([any, any, root], |link| {
        count += 1;
        links.constants.r#continue
    });

    assert_eq!(count, links.count_by(Query::new(&query[..])));
}

#[test]
fn rebase() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();
    let any = links.constants.any;

    let root = links.create_point().unwrap();

    for _ in 0..10 {
        let new = links.create_point().unwrap();
        links.create_and_update(new, root).unwrap();
    }

    let before = links.count_by(Query::new(&[any, any, root][..]));

    let new_root = links.create_point().unwrap();
    let root = links.rebase(root, new_root).unwrap();

    let after = links.count_by(Query::new(&[any, any, root][..]));

    assert_eq!(before, after);
}

#[test]
fn delete_all_usages() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    let root = links.create_point().unwrap();
    let a = links.create_point().unwrap();
    let b = links.create_point().unwrap();

    links.create_and_update(a, root).unwrap();
    links.create_and_update(b, root).unwrap();

    assert_eq!(links.count(), 5);

    links.delete_usages(root).unwrap();

    assert_eq!(links.count(), 3);
}

#[test]
fn hybrid() {
    let constants = LinksConstants::via_only_external(true);

    let to_raw = AddrToRaw::new();
    let to_adr = RawToAddr::new();

    let address = 'c' as usize;
    let raw = to_raw.convert(address);

    assert_eq!(to_adr.convert(raw), address);
    assert!(constants.is_external(raw));
}
