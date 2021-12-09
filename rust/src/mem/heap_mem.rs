use std::alloc::Layout;
use std::cmp::max;
use std::error::Error;
use std::ops::Add;
use std::ptr::{null, null_mut};

use crate::mem::mem_traits::{Mem, ResizeableMem};
use crate::mem::resizeable_base::ResizeableBase;

pub struct HeapMem {
    base: ResizeableBase,
}

impl HeapMem {
    pub fn reserve_new(mut capacity: usize) -> std::io::Result<Self> {
        let mut new = HeapMem {
            base: Default::default(),
        };
        capacity = max(capacity, ResizeableBase::MINIMUM_CAPACITY);

        new.reserve_mem(capacity)?;
        Ok(new)
    }

    pub fn new() -> std::io::Result<Self> {
        Self::reserve_new(ResizeableBase::MINIMUM_CAPACITY)
    }

    fn on_reserved(&mut self, old_capacity: usize, new_capacity: usize) -> std::io::Result<usize> {
        use std::io::{Error, ErrorKind};
        let layout = Layout::array::<u8>(new_capacity)
            .map_err(|e| Error::new(ErrorKind::Other, e.to_string()))?;
        if self.get_ptr().is_null() {
            unsafe {
                let ptr = std::alloc::alloc_zeroed(layout);
                self.set_ptr(ptr);
            }
        } else {
            unsafe {
                let ptr = self.get_ptr();
                let new = std::alloc::realloc(ptr, layout, new_capacity);
                if old_capacity < new_capacity {
                    let offset = new_capacity - old_capacity;
                    std::ptr::write_bytes(new.add(old_capacity), 0, offset);
                }
                self.set_ptr(new);
            }
        }
        Ok(new_capacity)
    }
}

impl Mem for HeapMem {
    fn get_ptr(&self) -> *mut u8 {
        self.base.get_ptr()
    }

    fn set_ptr(&mut self, ptr: *mut u8) {
        self.base.set_ptr(ptr)
    }
}

impl ResizeableMem for HeapMem {
    fn use_mem(&mut self, capacity: usize) -> std::io::Result<usize> {
        self.base.use_mem(capacity)
    }

    fn used_mem(&self) -> usize {
        self.base.used_mem()
    }

    fn reserve_mem(&mut self, capacity: usize) -> std::io::Result<usize> {
        let older = self.reserved_mem();
        let reserved = self.base.reserve_mem(capacity)?;
        self.on_reserved(older, capacity)?;
        Ok(reserved)
    }

    fn reserved_mem(&self) -> usize {
        self.base.reserved_mem()
    }
}

impl Drop for HeapMem {
    fn drop(&mut self) {
        let layout = Layout::array::<u8>(self.reserved_mem()).unwrap();
        unsafe { std::alloc::dealloc(self.get_ptr(), layout) }
    }
}
