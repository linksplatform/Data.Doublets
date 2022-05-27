use criterion::{black_box, criterion_group, criterion_main, Criterion};
use doublets::splited::Store;
use doublets::Doublets;
use mem::GlobalMem;

fn criterion_benchmark(c: &mut Criterion) {
    let mut links =
        Store::<usize, _, _>::new(GlobalMem::new().unwrap(), GlobalMem::new().unwrap()).unwrap();
    c.bench_function("without_zeroing", |b| {
        b.iter(|| {
            for _ in 0..1_000_000 {
                links.create().unwrap();
            }

            for i in 0..1_000_000 {
                links.delete(i + 1).unwrap();
            }
        });
    });
    c.bench_function("with_zeroing", |b| {
        b.iter(|| {
            for _ in 0..1_000_000 {
                links.create().unwrap();
            }

            for i in 0..1_000_000 {
                links.update(i + 1, 0, 0);
                links.delete(i + 1).unwrap();
            }
        });
    });
}

criterion_group!(benches, criterion_benchmark);
criterion_main!(benches);
