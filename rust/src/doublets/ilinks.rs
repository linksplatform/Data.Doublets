use std::default::default;

use num_traits::{one, zero};

use crate::doublets::data;
use crate::doublets::link::Link;
use crate::num::LinkType;

pub trait ILinks<T: LinkType>: data::IGenericLinks<T> + data::IGenericLinksExtensions<T> {}

pub trait ILinksExtensions<T: LinkType>: ILinks<T> {
    fn count_by<L>(&self, restrictions: L) -> T
    where
        L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>,
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
        L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>,
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

    fn delete(&mut self, index: T) {
        self.delete_generic([index])
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
    fn delete_query<L>(&mut self, query: L)
    where
        L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>
    {
        let query: Vec<T> = query.into_iter().collect();
        let len = self.count_by(query.clone()).as_();
        let mut vec = Vec::with_capacity(len);
        let constants = self.constants();
        self.each_by(|link| {
            vec.push(link.index);
            constants.r#continue
        }, query);

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
                return constants.r#break;
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
        self.get_generic_link(index).map(|link| link.collect())
    }

    fn count_usages(&self, index: T) -> T {
        let constants = self.constants();
        let any = constants.any;
        // TODO: expect
        let link = self.get_link(index).unwrap();

        let mut usage_source = self.count_by([any, link.index, any]);
        if link.index == link.source {
            usage_source = usage_source - one();
        }

        let mut usage_target = self.count_by([any, any, link.target]);
        if link.index == link.target {
            usage_target = usage_target - one();
        }

        return usage_source - usage_target
    }

    fn has_usages(&self, link: T) -> bool {
        self.count_usages(link) > zero()
    }

    fn reset(&mut self, link: T) {
        let null = self.constants().null;
        self.update(link, null, null);
    }

    fn format(&self, link: T) -> String {
        // TODO: expect
        self.get_link(link).unwrap().to_string()
    }
}

impl<T: LinkType, All: ILinks<T>> ILinksExtensions<T> for All {}
