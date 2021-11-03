use std::fs::{File, OpenOptions};
use std::io::{Read, Seek, SeekFrom, Write};
use crate::tests::make_mem;
use crate::tests::make_links;
use crate::doublets::{ILinksExtensions, Link};
use crate::doublets::data::IGenericLinks;
use crate::test_extensions::ILinksTestExtensions;
use crate::mem::{/*FileMappedMem, */ResizeableMem};
use std::time::Instant;
use crate::doublets::decorators::NonNullDeletionResolver;

#[test]
fn random_creations_and_deletions() {
    std::fs::remove_file("db.links");

    let mem = make_mem();
    let mut links = make_links(mem);
    let mut links = links.decorators_kit();

    let instant = Instant::now();
    links.test_random_creations_and_deletions(1000);

    println!("{:?}", instant.elapsed());
}

/*
#[test]
fn mapping() {
    std::fs::remove_file("map.txt").unwrap();
    let mut mem = FileMappedMem::new("map.txt").unwrap();

    mem.reserve_mem(1337).unwrap();
    mem.reserve_mem(228).unwrap();

    let mut file = std::fs::File::open("map.txt").unwrap();
    assert_eq!(1337, file.metadata().unwrap().len() as usize);
}

#[test]
fn billion_points() {
    std::fs::remove_file("db.txt");

    let mem = FileMappedMem::new("db.txt").unwrap();
    let mut links = make_links(mem);

    let instant = Instant::now();

    for _ in 0..10_000_000 {
        let link = links.create_point();
    }

    println!("{:?}", instant.elapsed());
}
*/