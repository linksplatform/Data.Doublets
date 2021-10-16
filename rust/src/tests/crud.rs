use crate::tests::make_mem;
use crate::tests::make_links;
use crate::doublets::{ILinksExtensions, Link};
use crate::doublets::data::IGenericLinks;
use crate::test_extensions::ILinksTestExtensions;
use crate::mem::FileMappedMem;

#[test]
fn random_creations_and_deletions() {
    std::fs::remove_file("db.links");

    let mem = make_mem();
    let mut links = make_links(mem);
    let mut links = links.decorators_kit();

    links.test_random_creations_and_deletions(1000);
}
