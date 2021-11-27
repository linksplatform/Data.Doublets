use std::time::Instant;
use criterion::{Criterion, criterion_group, criterion_main};
use doublets::doublets::ILinksExtensions;
use doublets::doublets::mem::{splited, united};
use doublets::mem::HeapMem;

fn criterion_benchmark(c: &mut Criterion) {
    c.bench_function("1_000_000 united points", |b| {
        let mem = HeapMem::new();
        let mut links = united::Links::<usize, _>::new(mem).unwrap();
        b.iter_custom(|iters| {
            let instant = Instant::now();
            for _ in 0..1_000_000 {
                links.create_point().unwrap();
            }
            instant.elapsed()
        });
    });

    c.bench_function("1_000_000 splited points", |b| {
        let mem1 = HeapMem::new();
        let mem2 = HeapMem::new();
        let mut links = splited::Links::<usize, _, _>::new(mem1, mem2).unwrap();
        b.iter_custom(|iters| {
            let instant = Instant::now();
            for _ in 0..1_000_000 {
                links.create_point().unwrap();
            }
            instant.elapsed()
        });
    });
}

criterion_group!(benches, criterion_benchmark);
criterion_main!(benches);