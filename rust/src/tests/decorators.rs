use decorators::NonNullDeletionResolver;

use crate::doublets::{ILinks, ILinksExtensions, Link};
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions};
use crate::doublets::decorators;
use crate::doublets::mem::united::Links;
use crate::mem::FileMappedMem;
use crate::tests::make_links;
use crate::tests::make_mem;

#[test]
fn non_null_deletions() {
    let mem = make_mem();
    let mut links = make_links(mem);
    let mut links = NonNullDeletionResolver::new(links);

    let point = links.create_point();
    assert_eq!(links.count(), 1);
    links.delete_all();

    let index = links.create();

    let link = links.get_link(index);
    let link = link.unwrap();
    assert_eq!(link, Link::new(point, 0, 0));
}
