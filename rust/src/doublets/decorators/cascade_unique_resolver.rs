use std::borrow::BorrowMut;
use std::default::default;
use std::io::BufRead;
use std::marker::PhantomData;
use std::ops::Try;

use num_traits::zero;

use crate::doublets::data::{
    IGenericLinks, IGenericLinksExtensions, LinksConstants, Query, ToQuery,
};
use crate::doublets::decorators::UniqueResolver;
use crate::doublets::{ILinks, ILinksExtensions, Link, LinksError, Result};
use crate::num::LinkType;

type Base<T, Links> = UniqueResolver<T, Links>;

pub struct CascadeUniqueResolver<T: LinkType, Links: ILinks<T>> {
    links: UniqueResolver<T, Links>,
}

impl<T: LinkType, Links: ILinks<T>> CascadeUniqueResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self {
            links: Base::with_resolver(links, Self::resolve_conflict),
        }
    }

    pub(in crate::doublets::decorators) fn resolve_conflict(
        links: &mut Links,
        old: T,
        new: T,
    ) -> Result<T> {
        links.rebase(old, new)?;
        Base::resolve_conflict(links, old, new)
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for CascadeUniqueResolver<T, Links> {
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
        self.links.update_by(query, replacement)
    }

    fn delete_by(&mut self, query: impl ToQuery<T>) -> Result<T> {
        self.links.delete_by(query)
    }
}
