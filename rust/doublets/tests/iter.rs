use doublets::data::LinksError;
use doublets::{splited, Doublets, Link};
use mem::GlobalMem;

#[test]
fn split_iter() -> Result<(), LinksError<usize>> {
    let mut store = splited::Store::<usize, _, _>::new(GlobalMem::new()?, GlobalMem::new()?)?;

    let a = store.create_point()?;
    let b = store.create_point()?;
    store.create_link(a, b)?;

    assert_eq!(
        store.iter().collect::<Vec<_>>(),
        vec![Link::new(1, 1, 1), Link::new(2, 2, 2), Link::new(3, 1, 2),]
    );

    Ok(())
}
