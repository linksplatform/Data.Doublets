use std::backtrace::Backtrace;
use std::default::default;
use std::ops::{ControlFlow, Try};
use std::result;

use num_traits::{one, zero};
use rand::{thread_rng, Rng};
use smallvec::SmallVec;

use crate::doublets::data::{query, LinksConstants, Point, Query};
//use crate::doublets::decorators::{
//    CascadeUniqueResolver, CascadeUsagesResolver, NonNullDeletionResolver,
//};
use crate::doublets::error::LinksError;
use crate::doublets::link::Link;
use crate::doublets::{data, Doublet};
use crate::num::LinkType;
use crate::query;

pub type Result<T, E = LinksError<T>> = std::result::Result<T, E>;

pub trait ILinks<T: LinkType>: Sized {
    fn constants(&self) -> LinksConstants<T>;

    fn count_by(&self, query: Query<'_, T>) -> T;

    fn create(&mut self) -> Result<T>;

    fn each_by<H>(&self, mut handler: H, restrictions: Query<'_, T>) -> T
    where
        H: FnMut(Link<T>) -> T,
    {
        let result = self.try_each_by(
            |link| {
                if handler(link) == self.constants().r#continue {
                    ControlFlow::Continue(())
                } else {
                    ControlFlow::Break(())
                }
            },
            restrictions,
        );

        match result {
            ControlFlow::Continue(_) => self.constants().r#continue,
            ControlFlow::Break(_) => self.constants().r#break,
        }
    }

    fn try_each_by<F, R>(&self, handler: F, restrictions: Query<'_, T>) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>;

    fn update(&mut self, index: T, source: T, target: T) -> Result<T>;

    fn delete(&mut self, index: T) -> Result<T>;

    fn try_get_link(&self, index: T) -> Result<Link<T>, LinksError<T>> {
        self.get_link(index).ok_or(LinksError::NotExists(index))
    }

    fn get_link(&self, index: T) -> Option<Link<T>> {
        let constants = self.constants();
        if constants.is_external_reference(index) {
            Some(Link::point(index))
        } else {
            let mut slice = None;
            self.each_by(
                |link| {
                    slice = Some(link);
                    constants.r#break
                },
                query![index],
            );
            slice
        }
    }

    fn count(&self) -> T {
        self.count_by(query![])
    }

    // TODO: maybe create `par_each`
    fn each<H>(&self, handler: H) -> T
    where
        H: FnMut(Link<T>) -> T,
    {
        self.each_by(handler, query![])
    }

    fn try_each<F, R>(&self, handler: F) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.try_each_by(handler, query![])
    }

    fn delete_all(&mut self) -> Result<(), LinksError<T>> {
        let mut count = self.count();
        while count > zero() {
            self.delete(count)?;
            let sup_count = self.count();
            if sup_count != count - one() {
                count = sup_count
            } else {
                count = count - one()
            }
        }
        Ok(())
    }

    fn delete_query(&mut self, query: Query<'_, T>) -> Result<(), LinksError<T>> {
        let constants = self.constants();
        let len = self.count_by(query.clone()).as_();
        let mut vec = Vec::with_capacity(len);

        self.each_by(
            |link| {
                vec.push(link.index);
                constants.r#continue
            },
            query,
        );

        for index in vec.into_iter().rev() {
            self.delete(index)?;
        }
        Ok(())
    }

    // TODO: Temporary implementation
    fn delete_usages(&mut self, index: T) -> Result<(), LinksError<T>> {
        let any = self.constants().any;
        let mut to_delete = vec![];
        self.each_by(
            |link| {
                if link.index != index {
                    to_delete.push(link.index);
                }
                self.constants().r#continue
            },
            query![any, index, any],
        );

        self.each_by(
            |link| {
                if link.index != index {
                    to_delete.push(link.index);
                }
                self.constants().r#continue
            },
            query![any, any, index],
        );

        for link in to_delete.into_iter().rev() {
            self.delete(link)?;
        }
        Ok(())
    }

    fn create_point(&mut self) -> Result<T> {
        let new = self.create()?;
        self.update(new, new, new)
    }

    fn create_and_update(&mut self, source: T, target: T) -> Result<T> {
        let new = self.create()?;
        self.update(new, source, target)
    }

    #[deprecated(note = "use `links.search(source, target).unwrap_or(or)`")]
    fn search_or(&self, source: T, target: T, or: T) -> T {
        self.search(source, target).unwrap_or(or)
    }

    fn search(&self, source: T, target: T) -> Option<T> {
        let constants = self.constants();
        let mut index = None;
        self.each_by(
            |link| {
                index = Some(link.index);
                T::zero()
            },
            query![constants.any, source, target],
        );
        index
    }

    fn single(&self, query: Query<'_, T>) -> Option<Link<T>> {
        let constants = self.constants();
        let r#break = constants.r#break;
        let r#continue = constants.r#continue;

        let mut result = None;
        let mut marker = false;
        self.each_by(
            |link| {
                if !marker {
                    result = Some(link);
                    marker = true;
                    r#continue
                } else {
                    result = None;
                    r#break
                }
            },
            query,
        );
        result
    }

    // TODO: use later `links.iter().map(|link| link.index).collect()`
    fn all_indices(&self, query: Query<'_, T>) -> Vec<T> {
        let len = self.count_by(Query::new(&query[..])).as_();
        let mut vec = Vec::with_capacity(len);
        self.each_by(
            |link| {
                vec.push(link.index);
                self.constants().r#continue
            },
            query,
        );
        vec
    }

    fn get_or_create(&mut self, source: T, target: T) -> Result<T> {
        if let Some(link) = self.search(source, target) {
            Ok(link)
        } else {
            self.create_and_update(source, target)
        }
    }

    fn count_usages(&self, index: T) -> T {
        let constants = self.constants();
        let any = constants.any;
        // TODO: expect
        let link = self.get_link(index).unwrap();
        let mut usage_source = self.count_by(Query::new(&[any, index, any][..]));
        if index == link.source {
            usage_source = usage_source - one();
        }

        let mut usage_target = self.count_by(Query::new(&[any, any, index][..]));
        if index == link.target {
            usage_target = usage_target - one();
        }

        usage_source - usage_target
    }

    fn exist(&self, link: T) -> bool {
        let constants = self.constants();
        if constants.is_external_reference(link) {
            true
        } else {
            constants.is_internal_reference(link)
                && self.count_by(Query::new(&[link][..])) != zero()
        }
    }

    fn has_usages(&self, link: T) -> bool {
        self.count_usages(link) != zero()
    }

    // TODO: old: `merge_usages`
    fn rebase(&mut self, old: T, new: T) -> Result<T> {
        let link = self.try_get_link(old)?;

        if old == new {
            return Ok(new);
        }

        let constants = self.constants();
        let any = constants.any;

        let sources_count = self.count_by(Query::new(&[any, old, any][..])).as_();
        let targets_count = self.count_by(Query::new(&[any, any, old][..])).as_();
        if sources_count == 0 && targets_count == 0 && Point::is_full(link) {
            return Ok(new);
        }

        let total = sources_count + targets_count;
        if total == 0 {
            return Ok(new);
        }

        let mut usages = Vec::with_capacity(sources_count);
        self.each_by(
            |link| {
                usages.push(link.index);
                constants.r#continue
            },
            query![any, old, any],
        );

        for index in usages {
            if index != old {
                let usage = self.try_get_link(index)?;
                self.update(index, new, usage.target)?;
            }
        }

        let mut usages = Vec::with_capacity(sources_count);
        self.each_by(
            |link| {
                usages.push(link.index);
                constants.r#continue
            },
            query![any, any, old],
        );

        for index in usages {
            if index != old {
                let usage = self.try_get_link(index)?;
                self.update(index, usage.source, new)?;
            }
        }
        Ok(new)
    }

    fn rebase_and_delete(&mut self, old: T, new: T) -> Result<T> {
        if old == new {
            Ok(new)
        } else {
            self.rebase(old, new)?;
            self.delete(old)
        }
    }

    fn reset(&mut self, link: T) -> Result<T, LinksError<T>> {
        // let null = self.constants().null; // TODO: assert null == 0
        self.update(link, T::zero(), T::zero())
    }

    fn format(&self, link: T) -> Option<String> {
        self.get_link(link).map(|link| link.to_string())
    }

    //fn decorators_kit(
    //    self,
    //) -> CascadeUniqueResolver<T, NonNullDeletionResolver<T, CascadeUsagesResolver<T, Self>>> {
    //    let links = self;
    //    let links = CascadeUsagesResolver::new(links);
    //    let links = NonNullDeletionResolver::new(links);
    //    CascadeUniqueResolver::new(links)
    //}

    #[deprecated(note = "use `links.try_get_link(...)?.is_full()`")]
    fn is_full_point(&self, link: T) -> Option<bool> {
        self.get_link(link).map(|link| link.is_full())
    }

    #[deprecated(note = "use `links.try_get_link(...)?.is_partial()`")]
    fn is_partial_point(&self, link: T) -> Option<bool> {
        self.get_link(link).map(|link| link.is_partial())
    }
}

#[deprecated(note = "use `ILinks`")]
pub trait ILinksExtensions<T: LinkType>: ILinks<T> {}

impl<T: LinkType, All: ILinks<T>> ILinksExtensions<T> for All {}
