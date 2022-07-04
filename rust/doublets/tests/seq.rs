use doublets::{
    data::{
        Flow::{Break, Continue},
        ToQuery,
    },
    mem::GlobalMem,
    num::LinkType,
    unit, Doublets, Error as LinksError, Link,
};
use num_traits::zero;
use std::{error::Error};

fn write_seq<T: LinkType>(store: &mut impl Doublets<T>, seq: &[T]) -> Result<T, LinksError<T>> {
    let mut aliases = vec![store.create()?];

    for id in seq {
        let link = store.create()?;
        aliases.push(store.update(link, link, *id)?)
    }

    for (i, cur) in aliases.iter().enumerate() {
        if let Some(next) = aliases.get(i + 1) {
            store.create_link(*cur, *next)?;
        }
    }
    Ok(*aliases.first().unwrap_or(&zero()))
}

fn custom_single<T: LinkType>(store: &impl Doublets<T>, query: impl ToQuery<T>) -> Option<Link<T>> {
    // todo:
    //  store.each_iter(query).filter(Link::is_partial);

    let mut single = None;
    store.each_by(query, |link| {
        if single.is_none() && link.index != link.source {
            single = Some(link);
            Continue
        } else if link.index != link.source {
            single = None;
            Break
        } else {
            Continue
        }
    });
    single
}

fn read_seq<T: LinkType>(store: &impl Doublets<T>, root: T) -> Result<Vec<T>, LinksError<T>> {
    let any = store.constants().any;
    let mut seq = vec![];
    let mut cur = root;
    while let Some(link) = custom_single(store, [any, cur, any]) {
        cur = link.target;
        let alias = store.try_get_link(link.target)?;
        seq.push(alias.target);
    }
    Ok(seq)
}

#[test]
fn seq() -> Result<(), Box<dyn Error>> {
    let mut store = unit::Store::<usize, _>::new(GlobalMem::new())?;

    // Simulate non-empty storage
    store.create_point()?;
    store.create_point()?;
    store.create_point()?;
    store.create_point()?;
    store.create_point()?;

    let root = write_seq(&mut store, &[1, 2, 3, 4])?;

    let seq = read_seq(&store, root)?;
    println!("{seq:?}");

    Ok(())
}
