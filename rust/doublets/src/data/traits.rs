use std::{
    default::default,
    ops::{ControlFlow, Try},
};

use crate::{FuseHandler, Link, LinksError};
use data::{Flow, LinksConstants, ToQuery};
use num::LinkType;
use num_traits::{one, zero};

pub type Result<T, E = LinksError<T>> = std::result::Result<T, E>;

pub type ReadHandler<'a, T> = &'a mut dyn FnMut(Link<T>) -> Flow;

pub type WriteHandler<'a, T> = &'a mut dyn FnMut(Link<T>, Link<T>) -> Flow;

pub trait Links<T: LinkType>: Send + Sync {
    fn constants(&self) -> &LinksConstants<T>;

    fn count_links(&self, query: &[T]) -> T;

    fn create_links(
        &mut self,
        query: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, LinksError<T>>;

    fn each_links(&self, query: &[T], handler: ReadHandler<T>) -> Flow;

    fn update_links(
        &mut self,
        query: &[T],
        change: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, LinksError<T>>;

    fn delete_links(
        &mut self,
        query: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, LinksError<T>>;
}

pub trait Doublets<T: LinkType>: Links<T> {
    fn count_by(&self, query: impl ToQuery<T>) -> T
    where
        Self: Sized,
    {
        self.count_links(&query.to_query()[..])
    }

    fn count(&self) -> T
    where
        Self: Sized,
    {
        self.count_by([])
    }

    fn create_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        let mut output = R::from_output(());
        let query = query.to_query();

        self.create_links(
            &query[..],
            &mut |before, after| match handler(before, after).branch() {
                ControlFlow::Continue(_) => Flow::Continue,
                ControlFlow::Break(residual) => {
                    output = R::from_residual(residual);
                    Flow::Break
                }
            },
        )
        .map(|_| output)
    }

    fn create_by(&mut self, query: impl ToQuery<T>) -> Result<T, LinksError<T>>
    where
        Self: Sized,
    {
        let mut index = default();
        self.create_by_with(query, |_before, link| {
            index = link.index;
            Flow::Continue
        })
        .map(|_| index)
    }

    fn create_with<F, R>(&mut self, handler: F) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        self.create_by_with([], handler)
    }

    fn create(&mut self) -> Result<T>
    where
        Self: Sized,
    {
        self.create_by([])
    }

    fn each_by<F, R>(&self, query: impl ToQuery<T>, mut handler: F) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        let mut output = R::from_output(());
        let query = query.to_query();

        self.each_links(&query[..], &mut |link| match handler(link).branch() {
            ControlFlow::Continue(_) => Flow::Continue,
            ControlFlow::Break(residual) => {
                output = R::from_residual(residual);
                Flow::Break
            }
        });

        output
    }

    fn each<F, R>(&self, handler: F) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        self.each_by([], handler)
    }

    fn update_by_with<H, R>(
        &mut self,
        query: impl ToQuery<T>,
        change: impl ToQuery<T>,
        mut handler: H,
    ) -> Result<R, LinksError<T>>
    where
        H: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        let mut output = R::from_output(());
        let query = query.to_query();
        let change = change.to_query();

        self.update_links(
            &query[..],
            &change[..],
            &mut |before, after| match handler(before, after).branch() {
                ControlFlow::Continue(_) => Flow::Continue,
                ControlFlow::Break(residual) => {
                    output = R::from_residual(residual);
                    Flow::Break
                }
            },
        )
        .map(|_| output)
    }

    fn update_by(&mut self, query: impl ToQuery<T>, change: impl ToQuery<T>) -> Result<T>
    where
        Self: Sized,
    {
        let mut result = default();
        self.update_by_with(query, change, |_, after| {
            result = after.index;
            Flow::Continue
        })
        .map(|_| result)
    }

    fn update_with<F, R>(
        &mut self,
        index: T,
        source: T,
        target: T,
        handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        self.update_by_with([index], [index, source, target], handler)
    }

    fn update(&mut self, index: T, source: T, target: T) -> Result<T>
    where
        Self: Sized,
    {
        self.update_by([index], [index, source, target])
    }

    fn delete_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        let mut output = R::from_output(());
        let query = query.to_query();

        self.delete_links(
            &query[..],
            &mut |before, after| match handler(before, after).branch() {
                ControlFlow::Continue(_) => Flow::Continue,
                ControlFlow::Break(residual) => {
                    output = R::from_residual(residual);
                    Flow::Break
                }
            },
        )
        .map(|_| output)
    }

    fn delete_by(&mut self, query: impl ToQuery<T>) -> Result<T>
    where
        Self: Sized,
    {
        let mut result = default();
        self.delete_by_with(query, |_before, after| {
            result = after.index;
            Flow::Continue
        })
        .map(|_| result)
    }

    fn delete_with<F, R>(&mut self, index: T, handler: F) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        self.delete_by_with([index], handler)
    }

    fn delete(&mut self, index: T) -> Result<T>
    where
        Self: Sized,
    {
        self.delete_by([index])
    }

    fn try_get_link(&self, index: T) -> Result<Link<T>, LinksError<T>>
    where
        Self: Sized,
    {
        self.get_link(index).ok_or(LinksError::NotExists(index))
    }

    fn get_link(&self, index: T) -> Option<Link<T>>
    where
        Self: Sized,
    {
        let constants = self.constants();
        if constants.is_external(index) {
            Some(Link::point(index))
        } else {
            let mut slice = None;
            self.each_by([index], |link| {
                slice = Some(link);
                Flow::Continue
            });
            slice
        }
    }

    fn delete_all(&mut self) -> Result<(), LinksError<T>>
    where
        Self: Sized,
    {
        // delete all links while self.count() != zero()
        let mut count = self.count();
        while count != zero() {
            self.delete(count)?;
            count = self.count();
        }
        Ok(())
    }

    fn delete_query_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        handler: F,
    ) -> Result<(), LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        let query = query.to_query();
        let len = self.count_by(query.to_query()).as_();
        let mut vec = Vec::with_capacity(len);

        self.each_by(query, |link| {
            vec.push(link.index);
            Flow::Continue
        });

        let mut handler = FuseHandler::new(handler);
        for index in vec.into_iter().rev() {
            self.delete_with(index, &mut handler)?;
        }
        Ok(())
    }

    fn delete_usages_with<F, R>(&mut self, index: T, handler: F) -> Result<(), LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        let any = self.constants().any;
        let mut to_delete = Vec::with_capacity(
            self.count_by([any, index, any]).as_() + self.count_by([any, any, index]).as_(),
        );
        self.each_by([any, index, any], |link| {
            if link.index != index {
                to_delete.push(link.index);
            }
            Flow::Continue
        });

        self.each_by([any, any, index], |link| {
            if link.index != index {
                to_delete.push(link.index);
            }
            Flow::Continue
        });

        let mut handler = FuseHandler::new(handler);
        for index in to_delete.into_iter().rev() {
            self.delete_with(index, &mut handler)?;
        }
        Ok(())
    }

    fn delete_usages(&mut self, index: T) -> Result<(), LinksError<T>>
    where
        Self: Sized,
    {
        self.delete_usages_with(index, |_, _| Flow::Continue)
    }

    fn create_point(&mut self) -> Result<T>
    where
        Self: Sized,
    {
        let new = self.create()?;
        self.update(new, new, new)
    }

    fn create_link_with<F, R>(
        &mut self,
        source: T,
        target: T,
        handler: F,
    ) -> Result<Flow, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        let mut new = default();
        let mut handler = FuseHandler::new(handler);
        self.create_with(|before, after| {
            new = after.index;
            handler(before, after);
            Flow::Continue
        })?;

        self.update_with(new, source, target, handler)
    }

    fn create_link(&mut self, source: T, target: T) -> Result<T>
    where
        Self: Sized,
    {
        let mut result = default();
        self.create_link_with(source, target, |_, link| {
            result = link.index;
            Flow::Continue
        })
        .map(|_| result)
    }

    fn found(&self, query: impl ToQuery<T>) -> bool
    where
        Self: Sized,
    {
        self.count_by(query) != zero()
    }

    fn find(&self, query: impl ToQuery<T>) -> Option<Link<T>>
    where
        Self: Sized,
    {
        let mut result = None;
        self.each_by(query, |link| {
            result = Some(link);
            Flow::Break
        });
        result
    }

    fn search(&self, source: T, target: T) -> Option<T>
    where
        Self: Sized,
    {
        self.find([self.constants().any, source, target])
            .map(|link| link.index)
    }

    fn single(&self, query: impl ToQuery<T>) -> Option<Link<T>>
    where
        Self: Sized,
    {
        let mut result = None;
        self.each_by(query, |link| {
            if result.is_none() {
                result = Some(link);
                Flow::Continue
            } else {
                result = None;
                Flow::Break
            }
        });
        result
    }

    fn get_or_create(&mut self, source: T, target: T) -> Result<T>
    where
        Self: Sized,
    {
        if let Some(link) = self.search(source, target) {
            Ok(link)
        } else {
            self.create_link(source, target)
        }
    }

    fn count_usages(&self, index: T) -> Result<T>
    where
        Self: Sized,
    {
        let constants = self.constants();
        let any = constants.any;

        let link = self.try_get_link(index)?;
        let mut usage_source = self.count_by([any, index, any]);
        if index == link.source {
            usage_source = usage_source - one();
        }

        let mut usage_target = self.count_by([any, any, index]);
        if index == link.target {
            usage_target = usage_target - one();
        }

        Ok(usage_source + usage_target)
    }

    fn usages(&self, index: T) -> Result<Vec<T>, LinksError<T>>
    where
        Self: Sized,
    {
        let any = self.constants().any;
        let mut usages = Vec::with_capacity(self.count_usages(index)?.as_());

        self.each_by([any, index, any], |link| {
            if link.index != index {
                usages.push(link.index);
            }
            Flow::Continue
        });

        self.each_by([any, any, index], |link| {
            if link.index != index {
                usages.push(link.index);
            }
            Flow::Continue
        });
        Ok(usages)
    }

    fn exist(&self, link: T) -> bool
    where
        Self: Sized,
    {
        let constants = self.constants();
        if constants.is_external(link) {
            true
        } else {
            constants.is_internal(link) && self.count_by([link]) != zero()
        }
    }

    fn has_usages(&self, link: T) -> bool
    where
        Self: Sized,
    {
        self.count_usages(link).map_or(false, |link| link != zero())
    }

    fn rebase_with<F, R>(&mut self, old: T, new: T, handler: F) -> Result<(), LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
        Self: Sized,
    {
        if old == new {
            return Ok(());
        }

        let any = self.constants().any;
        let as_source = [any, old, any];
        let as_target = [any, any, old];

        let sources_count: usize = self.count_by(as_source).as_();
        let targets_count: usize = self.count_by(as_target).as_();

        // not borrowed
        if sources_count + targets_count == 0 {
            return Ok(());
        }

        let mut handler = FuseHandler::new(handler);

        let mut usages = Vec::with_capacity(sources_count);
        self.each_by(as_source, |link| {
            usages.push(link);
            Flow::Continue
        });

        for usage in usages {
            if usage.index != old {
                self.update_with(usage.index, new, usage.target, &mut handler)?;
            }
        }

        let mut usages = Vec::with_capacity(sources_count);
        self.each_by(as_target, |link| {
            usages.push(link);
            Flow::Continue
        });

        for usage in usages {
            if usage.index != old {
                self.update_with(usage.index, usage.source, new, &mut handler)?;
            }
        }
        Ok(())
    }

    fn rebase(&mut self, old: T, new: T) -> Result<T>
    where
        Self: Sized,
    {
        let link = self.try_get_link(old)?;

        if old == new {
            return Ok(new);
        }

        let constants = self.constants();
        let any = constants.any;

        let sources_count = self.count_by([any, old, any]).as_();
        let targets_count = self.count_by([any, any, old]).as_();
        if sources_count == 0 && targets_count == 0 && link.is_full() {
            return Ok(new);
        }

        let total = sources_count + targets_count;
        if total == 0 {
            return Ok(new);
        }

        let mut usages = Vec::with_capacity(sources_count);
        self.each_by([any, old, any], |link| {
            usages.push(link.index);
            Flow::Continue
        });

        for index in usages {
            if index != old {
                let usage = self.try_get_link(index)?;
                self.update(index, new, usage.target)?;
            }
        }

        let mut usages = Vec::with_capacity(sources_count);
        self.each_by([any, any, old], |link| {
            usages.push(link.index);
            Flow::Continue
        });

        for index in usages {
            if index != old {
                let usage = self.try_get_link(index)?;
                self.update(index, usage.source, new)?;
            }
        }
        Ok(new)
    }

    fn rebase_and_delete(&mut self, old: T, new: T) -> Result<T>
    where
        Self: Sized,
    {
        if old == new {
            Ok(new)
        } else {
            self.rebase(old, new)?;
            self.delete(old)
        }
    }
}

impl<T: LinkType> Links<T> for Box<dyn Doublets<T>> {
    fn constants(&self) -> &LinksConstants<T> {
        (**self).constants()
    }

    fn count_links(&self, query: &[T]) -> T {
        (**self).count_links(query)
    }

    fn create_links(
        &mut self,
        query: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, LinksError<T>> {
        (**self).create_links(query, handler)
    }

    fn each_links(&self, query: &[T], handler: ReadHandler<T>) -> Flow {
        (**self).each_links(query, handler)
    }

    fn update_links(
        &mut self,
        query: &[T],
        change: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, LinksError<T>> {
        (**self).update_links(query, change, handler)
    }

    fn delete_links(
        &mut self,
        query: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, LinksError<T>> {
        (**self).delete_links(query, handler)
    }
}

impl<T: LinkType> Doublets<T> for Box<dyn Doublets<T>> {}
