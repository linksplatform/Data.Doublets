use crate::decorators::UniqueValidator;
use crate::mem::splited;
use crate::{Doublets, LinksError::AlreadyExists};
use data::Flow::Continue;
use log::error;
use mem::GlobalMem;
use std::error::Error;

// #[test]
fn null_reference() -> Result<(), Box<dyn Error>> {
    let mut links = splited::Store::<usize, _, _>::new(GlobalMem::new()?, GlobalMem::new()?)?;

    let mut links = UniqueValidator::new(links);
    links.create_link(1, 2)?;
    if let AlreadyExists(doublet) = links.create_link(1, 2).unwrap_err() {
        error!("link {} is already exists", doublet);
    }

    Ok(())
}
