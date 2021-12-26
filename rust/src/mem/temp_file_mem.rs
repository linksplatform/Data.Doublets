use crate::mem::{FileMappedMem, Mem};
use crate::ResizeableMem;
use std::fs;
use std::io;
use std::path::Path;

pub struct TempFileMem {
    mem: FileMappedMem,
}

impl TempFileMem {
    pub fn new() -> io::Result<Self> {
        let path = tempfile::tempfile()?;
        Ok(TempFileMem {
            mem: FileMappedMem::from_file(path)?,
        })
    }
}

impl Mem for TempFileMem {
    fn get_ptr(&self) -> *mut u8 {
        self.mem.get_ptr()
    }

    fn set_ptr(&mut self, ptr: *mut u8) {
        self.mem.set_ptr(ptr)
    }
}

impl ResizeableMem for TempFileMem {
    fn use_mem(&mut self, capacity: usize) -> io::Result<usize> {
        self.mem.use_mem(capacity)
    }

    fn used_mem(&self) -> usize {
        self.mem.used_mem()
    }

    fn reserve_mem(&mut self, capacity: usize) -> io::Result<usize> {
        self.mem.reserve_mem(capacity)
    }

    fn reserved_mem(&self) -> usize {
        self.mem.reserved_mem()
    }
}
