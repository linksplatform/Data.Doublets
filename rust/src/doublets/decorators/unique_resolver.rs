use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;
use std::ops::Try;

use smallvec::SmallVec;

use crate::doublets::data::ToQuery;
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::doublets::{ILinks, ILinksExtensions, Link, Result};
use crate::num::LinkType;

pub struct UniqueResolver<T: LinkType, Links: ILinks<T>> {
    links: Links,
    resolver: fn(&mut Links, old: T, new: T) -> Result<T>,
}

impl<T: LinkType, Links: ILinks<T>> UniqueResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self::with_resolver(links, Self::resolve_conflict)
    }

    pub fn with_resolver(
        links: Links,
        resolver: fn(&mut Links, old: T, new: T) -> Result<T>,
    ) -> Self {
        Self { links, resolver }
    }

    pub(in crate::doublets::decorators) fn resolve_conflict(
        links: &mut Links,
        old: T,
        new: T,
    ) -> Result<T> {
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

    fn count_by(&self, query: impl ToQuery<T>) -> T {
        self.links.count_by(query)
    }

    fn create_by(&mut self, query: impl ToQuery<T>) -> Result<T> {
        self.links.create_by(query)
    }

    fn try_each_by<F, R>(&self, restrictions: impl ToQuery<T>, handler: F) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.try_each_by(restrictions, handler)
    }

    fn update_by(&mut self, query: impl ToQuery<T>, replacement: impl ToQuery<T>) -> Result<T> {
        let links = self.links.borrow_mut();
        let query = query.to_query();
        if let Some(new) = links.find(replacement.to_query()) {
            (self.resolver)(links, query[0], new)
        } else {
            links.update_by(query, replacement)
        }
    }

    fn delete_by(&mut self, query: impl ToQuery<T>) -> Result<T> {
        self.links.delete_by(query)
    }
}
