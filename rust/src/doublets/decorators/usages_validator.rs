use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;

use num_traits::zero;
use smallvec::SmallVec;

use crate::doublets::{ILinks, ILinksExtensions, Link, LinksError, Result};
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::num::LinkType;

pub struct UsagesValidator<T: LinkType, Links: ILinks<T>> {
    links: Links,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, Links: ILinks<T>> UsagesValidator<T, Links> {
    pub fn new(links: Links) -> Self {
        Self {
            links,
            _phantom: default(),
        }
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for UsagesValidator<T, Links> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by<const L: usize>(&self, restrictions: [T; L]) -> T {
        self.links.count_by(restrictions)
    }

    fn create(&mut self) -> Result<T>  {
        self.links.create()
    }

    fn each_by<H, const L: usize>(&self, handler: H, restrictions: [T; L]) -> T
        where
            H: FnMut(Link<T>) -> T,
    {
        self.links.each_by(handler, restrictions)
    }

    fn update(&mut self, index: T, source: T, target: T) -> Result<T>  {
        let links = self.links.borrow_mut();
        if links.has_usages(index) {
            Err(LinksError::HasDeps(links.get_link(index).unwrap()))
        } else {
            links.update(index, source, target)
        }
    }

    fn delete(&mut self, index: T) -> Result<T> {
        let links = self.links.borrow_mut();
        if links.has_usages(index) {
            Err(LinksError::HasDeps(links.get_link(index).unwrap()))
        } else {
            links.delete(index)
        }
    }
}
