use std::{mem::transmute, ptr::NonNull};

use crate::{
    mem::{
        header::LinksHeader, traits::UnitList, unit::raw_link::LinkPart, LinksList, UnitUpdateMem,
    },
};
use methods::{AbsoluteCircularLinkedList, AbsoluteLinkedList, LinkedList};
use num::LinkType;

pub struct UnusedLinks<T: LinkType> {
    mem: NonNull<[LinkPart<T>]>,
}

impl<T: LinkType> UnusedLinks<T> {
    pub fn new(mem: NonNull<[LinkPart<T>]>) -> Self {
        Self { mem }
    }

    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { transmute(&self.mem.as_ref()[0]) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { transmute(&mut self.mem.as_mut()[0]) }
    }

    fn get_link(&self, link: T) -> &LinkPart<T> {
        unsafe { &self.mem.as_ref()[link.as_()] }
    }

    fn get_mut_link(&mut self, link: T) -> &mut LinkPart<T> {
        unsafe { &mut self.mem.as_mut()[link.as_()] }
    }
}

impl<T: LinkType> AbsoluteLinkedList<T> for UnusedLinks<T> {
    fn get_first(&self) -> T {
        self.get_header().first_free
    }

    fn get_last(&self) -> T {
        self.get_header().last_free
    }

    fn get_size(&self) -> T {
        self.get_header().free
    }

    fn set_first(&mut self, element: T) {
        self.get_mut_header().first_free = element
    }

    fn set_last(&mut self, element: T) {
        self.get_mut_header().last_free = element
    }

    fn set_size(&mut self, size: T) {
        self.get_mut_header().free = size
    }
}

impl<T: LinkType> LinkedList<T> for UnusedLinks<T> {
    fn get_previous(&self, element: T) -> T {
        self.get_link(element).source
    }

    fn get_next(&self, element: T) -> T {
        self.get_link(element).target
    }

    fn set_previous(&mut self, element: T, previous: T) {
        self.get_mut_link(element).source = previous
    }

    fn set_next(&mut self, element: T, next: T) {
        self.get_mut_link(element).target = next
    }
}

impl<T: LinkType> AbsoluteCircularLinkedList<T> for UnusedLinks<T> {}

impl<T: LinkType> LinksList<T> for UnusedLinks<T> {
    fn detach(&mut self, link: T) {
        AbsoluteCircularLinkedList::detach(self, link)
    }

    fn attach_as_first(&mut self, link: T) {
        AbsoluteCircularLinkedList::attach_as_first(self, link)
    }
}

impl<T: LinkType> UnitUpdateMem<T> for UnusedLinks<T> {
    fn update_mem(&mut self, mem: NonNull<[LinkPart<T>]>) {
        self.mem = mem;
    }
}

impl<T: LinkType> UnitList<T> for UnusedLinks<T> {}
