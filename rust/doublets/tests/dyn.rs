#![feature(box_syntax)]

use doublets::{unit, Doublets, Error};
use mem::GlobalMem;

#[test]
fn basic() -> Result<(), Error<usize>> {
    let mut store: Box<dyn Doublets<_>> = box unit::Store::<_, _>::new(GlobalMem::new())?;

    let a = store.create_point()?;
    let b = store.create_point()?;
    let _ = store.create_link(a, b)?;

    assert_eq!(store.count(), 3);

    Ok(())
}
