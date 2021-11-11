use std::io::Error;
use std::io::ErrorKind;
use std::ptr::null_mut;

use crate::mem::mem_traits::{Mem, ResizeableMem};

pub struct ResizeableBase {
    pub used: usize,
    pub reserved: usize,
    pub ptr: *mut u8,
}

impl ResizeableBase {
    // TODO: use macros for calculate in compile time
    const PAGE_SIZE: usize = 2 * 1024;

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
    fn use_mem(&mut self, capacity: usize) -> Result<usize, Box<dyn std::error::Error>> {
        if capacity <= self.reserved {
            self.used = capacity;
            Ok(self.used)
        } else {
            panic!("{} {}", capacity, self.reserved);
            Err(Error::new(
                ErrorKind::Other,
                "cannot use greater than the memory reserved",
            ).into())
        }
    }

    fn used_mem(&self) -> usize {
        self.used
    }

    fn reserve_mem(&mut self, capacity: usize) -> Result<usize, Box<dyn std::error::Error>> {
        if capacity >= self.used {
            self.reserved = capacity;
            Ok(self.reserved)
        } else {
            Err(Error::new(
                ErrorKind::Other,
                "cannot reserve less than the memory used",
            ).into())
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
            ptr: null_mut(),
        }
    }
}
