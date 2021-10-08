use crate::num::LinkType;
use num_traits::zero;
use crate::doublets::data::hybrid::{Hybrid};

pub struct AddrToRaw;

impl AddrToRaw {
    pub fn new() -> Self {
        Self
    }

    pub fn convert<T: LinkType>(&self, source: T) -> T {
        Hybrid::new(source, true).as_value()
    }
}

pub struct RawToAddr;

impl RawToAddr {
    pub fn new() -> Self {
        Self
    }

    pub fn convert<T: LinkType>(&self, source: T) -> T {
        Hybrid::new(source, false).absolute()
    }
}