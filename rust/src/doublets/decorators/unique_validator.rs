use std::borrow::BorrowMut;
use std::default::default;
use std::marker::PhantomData;

use num_traits::zero;
use smallvec::SmallVec;

use crate::doublets::{Doublet, ILinks, ILinksExtensions, Link, LinksError, Result};
use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions, LinksConstants};
use crate::num::LinkType;

pub struct UniqueValidator<T: LinkType, Links: ILinks<T>> {
    links: Links,

    _phantom: PhantomData<T>,
}

impl<T: LinkType, Links: ILinks<T>> UniqueValidator<T, Links> {
    pub fn new(links: Links) -> Self {
        Self { links, _phantom: default() }
    }
}

impl<T: LinkType, Links: ILinks<T>> ILinks<T> for UniqueValidator<T, Links> {
    fn constants(&self) -> LinksConstants<T> {
        self.links.constants()
    }

    fn count_by<const L: usize>(&self, restrictions: [T; L]) -> T {
        self.links.count_by(restrictions)
    }

    fn create(&mut self) -> Result<T> {
        self.links.create()
    }

    fn each_by<H, const L: usize>(&self, handler: H, restrictions: [T; L]) -> T
        where H: FnMut(Link<T>) -> T {
        self.links.each_by(handler, restrictions)
    }

    fn update(&mut self, index: T, source: T, target: T) -> Result<T> {
        let links = self.links.borrow_mut();
        let constants = links.constants();
        // TODO: create extension for this
        let found = links.count_by([
            constants.any,
            source,
            target,
        ]);
        if !found.is_zero() {
            Err(LinksError::AlreadyExists(
                Doublet::new(source, target)
            ))
        } else {
            links.update(index, source, target)
        }
    }

    fn delete(&mut self, index: T) -> Result<T> {
        self.links.delete(index)
    }
}
