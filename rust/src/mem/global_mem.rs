use std::alloc::Layout;
use std::cmp::max;
use std::ops::Add;

use std::ptr::{null_mut, NonNull};
use std::{alloc, io, ptr};

use crate::mem::ResizeableBase;
use crate::mem::{Mem, ResizeableMem};

pub struct GlobalMem {
    base: ResizeableBase,
}

impl GlobalMem {
    pub fn reserve_new(mut capacity: usize) -> io::Result<Self> {
        capacity = max(capacity, ResizeableBase::MINIMUM_CAPACITY);
        let mut new = GlobalMem {
            base: ResizeableBase {
                used: 0,
                reserved: 0,
                ptr: NonNull::slice_from_raw_parts(NonNull::dangling(), 0),
            },
        };
        new.base.reserve_mem(capacity)?;
        new.on_reserved_impl(0, capacity, false)?;
        Ok(new)
    }

    pub fn new() -> std::io::Result<Self> {
        Self::reserve_new(ResizeableBase::MINIMUM_CAPACITY)
    }

    fn layout_impl(capacity: usize) -> io::Result<Layout> {
        Layout::array::<u8>(capacity).map_err(|err| io::Error::new(io::ErrorKind::Other, err))
    }

    fn on_reserved_impl(
        &mut self,
        old_capacity: usize,
        new_capacity: usize,
        reallocate: bool,
    ) -> io::Result<usize> {
        if !reallocate {
            unsafe {
                let layout = Self::layout_impl(new_capacity)?;
                let ptr = alloc::alloc_zeroed(layout);
                self.set_ptr(NonNull::slice_from_raw_parts(
                    unsafe { NonNull::new_unchecked(ptr) },
                    layout.size(),
                ));
            }
        } else {
            unsafe {
                let ptr = self.get_ptr();
                let layout = Self::layout_impl(old_capacity)?;
                let new = alloc::realloc(ptr.as_mut_ptr(), layout, new_capacity);
                if old_capacity < new_capacity {
                    let offset = new_capacity - old_capacity;
                    ptr::write_bytes(new.add(old_capacity), 0, offset);
                }
                self.set_ptr(NonNull::slice_from_raw_parts(
                    unsafe { NonNull::new_unchecked(new) },
                    new_capacity,
                ));
            }
        }
        Ok(new_capacity)
    }
}

impl Mem for GlobalMem {
    fn get_ptr(&self) -> NonNull<[u8]> {
        self.base.get_ptr()
    }

    fn set_ptr(&mut self, ptr: NonNull<[u8]>) {
        self.base.set_ptr(ptr)
    }
}

impl ResizeableMem for GlobalMem {
    fn use_mem(&mut self, capacity: usize) -> io::Result<usize> {
        self.base.use_mem(capacity)
    }

    fn used_mem(&self) -> usize {
        self.base.used_mem()
    }

    fn reserve_mem(&mut self, capacity: usize) -> io::Result<usize> {
        let older = self.reserved_mem();
        let reserved = self.base.reserve_mem(capacity)?;
        self.on_reserved_impl(older, capacity, true)?;
        Ok(reserved)
    }

    fn reserved_mem(&self) -> usize {
        self.base.reserved_mem()
    }
}

impl Drop for GlobalMem {
    fn drop(&mut self) {
        unsafe {
            let ptr = self.get_ptr();
            let layout = Layout::array::<u8>(ptr.len()).unwrap(); // TODO: unchecked
            alloc::dealloc(ptr.as_mut_ptr(), layout)
        }
    }
}
