use std::default::default;
use std::marker::PhantomData;
use std::ops::Try;

use crate::doublets::data::LinksConstants;
use crate::doublets::data::ToQuery;
use crate::doublets::LinksError;
use crate::doublets::{Link, Links, Result};
use crate::num::LinkType;

pub struct LogAllChanges<T: LinkType, L: Links<T>> {
    links: L,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, L: Links<T>> LogAllChanges<T, L> {
    pub fn new(links: L) -> Self {
        Self {
            links,
            _phantom: default(),
        }
    }
}

impl<T: LinkType, L: Links<T>> Links<T> for LogAllChanges<T, L> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by(&self, query: impl ToQuery<T>) -> T {
        self.links.count_by(query)
    }

    fn create_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.create_by_with(query, move |before, after| {
            log::info!("{} ==> {}", before, after);
            handler(before, after)
        })
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
        self.links
            .update_by_with(query, replacement, move |before, after| {
                log::info!("{} ==> {}", before, after);
                handler(before, after)
            })
    }

    fn delete_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.links.delete_by_with(query, move |before, after| {
            log::info!("{} ==> {}", before, after);
            handler(before, after)
        })
    }
}
