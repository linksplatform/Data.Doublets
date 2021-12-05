use std::marker::PhantomData;

use crate::doublets::mem::ilinks_list_methods::ILinksListMethods;
use crate::doublets::mem::links_header::LinksHeader;
use crate::doublets::mem::united::generic::UpdatePointers;
use crate::doublets::mem::united::raw_link::RawLink;
use crate::methods::AbsoluteCircularDoublyLinkedList;
use crate::methods::AbsoluteDoublyLinkedListBase;
use crate::methods::DoublyLinkedListBase;
use crate::num::LinkType;

pub struct UnusedLinks<T: LinkType> {
    links: *mut u8,
    header: *mut u8,

    _phantom: PhantomData<T>,
}

impl<T: LinkType> UnusedLinks<T> {
    pub fn new(links: *mut u8, header: *mut u8) -> Self {
        assert!(!links.is_null()); // TODO: messages
        assert!(!header.is_null()); // TODO: messages
        Self { links, header, _phantom: Default::default() }
    }

    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { &*(self.header as *const LinksHeader<T>) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { &mut *(self.header as *mut LinksHeader<T>) }
    }

    fn get_link(&self, link: T) -> &RawLink<T> {
        unsafe { &*((self.links as *const RawLink<T>).add(link.as_())) }
    }

    fn get_mut_link(&mut self, link: T) -> &mut RawLink<T> {
        unsafe { &mut *((self.links as *mut RawLink<T>).add(link.as_())) }
    }
}

impl<T: LinkType> AbsoluteDoublyLinkedListBase<T> for UnusedLinks<T> {
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

impl<T: LinkType> DoublyLinkedListBase<T> for UnusedLinks<T> {
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

impl<T: LinkType> AbsoluteCircularDoublyLinkedList<T> for UnusedLinks<T> {}

impl<T: LinkType> UpdatePointers for UnusedLinks<T> {
    fn update_pointers(&mut self, links: *mut u8, header: *mut u8) {
        self.links = links;
        self.header = header;
    }
}

impl<T: LinkType> ILinksListMethods<T> for UnusedLinks<T> {
    fn detach(&mut self, link: T) {
        AbsoluteCircularDoublyLinkedList::detach(self, link)
    }

    fn attach_as_first(&mut self, link: T) {
        AbsoluteCircularDoublyLinkedList::attach_as_first(self, link)
    }
}
