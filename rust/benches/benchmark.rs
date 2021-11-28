use std::time::Instant;
use criterion::{black_box, Criterion, criterion_group, criterion_main};
use doublets::doublets::ILinks;
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

    c.bench_function("1_000_000 united get or create", |b| {
        let mem = HeapMem::new();
        let mut links = united::Links::<usize, _>::new(mem).unwrap();
        links.get_or_create(black_box(1), black_box(1)).unwrap();
        for i in 1..=1000 {
            links.get_or_create(black_box(i), black_box(i)).unwrap();
        }
        b.iter(|| {
            for _ in 0..1_000_000 {
                links.get_or_create(black_box(1), black_box(1)).unwrap();
            }
        });
    });
}

criterion_group!(benches, criterion_benchmark);
criterion_main!(benches);