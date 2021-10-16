use crate::doublets::{ILinks, ILinksExtensions};
use crate::num::LinkType;
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;
use libc::read;

pub struct UniqueResolver<T: LinkType, Links: ILinks<T>> {
    links: Links,
    resolver: fn(&mut Links, old: T, new: T) -> T,
}

impl<T: LinkType, Links: ILinks<T>> UniqueResolver<T, Links> {
    pub fn new(links: Links) -> Self {
        Self::with_resolver(links, Self::resolve_conflict)
    }

    pub fn with_resolver(links: Links, resolver: fn(&mut Links, old: T, new: T) -> T) -> Self {
        Self { links, resolver }
    }

    pub(in crate::doublets::decorators) fn resolve_conflict(links: &mut Links, old: T, new: T) -> T {
        if old != new && links.exist(old) {
            links.delete(old);
        }
        return new;
    }
}

impl<T: LinkType, Links: ILinks<T>> IGenericLinks<T> for UniqueResolver<T, Links> {
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

    fn delete_generic<L>(&mut self, restrictions: L)
    where
        L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
    {
        self.links.delete_generic(restrictions)
    }


    fn update_generic<Lr, Ls>(&mut self, restrictions: Lr, substitution: Ls) -> T
    where
        Lr: IntoIterator<Item = T, IntoIter: ExactSizeIterator>,
        Ls: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
    {

        // TODO: later use overloading style


        let links = self.links.borrow_mut();
        let constants = links.constants();
        let restrictions: Vec<T> = restrictions.into_iter().collect();
        let substitution: Vec<T> = substitution.into_iter().collect();
        let index_part = constants.index_part.as_();
        let source_part = constants.source_part.as_();
        let target_part = constants.target_part.as_();
        let new = links.search_or(
            substitution[source_part],
            substitution[target_part],
            default(),
        );

        if new == default() {
            links.update_generic(restrictions, substitution)
        } else {
            (self.resolver)(links, restrictions[index_part], new)
        }
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for UniqueResolver<T, Links> { }
