use std::{marker::PhantomData, mem::transmute, ptr::NonNull};

use crate::{
    mem::{
        header::LinksHeader, split::DataPart, traits::SplitList, unit::LinkPart, LinksList,
        SplitUpdateMem, UnitUpdateMem,
    },
    split::IndexPart,
};
use methods::{AbsoluteCircularLinkedList, AbsoluteLinkedList, LinkedList};
use num::LinkType;

pub struct UnusedLinks<T: LinkType> {
    links: NonNull<[DataPart<T>]>,
    header: NonNull<[IndexPart<T>]>,
}

impl<T: LinkType> UnusedLinks<T> {
    pub fn new(links: NonNull<[DataPart<T>]>, header: NonNull<[IndexPart<T>]>) -> Self {
        Self { links, header }
    }

    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { transmute(&self.header.as_ref()[0]) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { transmute(&mut self.header.as_mut()[0]) }
    }

    fn get_link(&self, link: T) -> &DataPart<T> {
        unsafe { &self.links.as_ref()[link.as_()] }
    }

    fn get_mut_link(&mut self, link: T) -> &mut DataPart<T> {
        unsafe { &mut self.links.as_mut()[link.as_()] }
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

impl<T: LinkType> SplitUpdateMem<T> for UnusedLinks<T> {
    fn update_mem(&mut self, data: NonNull<[DataPart<T>]>, index: NonNull<[IndexPart<T>]>) {
        self.links = data;
        self.header = index;
    }
}

impl<T: LinkType> LinksList<T> for UnusedLinks<T> {
    fn detach(&mut self, link: T) {
        AbsoluteCircularLinkedList::detach(self, link)
    }

    fn attach_as_first(&mut self, link: T) {
        AbsoluteCircularLinkedList::attach_as_first(self, link)
    }
}

impl<T: LinkType> SplitList<T> for UnusedLinks<T> {}
