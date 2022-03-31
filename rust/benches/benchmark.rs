#![feature(backtrace)]
#![feature(allocator_api)]
#![feature(result_option_inspect)]

use criterion::{black_box, criterion_group, criterion_main, Criterion};
use doublets::data::LinksConstants;
use doublets::doublets::mem::{splited, united};
use doublets::doublets::Doublets;
use doublets::mem::{AllocMem, GlobalMem};
use doublets::query;
use std::alloc::Global;
use std::error::Error;
use std::time::Instant;

fn criterion_benchmark(c: &mut Criterion) {
    c.bench_function("array", |b| {
        b.iter(|| black_box([1_u32]));
    });
    c.bench_function("tuple", |b| {
        b.iter(|| black_box(&[1_u32]));
    });
    c.bench_function("struct", |b| {
        b.iter(|| black_box(query![1_u32]));
    });
}

criterion_group!(benches, criterion_benchmark);
criterion_main!(benches);
