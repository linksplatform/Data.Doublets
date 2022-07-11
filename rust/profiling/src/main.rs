use bumpalo::Bump;
use std::alloc::System;
// use platform_mem::{AllocMem, GlobalMem};
use doublets::{mem::AllocMem, unit, Doublets};
use mimalloc::MiMalloc;
use std::time::Instant;

#[global_allocator]
static GLOBAL: MiMalloc = MiMalloc;

fn main() {
    let bump = Bump::new();
    let mut store = unit::Store::<usize, _>::new(AllocMem::new(&bump)).unwrap();

    let instant = Instant::now();

    for _ in 0..1_000_000 {
        //store.create().unwrap();
        store.create_point().unwrap();
    }

    println!("{}", store.count());

    println!("{:?}", instant.elapsed());
}
