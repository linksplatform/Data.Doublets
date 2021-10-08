use std::ptr::null_mut;

use crate::mem::mem::{Mem, ResizeableMem};

pub(in crate::mem) struct ResizeableBase {
    pub used: usize,
    pub reserved: usize,
    pub ptr: *mut u8
}

impl ResizeableBase {
    // TODO: use macros for calculate in compile time
    const PAGE_SIZE: usize = 1 * 1024;

    pub const MINIMUM_CAPACITY: usize = Self::PAGE_SIZE;
}

impl Mem for ResizeableBase {
    fn get_ptr(&self) -> *mut u8 {
        self.ptr
    }

    fn set_ptr(&mut self, ptr: *mut u8) {
        self.ptr = ptr
    }
}

impl ResizeableMem for ResizeableBase {
    fn use_mem(&mut self, capacity: usize) -> Result<(), ()> {
        if capacity <= self.reserved {
            self.used = capacity;
            Ok(())
        } else {
            Err(())
        }
    }

    fn used_mem(&self) -> usize {
        self.used
    }

    fn reserve_mem(&mut self, capacity: usize) -> Result<(), ()> {
        if capacity >= self.used {
            self.reserved = capacity;
            Ok(())
        } else {
            Err(())
        }
    }

    fn reserved_mem(&self) -> usize {
        self.reserved
    }
}

impl Default for ResizeableBase {
    fn default() -> Self {
        ResizeableBase {
            used: 0,
            reserved: 0,
            ptr: null_mut()
        }
    }
}