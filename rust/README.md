# Doublets
A library that represents database engine that uses doublets.

## [Overview](https://github.com/linksplatform)

## Example

A basic CRUD in doublets

```rust
use doublets::data::Flow::Continue;
use doublets::mem::FileMappedMem;
use doublets::{united, Doublets};
use std::fs::File;

fn main() -> Result<(), Box<dyn std::error::Error>> {
    // create or open read/write file
    let file = File::options()
        .create(true)
        .read(true)
        .write(true)
        .open("db.links")?;

    let mem = FileMappedMem::new(file)?;
    let mut links = united::Store::<usize, _>::new(mem)?;

    // Creation of the doublet in tiny style
    let mut point = links.create()?;

    // Update of the doublet in handler style
    // The link is updated to reference itself twice (as source and target):
    links.update_with(point, point, point, |_, after| {
        // link is { index, source, target }
        point = after.index;
        // give handler state (any ops::Try)
        Continue
    })?;

    println!("The number of links in the data store is {}", links.count());
    println!("Data store contents:");

    links.try_each(|link| {
        println!("{}", link);
        Continue
    });

    // The link deletion in full style:
    let any = links.constants().any;
    // query in [index source target] style
    links.delete_by_with([point, any, any], |before, _| {
        println!("Goodbye {}", before);
        Continue
    })?;
    Ok(())
}
```
