use crate::doublets::data::LinksConstants;
use crate::doublets::mem::united::{
    Links, LinksSourcesRecursionlessSizeBalancedTree, LinksTargetsRecursionlessSizeBalancedTree,
    UnusedLinks,
};
use crate::doublets::mem::{splited, united};
use crate::doublets::LinksError;
use crate::mem::{AllocMem, HeapMem, ResizeableMem};
use crate::num::LinkType;
use std::alloc::Global;

// TODO: cfg!
pub fn make_mem() -> AllocMem<Global> {
    HeapMem::new().unwrap()
}

//pub fn make_mem() -> (HeapMem, HeapMem) {
//    (HeapMem::new().unwrap(), HeapMem::new().unwrap())
//}

pub fn make_links<M: ResizeableMem>(mem: M) -> Result<Links<usize, M>, LinksError<usize>> {
    united::Links::<usize, _>::new(mem)
}

//pub fn make_links<M1: ResizeableMem, M2: ResizeableMem>(
//    mem: (M1, M2),
//) -> Result<splited::Links<usize, M1, M2>, LinksError<usize>> {
//    let constants = LinksConstants::via_only_external(true);
//    splited::Links::<usize, _, _>::with_constants(mem.0, mem.1, constants)
//}

pub fn typed_links<T: LinkType, M: ResizeableMem>(mem: M) -> Result<Links<T, M>, LinksError<T>> {
    united::Links::<T, _>::with_constants(mem, LinksConstants::via_only_external(true))
}

//pub fn typed_links<T: LinkType, M1: ResizeableMem, M2: ResizeableMem>(
//    mem: (M1, M2),
//) -> Result<splited::Links<T, M1, M2>, LinksError<T>> {
//    splited::Links::<T, _, _>::with_constants(mem.0, mem.1, LinksConstants::via_only_external(true))
//}

// TODO: use the follow style:
//  #[cfg(test)]
//  mod tests {
//     mods...
//  }

#[cfg(test)]
mod crud;
#[cfg(test)]
mod decorators;
#[cfg(test)]
mod hybrid;
#[cfg(test)]
mod ilinks_basic;
#[cfg(test)]
mod sync;
#[cfg(test)]
mod trees;
