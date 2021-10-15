use crate::tests::make_mem;
use crate::tests::make_links;
use crate::doublets::{ILinksExtensions, Link};

#[test]
fn create() {
    let mem = make_mem();
    let mut links = make_links(mem);

    links.create();

    assert_eq!(1, links.count());
}

#[test]
fn create_point() {
    let mem = make_mem();
    let mut links = make_links(mem);

    let point = links.create_point();

    assert_eq!(1, links.count());
    assert_eq!(links.get_link(point), Link::from_once(point));
}
