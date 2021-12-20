use crate::mem::AllocMem;
use std::alloc::{Global, Layout};
use std::cmp::max;
use std::error::Error;
use std::ops::Add;
use std::ptr::{null, null_mut};

use crate::mem::mem_traits::{Mem, ResizeableMem};
use crate::mem::resizeable_base::ResizeableBase;

pub struct HeapMem {
    base: ResizeableBase,
}

#[deprecated(note = "use `AllocMem` instead")]
impl HeapMem {
    pub fn reserve_new(mut capacity: usize) -> std::io::Result<AllocMem<Global>> {
        let mut new = AllocMem::new(Global)?;
        new.reserve_mem(capacity)?;
        Ok(new)
    }

    #[deprecated(note = "use `AllocMem::new` instead")]
    pub fn new() -> std::io::Result<AllocMem<Global>> {
        Self::reserve_new(ResizeableBase::MINIMUM_CAPACITY)
    }
}
