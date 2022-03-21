# Doublets

## Example 
```rust,no_run
// alpha does not has human prelude
use doublets::{
    data::Flow::{Break, Continue}, // also can use std::ops::ControlFlow
    doublets::mem::united,
    doublets::Doublets,
    mem::FileMappedMem,
};
use std::error::Error;

fn main() -> Result<(), Box<dyn Error>> {
    // A doublet links store is mapped to "db.links" file:
    let mem = FileMappedMem::new("db.links")?;
    let mut links = united::Store::<usize, _>::new(mem)?;

    // A creation of the doublet link:
    let link = links.create()?;

    // The link is updated to reference itself twice (as a source and a target):
    let link = links.update(link, link, link)?;

    println!("The number of links in the data store is {}", links.count());
    println!("Data store contents:");

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