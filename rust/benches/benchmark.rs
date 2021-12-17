#![feature(backtrace)]
#![feature(allocator_api)]
#![feature(result_option_inspect)]

use criterion::{black_box, criterion_group, criterion_main, Criterion};
use doublets::doublets::data::LinksConstants;
use doublets::doublets::mem::{splited, united};
use doublets::doublets::ILinks;
use doublets::mem::{AllocMem, HeapMem};
use std::alloc::Global;
use std::error::Error;
use std::time::Instant;

fn criterion_benchmark(c: &mut Criterion) {
    c.bench_function("heap mem", |b| {
        let mem = HeapMem::new().unwrap();
        let mut links = united::Links::<usize, _>::with_options()
            .mem(mem)
            .constants(LinksConstants::via_only_external(true))
            .reserve_step(1024 * 10)
            .open()
            .unwrap();
        b.iter(|| {
            for _ in 0..1_0_000 {
                links.create_point().unwrap();
            }
        });
    });

    c.bench_function("alloc global", |b| {
        let mem = AllocMem::new(Global).unwrap();
        let mut links = united::Links::<usize, _>::with_options()
            .mem(mem)
            .constants(LinksConstants::via_only_external(true))
            .reserve_step(1024 * 10)
            .open()
            .unwrap();
        b.iter(|| {
            for _ in 0..1_0_000 {
                links.create_point().unwrap();
            }
        });
    });
}

criterion_group!(benches, criterion_benchmark);
criterion_main!(benches);
