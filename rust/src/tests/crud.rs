use bumpalo::Bump;
use rand::{random, thread_rng, Rng};
use std::alloc::{AllocError, Allocator, Global, Layout, System};
use std::default::default;
use std::error::Error;
use std::fs::{File, OpenOptions};
use std::io::{Read, Seek, SeekFrom, Write};
use std::ptr::{null_mut, NonNull};
use std::time::Instant;

use crate::doublets::data::{AddrToRaw, IGenericLinks, LinksConstants};
use crate::doublets::mem::splited;
use crate::doublets::{ILinks, ILinksExtensions, Link, LinksError};
//use crate::doublets::decorators::{CascadeUsagesResolver, NonNullDeletionResolver};
//use crate::doublets::mem::splited;
use crate::doublets::mem::united::Links;
use crate::mem::{
    AllocMem, FileMappedMem, GlobalMem, Mem, ResizeableBase, ResizeableMem, TempFileMem,
};
use crate::test_extensions::ILinksTestExtensions;
use crate::tests::make_links;
use crate::tests::make_mem;
/*
#[tokio::test]
fn random_creations_and_deletions() {
    std::fs::remove_file("db.links");

    let mem = make_mem().unwrap();
    let mut links = make_links(mem).unwrap();
    let mut links = links.decorators_kit();

    let instant = Instant::now();
    links.test_random_creations_and_deletions(1000);

    println!("{:?}", instant.elapsed());
}
*/
// TODO: `into_file()`
#[tokio::test]
async fn mapping() {
    std::fs::remove_file("mapping_file");
    let mut mem = FileMappedMem::new("mapping_file").unwrap();

    mem.reserve_mem(13370).unwrap();
    mem.reserve_mem(2280).unwrap();

    let mut file = std::fs::File::open("mapping_file").unwrap();
    assert_eq!(13370, file.metadata().unwrap().len() as usize);
}

#[tokio::test]
async fn billion_points_file_mapped() {
    std::fs::remove_file("mem_bpfm");

    let mem = TempFileMem::new().unwrap();
    let mut links = Links::<usize, _>::new(mem).unwrap();

    let instant = Instant::now();

    for _ in 0..1_000_000 {
        links.create_point().await.unwrap();
    }

    println!("{:?}", instant.elapsed());
}

#[tokio::test]
async fn billion_points_heap_mem() -> Result<(), LinksError<usize>> {
    let mem = GlobalMem::new()?;
    let mut links = Links::<usize, _>::new(mem)?;

    let instant = Instant::now();

    for _ in 0..1_000_000 {
        links.create_point().await?;
    }

    println!("{:?}", instant.elapsed());
    Ok(())
}

#[tokio::test]
async fn billion_points_bump_alloc() {
    let bump = Bump::new();
    let mem = AllocMem::new(&bump).unwrap();
    let mut links = Links::<usize, _>::new(mem).unwrap();

    let instant = Instant::now();

    for _ in 0..1_000_000 {
        links.create_point().await.unwrap();
    }

    println!("{:?}", instant.elapsed());
}

#[tokio::test]
async fn many_points_and_searches() {
    let bump = Bump::new();
    let mem = AllocMem::new(System).unwrap();
    let mut links = Links::<usize, _>::new(mem).unwrap();

    let instant = Instant::now();
    for _ in 0..1_000_000 {
        links.create_point().await.unwrap();
    }
    println!("{:?}", instant.elapsed());

    let instant = Instant::now();
    for i in 1..=1_000_000 {
        // links.search_or(i, i, 0);
        links.get_link(i).await.unwrap();
    }
    println!("{:?}", instant.elapsed());
}

// TODO: Create `TempFileMappedMem`

#[tokio::test]
async fn billion_points_file_mapped_splited() {
    let mem = TempFileMem::new().unwrap();
    let index = TempFileMem::new().unwrap();
    let mut links = splited::Links::<usize, _, _>::new(mem, index).unwrap();

    let instant = Instant::now();

    for _ in 0..1_000_000 {
        links.create_point().await.unwrap();
    }

    println!("{:?}", instant.elapsed());
}

#[tokio::test]
async fn billion_points_heap_mem_splited() {
    let mem = GlobalMem::new().unwrap();
    let index = GlobalMem::new().unwrap();
    let mut links = splited::Links::<usize, _, _>::new(mem, index).unwrap();

    let instant = Instant::now();

    for _ in 0..1_000_000 {
        links.create_point().await.unwrap();
    }

    println!("{:?}", instant.elapsed());
}

#[tokio::test]
async fn billion_points_bump_alloc_splited() {
    let bump = Bump::new();
    let mut mem = AllocMem::new(&bump).unwrap();
    let mut index = AllocMem::new(&bump).unwrap();
    index.reserve_mem(1023 * 1023).unwrap();
    let mut links = splited::Links::<usize, _, _>::new(mem, index).unwrap();

    let instant = Instant::now();

    for _ in 0..1_000_000 {
        links.create_point().await.unwrap();
    }

    println!("{:?}", instant.elapsed());
}

#[tokio::test]
async fn many_points_and_searches_splited() {
    let mem = AllocMem::new(System).unwrap();
    let index = AllocMem::new(System).unwrap();
    let mut links = splited::Links::<usize, _, _>::new(mem, index).unwrap();

    let instant = Instant::now();
    for _ in 0..1_000_000 {
        links.create_point().await.unwrap();
    }
    println!("{:?}", instant.elapsed());

    let instant = Instant::now();
    for i in 1..=1_000_000 {
        links.search_or(i, i, 0).await;
    }
    println!("{:?}", instant.elapsed());
}
