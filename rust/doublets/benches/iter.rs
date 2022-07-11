use criterion::{black_box, criterion_group, criterion_main, Criterion};
use data::Flow::Continue;
use doublets::{split::Store, Doublets, Links};
use mem::GlobalMem;

fn iter(c: &mut Criterion) {
    let mut store = Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new()).unwrap();
    let _any = store.constants().any;

    for _ in 0..1_000_000 {
        store.create_point().unwrap();
    }

    (1..=1_000_000).filter(|x| x % 172 == 0).for_each(|x| {
        store.delete(x).unwrap();
    });

    c.bench_function("iter", |b| {
        b.iter(|| {
            store.iter().for_each(|item| {
                black_box(item);
            })
        });
    });
    c.bench_function("each", |b| {
        b.iter(|| {
            store.each(|link| {
                black_box(link);
                Continue
            });
        });
    });
    c.bench_function("each_with_vec", |b| {
        b.iter(|| {
            let mut vec = Vec::with_capacity(store.count());
            store.each(|link| {
                vec.push(black_box(link));
                Continue
            });
            black_box(vec);
        });
    });
}

criterion_group!(benches, iter);
criterion_main!(benches);
