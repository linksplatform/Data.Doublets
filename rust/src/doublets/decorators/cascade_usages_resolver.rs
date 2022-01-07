use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;
use std::ops::Try;

use num_traits::zero;
use smallvec::SmallVec;

use crate::doublets::data::ToQuery;
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::doublets::LinksError;
use crate::doublets::{ILinks, ILinksExtensions, Link, Result};
use crate::num::LinkType;

pub struct CascadeUsagesResolver<T: LinkType, Links: ILinks<T>> {
    links: Links,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, Links: ILinks<T>> CascadeUsagesResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self {
            links,
            _phantom: default(),
        }
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for CascadeUsagesResolver<T, Links> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by(&self, query: impl ToQuery<T> + 'async_trait) -> T {
        self.links.count_by(query)
    }

    fn create_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T> + 'async_trait,
        handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        self.links.create_by_with(query, handler)
    }

    fn try_each_by<F, R>(&self, restrictions: impl ToQuery<T> + 'async_trait, handler: F) -> R
    where
        F: FnMut(Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        self.links.try_each_by(restrictions, handler)
    }

    fn update_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T> + 'async_trait,
        replacement: impl ToQuery<T> + 'async_trait,
        handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        self.links.update_by_with(query, replacement, handler)
    }

    fn delete_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T> + 'async_trait,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        self.links
            .delete_usages_with(query.to_query()[0], &mut handler)?;
        self.links.delete_by_with(query, handler)
    }
}
