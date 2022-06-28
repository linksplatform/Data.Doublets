use crate::Link;
use data::LinksConstants;
use std::mem::transmute;

use std::{ops::Try, ptr::NonNull};

use crate::mem::{
    links_header::LinksHeader,
    splited::{DataPart, IndexPart},
    SplitTree, SplitUpdateMem,
};

use crate::mem::links_traits::SplitList;
use methods::{LinkedList, RelativeCircularLinkedList, RelativeLinkedList};
use num::LinkType;

pub struct InternalSourcesLinkedList<T: LinkType> {
    data: NonNull<[DataPart<T>]>,
    indexes: NonNull<[IndexPart<T>]>,
    r#continue: T,
    r#break: T,
}

impl<T: LinkType> InternalSourcesLinkedList<T> {
    pub fn new(
        constants: LinksConstants<T>,
        data: NonNull<[DataPart<T>]>,
        indexes: NonNull<[IndexPart<T>]>,
    ) -> Self {
        Self {
            data,
            indexes,
            r#continue: constants.r#continue,
            r#break: constants.r#break,
        }
    }

    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { transmute(&self.indexes.as_ref()[0]) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { transmute(&mut self.indexes.as_mut()[0]) }
    }

    fn get_data_part(&self, link: T) -> &DataPart<T> {
        unsafe { &self.data.as_ref()[link.as_()] }
    }

    fn get_mut_data_part(&mut self, link: T) -> &mut DataPart<T> {
        unsafe { &mut self.data.as_mut()[link.as_()] }
    }

    fn get_index_part(&self, link: T) -> &IndexPart<T> {
        unsafe { &self.indexes.as_ref()[link.as_()] }
    }

    fn get_mut_index_part(&mut self, link: T) -> &mut IndexPart<T> {
        unsafe { &mut self.indexes.as_mut()[link.as_()] }
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

impl<T: LinkType> RelativeLinkedList<T> for InternalSourcesLinkedList<T> {
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

    fn set_last(&mut self, _head: T, _element: T) {
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

impl<T: LinkType> LinkedList<T> for InternalSourcesLinkedList<T> {
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

impl<T: LinkType> RelativeCircularLinkedList<T> for InternalSourcesLinkedList<T> {}

impl<T: LinkType> SplitUpdateMem<T> for InternalSourcesLinkedList<T> {
    fn update_mem(&mut self, data: NonNull<[DataPart<T>]>, indexes: NonNull<[IndexPart<T>]>) {
        self.data = data;
        self.indexes = indexes;
    }
}
