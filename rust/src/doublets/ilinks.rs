use std::backtrace::Backtrace;
use std::default::default;
use std::ops::{ControlFlow, Try};
use std::result;

use num_traits::{one, zero};
use rand::{thread_rng, Rng};
use smallvec::SmallVec;

use crate::doublets::data::ToQuery;
use crate::doublets::data::{LinksConstants, Point, Query};
use crate::doublets::error::LinksError;
use crate::doublets::link::Link;
use crate::doublets::StoppedHandler;
use crate::doublets::{data, Doublet, Flow};
use crate::num::LinkType;
use crate::query;
use ControlFlow::{Break, Continue};

use async_trait::async_trait;

// use crate::doublets::decorators::{
//     CascadeUniqueResolver, CascadeUsagesResolver, NonNullDeletionResolver,
// };

pub type Result<T, E = LinksError<T>> = std::result::Result<T, E>;

fn IGNORE<T: LinkType>(_: Link<T>, _: Link<T>) -> Result<(), ()> {
    Err(())
}

#[async_trait]
pub trait ILinks<T: LinkType>: Sync + Send {
    fn constants(&self) -> LinksConstants<T>;

    async fn count_by(&self, query: impl ToQuery<T> + 'async_trait) -> T;

    async fn count(&self) -> T {
        self.count_by([]).await
    }

    async fn create_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T> + 'async_trait,
        handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send;

    async fn create_by(
        &mut self,
        query: impl ToQuery<T> + 'async_trait,
    ) -> Result<T, LinksError<T>> {
        let mut index = default();
        self.create_by_with(query, |before, link| {
            index = link.index;
            Flow::Continue
        })
        .await
        .map(|_| index)
    }

    async fn create_with<F, R>(&mut self, handler: F) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        self.create_by_with([], handler).await
    }

    async fn create(&mut self) -> Result<T> {
        self.create_by([]).await
    }

    async fn try_each_by<F, R>(
        &self,
        restrictions: impl ToQuery<T> + 'async_trait,
        handler: F,
    ) -> R
    where
        F: FnMut(Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send;

    async fn try_each<F, R>(&self, handler: F) -> R
    where
        F: FnMut(Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        self.try_each_by([], handler).await
    }

    async fn each_by<H>(&self, restrictions: impl ToQuery<T> + 'async_trait, mut handler: H) -> T
    where
        H: FnMut(Link<T>) -> T + Send,
    {
        let result = self
            .try_each_by(restrictions, |link| {
                let result = handler(link);
                if result == self.constants().r#continue {
                    Continue(())
                } else {
                    Break(result)
                }
            })
            .await;

        match result {
            Continue(_) => self.constants().r#continue,
            Break(result) => result,
        }
    }

    async fn each<H>(&self, handler: H) -> T
    where
        H: FnMut(Link<T>) -> T + Send,
    {
        self.each_by([], handler).await
    }

    async fn update_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T> + 'async_trait,
        replacement: impl ToQuery<T> + 'async_trait,
        handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send;

    async fn update_by(
        &mut self,
        query: impl ToQuery<T> + 'async_trait,
        replacement: impl ToQuery<T> + 'async_trait,
    ) -> Result<T> {
        let r#continue = self.constants().r#continue;
        let mut result = default();
        self.update_by_with(query, replacement, |before, after| {
            result = after.index;
            Flow::Continue
        })
        .await
        .map(|_| result)
    }

    async fn update_with<F, R>(
        &mut self,
        index: T,
        source: T,
        target: T,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        self.update_by_with([index], [index, source, target], handler)
            .await
    }

    async fn update(&mut self, index: T, source: T, target: T) -> Result<T> {
        self.update_by([index], [index, source, target]).await
    }

    async fn delete_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T> + 'async_trait,
        handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send;

    async fn delete_by(&mut self, query: impl ToQuery<T> + 'async_trait) -> Result<T> {
        let r#continue = self.constants().r#continue;
        let mut result = default();
        self.delete_by_with(query, |before, after| {
            result = after.index;
            Flow::Continue
        })
        .await
        .map(|_| result)
    }

    async fn delete_with<F, R>(&mut self, index: T, mut handler: F) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        self.delete_by_with([index], handler).await
    }

    async fn delete(&mut self, index: T) -> Result<T> {
        self.delete_by([index]).await
    }

    async fn try_get_link(&self, index: T) -> Result<Link<T>, LinksError<T>> {
        self.get_link(index)
            .await
            .ok_or(LinksError::NotExists(index))
    }

    async fn get_link(&self, index: T) -> Option<Link<T>> {
        let constants = self.constants();
        if constants.is_external_reference(index) {
            Some(Link::point(index))
        } else {
            let mut slice = None;
            self.each_by([index], |link| {
                slice = Some(link);
                constants.r#break
            })
            .await;
            slice
        }
    }

    async fn delete_all(&mut self) -> Result<(), LinksError<T>> {
        let mut count = self.count().await;
        while count > zero() {
            self.delete(count).await?;
            let sup_count = self.count().await;
            if sup_count != count - one() {
                count = sup_count
            } else {
                count = count - one()
            }
        }
        Ok(())
    }

    async fn delete_query_with<F, R>(
        &mut self,
        query: impl ToQuery<T> + 'async_trait,
        mut handler: F,
    ) -> Result<(), LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        let query = query.to_query();
        let constants = self.constants();
        let len = self.count_by(query.to_query()).await.as_();
        let mut vec = Vec::with_capacity(len);

        self.each_by(query, |link| {
            vec.push(link.index);
            constants.r#continue
        })
        .await;

        let mut handler = StoppedHandler::new(handler);
        for index in vec.into_iter().rev() {
            self.delete_with(index, &mut handler).await?;
        }
        Ok(())
    }

    async fn delete_usages_with<F, R>(
        &mut self,
        index: T,
        mut handler: F,
    ) -> Result<(), LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        let any = self.constants().any;
        let mut to_delete = Vec::with_capacity(
            self.count_by([any, index, any]).await.as_()
                + self.count_by([any, any, index]).await.as_(),
        );
        self.try_each_by([any, index, any], |link| {
            if link.index != index {
                to_delete.push(link.index);
            }
            Flow::Continue
        })
        .await;

        self.try_each_by([any, any, index], |link| {
            if link.index != index {
                to_delete.push(link.index);
            }
            Flow::Continue
        })
        .await;

        let mut handler = StoppedHandler::new(handler);
        for index in to_delete.into_iter().rev() {
            self.delete_with(index, &mut handler).await?;
        }
        Ok(())
    }

    async fn delete_usages(&mut self, index: T) -> Result<(), LinksError<T>> {
        self.delete_usages_with(index, IGNORE).await
    }

    async fn create_point(&mut self) -> Result<T> {
        let new = self.create().await?;
        self.update(new, new, new).await
    }

    #[deprecated(note = "use `create_link` instead")]
    async fn create_and_update(&mut self, source: T, target: T) -> Result<T> {
        self.create_link(source, target).await
    }

    async fn create_link_with<F, R>(
        &mut self,
        source: T,
        target: T,
        mut handler: F,
    ) -> Result<Flow, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        // todo macro?
        let mut new = default();
        let mut handler = StoppedHandler::new(handler);
        self.create_with(|before, after| {
            new = after.index;
            handler(before, after);
            Flow::Continue
        })
        .await?;

        self.update_with(new, source, target, handler).await
    }

    async fn create_link(&mut self, source: T, target: T) -> Result<T> {
        let mut result = default();
        self.create_link_with(source, target, |_, link| {
            result = link.index;
            Flow::Continue
        })
        .await
        .map(|_| result)
    }

    #[deprecated(note = "use `links.search(source, target).unwrap_or(or)`")]
    async fn search_or(&self, source: T, target: T, or: T) -> T {
        self.search(source, target).await.unwrap_or(or)
    }

    async fn found(&self, query: impl ToQuery<T> + 'async_trait) -> bool {
        self.count_by(query).await != zero()
    }

    async fn find(&self, query: impl ToQuery<T> + 'async_trait) -> Option<T> {
        let mut result = None;
        self.try_each_by(query, |link| {
            result = Some(link.index);
            Flow::Break
        })
        .await;
        result
    }

    async fn search(&self, source: T, target: T) -> Option<T> {
        self.find([self.constants().any, source, target]).await
    }

    async fn single(&self, query: impl ToQuery<T> + 'async_trait) -> Option<Link<T>> {
        let query = query.to_query();
        let constants = self.constants();

        let mut result = None;
        let mut marker = false;
        self.try_each_by(query, |link| {
            if !marker {
                result = Some(link);
                marker = true;
                Flow::Continue
            } else {
                result = None;
                Flow::Break
            }
        })
        .await;
        result
    }

    // TODO: use later `links.iter().map(|link| link.index).collect()`
    async fn all_indices(&self, query: impl ToQuery<T> + 'async_trait) -> Vec<T> {
        let query = query.to_query();
        let len = self.count_by(query.to_query()).await.as_();
        let mut vec = Vec::with_capacity(len);
        self.each_by(query, |link| {
            vec.push(link.index);
            self.constants().r#continue
        });
        vec
    }

    async fn get_or_create(&mut self, source: T, target: T) -> Result<T> {
        if let Some(link) = self.search(source, target).await {
            Ok(link)
        } else {
            self.create_and_update(source, target).await
        }
    }

    async fn count_usages(&self, index: T) -> T {
        let constants = self.constants();
        let any = constants.any;
        // TODO: expect
        let link = self.get_link(index).await.unwrap();
        let mut usage_source = self.count_by([any, index, any]).await;
        if index == link.source {
            usage_source = usage_source - one();
        }

        let mut usage_target = self.count_by([any, any, index]).await;
        if index == link.target {
            usage_target = usage_target - one();
        }

        usage_source + usage_target
    }

    async fn exist(&self, link: T) -> bool {
        let constants = self.constants();
        if constants.is_external_reference(link) {
            true
        } else {
            constants.is_internal_reference(link) && self.count_by([link]).await != zero()
        }
    }

    async fn has_usages(&self, link: T) -> bool {
        self.count_usages(link).await != zero()
    }

    async fn rebase_with<F, R>(
        &mut self,
        old: T,
        new: T,
        mut handler: F,
    ) -> Result<(), LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R + Send,
        R: Try<Output = (), Residual: Send> + Send,
    {
        if old == new {
            return Ok(());
        }

        let any = self.constants().any;
        let as_source = [any, old, any];
        let as_target = [any, any, old];

        let sources_count: usize = self.count_by(as_source).await.as_();
        let targets_count: usize = self.count_by(as_target).await.as_();

        // not borrowed
        if sources_count + targets_count == 0 {
            return Ok(());
        }

        let mut handler = StoppedHandler::new(handler);

        let mut usages = Vec::with_capacity(sources_count);
        self.try_each_by(as_source, |link| {
            usages.push(link);
            Flow::Continue
        })
        .await;

        for usage in usages {
            if usage.index != old {
                self.update_with(usage.index, new, usage.target, &mut handler)
                    .await?;
            }
        }

        let mut usages = Vec::with_capacity(sources_count);
        self.try_each_by(as_target, |link| {
            usages.push(link);
            Flow::Continue
        })
        .await;

        for usage in usages {
            if usage.index != old {
                self.update_with(usage.index, usage.source, new, &mut handler)
                    .await?;
            }
        }
        Ok(())
    }

    // TODO: old: `merge_usages`
    async fn rebase(&mut self, old: T, new: T) -> Result<T> {
        let link = self.try_get_link(old).await?;

        if old == new {
            return Ok(new);
        }

        let constants = self.constants();
        let any = constants.any;

        let sources_count = self.count_by([any, old, any]).await.as_();
        let targets_count = self.count_by([any, any, old]).await.as_();
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
            constants.r#continue
        })
        .await;

        for index in usages {
            if index != old {
                let usage = self.try_get_link(index).await?;
                self.update(index, new, usage.target).await?;
            }
        }

        let mut usages = Vec::with_capacity(sources_count);
        self.each_by([any, any, old], |link| {
            usages.push(link.index);
            constants.r#continue
        })
        .await;

        for index in usages {
            if index != old {
                let usage = self.try_get_link(index).await?;
                self.update(index, usage.source, new).await?;
            }
        }
        Ok(new)
    }

    async fn rebase_and_delete(&mut self, old: T, new: T) -> Result<T> {
        if old == new {
            Ok(new)
        } else {
            self.rebase(old, new).await?;
            self.delete(old).await
        }
    }

    async fn reset(&mut self, link: T) -> Result<T, LinksError<T>> {
        self.update(link, T::zero(), T::zero()).await
    }

    async fn format(&self, link: T) -> Option<String> {
        self.get_link(link).await.map(|link| link.to_string())
    }

    // async fn decorators_kit(
    //     self,
    // ) -> CascadeUniqueResolver<T, NonNullDeletionResolver<T, CascadeUsagesResolver<T, Self>>>
    // where
    //     Self: Sized,
    // {
    //     let links = self;
    //     let links = CascadeUsagesResolver::new(links);
    //     let links = NonNullDeletionResolver::new(links);
    //     CascadeUniqueResolver::new(links)
    // }

    #[deprecated(note = "use `links.try_get_link(...)?.is_full()`")]
    async fn is_full_point(&self, link: T) -> Option<bool> {
        self.get_link(link).await.map(|link| link.is_full())
    }

    #[deprecated(note = "use `links.try_get_link(...)?.is_partial()`")]
    async fn is_partial_point(&self, link: T) -> Option<bool> {
        self.get_link(link).await.map(|link| link.is_partial())
    }

    #[deprecated(note = "only development")]
    async fn continue_break(&self) -> (T, T) {
        (self.constants().r#continue, self.constants().r#break)
    }
}

#[deprecated(note = "use `ILinks`")]
pub trait ILinksExtensions<T: LinkType>: ILinks<T> {}

impl<T: LinkType, All: ILinks<T>> ILinksExtensions<T> for All {}
