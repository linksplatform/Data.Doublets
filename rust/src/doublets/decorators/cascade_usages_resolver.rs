use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;

use crate::doublets::{ILinks, ILinksExtensions, Link};
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::num::LinkType;
use num_traits::zero;
use smallvec::SmallVec;

pub struct CascadeUsagesResolver<T: LinkType, Links: ILinks<T>> {
    links: Links,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, Links: ILinks<T>> CascadeUsagesResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self { links, _phantom: default() }
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for CascadeUsagesResolver<T, Links> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by<const L: usize>(&self, restrictions: [T; L]) -> T {
        self.links.count_by(restrictions)
    }

    fn create(&mut self) -> T {
        self.links.create()
    }

    fn each_by<H, const L: usize>(&self, handler: H, restrictions: [T; L]) -> T
    where
        H: FnMut(Link<T>) -> T,
    {
        self.links.each_by(handler, restrictions)
    }

    fn update(&mut self, index: T, source: T, target: T) -> T {
        self.links.update(index, source, target)
    }

    fn delete(&mut self, index: T) -> T {
        let links = self.links.borrow_mut();
        links.delete_usages(index);
        links.delete(index)
    }
}
