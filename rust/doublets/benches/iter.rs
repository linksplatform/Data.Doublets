use criterion::{black_box, criterion_group, criterion_main, Criterion};
use data::Flow::Continue;
use doublets::{split::Store, Doublets};
use mem::GlobalMem;

fn each_iter_searching(c: &mut Criterion) {
    let mut store = Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new()).unwrap();
    let any = store.constants().any;

    store.create_link(1, 1).unwrap();

    c.bench_function("each_iter", |b| {
        b.iter(|| {
            store.each_iter([any, 1, 1]);
        });
    });
    c.bench_function("each", |b| {
        b.iter(|| {
            store.search(1, 1);
        });
    });
}

fn iter(c: &mut Criterion) {
    let mut store = Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new()).unwrap();
    let any = store.constants().any;

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
            store.try_each(|link| {
                black_box(link);
                Continue
            });
        });
    });
    c.bench_function("each_with_vec", |b| {
        b.iter(|| {
            let mut vec = Vec::with_capacity(store.count());
            store.try_each(|link| {
                vec.push(black_box(link));
                Continue
            });
            black_box(vec);
        });
    });
}

criterion_group!(benches, each_iter_searching, iter);
criterion_main!(benches);
