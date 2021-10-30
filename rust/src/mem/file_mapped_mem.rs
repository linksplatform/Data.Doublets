use std::cmp::max;
use std::fs::{File, OpenOptions};
use std::io::{Error, Write};
use std::io::ErrorKind;
use std::io::Read;
use std::ops::Deref;
use std::path::Path;

use memmap2;
use memmap2::MmapOptions;

use crate::mem::mem::{Mem, ResizeableMem};
use crate::mem::resizeable_base::ResizeableBase;

pub struct FileMappedMem {
    base: ResizeableBase,
    file: File,
    mapping: memmap2::MmapMut,
}

impl FileMappedMem {
    pub fn reserve_new<P: AsRef<Path>>(path: P, mut capacity: usize) -> std::io::Result<Self> {
        if let Err(_) = File::open(&path) {
            println!("CREATE");
            let file = File::create(&path)?;
        }

        let file = OpenOptions::new() // TODO: with_options feature
            .read(true)
            .write(true)
            .truncate(false)
            .open(path)?;

        capacity = max(capacity, ResizeableBase::MINIMUM_CAPACITY);
        let len = file.metadata().unwrap().len() as usize;
        let to_reserve = if len > capacity {
            (len / capacity + 1) * capacity
        } else {
            capacity
        };

        let mapping = MmapOptions::new().map_anon().unwrap();
        let mut new = Self {
            base: Default::default(),
            mapping,
            file,
        };

        new.reserve_mem(to_reserve);
        Ok(new)
    }

    pub fn new<P: AsRef<Path>>(path: P) -> std::io::Result<Self> {
        Self::reserve_new(path, ResizeableBase::MINIMUM_CAPACITY)
    }

    fn map(&mut self, capacity: usize) -> *mut u8 {
        let mapping = unsafe {
            memmap2::MmapOptions::new()
                .len(capacity)
                .map_mut(&self.file)
                .unwrap()
        };
        self.mapping = mapping;
        return self.mapping.as_mut_ptr();
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
