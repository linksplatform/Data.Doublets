use crate::num::LinkType;
use crate::doublets::{ILinks, ILinksExtensions};
use std::marker::PhantomData;
use crate::sequences::walkers::SequenceWalker;
use smallvec::SmallVec;
use smallvec::smallvec;
use std::fmt::Display;
use num_traits::zero;
use std::default::default;

pub struct LeveledWalker<'a, T, Links, F> {
    links: &'a Links,
    is_element: F,

    _phantom: PhantomData<T>,
}

const CAPACITY: usize = 1024;

impl<'a, T, Links, F> LeveledWalker<'a, T, Links, F>
where
    T: LinkType + std::fmt::Display,
    Links: ILinks<T>,
    F: Fn(T) -> bool
{
    pub fn with_pred(links: &'a Links, pred: F) -> Self {
        Self { links, is_element: pred, _phantom: PhantomData }
    }

    fn is_element(&self, element: T) -> bool {
        (self.is_element)(element)
    }

    fn count_filled(array: &SmallVec<<Self as SequenceWalker<T, CAPACITY>>::Capacity>) -> usize {
        array.iter().filter(|x| **x != zero()).count()
    }

    fn copy_filled(array: &SmallVec<<Self as SequenceWalker<T, CAPACITY>>::Capacity>) -> SmallVec<<Self as SequenceWalker<T, CAPACITY>>::Capacity> {
        array.iter().filter(|x| **x != zero()).map(|e| *e).collect()
    }

    fn to_vec(&self, sequence: T) -> SmallVec<<Self as SequenceWalker<T, CAPACITY>>::Capacity> {
        let mut len = 1;
        let mut has = false;
        let mut array = smallvec![sequence];
        if self.is_element(sequence) {
           return array;
        }
        loop {
            len *= 2;
            let mut vec = SmallVec::new();
            vec.resize(len, zero());
            has = false;
            for i in 0..array.len() {
                let candidate = array[i];
                if candidate.is_zero() {
                    continue;
                }
                let double_offset = i * 2;
                if self.is_element(candidate) {
                    vec[double_offset] = candidate;
                } else {
                    let link = self.links.get_link(candidate).unwrap(); // TODO: unwrap
                    let source = link.source;
                    let target = link.target;
                    vec[double_offset] = source;
                    vec[double_offset + 1] = target;
                    if !has {
                        has = !(self.is_element(source) && self.is_element(target));
                    }
                }
            }

            array = vec;

            if !has {
                break;
            }
        }

        if Self::count_filled(&array) == array.len() {
            array
        } else {
            println!("{:?}", array);
            Self::copy_filled(&array)
        }
    }
}

impl<'a, T, Links, F> SequenceWalker<T, CAPACITY> for LeveledWalker<'a, T, Links, F>
where
    T: LinkType + std::fmt::Display,
    Links: ILinks<T>,
    F: Fn(T) -> bool
{
    fn walk(&self, sequence: T) -> SmallVec<Self::Capacity> {
        self.to_vec(sequence)
    }
}
