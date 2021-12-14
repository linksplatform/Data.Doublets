use std::cmp::max;
use std::error::Error;
use std::fs::{File, OpenOptions};
use std::io;
use std::mem::ManuallyDrop;
use std::path::Path;

use memmap2;
use memmap2::{MmapMut, MmapOptions};

use crate::mem::mem_traits::{Mem, ResizeableMem};
use crate::mem::resizeable_base::ResizeableBase;

pub struct FileMappedMem {
    base: ResizeableBase,
    file: File,
    mapping: ManuallyDrop<MmapMut>, // TODO: `MaybeUninit`
}

impl FileMappedMem {
    pub fn reserve_new<P: AsRef<Path>>(path: P, capacity: usize) -> io::Result<Self> {
        let capacity = max(capacity, ResizeableBase::MINIMUM_CAPACITY);
        let file = OpenOptions::new()
            .create(true)
            .read(true)
            .write(true)
            .open(path)?;

        let len = file.metadata()?.len() as usize;
        let to_reserve = max(len, capacity);

        let mapping = MmapOptions::new().map_anon()?;
        let mut new = Self {
            base: Default::default(),
            mapping: ManuallyDrop::new(mapping),
            file,
        };

        new.reserve_mem(to_reserve).map(|_| new)
    }

    pub fn new<P: AsRef<Path>>(path: P) -> std::io::Result<Self> {
        Self::reserve_new(path, ResizeableBase::MINIMUM_CAPACITY)
    }

    fn map(&mut self, capacity: usize) -> std::io::Result<*mut u8> {
        let mapping = unsafe { MmapOptions::new().len(capacity).map_mut(&self.file)? };
        self.mapping = ManuallyDrop::new(mapping);
        Ok(self.mapping.as_mut_ptr())
    }

    fn unmap(&mut self) {
        unsafe { ManuallyDrop::drop(&mut self.mapping) }
    }
}

impl Mem for FileMappedMem {
    fn get_ptr(&self) -> *mut u8 {
        self.base.get_ptr()
    }

    fn set_ptr(&mut self, ptr: *mut u8) {
        self.base.set_ptr(ptr)
    }
}

impl ResizeableMem for FileMappedMem {
    fn use_mem(&mut self, capacity: usize) -> std::io::Result<usize> {
        self.base.use_mem(capacity)
    }

    fn used_mem(&self) -> usize {
        self.base.used_mem()
    }

    fn reserve_mem(&mut self, capacity: usize) -> std::io::Result<usize> {
        let reserved = self.base.reserve_mem(capacity)?;

        self.unmap();
        // TODO: file.set_len
        //  self.file.set_len(capacity as u64)?;

        // TODO: hack for parody on `self.file.set_len(capacity.max(`file len`))`
        // self.file.seek(SeekFrom::Start(capacity as u64))?;
        // self.file.seek(SeekFrom::Start(0))?;

        // TODO: current impl
        let file_len = self.file.metadata()?.len();
        self.file.set_len(file_len.max(capacity as u64))?;

        let ptr = self.map(capacity)?;
        self.set_ptr(ptr);

        Ok(reserved)
    }

    fn reserved_mem(&self) -> usize {
        self.base.reserved_mem()
    }
}

impl Drop for FileMappedMem {
    fn drop(&mut self) {
        unsafe {
            ManuallyDrop::drop(&mut self.mapping);
        }
        let used = self.used_mem();
        // TODO: maybe remove `unwrap()` and ignore error
        self.file.set_len(used as u64);
    }
}
