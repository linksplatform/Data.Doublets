use std::{borrow::BorrowMut, marker::PhantomData, ops::Try};

use doublets::{
    data::{LinksConstants, ToQuery},
    num::LinkType,
};

use doublets::{Doublets, Error, Link};

use crate::UniqueResolver;

type Base<T, Links> = UniqueResolver<T, Links>;

pub struct CascadeUniqueResolver<T: LinkType, L: Doublets<T>> {
    links: L,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, L: Doublets<T>> CascadeUniqueResolver<T, L> {
    pub fn new(links: L) -> Self {
        Self {
            links,
            _phantom: PhantomData,
        }
    }
}

impl<T: LinkType, L: Doublets<T>> Doublets<T> for CascadeUniqueResolver<T, L> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by(&self, query: impl ToQuery<T>) -> T {
        self.links.count_by(query)
    }

    fn create_by_with<F, R>(&mut self, query: impl ToQuery<T>, handler: F) -> Result<R, Error<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.create_by_with(query, handler)
    }

    fn each_by<F, R>(&self, restrictions: impl ToQuery<T>, handler: F) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.each_by(restrictions, handler)
    }

    fn update_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        change: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, Error<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let links = self.links.borrow_mut();
        let query = query.to_query();
        let change = change.to_query();
        let (new, source, target) = (query[0], change[1], change[2]);
        // todo find by [[any], change[1..]].concat()
        let index = if let Some(old) = links.search(source, target) {
            links.rebase_with(new, old, &mut handler)?;
            links.delete_with(new, &mut handler)?;
            old
        } else {
            new
        };

        // TODO: update_by maybe has query style
        links.update_with(index, source, target, handler)
    }

    fn delete_by_with<F, R>(&mut self, query: impl ToQuery<T>, handler: F) -> Result<R, Error<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.delete_by_with(query, handler)
    }
}
