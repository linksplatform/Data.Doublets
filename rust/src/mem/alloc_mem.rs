use crate::mem::{Mem, ResizeableBase, ResizeableMem};
use std::alloc::{AllocError, Allocator, Layout, LayoutError};
use std::default::default;
use std::error::Error;
use std::io;
use std::ptr::{null_mut, NonNull};

pub struct AllocMem<A: Allocator> {
    base: ResizeableBase,
    alloc: A,
}

impl<A: Allocator> AllocMem<A> {
    pub fn new(alloc: A) -> std::io::Result<Self> {
        let mut new = Self {
            base: ResizeableBase {
                used: 0,
                reserved: 0,
                ptr: NonNull::slice_from_raw_parts(NonNull::dangling(), 0),
            },
            alloc,
        };
        new.reserve_impl(ResizeableBase::MINIMUM_CAPACITY, false)?;
        Ok(new)
    }

    fn reserve_impl(&mut self, capacity: usize, reallocate: bool) -> io::Result<usize> {
        let old_capacity = self.reserved_mem();
        let new_capacity = capacity;

        let result: Result<(), Box<dyn Error + Sync + Send>> = try {
            if !reallocate {
                let layout = Layout::array::<u8>(capacity)?;
                let ptr = self.alloc.allocate_zeroed(layout)?;
                self.set_ptr(ptr);
            } else {
                let old_layout = Layout::array::<u8>(old_capacity)?;
                let new_layout = Layout::array::<u8>(new_capacity)?;

                let ptr = self.get_ptr();
                self.set_ptr(match old_capacity {
                    old if old < new_capacity => unsafe {
                        self.alloc
                            .grow_zeroed(ptr.as_non_null_ptr(), old_layout, new_layout)?
                    },
                    old if old > new_capacity => unsafe {
                        self.alloc
                            .shrink(ptr.as_non_null_ptr(), old_layout, new_layout)?
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
}

impl<A: Allocator> Mem for AllocMem<A> {
    fn get_ptr(&self) -> NonNull<[u8]> {
        self.base.get_ptr()
    }

    fn set_ptr(&mut self, ptr: NonNull<[u8]>) {
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
        self.reserve_impl(capacity, true)
    }

    fn reserved_mem(&self) -> usize {
        self.base.reserved_mem()
    }
}

impl<A: Allocator> Drop for AllocMem<A> {
    fn drop(&mut self) {
        unsafe {
            let ptr = self.get_ptr();
            let layout = Layout::for_value_raw(ptr.as_ptr());
            self.alloc.deallocate(ptr.as_non_null_ptr(), layout);
        }
    }
}

unsafe impl<A: Allocator> Send for AllocMem<A> {}
unsafe impl<A: Allocator> Sync for AllocMem<A> {}
