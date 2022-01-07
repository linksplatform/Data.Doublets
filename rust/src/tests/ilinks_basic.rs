use num_traits::ToPrimitive;
use std::ops::ControlFlow;

use crate::doublets::data::{AddrToRaw, Hybrid, IGenericLinks, LinksConstants, Query, RawToAddr};
use crate::doublets::{ILinks, ILinksExtensions, Link, LinksError};
use crate::mem::GlobalMem;
use crate::num::ToSigned;
use crate::tests::make_links;
use crate::tests::make_mem;
use crate::{query, Links};

#[tokio::test]
async fn create() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    links.create().await.unwrap();

    assert_eq!(1, links.count().await);
}

#[tokio::test]
async fn create_point() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    let point = links.create_point().await.unwrap();

    assert_eq!(1, links.count().await);
    // TODO: expect
    assert_eq!(links.get_link(point).await.unwrap(), Link::point(point));
}

#[tokio::test]
async fn each_eq_count() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    let root = links.create_point().await.unwrap();

    for _ in 0..10 {
        let new = links.create_point().await.unwrap();
        links.create_and_update(new, root).await.unwrap();
    }

    let any = links.constants.any;
    let query = [any, any, root];

    let mut count = 0;
    links
        .each_by([any, any, root], |link| {
            count += 1;
            links.constants.r#continue
        })
        .await;

    assert_eq!(count, links.count_by(Query::new(&query[..])).await);
}

#[tokio::test]
async fn rebase() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();
    let any = links.constants.any;

    let root = links.create_point().await.unwrap();

    for _ in 0..10 {
        let new = links.create_point().await.unwrap();
        links.create_and_update(new, root).await.unwrap();
    }

    let before = links.count_by(Query::new(&[any, any, root][..])).await;

    let new_root = links.create_point().await.unwrap();
    let root = links.rebase(root, new_root).await.unwrap();

    let after = links.count_by(Query::new(&[any, any, root][..])).await;

    assert_eq!(before, after);
}

#[tokio::test]
async fn delete_all_usages() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    let root = links.create_point().await.unwrap();
    let a = links.create_point().await.unwrap();
    let b = links.create_point().await.unwrap();

    links.create_and_update(a, root).await.unwrap();
    links.create_and_update(b, root).await.unwrap();

    assert_eq!(links.count().await, 5);

    links.delete_usages(root).await.unwrap();

    assert_eq!(links.count().await, 3);
}

#[tokio::test]
async fn hybrid() {
    let constants = LinksConstants::via_only_external(true);

    let to_raw = AddrToRaw::new();
    let to_adr = RawToAddr::new();

    let address = 'c' as usize;
    let raw = to_raw.convert(address);

    assert_eq!(to_adr.convert(raw), address);
    assert!(constants.is_external_reference(raw));
}
