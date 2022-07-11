use std::{
    fmt,
    fmt::{Debug, Formatter},
    ops::Index,
    slice::{from_raw_parts, SliceIndex},
};

use num_traits::zero;

use data::{Query, ToQuery};
use num::LinkType;

#[derive(Default, Eq, PartialEq, Clone, Hash)]
#[repr(C)]
pub struct Link<T: LinkType> {
    pub index: T,
    pub source: T,
    pub target: T,
}

impl<T: LinkType> Link<T> {
    pub fn nothing() -> Self {
        Self::default()
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

    pub fn from_slice(slice: &[T]) -> Link<T> {
        assert!(slice.len() >= 3);

        Self::from_slice_unchecked(slice)
    }

    pub fn from_slice_unchecked(slice: &[T]) -> Link<T> {
        let (index, source, target) = (slice[0], slice[1], slice[2]);
        Self::new(index, source, target)
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

    pub fn as_slice(&self) -> &[T] {
        // SAFETY: Link is repr(C) and therefore is safe to transmute to a slice
        unsafe { from_raw_parts(&self.index, 3) }
    }
}

impl<T: LinkType> Debug for Link<T> {
    fn fmt(&self, f: &mut Formatter<'_>) -> fmt::Result {
        write!(f, "({}: {} {})", self.index, self.source, self.target)
    }
}

impl<T: LinkType> ToQuery<T> for Link<T> {
    fn to_query(&self) -> Query<'_, T> {
        self.as_slice().to_query()
    }
}
