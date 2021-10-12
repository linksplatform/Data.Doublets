use crate::num::LinkType;
use num_traits::zero;
use std::fmt::{Debug, Formatter, Display};
use std::iter;
use std::iter::FromIterator;
use std::ops::{Index, IndexMut};
use std::slice::from_raw_parts;

#[derive(Default, Debug, Eq, PartialEq, Clone, Hash)]
pub struct Link<T: LinkType> {
    pub index: T,
    pub source: T,
    pub target: T,
}

impl<T: LinkType> Link<T> {
    const LEN: usize = 3;

    pub fn new(index: T, source: T, target: T) -> Self {
        // TODO: use new for default construct
        //    // TODO: use default_feature
        //    Default::default()
        Self {
            index,
            source,
            target,
        }
    }

    pub fn from_once(val: T) -> Self {
        Self::new(val, val, val)
    }

    pub fn len(&self) -> usize {
        Self::LEN
    }

    pub fn is_null(&self) -> bool {
        *self == Self::from_once(zero())
    }

    pub fn as_slice(&self) -> &[T] {
        unsafe { from_raw_parts(&self.index as *const T, self.len()) }
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
                panic!("TODO: LINK INDEX");
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
                panic!("TODO: LINK INDEX");
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

impl<T: LinkType + Display> Display for Link<T> {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "({}: {} {})", self.index, self.source, self.target)
    }
}
