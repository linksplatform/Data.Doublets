extern crate test;

use test::Bencher;
use crate::doublets::mem::united;
use crate::tests::make_mem;
use crate::test_extensions::ILinksTestExtensions;
use crate::doublets::ILinksExtensions;

#[bench]
fn united(b: &mut Bencher) {
    let mem = make_mem();
    let mut links = united::Links::<usize, _>::new(mem);
    let mut links = links.decorators_kit();

    b.iter(|| {
        links.test_random_creations_and_deletions(100)
    })
}