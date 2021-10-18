use crate::num::LinkType;
use smallvec::{SmallVec, Array};



pub trait SequenceWalker<T: LinkType, const STACK_CAPACITY: usize=1024>
where [T; STACK_CAPACITY]: Array
{
    type Capacity: Array = [T; STACK_CAPACITY];

    fn walk(&self, sequence: T) -> SmallVec<Self::Capacity>;
}
