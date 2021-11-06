use num_traits::zero;

use crate::methods::lists::absolute_doubly_linked_list_base::AbsoluteDoublyLinkedListBase;
use crate::num::Num;

pub trait AbsoluteCircularDoublyLinkedList<T: Num>: AbsoluteDoublyLinkedListBase<T> {
    fn attach_before(&mut self, base_element: T, new_element: T) {
        let base_element_previous = self.get_previous(base_element);
        self.set_previous(new_element, base_element_previous);
        self.set_next(new_element, base_element);
        if base_element == self.get_first() {
            self.set_first(new_element);
        }
        self.set_next(base_element_previous, new_element);
        self.set_previous(base_element, new_element);
        self.inc_size();
    }

    fn attach_after(&mut self, base_element: T, new_element: T) {
        let base_element_next = self.get_next(base_element);
        self.set_previous(new_element, base_element);
        self.set_next(new_element, base_element_next);
        if base_element == self.get_last() {
            self.set_last(new_element);
        }
        self.set_previous(base_element_next, new_element);
        self.set_next(base_element, new_element);
        self.inc_size();
    }

    fn attach_as_first(&mut self, element: T) {
        let first = self.get_first();
        if first == zero() {
            self.set_first(element);
            self.set_last(element);
            self.set_previous(element, element);
            self.set_next(element, element);
            self.inc_size();
        } else {
            self.attach_before(first, element);
        }
    }

    fn attach_as_last(&mut self, element: T) {
        let last = self.get_last();
        if last == zero() {
            self.attach_as_first(element);
        } else {
            self.attach_after(last, element);
        }
    }

    fn detach(&mut self, element: T) {
        let element_previous = self.get_previous(element);
        let element_next = self.get_next(element);
        if element_next == element {
            self.set_first(zero());
            self.set_last(zero());
        } else {
            self.set_next(element_previous, element_next);
            self.set_previous(element_next, element_previous);
            if element == self.get_first() {
                self.set_first(element_next);
            }
            if element == self.get_last() {
                self.set_last(element_previous);
            }
        }
        self.set_previous(element, zero());
        self.set_next(element, zero());
        self.dec_size();
    }
}
