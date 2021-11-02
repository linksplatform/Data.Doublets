use crate::doublets::data::hybrid::Hybrid;
use crate::num::LinkType;

pub struct AddrToRaw;

impl AddrToRaw {
    pub fn new() -> Self {
        Self
    }

    pub fn convert<T: LinkType>(&self, source: T) -> T {
        Hybrid::external(source).as_value()
    }
}

pub struct RawToAddr;

impl RawToAddr {
    pub fn new() -> Self {
        Self
    }

    pub fn convert<T: LinkType>(&self, source: T) -> T {
        Hybrid::internal(source).absolute()
    }
}
