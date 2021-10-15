use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;

use crate::doublets::{ILinks, ILinksExtensions};
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::num::LinkType;
use num_traits::zero;

pub struct UniqueValidator<T: LinkType, Links: ILinks<T>> {
    links: Links,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, Links: ILinks<T>> UniqueValidator<T, Links> {
    pub fn new(links: Links) -> Self {
        Self { links, _phantom: default() }
    }
}

impl<T: LinkType, Links: ILinks<T>> IGenericLinks<T> for UniqueValidator<T, Links> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_generic<L>(&self, restrictions: L) -> T
    where
        L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>
    {
        self.links.count_generic(restrictions)
    }

    fn each_generic<F, L>(&self, handler: F, restrictions: L) -> T
    where
        F: FnMut(&[T]) -> T,
        L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>
    {
        self.links.each_generic(handler, restrictions)
    }

    fn create_generic<L>(&mut self, restrictions: L) -> T
    where
        L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>
    {
        self.links.create_generic(restrictions)
    }

    fn delete_generic<L>(&mut self, restrictions: L)
    where
        L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>
    {
        self.links.delete_generic(restrictions)
    }


    fn update_generic<Lr, Ls>(&mut self, restrictions: Lr, substitution: Ls) -> T
    where
        Lr: IntoIterator<Item=T, IntoIter: ExactSizeIterator>,
        Ls: IntoIterator<Item=T, IntoIter: ExactSizeIterator>
    {
        let links = self.links.borrow_mut();
        let constants = links.constants();
        let restrictions: Vec<T> = restrictions.into_iter().collect();
        let substitution: Vec<T> = substitution.into_iter().collect();
        let index_part = constants.index_part.as_();
        let source_part = constants.source_part.as_();
        let target_part = constants.target_part.as_();

        let source = substitution[source_part];
        let target = substitution[target_part];

        // TODO: create extension for this
        let found = links.count_by([
            constants.any,
            source,
            target,
        ]);
        // TODO: use links formatter
        assert!(found != zero(), format!("link [{}->{}] already exists", source, target));
        links.update_generic(restrictions, substitution)
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for UniqueValidator<T, Links> {}
