use doublets::{data::LinksError, split, unit, Doublets};
use mem::GlobalMem;

#[test]
fn unit_million() -> Result<(), LinksError<usize>> {
    let mut store = unit::Store::<usize, _>::new(GlobalMem::new())?;

    for _ in 0..10_000_000 {
        store.create().unwrap();
    }

    assert_eq!(store.count(), 10_000_000);

    Ok(())
}

#[test]
fn split_million() -> Result<(), LinksError<usize>> {
    let mut store = split::Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new())?;

    for _ in 0..10_000_000 {
        store.create().unwrap();
    }

    assert_eq!(store.count(), 10_000_000);

    Ok(())
}
