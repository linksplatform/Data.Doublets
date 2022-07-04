use doublets::{split, unit, Doublets, Error};
use mem::GlobalMem;

#[test]
fn unit_million() -> Result<(), Error<usize>> {
    let mut store = unit::Store::<usize, _>::new(GlobalMem::new())?;

    for _ in 0..1_000_000 {
        store.create().unwrap();
    }

    assert_eq!(store.count(), 1_000_000);

    Ok(())
}

#[test]
fn split_million() -> Result<(), Error<usize>> {
    let mut store = split::Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new())?;

    for _ in 0..1_000_000 {
        store.create().unwrap();
    }

    assert_eq!(store.count(), 1_000_000);

    Ok(())
}
