/*use crate::doublets::decorators::UniqueValidator;
use crate::doublets::mem::splited;
use crate::doublets::Flow::Continue;
use crate::doublets::{ILinks, LinksError::AlreadyExists};
use crate::mem::GlobalMem;
use log::error;
use std::error::Error;

// #[tokio::test]
fn null_reference() -> Result<(), Box<dyn Error>> {
    let mut links = splited::Links::<usize, _, _>::new(GlobalMem::new()?, GlobalMem::new()?)?;

    let mut links = UniqueValidator::new(links);
    links.create_link(1, 2)?;
    if let AlreadyExists(doublet) = links.create_link(1, 2).unwrap_err() {
        error!("link {} is already exists", doublet);
    }

    Ok(())
}
*/
