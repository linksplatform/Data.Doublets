use num_traits::zero;

use crate::methods::lists::absolute_doubly_linked_list_base::AbsoluteDoublyLinkedListBase;
use crate::num::Num;

pub trait AbsoluteCircularDoublyLinkedList<T: Num>: AbsoluteDoublyLinkedListBase<T> {
    fn attach_before(&mut self, baseElement: T, newElement: T) {
        let baseElementPrevious = self.get_previous(baseElement);
        self.set_previous(newElement, baseElementPrevious);
        self.set_next(newElement, baseElement);
        if (baseElement == self.get_first())
        {
            self.set_first(newElement);
        }
        self.set_next(baseElementPrevious, newElement);
        self.set_previous(baseElement, newElement);
        self.inc_size();
    }

    fn attach_after(&mut self, baseElement: T, newElement: T) {
        let baseElementNext = self.get_next(baseElement);
        self.set_previous(newElement, baseElement);
        self.set_next(newElement, baseElementNext);
        if (baseElement == self.get_last())
        {
            self.set_last(newElement);
        }
        self.set_previous(baseElementNext, newElement);
        self.set_next(baseElement, newElement);
        self.inc_size();
    }

    fn attach_as_first(&mut self, element: T) {
        let first = self.get_first();
        if (first == zero())
        {
            self.set_first(element);
            self.set_last(element);
            self.set_previous(element, element);
            self.set_next(element, element);
            self.inc_size();
        }
        else
        {
            self.attach_before(first, element);
        }
    }

    fn attach_as_last(&mut self, element: T) {
        let last = self.get_last();
        if (last == zero())
        {
            self.attach_as_first(element);
        }
        else
        {
            self.attach_after(last, element);
        }
    }

    fn detach(&mut self, element: T) {
        let elementPrevious = self.get_previous(element);
        let elementNext = self.get_next(element);
        if (elementNext == element)
        {
            self.set_first(zero());
            self.set_last(zero());
        }
        else
        {
            self.set_next(elementPrevious, elementNext);
            self.set_previous(elementNext, elementPrevious);
            if (element == self.get_first())
            {
                self.set_first(elementNext);
            }
            if (element == self.get_last())
            {
                self.set_last(elementPrevious);
            }
        }
        self.set_previous(element, zero());
        self.set_next(element, zero());
        self.dec_size();
    }
}
