use crate::doublets::mem::splited;
use crate::doublets::Flow::Continue;
use crate::doublets::ILinks;
use crate::mem::HeapMem;
use std::error::Error;

// #[test]
fn null_reference() -> Result<(), Box<dyn Error>> {
    let mut links = splited::Links::<usize, _, _>::new(HeapMem::new()?, HeapMem::new()?)?;

    let mut link = 0;
    links.create_link_with(2280000, 13370000, |before, after| {
        link = after.index;
        println!("{} ==> {}", before, after);
        Continue
    });

    links.delete(link);

    Ok(())
}
