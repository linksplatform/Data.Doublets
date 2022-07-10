use doublets::{split, unit, Doublets, Error};
use mem::GlobalMem;
use std::time::Instant;

const MILLION: usize = 1_000_000;

#[test]
fn unit_million() -> Result<(), Error<usize>> {
    let mut store = unit::Store::<usize, _>::new(GlobalMem::new())?;

    for _ in 0..MILLION {
        store.create().unwrap();
    }

    assert_eq!(store.count(), MILLION);

    Ok(())
}

#[test]
fn split_million() -> Result<(), Error<usize>> {
    let mut store = split::Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new())?;

    for _ in 0..MILLION {
        store.create().unwrap();
    }

    assert_eq!(store.count(), MILLION);

    Ok(())
}

#[test]
fn unit_million_points() -> Result<(), Error<usize>> {
    let mut store = unit::Store::<usize, _>::new(GlobalMem::new())?;

    let instant = Instant::now();
    for _ in 0..MILLION {
        store.create_point().unwrap();
    }
    println!("{:?}", instant.elapsed());

    assert_eq!(store.count(), MILLION);

    Ok(())
}

#[test]
fn split_million_points() -> Result<(), Error<usize>> {
    let mut store = split::Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new())?;

    let instant = Instant::now();
    for _ in 0..MILLION {
        store.create_point().unwrap();
    }
    println!("{:?}", instant.elapsed());

    assert_eq!(store.count(), MILLION);

    Ok(())
}
