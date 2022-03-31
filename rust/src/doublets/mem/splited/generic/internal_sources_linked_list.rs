use crate::data::LinksConstants;
use crate::doublets::Link;
use std::marker::PhantomData;
use std::ops::Try;

use crate::doublets::mem::ilinks_list_methods::ILinksListMethods;
use crate::doublets::mem::links_header::LinksHeader;
use crate::doublets::mem::splited::{DataPart, IndexPart};
use crate::doublets::mem::united::generic::UpdatePointers;
use crate::doublets::mem::united::UpdatePointersSplit;
use crate::methods::{
    DoublyLinkedListBase, RelativeCircularDoublyLinkedList, RelativeDoublyLinkedListBase,
};
use crate::num::LinkType;

pub struct InternalSourcesLinkedList<T: LinkType> {
    data: *mut u8,
    indexes: *mut u8,
    header: *mut u8,
    r#continue: T,
    r#break: T,
}

impl<T: LinkType> InternalSourcesLinkedList<T> {
    pub fn new(
        constants: LinksConstants<T>,
        data: *mut u8,
        indexes: *mut u8,
        header: *mut u8,
    ) -> Self {
        assert!(!data.is_null()); // TODO: messages
        assert!(!indexes.is_null()); // TODO: messages
        assert!(!header.is_null()); // TODO: messages
        Self {
            data,
            indexes,
            header,
            r#continue: constants.r#continue,
            r#break: constants.r#break,
        }
    }

    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { &*(self.header as *const LinksHeader<T>) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { &mut *(self.header as *mut LinksHeader<T>) }
    }

    fn get_data_part(&self, link: T) -> &DataPart<T> {
        unsafe { &*((self.data as *const DataPart<T>).add(link.as_())) }
    }

    fn get_mut_data_part(&self, link: T) -> &mut DataPart<T> {
        unsafe { &mut *((self.data as *mut DataPart<T>).add(link.as_())) }
    }

    fn get_index_part(&self, link: T) -> &IndexPart<T> {
        unsafe { &*((self.indexes as *const IndexPart<T>).add(link.as_())) }
    }

    fn get_mut_index_part(&self, link: T) -> &mut IndexPart<T> {
        unsafe { &mut *((self.indexes as *mut IndexPart<T>).add(link.as_())) }
    }

    fn get_link_value(&self, link: T) -> Link<T> {
        let data = self.get_data_part(link);
        Link::new(link, data.source, data.target)
    }

    pub fn count_usages(&self, head: T) -> T {
        self.get_size(head)
    }

    pub fn each_usages<R: Try<Output = ()>, H: FnMut(Link<T>) -> R>(
        &self,
        source: T,
        handler: &mut H,
    ) -> R {
        let mut current = self.get_first(source);
        let first = current;

        while !current.is_zero() {
            handler(self.get_link_value(current))?;
            current = self.get_next(current);
            if current == first {
                return R::from_output(());
            }
        }
        R::from_output(())
    }
}

impl<T: LinkType> RelativeDoublyLinkedListBase<T> for InternalSourcesLinkedList<T> {
    fn get_first(&self, head: T) -> T {
        self.get_index_part(head).root_as_source
    }

    fn get_last(&self, head: T) -> T {
        let first = self.get_first(head);
        if first.is_zero() {
            first
        } else {
            self.get_previous(first)
        }
    }

    fn get_size(&self, head: T) -> T {
        self.get_index_part(head).size_as_source
    }

    fn set_first(&mut self, head: T, element: T) {
        self.get_mut_index_part(head).root_as_source = element
    }

    fn set_last(&mut self, head: T, element: T) {
        // todo!("empty")
        // let first = self.get_index_part(head).root_as_source;
        // if first.is_zero() {
        //     self.set_first(head, element);
        // } else {
        //     self.set_previous(first, element);
        // }
    }

    fn set_size(&mut self, head: T, size: T) {
        self.get_mut_index_part(head).size_as_source = size
    }
}

impl<T: LinkType> DoublyLinkedListBase<T> for InternalSourcesLinkedList<T> {
    fn get_previous(&self, element: T) -> T {
        self.get_index_part(element).left_as_source
    }

    fn get_next(&self, element: T) -> T {
        self.get_index_part(element).right_as_source
    }

    fn set_previous(&mut self, element: T, previous: T) {
        self.get_mut_index_part(element).left_as_source = previous
    }

    fn set_next(&mut self, element: T, next: T) {
        self.get_mut_index_part(element).right_as_source = next
    }
}

impl<T: LinkType> RelativeCircularDoublyLinkedList<T> for InternalSourcesLinkedList<T> {}

impl<T: LinkType> UpdatePointersSplit for InternalSourcesLinkedList<T> {
    fn update_pointers(&mut self, data: *mut u8, indexes: *mut u8, header: *mut u8) {
        self.data = data;
        self.indexes = indexes;
        self.header = header;
    }
}
