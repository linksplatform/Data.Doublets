use crate::tests::make_mem;
use crate::tests::make_links;
use crate::doublets::{ILinksExtensions, Link};
use crate::doublets::data::IGenericLinks;

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
    // TODO: expect
    assert_eq!(links.get_link(point).unwrap(), Link::from_once(point));
}

#[test]
fn delete_all_usages() {
    let mem = make_mem();
    let mut links = make_links(mem);

    let root = links.create_point();
    let a = links.create_point();
    let b = links.create_point();

    links.create_and_update(a, root);
    links.create_and_update(b, root);

    assert_eq!(links.count(), 5);

    links.delete_usages(root);

    assert_eq!(links.count(), 2);
}
