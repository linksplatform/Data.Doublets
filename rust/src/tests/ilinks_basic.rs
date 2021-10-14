use crate::mem::{HeapMem, ResizeableMem};
use crate::doublets::{mem, ILinks, ILinksExtensions};
use crate::doublets::mem::united;
use crate::doublets::Link;
use crate::doublets::mem::united::Links;

// TODO: cfg!
fn make_mem() -> HeapMem {
    HeapMem::new()
}

// TODO: cfg!
fn make_links<M: ResizeableMem>(mem: M) -> united::Links<usize, M> {
    united::Links::<usize, _>::new(mem)
}

#[test]
fn create() {
    let mut links = make_links(mem);

    links.create();

    assert_eq!(1, links.count());
}

#[test]
fn create_point() {
    let mut links = make_links(mem);

    let point = links.create_point();

    assert_eq!(1, links.count());
    assert_eq!(links.get_link(point), Link::from_once(point));
}