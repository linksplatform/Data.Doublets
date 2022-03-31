use std::ops::Index;
use std::ops::Range;

pub struct Point<T> {
    index: T,
    size: usize,
}

impl<T: PartialEq + Clone> Point<T> {
    pub fn new(index: T, size: usize) -> Self {
        Self { index, size }
    }

    pub fn len(&self) -> usize {
        self.size
    }

    // TODO: use support private is_ function
    pub fn is_full<L>(list: L) -> bool
    where
        L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>,
    {
        let mut iter = list.into_iter();
        assert!(
            iter.len() >= 2,
            "cannot determine link's pointless using only its identifier"
        );

        if let Some(first) = iter.next() {
            iter.all(|item| item == first)
        } else {
            false
        }
    }

    pub fn is_partial<L>(list: L) -> bool
    where
        L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>,
    {
        let mut iter = list.into_iter();
        assert!(
            iter.len() >= 2,
            "cannot determine link's pointless using only its identifier"
        );

        if let Some(first) = iter.next() {
            iter.any(|item| item == first)
        } else {
            false
        }
    }
}

impl<T: PartialEq> Index<usize> for Point<T> {
    type Output = T;

    fn index(&self, index: usize) -> &Self::Output {
        if index < self.size {
            &self.index
        } else {
            panic!("TODO: POINT")
        }
    }
}

pub struct Iter<T: PartialEq + Copy> {
    range: Range<usize>,
    index: T,
}

impl<T: PartialEq + Copy> Iterator for Iter<T> {
    type Item = T;

    fn next(&mut self) -> Option<Self::Item> {
        let next = self.range.next();
        if next.is_some() {
            Some(self.index)
        } else {
            None
        }
    }
}

impl<T: PartialEq + Copy> ExactSizeIterator for Iter<T> {
    fn len(&self) -> usize {
        self.range.end
    }
}

impl<T: PartialEq + Copy> IntoIterator for Point<T> {
    type Item = T;
    type IntoIter = Iter<T>;

    fn into_iter(self) -> Self::IntoIter {
        Iter {
            range: 0..self.len(),
            index: self.index,
        }
    }
}
