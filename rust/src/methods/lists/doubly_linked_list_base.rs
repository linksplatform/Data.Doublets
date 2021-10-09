use crate::num::Num;

pub trait DoublyLinkedListBase<T: Num> {
    fn get_previous(&self, element: T) -> T;
    fn get_next(&self, element: T) -> T;

    fn set_previous(&mut self, element: T, previous: T);
    fn set_next(&mut self, element: T, next: T);
}
