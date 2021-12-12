use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;
use std::ops::Try;

use num_traits::zero;
use smallvec::SmallVec;

use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::doublets::{ILinks, ILinksExtensions, Link, Result};
use crate::num::LinkType;

pub struct NonNullDeletionResolver<T: LinkType, Links: ILinks<T>> {
    links: Links,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, Links: ILinks<T>> NonNullDeletionResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self {
            links,
            _phantom: default(),
        }
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for NonNullDeletionResolver<T, Links> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by<const L: usize>(&self, restrictions: [T; L]) -> T {
        self.links.count_by(restrictions)
    }

    fn create(&mut self) -> Result<T> {
        self.links.create()
    }

    fn try_each_by<F, R, const L: usize>(&self, handler: F, restrictions: [T; L]) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.try_each_by(handler, restrictions)
    }

    fn update(&mut self, index: T, source: T, target: T) -> Result<T> {
        let links = self.links.borrow_mut();
        links.update(index, source, target)
    }

    fn delete(&mut self, index: T) -> Result<T> {
        let links = self.links.borrow_mut();
        let null = links.constants().null;
        links.update(index, null, null)?;
        links.delete(index)
    }
}
