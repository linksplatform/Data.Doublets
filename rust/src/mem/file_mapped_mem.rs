use std::cmp::max;
use std::fs::{File, OpenOptions};
use std::io::Error;
use std::io::ErrorKind;
use std::io::Read;
use std::ops::Deref;
use std::path::Path;

use memmap::MmapOptions;

use crate::mem::mem::{Mem, ResizeableMem};
use crate::mem::resizeable_base::ResizeableBase;

pub struct FileMappedMem {
    base: ResizeableBase,
    file: File,
    mapping: memmap::MmapMut
}

impl FileMappedMem {
    pub fn reserve_new<P: AsRef<Path>>(path: P, mut capacity: usize) -> std::io::Result<Self> {
        let file = OpenOptions::new() // TODO: with_options feature
            .read(true)
            .write(true)
            .create(true)
            .open(path)?;

        file.set_len(capacity as u64)?;

        let mapping = unsafe { MmapOptions::new().len(capacity).map_mut(&file) };
        if mapping.is_err() {
            return Err(Error::new(ErrorKind::Other, "TODO: MAPPING ERROR"))
        }

        let mapping = mapping.unwrap();
        let mut new = Self {
            base: Default::default(),
            mapping,
            file
        };

        capacity = max(capacity, ResizeableBase::MINIMUM_CAPACITY);
        new.reserve_mem(capacity);
        new.map(new.reserved_mem());
        let mapping = new.mapping.as_mut_ptr();
        new.set_ptr(mapping);
        Ok(new)
    }

    pub fn new<P: AsRef<Path>>(path: P) -> std::io::Result<Self> {
        Self::reserve_new(path, ResizeableBase::MINIMUM_CAPACITY)
    }

    fn map(&mut self, capacity: usize) -> *mut u8 {
        let mapping = unsafe { memmap::MmapOptions::new().len(capacity).map_mut(&self.file).unwrap() };
        self.mapping = mapping;
        return self.mapping.as_mut_ptr()
    }

    fn unmap(&mut self) {
        //self.file.take(0);
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
    fn use_mem(&mut self, capacity: usize) -> Result<(), ()> {
        self.base.use_mem(capacity)
    }

    fn used_mem(&self) -> usize {
        self.base.used_mem()
    }

    fn reserve_mem(&mut self, capacity: usize) -> Result<(), ()> {
        let result = self.base.reserve_mem(capacity);

        self.unmap();
        if self.file.set_len(capacity as u64).is_err() {
            return Err(());
        }
        let ptr = self.map(capacity);
        self.set_ptr(ptr);

        return result;
    }

    fn reserved_mem(&self) -> usize {
        self.base.reserved_mem()
    }
}
