use std::default::default;

use num_traits::{one, zero};
use smallvec::SmallVec;

use crate::doublets::data;
use crate::doublets::data::{LinksConstants, Point};
use crate::doublets::decorators::{
    CascadeUniqueResolver, CascadeUsagesResolver, NonNullDeletionResolver,
};
use crate::doublets::link::Link;
use crate::num::LinkType;

pub trait ILinks<T: LinkType>:
/*data::IGenericLinks<T> + data::IGenericLinksExtensions<T> + */ Sized
{
    fn constants(&self) -> LinksConstants<T>;

    fn count_by<const L: usize>(&self, restrictions: [T; L]) -> T;

    fn create(&mut self) -> T;

    fn each_by<H, const L: usize>(&self, handler: H, restrictions: [T; L]) -> T
        where
            H: FnMut(Link<T>) -> T;

    fn update(&mut self, index: T, source: T, target: T) -> T;

    fn delete(&mut self, index: T) -> T;

    fn get_link(&self, index: T) -> Option<Link<T>> {
        // TODO: compare performance
        // self.get_generic_link(index).map(|link| link.collect())

        // TODO: Use cfg macro
        let constants = self.constants();
        if constants.is_external_reference(index) {
            Some(Link::from_once(index))
        } else {
            let mut slice = None;
            self.each_by(
                |link| {
                    slice = Some(link);
                    constants.r#break
                },
                [index],
            );
            slice
        }
    }
}

pub trait ILinksExtensions<T: LinkType>: ILinks<T> {
    fn count(&self) -> T {
        self.count_by([])
    }

    // TODO: maybe create `par_each`
    fn each<H>(&self, handler: H) -> T
        where
            H: FnMut(Link<T>) -> T,
    {
        self.each_by(handler, [])
    }

    fn delete_all(&mut self) {
        let mut count = self.count();
        while count > zero() {
            self.delete(count);
            let sup_count = self.count();
            if sup_count != count - one() {
                count = sup_count
            } else {
                count = count - one()
            }
        }
    }

    // TODO: use .del().query()
    // TODO: maybe `query: Link<T>`
    fn delete_query<const L: usize>(&mut self, query: [T; L]) {
        let constants = self.constants();
        let len = self.count_by(query).as_();
        let mut vec = Vec::with_capacity(len);

        self.each_by(
            |link| {
                vec.push(link.index);
                constants.r#continue
            },
            query,
        );

        for index in vec.into_iter().rev() {
            self.delete(index);
        }
    }

    fn delete_usages(&mut self, index: T) {
        // TODO: Bug
        //  let any = self.constants().any;
        //  self.delete_query([any, index, any]);
        //  self.delete_query([any, any, index]);

        let any = self.constants().any;
        self.delete_query([any, index, any]);
        self.delete_query([any, any, index]);
    }

    fn create_point(&mut self) -> T {
        let new = self.create();
        self.update(new, new, new)
    }

    fn create_and_update(&mut self, source: T, target: T) -> T {
        let new = self.create();
        self.update(new, source, target)
    }

    // TODO: use `fn search(&self, source: T, target: T) -> Option<T>`
    //  then `let result = links.search(source, target).unwrap_or(or)`
    fn search_or(&self, source: T, target: T, or: T) -> T {
        let constants = self.constants();
        let mut index = or;
        self.each_by(
            |link| {
                index = link.index;
                constants.r#break
            },
            [constants.any, source, target],
        );
        index
    }

    fn single<const L: usize>(&self, query: [T; L]) -> Option<Link<T>> {
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

    fn all_indices<const L: usize>(&self, query: [T; L]) -> Vec<T> {
        let len = self.count_by(query).as_();
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

    fn get_or_create(&mut self, source: T, target: T) -> T {
        let link = self.search_or(source, target, zero());
        if link == zero() {
            self.create_and_update(source, target)
        } else {
            link
        }
    }

    fn count_usages(&self, index: T) -> T {
        let constants = self.constants();
        let any = constants.any;
        // TODO: expect
        let link = self.get_link(index).unwrap();
        let mut usage_source = self.count_by([any, index, any]);
        if index == link.source {
            usage_source = usage_source - one();
        }

        let mut usage_target = self.count_by([any, any, index]);
        if index == link.target {
            usage_target = usage_target - one();
        }

        usage_source - usage_target
    }

    fn exist(&self, link: T) -> bool {
        let constants = self.constants();
        if constants.is_external_reference(link) {
            self.count_by([link]) != zero()
        } else {
            constants.is_internal_reference(link)
        }
    }

    fn has_usages(&self, link: T) -> bool {
        self.count_usages(link) != zero()
    }

    // TODO: old: `merge_usages`
    fn rebase(&mut self, old: T, new: T) -> T {
        let link = self.get_link(old);
        assert!(link.is_some(), "link [{}] does not exist", old);

        if old == new {
            return new;
        }

        let constants = self.constants();
        let any = constants.any;

        let sources_count = self.count_by([any, old, any]).as_();
        let targets_count = self.count_by([any, any, old]).as_();
        let link = link.unwrap();
        if sources_count == 0 && targets_count == 0 && Point::is_full(link) {
            return new;
        }

        let total = sources_count + targets_count;
        if total == zero() {
            return new;
        }

        let mut usages = Vec::with_capacity(sources_count);
        self.each_by(
            |link| {
                usages.push(link.index);
                constants.r#continue
            },
            [any, old, any],
        );

        // TODO: maybe `unwrap_unchecked()`
        for index in usages {
            if index != old {
                let usage = self.get_link(index).unwrap(); // TODO: 100% some
                self.update(index, new, usage.target);
            }
        }

        let mut usages = SmallVec::<[T; 1024]>::with_capacity(sources_count);
        self.each_by(
            |link| {
                usages.push(link.index);
                constants.r#continue
            },
            [any, any, old],
        );

        for index in usages {
            if index != old {
                let usage = self.get_link(index).unwrap(); // TODO: 100% some
                self.update(index, usage.source, new);
            }
        }
        new
    }

    fn rebase_and_delete(&mut self, old: T, new: T) -> T {
        if old == new {
            new
        } else {
            self.rebase(old, new);
            self.delete(old)
        }
    }

    fn reset(&mut self, link: T) {
        let null = self.constants().null;
        self.update(link, null, null);
    }

    fn format(&self, link: T) -> Option<String> {
        self.get_link(link).map(|link| link.to_string())
    }

    fn decorators_kit(
        self,
    ) -> CascadeUniqueResolver<T, NonNullDeletionResolver<T, CascadeUsagesResolver<T, Self>>> {
        let links = self;
        let links = CascadeUsagesResolver::new(links);
        let links = NonNullDeletionResolver::new(links);
        CascadeUniqueResolver::new(links)
    }

    fn is_full_point(&self, link: T) -> bool {
        let constants = self.constants();
        if constants.is_external_reference(link) {
            true
        } else {
            debug_assert!(self.exist(link));
            // TODO:
            // Point::is_full(self.get_link(link).unwrap())
            let link = self.get_link(link).unwrap();
            link.index == link.target && link.index == link.source
        }
    }

    fn is_partial_point(&self, link: T) -> bool {
        let constants = self.constants();
        if constants.is_external_reference(link) {
            true
        } else {
            debug_assert!(self.exist(link));
            // TODO:
            // Point::is_partial(self.get_link(link).unwrap())
            let link = self.get_link(link).unwrap();
            link.index == link.target || link.index == link.source
        }
    }
}

impl<T: LinkType, All: ILinks<T>> ILinksExtensions<T> for All {}
