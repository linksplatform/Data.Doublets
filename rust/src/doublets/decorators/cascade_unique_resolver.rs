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
use crate::doublets::{Flow, ILinks, ILinksExtensions, Link, LinksError, Result};
use crate::num::LinkType;

type Base<T, Links> = UniqueResolver<T, Links>;

pub struct CascadeUniqueResolver<T: LinkType, Links: ILinks<T>> {
    links: Links,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, Links: ILinks<T>> CascadeUniqueResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self {
            links,
            _phantom: PhantomData,
        }
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for CascadeUniqueResolver<T, Links> {
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

        // todo find by [[any], replacement[1..]].concat()
        if let Some(old) = links.search(replacement[1], replacement[2]) {
            let new = query[0];
            links.rebase_with(old, new, &mut handler)?;
            if old != new {
                links.delete_with(old, handler)
            } else {
                links.update_by_with(query, replacement, handler)
            }
        } else {
            links.update_by_with(query, replacement, handler)
        }
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
