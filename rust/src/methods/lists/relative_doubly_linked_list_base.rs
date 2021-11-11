use num_traits::one;

use crate::methods::lists::doubly_linked_list_base::DoublyLinkedListBase;
use crate::num::Num;

pub trait RelativeDoublyLinkedListBase<T: Num>: DoublyLinkedListBase<T> {
    fn get_first(&self, head: T) -> T;
    fn get_last(&self, head: T) -> T;
    fn get_size(&self, head: T) -> T;

    fn set_first(&mut self, head: T, element: T);
    fn set_last(&mut self, head: T, element: T);
    fn set_size(&mut self, head: T, size: T);

    fn inc_size(&mut self, head: T) {
        self.set_size(head, self.get_size(head) + one())
    }
    fn dec_size(&mut self, head: T) {
        self.set_size(head, self.get_size(head) - one())
    }
}
