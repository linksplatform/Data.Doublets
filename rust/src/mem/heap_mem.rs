use std::alloc::Layout;
use std::cmp::max;
use std::ops::Add;
use std::ptr::{null, null_mut};

use crate::mem::mem::{Mem, ResizeableMem};
use crate::mem::resizeable_base::ResizeableBase;

pub struct HeapMem {
    base: ResizeableBase,
}

impl HeapMem {
    pub fn reserve_new(mut capacity: usize) -> Self {
        let mut new = HeapMem {
            base: Default::default(),
        };
        capacity = max(capacity, ResizeableBase::MINIMUM_CAPACITY);

        new.reserve_mem(capacity);
        new
    }

    pub fn new() -> Self {
        Self::reserve_new(ResizeableBase::MINIMUM_CAPACITY)
    }

    fn on_reserved(&mut self, old_capacity: usize, new_capacity: usize) {
        unsafe fn block_zero(ptr: *mut u8, capacity: usize) {
            for i in 0..capacity {
                *(ptr.offset(i as isize)) = 0
            }
        }

        if self.get_ptr() == null_mut() {
            unsafe {
                /// TODO: use `std::alloc`
                self.set_ptr(libc::malloc(new_capacity as u64) as *mut u8);
                block_zero(self.get_ptr(), new_capacity);
            }
        } else {
            unsafe {
                let mut_ptr = self.get_ptr() as *mut libc::c_void;
                /// TODO: use `std::alloc`
                self.set_ptr(libc::realloc(mut_ptr, new_capacity as u64) as *mut u8);
                block_zero(self.get_ptr(), new_capacity - old_capacity)
            }
        }
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
    fn use_mem(&mut self, capacity: usize) -> Result<(), ()> {
        self.base.use_mem(capacity)
    }

    fn used_mem(&self) -> usize {
        self.base.used_mem()
    }

    fn reserve_mem(&mut self, capacity: usize) -> Result<(), ()> {
        let older = self.reserved_mem();
        let result = self.base.reserve_mem(capacity);
        self.on_reserved(older, capacity);
        return result;
    }

    fn reserved_mem(&self) -> usize {
        self.base.reserved_mem()
    }
}

impl Drop for HeapMem {
    fn drop(&mut self) {
        unsafe { libc::free(self.get_ptr() as *mut libc::c_void) }
    }
}
