use std::error::Error;

pub trait Mem {
    fn get_ptr(&self) -> *mut u8;
    fn set_ptr(&mut self, ptr: *mut u8);
}

pub trait ResizeableMem: Mem {
    fn use_mem(&mut self, capacity: usize) -> Result<usize, Box<dyn Error>>;
    fn used_mem(&self) -> usize;

    fn reserve_mem(&mut self, capacity: usize) -> Result<usize, Box<dyn Error>>;
    fn reserved_mem(&self) -> usize;
}
