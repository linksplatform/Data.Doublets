use crate::doublets::data::{AddrToRaw, IGenericLinks, LinksConstants, Query, RawToAddr};
use crate::doublets::{ILinks, ILinksExtensions};
use crate::query;
use crate::test_extensions::ILinksTestExtensions;
use crate::tests::{make_links, make_mem, typed_links};

#[tokio::test]
async fn non_exist_reference() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    let link = links.create().await.unwrap();
    links.update(link, usize::MAX, usize::MAX).await.unwrap();

    let mut result = 0;
    links
        .each_by([links.constants().any, usize::MAX, usize::MAX], |found| {
            result = found.index;
            links.constants().r#break
        })
        .await;

    assert_eq!(result, link);
    assert_eq!(links.count_by([usize::MAX]).await, 0);
    links.delete(link).await.unwrap();
}

#[tokio::test]
async fn raw_numbers() {
    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();

    links.test_raw_numbers_crud().await;
}

#[tokio::test]
async fn u128_raw_numbers() {
    let mem = make_mem().unwrap();
    //let mut links = typed_links::<u128, _>(mem);
    let mut links = typed_links(mem).unwrap();

    links.get_link(0_u128).await;

    let constants = LinksConstants::via_only_external(true);
    let to_raw = AddrToRaw;
    let to_adr = RawToAddr;

    let raw = to_raw.convert(1_u128);
    assert!(constants.is_external_reference(raw));

    let adr = to_adr.convert(raw);
    assert!(constants.is_internal_reference(adr));

    let source = to_raw.convert(228_1337_1754_177013_666_777_u128);
    let target = to_raw.convert(10_1011_0011_0111_0101_u128);
    let address = links.create_and_update(source, target).await.unwrap();

    let link = links.get_link(address).await.unwrap();
    assert_eq!(
        to_adr.convert(link.source),
        228_1337_1754_177013_666_777_u128
    );
    assert_eq!(to_adr.convert(link.target), 10_1011_0011_0111_0101_u128);
}
