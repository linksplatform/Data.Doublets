use doublets::test_extensions::ILinksTestExtensions;
use doublets::{mem::united::Store, Doublets};
use mem::GlobalMem;
use std::error::Error;

fn main() -> Result<(), Box<dyn Error>> {
    let mut links = Store::<usize, _>::new(GlobalMem::new()?)?;
    links.test_random_creations_and_deletions(1000);
    Ok(())
}
