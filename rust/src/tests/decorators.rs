use crate::tests::make_mem;
use crate::tests::make_links;
use crate::doublets::mem::united::Links;
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions};
use crate::doublets::{ILinks, ILinksExtensions, Link};
use crate::doublets::decorators;


use decorators::NonNullDeletionResolver;
use crate::mem::FileMappedMem;

#[test]
fn non_null_deletions() {
    let mem = make_mem();
    let mut links = make_links(mem);
    let mut links = NonNullDeletionResolver::new(links);

    for _ in 0..1000 {
        links.create_point();
    }

    assert_eq!(links.count(), 1000);
    links.delete_all();

    for index in 1..=1000 {
        let link = links.get_link(index);
        assert_eq!(link, Link::from_once(0));
        assert_eq!(link.is_null());
    }
}
