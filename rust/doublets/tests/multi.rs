use doublets::{split, test_extensions::DoubletsTestExt, unit, Error};
use mem::GlobalMem;
use std::time::Instant;

#[test]
fn random_crud_unit() -> Result<(), Error<usize>> {
    let mut store = unit::Store::<usize, _>::new(GlobalMem::new())?;

    let instant = Instant::now();
    store.test_random_creations_and_deletions(1000);
    println!("{:?}", instant.elapsed());

    Ok(())
}

#[test]
fn random_crud_split() -> Result<(), Error<usize>> {
    let mut store = split::Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new())?;

    let instant = Instant::now();
    store.test_random_creations_and_deletions(1000);
    println!("{:?}", instant.elapsed());

    Ok(())
}
