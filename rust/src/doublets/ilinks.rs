use std::default::default;

use num_traits::{one, zero};
use smallvec::SmallVec;

use crate::doublets::data;
use crate::doublets::data::Point;
use crate::doublets::decorators::{
    CascadeUniqueResolver, CascadeUsagesResolver, NonNullDeletionResolver,
};
use crate::doublets::link::Link;
use crate::num::LinkType;

pub trait ILinks<T: LinkType>:
    data::IGenericLinks<T>/* + data::IGenericLinksExtensions<T>*/ + Sized
{
}

pub trait ILinksExtensions<T: LinkType>: ILinks<T> {
    fn count_by<L>(&self, restrictions: L) -> T
    where
        L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>,
    {
        self.count_generic(restrictions)
    }

    fn count(&self) -> T {
        self.count_generic([])
    }

    fn create(&mut self) -> T {
        self.create_generic([])
    }

    fn each_by<H, L>(&self, mut handler: H, restrictions: L) -> T
    where
        H: FnMut(Link<T>) -> T,
        L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>,
    {
        let constants = self.constants();
        let index = constants.index_part.as_();
        let source = constants.source_part.as_();
        let target = constants.target_part.as_();

        self.each_generic(
            |slice| handler(Link::new(slice[index], slice[source], slice[target])),
            restrictions,
        )
    }

    // TODO: maybe create `par_each`
    fn each<H>(&self, handler: H) -> T
    where
        H: FnMut(Link<T>) -> T,
    {
        self.each_by(handler, [])
    }

    fn update(&mut self, index: T, source: T, target: T) -> T {
        self.update_generic([index], [index, source, target])
    }

    fn delete(&mut self, index: T) -> T {
        // TODO: maybe change `delete_generic`
        self.delete_generic([index]);
        return index;
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
    fn delete_query<L: Clone>(&mut self, query: L)
    where
        L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>,
    {

        let constants = self.constants();
        // TODO: use `Link::LEN`
        let query: SmallVec<[T; 3]> = query.into_iter().collect();
        let len = self.count_by(query.clone()).as_();
        let mut vec: SmallVec<[T; 1024]> = SmallVec::with_capacity(len);

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
        return index;
    }

    fn get_or_create(&mut self, source: T, target: T) -> T {
        let link = self.search_or(source, target, zero());
        if link == zero() {
            self.create_and_update(source, target)
        } else {
            link
        }
    }

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
                    return constants.r#break;
                },
                [index],
            );
            return slice;
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

        return usage_source - usage_target;
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
        if sources_count == 0 && targets_count == 0 && Point::is_full(link.clone()) {
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

        let mut usages = Vec::with_capacity(targets_count);
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
        return new;
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
        let links = CascadeUniqueResolver::new(links);
        return links;
    }
}

impl<T: LinkType, All: ILinks<T>> ILinksExtensions<T> for All {}
