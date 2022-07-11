use crate::{
    mem::unit::LinkPart,
    split::{DataPart, IndexPart},
    Link,
};
use data::Flow;
use num::LinkType;
use std::ptr::NonNull;

pub trait LinksTree<T: LinkType> {
    fn count_usages(&self, root: T) -> T;

    fn search(&self, source: T, target: T) -> T;

    fn each_usages<H: FnMut(Link<T>) -> Flow + ?Sized>(&self, root: T, handler: &mut H) -> Flow;

    fn detach(&mut self, root: &mut T, index: T);

    fn attach(&mut self, root: &mut T, index: T);
}

pub trait UnitUpdateMem<T: LinkType> {
    fn update_mem(&mut self, mem: NonNull<[LinkPart<T>]>);
}

pub trait UnitTree<T: LinkType>: LinksTree<T> + UnitUpdateMem<T> {}

pub trait SplitUpdateMem<T: LinkType> {
    fn update_mem(&mut self, data: NonNull<[DataPart<T>]>, index: NonNull<[IndexPart<T>]>);
}

pub trait SplitTree<T: LinkType>: LinksTree<T> + SplitUpdateMem<T> {}

pub trait LinksList<T: LinkType> {
    fn detach(&mut self, link: T);

    fn attach_as_first(&mut self, link: T);
}

pub trait UnitList<T: LinkType>: LinksList<T> + UnitUpdateMem<T> {}

pub trait SplitList<T: LinkType>: LinksList<T> + SplitUpdateMem<T> {}
