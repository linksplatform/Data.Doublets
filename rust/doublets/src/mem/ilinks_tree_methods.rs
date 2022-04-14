use crate::mem::UpdatePointers;
use crate::Link;
use num::LinkType;
use std::ops::Try;

pub trait ILinksTreeMethods<T: LinkType> {
    fn count_usages(&self, root: T) -> T;

    fn search(&self, source: T, target: T) -> T;

    fn each_usages<H: FnMut(Link<T>) -> R, R: Try<Output = ()>>(&self, root: T, handler: H) -> R;

    fn detach(&mut self, root: &mut T, index: T);

    fn attach(&mut self, root: &mut T, index: T);
}
