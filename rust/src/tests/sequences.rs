use smallvec::SmallVec;
use crate::tests::make_mem;
use crate::tests::make_links;
use crate::doublets::{ILinksExtensions, Link};
use crate::doublets::data::{AddrToRaw, Hybrid, IGenericLinks, IGenericLinksExtensions, LinksConstants, RawToAddr};
use crate::doublets::mem::united::Links;
use crate::sequences::converters::balanced_variant::BalancedVariant;
use crate::sequences::converters::links_to_sequence_base::LinksToSequence;
use crate::sequences::walkers::{LeveledWalker, SequenceWalker};

#[test]
fn basic_sequences() {
    let mem = make_mem();
    let constants = LinksConstants::via_only_external(true);
    let mut links = Links::<usize, _>::with_constants(mem, constants);


    let to_raw = AddrToRaw::new();
    let to_adr = RawToAddr::new();
    let sequence: Vec<usize> = "Неправильный не я... Неправильный весь этот мир. (c) Канеки Кен".to_string()
        .chars()
        .map(|c| c as usize)
        .map(|e| to_raw.convert(e))
        .collect();

    let mut x = BalancedVariant::new();
    let seq = x.convert(&mut links, sequence);
//
    let constants = links.constants();
    links.each(|link| {
        let to_raw = AddrToRaw::new();
        let to_adr = RawToAddr::new();


        if !constants.is_external_reference(link.source) {
            print!("{}", link.source);
        } else {
            print!("\"{}\"", char::from_u32((to_adr.convert(link.source) - 1) as u32).unwrap())
        }
        print!("->");
        if !constants.is_external_reference(link.target) {
            println!("{}", link.target);
        } else {
            println!("\"{}\"", char::from_u32((to_adr.convert(link.source) - 1) as u32).unwrap())
        }

        constants.r#continue
    });
//
    let walker = LeveledWalker::with_pred(&links, |link| links.is_partial_point(link));
    let data: SmallVec<[usize; 1024]> = walker.walk(seq);
//
    println!("{:?}", seq);
    println!("{:?}", data);
    println!("{}", to_adr.convert(to_raw.convert(1023 as usize)));

    let string: String = data.iter()
        .map(|e| to_adr.convert(*e) - 1)
        .map(|e| char::from_u32(e as u32).unwrap())
        .collect();
    println!("{}", string);
}
