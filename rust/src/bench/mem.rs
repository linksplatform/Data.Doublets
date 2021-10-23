extern crate test;

use crate::doublets::mem::united;
use crate::doublets::ILinksExtensions;
use crate::mem::{FileMappedMem, HeapMem, ResizeableMem};
use crate::test_extensions::ILinksTestExtensions;
use crate::tests::make_mem;
use test::Bencher;

#[bench]
fn united_random_links(b: &mut Bencher) {
    let mem = make_mem();
    let mut links = united::Links::<usize, _>::new(mem);
    let mut links = links.decorators_kit();

    b.iter(|| links.test_random_creations_and_deletions(100))
}

#[bench]
fn heap_reserve(b: &mut Bencher) {
    let mut mem = HeapMem::new();

    let over = 1_000_000_usize;
    b.iter(|| {
        mem.reserve_mem(over);
        mem.reserve_mem(0);
    });
}

#[bench]
fn file_reserve(b: &mut Bencher) {
    // TODO: use TempFileMappedMem
    std::fs::remove_file("anonymous_@@_.file").unwrap();
    let mut mem = FileMappedMem::new("anonymous_@@_.file").unwrap();

    let over = 1_000_000_usize;
    b.iter(|| {
        mem.reserve_mem(over);
        mem.reserve_mem(0);
    });
}
