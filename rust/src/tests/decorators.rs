use crate::doublets::decorators::NonNullDeletionResolver;
use crate::doublets::Flow::Continue;
use crate::doublets::{ILinks, Link};
use crate::tests::make_links;
use crate::tests::make_mem;
use std::error::Error;

#[test]
fn non_null_deletions() -> Result<(), Box<dyn Error>> {
    let mem = make_mem();
    let mut links = make_links(mem).unwrap();
    let mut links = NonNullDeletionResolver::new(links);

    let point = links.create_point()?;

    links.delete_with(point, |before, after| {
        println!("{} ==> {}", before, after);
        Continue
    });

    Ok(())
}
