use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;

use crate::doublets::{ILinks, ILinksExtensions};
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::num::LinkType;
use num_traits::zero;
use crate::doublets::decorators::UniqueResolver;

type Base<T, Links> = UniqueResolver<T, Links>;

pub struct CascadeUniqueResolver<T: LinkType, Links: ILinks<T>> {
    links: UniqueResolver<T, Links>,
}

impl<T: LinkType, Links: ILinks<T>> CascadeUniqueResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self { links: Base::with_resolver(links, Self::resolve_conflict) }
    }

    pub(in crate::doublets::decorators) fn resolve_conflict(links: &mut Links, old: T, new: T) -> T {
        links.rebase(old, new);
        Base::resolve_conflict(links, old, new)
    }
}

impl<T: LinkType, Links: ILinks<T>> IGenericLinks<T> for CascadeUniqueResolver<T, Links> {
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

    fn update_generic<Lr, Ls>(&mut self, restrictions: Lr, substitution: Ls) -> T
    where
        Lr: IntoIterator<Item=T, IntoIter: ExactSizeIterator>,
        Ls: IntoIterator<Item=T, IntoIter: ExactSizeIterator>
    {
        self.links.update_generic(restrictions, substitution)
    }

    fn delete_generic<L>(&mut self, restrictions: L)
    where
        L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>
    {
        self.links.delete_generic(restrictions)
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for CascadeUniqueResolver<T, Links> {}
