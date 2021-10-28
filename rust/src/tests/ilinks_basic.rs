use num_traits::ToPrimitive;
use crate::tests::make_mem;
use crate::tests::make_links;
use crate::doublets::{ILinks, ILinksExtensions, Link};
use crate::doublets::data::{AddrToRaw, Hybrid, IGenericLinks, LinksConstants, RawToAddr};
use crate::num::ToSigned;

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
fn each_eq_count() {
    let mem = make_mem();
    let mut links = make_links(mem);

    let root = links.create_point();

    for _ in 0..10 {
        let new = links.create_point();
        links.create_and_update(new, root);
    }

    let any = links.constants.any;
    let query = [any, any, root];

    let mut count = 0;
    links.each_by(|link| {
        count += 1;
        links.constants.r#continue
    }, [any, any, root]);

    assert_eq!(count, links.count_by(query));
}

#[test]
fn rebase() {
    let mem = make_mem();
    let mut links = make_links(mem);
    let any = links.constants.any;

    let root = links.create_point();

    for _ in 0..10 {
        let new = links.create_point();
        links.create_and_update(new, root);
    }

    let before = links.count_by([any, any, root]);

    let new_root = links.create_point();
    let root = links.rebase(root, new_root);

    let after = links.count_by([any, any, root]);

    assert_eq!(before, after);
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

#[test]
fn hybrid() {
    let constants = LinksConstants::via_only_external(true);

    let to_raw = AddrToRaw::new();
    let to_adr = RawToAddr::new();

    let address = 'c' as usize;
    let raw = to_raw.convert(address);

    assert_eq!(to_adr.convert(raw), address);
    assert!(constants.is_external_reference(raw));
}