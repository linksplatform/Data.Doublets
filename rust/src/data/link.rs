use std::default::default;
use std::fmt::{Debug, Display, Formatter};
use std::ops::Index;
use std::slice::{from_raw_parts, SliceIndex};

use num_traits::zero;

use data::Query;
use data::ToQuery;
use num::LinkType;

#[derive(Default, Debug, Eq, PartialEq, Clone, Hash)]
pub struct Link<T: LinkType> {
    pub index: T,
    pub source: T,
    pub target: T,
}

impl<T: LinkType> Link<T> {
    pub fn nothing() -> Self {
        default()
    }

    pub fn new(index: T, source: T, target: T) -> Self {
        Self {
            index,
            source,
            target,
        }
    }

    pub fn point(val: T) -> Self {
        Self::new(val, val, val)
    }

    pub fn is_null(&self) -> bool {
        *self == Self::point(zero())
    }

    pub fn is_full(&self) -> bool {
        self.index == self.source && self.index == self.target
    }

    pub fn is_partial(&self) -> bool {
        self.index == self.source || self.index == self.target
    }
}

impl<T: LinkType, I: SliceIndex<[T]>> Index<I> for Link<T> {
    type Output = I::Output;

    fn index(&self, index: I) -> &Self::Output {
        let slice = unsafe { from_raw_parts(&self.index, 3) };
        Index::index(slice, index)
    }
}

impl<T: LinkType> Display for Link<T> {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "({}: {} {})", self.index, self.source, self.target)
    }
}

impl<T: LinkType> ToQuery<T> for Link<T> {
    fn to_query(&self) -> Query<'_, T> {
        Query::new(unsafe { from_raw_parts(&self.index, 3) })
    }
}
