extern crate test;

use test::Bencher;

use crate::doublets::{ILinks, ILinksExtensions};
use crate::doublets::decorators::NonNullDeletionResolver;
use crate::doublets::mem::united;
use crate::mem::{FileMappedMem, HeapMem, ResizeableMem};
use crate::test_extensions::ILinksTestExtensions;
use crate::tests::make_mem;

#[bench]
fn united_random_links(b: &mut Bencher) {
    let mem = HeapMem::new().unwrap();
    let mut links = united::Links::<usize, _>::new(mem).unwrap();
    let mut links = links.decorators_kit();

    b.iter(|| links.test_random_creations_and_deletions(100))
}

#[bench]
fn united_over_points(b: &mut Bencher) {
    let mem = HeapMem::new().unwrap();
    let links = united::Links::<usize, _>::new(mem).unwrap();
    let mut links = NonNullDeletionResolver::new(links);
    b.iter(|| {
        let to_create = 100_000;
        let mut vec = Vec::with_capacity(to_create);

        for _ in 0..to_create {
            vec.push(links.create_point().unwrap());
        }

        for link in vec {
            links.delete(link).unwrap();
        }
    })
}

#[bench]
fn heap_reserve(b: &mut Bencher) {
    let mut mem = HeapMem::new().unwrap();

    let over = 1_000_000_usize;
    b.iter(|| {
        mem.reserve_mem(over).unwrap();
        mem.reserve_mem(0).unwrap();
    });
}

#[bench]
fn file_reserve(b: &mut Bencher) {
    // TODO: use `TempFileMappedMem`
    std::fs::remove_file("anonymous_@@_.file");
    let mut mem = FileMappedMem::new("anonymous_@@_.file").unwrap();

    let over = 1_000_000_usize;
    b.iter(|| {
        mem.reserve_mem(over).unwrap();
        mem.reserve_mem(0).unwrap();
    });
}
