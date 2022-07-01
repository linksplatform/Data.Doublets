use data::ToQuery;
use doublets::Link;

#[test]
fn link_to_query() {
    let link = Link::<usize>::new(1, 2, 3);
    assert_eq!([1, 2, 3], &link.to_query()[..]);
}
