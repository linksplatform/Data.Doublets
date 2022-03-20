use std::error::Error;
use doublets::doublets::{Doublets, mem::united::Store};
use doublets::mem::GlobalMem;
use doublets::test_extensions::ILinksTestExtensions;


fn main() -> Result<(), Box<dyn Error>> {
    let mut links = Store::<usize, _>::new(GlobalMem::new()?)?;
    links.test_random_creations_and_deletions(1000)?;
    Ok(())
}