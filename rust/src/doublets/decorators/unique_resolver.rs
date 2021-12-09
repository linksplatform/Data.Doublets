use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;

use smallvec::SmallVec;

use crate::doublets::{ILinks, ILinksExtensions, Link, Result};
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::num::LinkType;

pub struct UniqueResolver<T: LinkType, Links: ILinks<T>> {
    links: Links,
    resolver: fn(&mut Links, old: T, new: T) -> Result<T> ,
}

impl<T: LinkType, Links: ILinks<T>> UniqueResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self::with_resolver(links, Self::resolve_conflict)
    }

    pub fn with_resolver(links: Links, resolver: fn(&mut Links, old: T, new: T) -> Result<T>) -> Self {
        Self { links, resolver }
    }

    pub(in crate::doublets::decorators) fn resolve_conflict(links: &mut Links, old: T, new: T) -> Result<T>  {
        if old != new && links.exist(old) {
            links.delete(old)?;
        }
        Ok(new)
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for UniqueResolver<T, Links> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by<const L: usize>(&self, restrictions: [T; L]) -> T {
        self.links.count_by(restrictions)
    }

    fn create(&mut self) -> Result<T>  {
        self.links.create()
    }

    fn each_by<H, const L: usize>(&self, handler: H, restrictions: [T; L]) -> T where H: FnMut(Link<T>) -> T {
        self.links.each_by(handler, restrictions)
    }

    fn update(&mut self, index: T, source: T, target: T) -> Result<T>  {
        let links = self.links.borrow_mut();
        let new = links.search_or(
            source,
            target,
            default(),
        );

        if new == default() {
            links.update(index, source, target)
        } else {
            (self.resolver)(links, index, new)
        }
    }

    fn delete(&mut self, index: T) -> Result<T>  {
        self.links.delete(index)
    }
}
