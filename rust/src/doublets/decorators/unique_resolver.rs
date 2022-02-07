use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;
use std::ops::Try;

use smallvec::SmallVec;

use crate::doublets::data::ToQuery;
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::doublets::{Flow, LinksError};
use crate::doublets::{ILinksExtensions, Link, Links, Result};
use crate::num::LinkType;

pub struct UniqueResolver<T: LinkType, L: Links<T>> {
    links: L,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, L: Links<T>> UniqueResolver<T, L> {
    pub fn new(links: L) -> Self {
        UniqueResolver {
            links,
            _phantom: PhantomData,
        }
    }
}

impl<T: LinkType, L: Links<T>> Links<T> for UniqueResolver<T, L> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by(&self, query: impl ToQuery<T>) -> T {
        self.links.count_by(query)
    }

    fn create_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.create_by_with(query, handler)
    }

    fn try_each_by<F, R>(&self, restrictions: impl ToQuery<T>, handler: F) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.try_each_by(restrictions, handler)
    }

    fn update_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        replacement: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let links = self.links.borrow_mut();
        let query = query.to_query();
        let replacement = replacement.to_query();
        let (new, source, target) = (query[0], replacement[1], replacement[2]);
        // todo find by [[any], replacement[1..]].concat()
        let index = if let Some(old) = links.search(source, target) {
            links.delete_with(new, &mut handler)?;
            old
        } else {
            new
        };

        // TODO: update_by maybe has query style
        links.update_with(index, source, target, handler)
    }

    fn delete_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.delete_by_with(query, handler)
    }
}
