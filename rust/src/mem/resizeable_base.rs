use std::io::Error;
use std::io::ErrorKind;
use std::ptr::{null_mut, NonNull};

use crate::mem::mem_traits::{Mem, ResizeableMem};

pub struct ResizeableBase {
    pub used: usize,
    pub reserved: usize,
    pub ptr: NonNull<[u8]>,
}

impl ResizeableBase {
    // TODO: use macros for calculate in compile time
    const PAGE_SIZE: usize = 4 * 1024;

    pub const MINIMUM_CAPACITY: usize = Self::PAGE_SIZE;
}

impl Mem for ResizeableBase {
    fn get_ptr(&self) -> NonNull<[u8]> {
        self.ptr
    }

    fn set_ptr(&mut self, ptr: NonNull<[u8]>) {
        self.ptr = ptr
    }
}

impl ResizeableMem for ResizeableBase {
    fn use_mem(&mut self, capacity: usize) -> std::io::Result<usize> {
        if capacity <= self.reserved {
            self.used = capacity;
            Ok(self.used)
        } else {
            Err(Error::new(
                ErrorKind::Other,
                "cannot use greater than the memory reserved",
            ))
        }
    }

    fn used_mem(&self) -> usize {
        self.used
    }

    fn reserve_mem(&mut self, capacity: usize) -> std::io::Result<usize> {
        if capacity >= self.used {
            self.reserved = capacity;
            Ok(self.used)
        } else {
            Err(Error::new(
                ErrorKind::Other,
                "cannot reserve less than the memory used",
            ))
        }
    }

    fn reserved_mem(&self) -> usize {
        self.reserved
    }
}
