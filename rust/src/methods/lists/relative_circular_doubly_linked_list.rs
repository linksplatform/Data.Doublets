use num_traits::zero;

use crate::methods::lists::relative_doubly_linked_list_base::RelativeDoublyLinkedListBase;
use crate::num::Num;

pub trait RelativeCircularDoublyLinkedList<T: Num>: RelativeDoublyLinkedListBase<T> {
    fn attach_before(&mut self, head: T, base_element: T, new_element: T) {
        let base_element_previous = self.get_previous(base_element);
        self.set_previous(new_element, base_element_previous);
        self.set_next(new_element, base_element);
        if base_element == self.get_first(head) {
            self.set_first(head, new_element);
        }
        self.set_next(base_element_previous, new_element);
        self.set_previous(base_element, new_element);
        self.inc_size(head);
    }

    fn attach_after(&mut self, head: T, base_element: T, new_element: T) {
        let base_element_next = self.get_next(base_element);
        self.set_previous(new_element, base_element);
        self.set_next(new_element, base_element_next);
        if base_element == self.get_last(head) {
            self.set_last(head, new_element);
        }
        self.set_previous(base_element_next, new_element);
        self.set_next(base_element, new_element);
        self.inc_size(head);
    }

    fn attach_as_first(&mut self, head: T, element: T) {
        let first = self.get_first(head);
        if first == zero() {
            self.set_first(head, element);
            self.set_last(head, element);
            self.set_previous(element, element);
            self.set_next(element, element);
            self.inc_size(head);
        } else {
            self.attach_before(head, first, element);
        }
    }

    fn attach_as_last(&mut self, head: T, element: T) {
        let last = self.get_last(head);
        if last == zero() {
            self.attach_as_first(head, element);
        } else {
            self.attach_after(head, last, element);
        }
    }

    fn detach(&mut self, head: T, element: T) {
        let element_previous = self.get_previous(element);
        let element_next = self.get_next(element);
        if element_next == element {
            self.set_first(head, zero());
            self.set_last(head, zero());
        } else {
            self.set_next(element_previous, element_next);
            self.set_previous(element_next, element_previous);
            if element == self.get_first(head) {
                self.set_first(head, element_next);
            }
            if element == self.get_last(head) {
                self.set_last(head, element_previous);
            }
        }
        self.set_previous(element, zero());
        self.set_next(element, zero());
        self.dec_size(head);
    }
}
