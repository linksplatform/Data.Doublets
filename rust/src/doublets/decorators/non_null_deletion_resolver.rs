use crate::doublets::{ILinks, ILinksExtensions};
use crate::num::LinkType;
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;
use num_traits::zero;

pub struct NonNullDeletionResolver<T: LinkType, Links: ILinks<T>> {
    links: Links,

    _phantom: PhantomData<T>
}

impl<T: LinkType, Links: ILinks<T>> NonNullDeletionResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self { links, _phantom: default() }
    }
}

impl<T: LinkType, Links: ILinks<T>> IGenericLinks<T> for NonNullDeletionResolver<T, Links> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_generic<L>(&self, restrictions: L) -> T
    where
        L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
    {
        self.links.count_generic(restrictions)
    }

    fn each_generic<F, L>(&self, handler: F, restrictions: L) -> T
    where
        F: FnMut(&[T]) -> T,
        L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
    {
        self.links.each_generic(handler, restrictions)
    }

    fn create_generic<L>(&mut self, restrictions: L) -> T
    where
        L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
    {
        self.links.create_generic(restrictions)
    }

    fn update_generic<Lr, Ls>(&mut self, restrictions: Lr, substitution: Ls) -> T
    where
        Lr: IntoIterator<Item = T, IntoIter: ExactSizeIterator>,
            Ls: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
    {
        self.links.update_generic(restrictions, substitution)
    }

    fn delete_generic<L>(&mut self, restrictions: L)
        where
            L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
    {
        let restrictions: Vec<T> = restrictions.into_iter().collect();
        let constants = self.constants();
        let index = restrictions[constants.index_part.as_()];
        let null = constants.null;
        self.links.update(index, null, null);
        self.links.delete(index);
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for NonNullDeletionResolver<T, Links> { }
