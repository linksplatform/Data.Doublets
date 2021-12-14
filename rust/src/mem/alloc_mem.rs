use crate::mem::{Mem, ResizeableBase, ResizeableMem};
use std::alloc::{AllocError, Allocator, Layout, LayoutError};
use std::default::default;
use std::error::Error;
use std::io;
use std::ptr::NonNull;

pub struct AllocMem<A: Allocator> {
    base: ResizeableBase,
    alloc: A,
}

impl<A: Allocator> AllocMem<A> {
    pub fn new(alloc: A) -> std::io::Result<Self> {
        let mut new = Self {
            base: default(),
            alloc,
        };
        new.reserve_mem(ResizeableBase::MINIMUM_CAPACITY)?;
        Ok(new)
    }
}

impl<A: Allocator> Mem for AllocMem<A> {
    fn get_ptr(&self) -> *mut u8 {
        self.base.get_ptr()
    }

    fn set_ptr(&mut self, ptr: *mut u8) {
        self.base.set_ptr(ptr)
    }
}

impl<A: Allocator> ResizeableMem for AllocMem<A> {
    fn use_mem(&mut self, capacity: usize) -> std::io::Result<usize> {
        self.base.use_mem(capacity)
    }

    fn used_mem(&self) -> usize {
        self.base.used_mem()
    }

    fn reserve_mem(&mut self, capacity: usize) -> std::io::Result<usize> {
        let old_capacity = self.reserved_mem();
        let new_capacity = capacity;

        let result: Result<(), Box<dyn Error + Sync + Send>> = try {
            if self.get_ptr().is_null() {
                let layout = Layout::array::<u8>(capacity)?;
                let ptr = self.alloc.allocate_zeroed(layout)?;
                self.set_ptr(ptr.as_mut_ptr());
            } else {
                let old_layout = Layout::array::<u8>(old_capacity)?;
                let new_layout = Layout::array::<u8>(new_capacity)?;

                let ptr = self.get_ptr();
                self.set_ptr(match old_capacity {
                    old if old < new_capacity => unsafe {
                        self.alloc
                            .grow_zeroed(NonNull::new_unchecked(ptr), old_layout, new_layout)?
                            .as_mut_ptr()
                    },
                    old if old > new_capacity => unsafe {
                        self.alloc
                            .shrink(NonNull::new_unchecked(ptr), old_layout, new_layout)?
                            .as_mut_ptr()
                    },
                    _ => self.get_ptr(),
                });
            }
        };

        match result {
            Ok(_) => self.base.reserve_mem(capacity),
            Err(err) => Err(io::Error::new(io::ErrorKind::Other, err)),
        }
    }

    fn reserved_mem(&self) -> usize {
        self.base.reserved_mem()
    }
}
