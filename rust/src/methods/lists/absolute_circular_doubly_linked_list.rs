use num_traits::zero;

use crate::methods::lists::absolute_doubly_linked_list_base::AbsoluteDoublyLinkedListBase;
use crate::num::Num;

pub trait AbsoluteCircularDoublyLinkedList<T: Num>: AbsoluteDoublyLinkedListBase<T> {
    fn attach_before(&mut self, old: T, new: T) {
        let old_previous = self.get_previous(old);
        self.set_previous(new, old_previous);
        self.set_next(new, old);
        if old == self.get_first() {
            self.set_first(new)
        }
        self.set_next(old_previous, new);
        self.set_previous(old, new);
        self.inc_size();
    }

    fn attach_after(&mut self, old: T, new: T) {
        let old_next = self.get_next(old);
        self.set_previous(new, old);
        self.set_next(new, old_next);
        if old == self.get_last() {
            self.set_last(new)
        }
        self.set_previous(old_next, new);
        self.set_next(old, new);
        self.inc_size();
    }

    fn attach_as_first(&mut self, new: T) {
        let first = self.get_first();
        if first == zero() {
            self.set_first(new);
            self.set_last(new);
            self.set_previous(new, new);
            self.set_next(new, new);
            self.inc_size();
        } else {
            self.attach_before(first, new);
        }
    }

    fn attach_as_last(&mut self, new: T) {
        let last = self.get_last();
        if last == zero() {
            self.attach_as_first(new);
        } else {
            self.attach_after(last, new);
        }
    }

    fn detach(&mut self, element: T) {
        let previous = self.get_previous(element);
        let next = self.get_next(element);
        if next == element {
            self.set_first(zero());
            self.set_last(zero());
        } else {
            self.set_next(previous, next);
            self.set_previous(next, previous);
            if element == self.get_first() {
                self.set_first(next);
            }
            if element == self.get_last() {
                self.set_last(previous);
            }
        }
        self.set_previous(element, zero());
        self.set_next(element, zero());
        self.dec_size();
    }
}
