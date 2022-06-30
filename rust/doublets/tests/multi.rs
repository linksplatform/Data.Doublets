use doublets::{data::LinksError, split, test_extensions::DoubletsTestExt, unit};
use mem::GlobalMem;

#[test]
fn random_crud_unit() -> Result<(), LinksError<usize>> {
    let mut store = unit::Store::<usize, _>::new(GlobalMem::new())?;

    store.test_random_creations_and_deletions(1000);

    Ok(())
}

#[test]
fn random_crud_split() -> Result<(), LinksError<usize>> {
    let mut store = split::Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new())?;

    store.test_random_creations_and_deletions(1000);

    Ok(())
}
