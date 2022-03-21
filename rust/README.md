# Doublets

## Example 
```rust,no_run
// alpha does not has human prelude
use std::error::Error;
use doublets::{
    data::Flow::{Continue, Break}, // also can use std::ops::ControlFlow
    mem::FileMappedMem,
    doublets::Doublets,
    doublets::mem::united,
};

fn main() -> Result<(), Box<dyn Error>> {
    // A doublet links store is mapped to "db.links" file:
    let mem = FileMappedMem::new("db.links")?;
    let mut links = united::Links::<usize, _>::new(mem)?;

    // A creation of the doublet link:
    let link = links.create()?;

    // The link is updated to reference itself twice (as a source and a target):
    let link = links.update(link, link, link)?;

    println!("The number of links in the data store is {}", links.count());
    println!("Data store contents:");

    let r#continue = links.constants().r#continue;
    links.try_each(|link| {
        println!("{}", link);
        Continue
    });

    // The link's content reset:
    let link = links.update(link, 0, 0)?;

    // The link deletion:
    links.delete(link)?;
    Ok(())
}
```