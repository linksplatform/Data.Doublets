use crate::mem::{HeapMem, ResizeableMem};
use crate::doublets::mem::united;

// TODO: cfg!
pub fn make_mem() -> HeapMem {
    HeapMem::new()
}

// TODO: cfg!
pub fn make_links<M: ResizeableMem>(mem: M) -> united::Links<usize, M> {
    united::Links::<usize, _>::new(mem)
}

#[cfg(test)]
mod ilinks_basic;
#[cfg(test)]
mod decorators;
#[cfg(test)]
mod crud;
#[cfg(test)]
mod sync;
