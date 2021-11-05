use crate::doublets::mem::united;
use crate::mem::{HeapMem, ResizeableMem};
use crate::num::LinkType;

// TODO: cfg!
pub fn make_mem() -> HeapMem {
    HeapMem::new()
}

// TODO: cfg!
pub fn make_links<M: ResizeableMem>(mem: M) -> united::Links<usize, M> {
    united::Links::<usize, _>::new(mem)
}

// TODO: cfg!
pub fn typed_links<T: LinkType, M: ResizeableMem>(mem: M) -> united::Links<T, M> {
    united::Links::<T, _>::new(mem)
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
