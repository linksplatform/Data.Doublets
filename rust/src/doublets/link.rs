use std::fmt::{Debug, Display, Formatter};
use std::iter;
use std::iter::FromIterator;
use std::ops::{Index, IndexMut};
use std::slice::from_raw_parts;

use num_traits::zero;

use crate::num::LinkType;

#[derive(Default, Debug, Eq, PartialEq, Clone, Hash, Copy, Ord, PartialOrd)]
pub struct Link<T: LinkType> {
    pub index: T,
    pub source: T,
    pub target: T,
}

impl<T: LinkType> Link<T> {
    const LEN: usize = 3;

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

    pub fn len(&self) -> usize {
        Self::LEN
    }

    pub fn is_null(&self) -> bool {
        *self == Self::point(zero())
    }

    #[deprecated(note = "internal functions expect `Link<T>`")]
    pub fn as_slice(&self) -> &[T] {
        unsafe { from_raw_parts(&self.index as *const T, self.len()) }
    }

    pub fn is_full(&self) -> bool {
        self.index == self.source && self.index == self.target
    }

    pub fn is_partial(&self) -> bool {
        self.index == self.source || self.index == self.target
    }
}

impl<T: LinkType> IntoIterator for Link<T> {
    type Item = T;
    type IntoIter = std::array::IntoIter<T, 3>;

    fn into_iter(self) -> Self::IntoIter {
        std::array::IntoIter::new([self.index, self.source, self.target])
    }
}

impl<'a, T: LinkType> IntoIterator for &'a Link<T> {
    type Item = &'a T;
    type IntoIter = std::array::IntoIter<&'a T, 3>;

    fn into_iter(self) -> Self::IntoIter {
        std::array::IntoIter::new([&self.index, &self.source, &self.target])
    }
}

impl<T: LinkType> Index<usize> for Link<T> {
    type Output = T;

    fn index(&self, index: usize) -> &Self::Output {
        match index {
            0 => &self.index,
            1 => &self.source,
            2 => &self.target,
            _ => {
                todo!("link index panic")
            }
        }
    }
}

impl<'a, T: LinkType> IndexMut<usize> for Link<T> {
    fn index_mut(&mut self, index: usize) -> &mut Self::Output {
        match index {
            0 => &mut self.index,
            1 => &mut self.source,
            2 => &mut self.target,
            _ => {
                todo!("link index panic")
            }
        }
    }
}

impl<T: LinkType> FromIterator<T> for Link<T> {
    fn from_iter<I: IntoIterator<Item = T>>(iter: I) -> Self {
        let mut new: Link<T> = Default::default(); // TODO: strange compiler type mismatching
        for (i, item) in iter.into_iter().take(new.len()).enumerate() {
            new[i] = item
        }
        new
    }
}

impl<T: LinkType> Display for Link<T> {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "({}: {} {})", self.index, self.source, self.target)
    }
}
