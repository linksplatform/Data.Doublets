use crate::doublets::mem::united::generic::UpdatePointers;
use crate::num::LinkType;

pub trait ILinksListMethods<T: LinkType>: UpdatePointers {
    fn detach(&mut self, link: T);

    fn attach_as_first(&mut self, link: T);
}
