use std::borrow::BorrowMut;
use std::default::default;
use std::io::BufRead;
use std::marker::PhantomData;

use num_traits::zero;

use crate::doublets::{ILinks, ILinksExtensions, Link};
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::doublets::decorators::UniqueResolver;
use crate::num::LinkType;

type Base<T, Links> = UniqueResolver<T, Links>;

pub struct CascadeUniqueResolver<T: LinkType, Links: ILinks<T>> {
    links: UniqueResolver<T, Links>,
}

impl<T: LinkType, Links: ILinks<T>> CascadeUniqueResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self { links: Base::with_resolver(links, Self::resolve_conflict) }
    }

    pub(in crate::doublets::decorators) fn resolve_conflict(links: &mut Links, old: T, new: T) -> T {
        links.rebase(old, new);
        Base::resolve_conflict(links, old, new)
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for CascadeUniqueResolver<T, Links> {
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
        self.links.delete(index)
    }
}
