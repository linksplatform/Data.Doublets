use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;
use std::ops::Try;

use num_traits::zero;
use smallvec::SmallVec;

use crate::doublets::data::ToQuery;
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::doublets::{ILinks, ILinksExtensions, Link, LinksError, Result};
use crate::num::LinkType;
use crate::query;

pub struct UsagesValidator<T: LinkType, Links: ILinks<T>> {
    links: Links,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, Links: ILinks<T>> UsagesValidator<T, Links> {
    pub fn new(links: Links) -> Self {
        Self {
            links,
            _phantom: default(),
        }
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for UsagesValidator<T, Links> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by(&self, query: impl ToQuery<T>) -> T {
        self.links.count_by(query)
    }

    fn try_create_by_with<F, R>(&mut self, query: impl ToQuery<T>, handler: F) -> Result<T>
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.try_create_by_with(query, handler)
    }

    fn try_each_by<F, R>(&self, restrictions: impl ToQuery<T>, handler: F) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.try_each_by(restrictions, handler)
    }

    fn try_update_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        replacement: impl ToQuery<T>,
        handler: F,
    ) -> Result<T>
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let links = self.links.borrow_mut();
        let query = query.to_query();
        let index = query[0];
        if links.has_usages(index) {
            Err(LinksError::HasDeps(links.get_link(index).unwrap()))
        } else {
            links.update_by(query, replacement)
        }
    }

    fn try_delete_by_with<F, R>(&mut self, query: impl ToQuery<T>, handler: F) -> Result<T>
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let links = self.links.borrow_mut();
        let query = query.to_query();
        let index = query[0];
        if links.has_usages(index) {
            Err(LinksError::HasDeps(links.get_link(index).unwrap()))
        } else {
            links.delete(index)
        }
    }
}
