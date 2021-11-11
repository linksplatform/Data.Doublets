use crate::doublets::data::LinksConstants;
use crate::doublets::mem::{splited, united};
use crate::mem::{HeapMem, ResizeableMem};
use crate::num::LinkType;

// TODO: cfg!
//pub fn make_mem() -> HeapMem {
//    HeapMem::new()
//}

pub fn make_mem() -> (HeapMem, HeapMem) {
    (HeapMem::new(), HeapMem::new())
}

//pub fn make_links<M: ResizeableMem>(mem: M) -> united::Links<usize, M> {
//    united::Links::<usize, _>::new(mem)
//}

#[cfg(windows)]
pub fn make_links<M1: ResizeableMem, M2: ResizeableMem>(mem: (M1, M2)) -> splited::Links<usize, M1, M2> {
    let constants = LinksConstants::via_only_external(true);
    splited::Links::<usize, _, _>::with_constants(mem.0, mem.1, constants)
}

//pub fn typed_links<T: LinkType, M: ResizeableMem>(mem: M) -> united::Links<T, M> {
//    united::Links::<T, _>::with_constants(mem, LinksConstants::via_only_external(true))
//}

pub fn typed_links<T: LinkType, M1: ResizeableMem, M2: ResizeableMem>(mem: (M1, M2)) -> splited::Links<T, M1, M2> {
    splited::Links::<T, _, _>::with_constants(mem.0, mem.1, LinksConstants::via_only_external(true))
}


// TODO: use the follow style:
//  #[cfg(test)]
//  mod tests {
//     mods...
//  }

#[cfg(test)]
mod ilinks_basic;
#[cfg(test)]
mod decorators;
#[cfg(test)]
mod crud;
#[cfg(test)]
mod sync;
#[cfg(test)]
mod trees;
#[cfg(test)]
mod hybrid;
