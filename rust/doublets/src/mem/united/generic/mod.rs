use data::LinksConstants;
use std::cell::RefCell;
use std::ptr::NonNull;
use std::rc::Rc;

use crate::splited::{DataPart, IndexPart};
use num::LinkType;
//pub(crate) use sources_recursionless_size_balanced_tree::*;
//pub(crate) use targets_recursionless_size_balanced_tree::*;
//pub(crate) use unused_links::*;

// TODO: move
pub trait UpdatePointers {
    fn update_pointers(&mut self, links: *mut u8, header: *mut u8);
}

pub trait NewTree<T: LinkType> {
    fn new(constants: LinksConstants<T>, links: *mut u8, header: *mut u8) -> Self;
}

pub trait NewList<T: LinkType> {
    fn new(links: *mut u8, header: *mut u8) -> Self;
}

pub trait UpdatePointersSplit<T: LinkType> {
    fn update_pointers(&mut self, data: NonNull<[DataPart<T>]>, indexes: NonNull<[IndexPart<T>]>);
}

//mod links_recursionless_size_balanced_tree_base;
//mod links_size_balanced_tree_base;
//mod links_sources_size_balanced_tree;
//mod links_targets_size_balanced_tree;
//mod sources_recursionless_size_balanced_tree;
//mod targets_recursionless_size_balanced_tree;
mod unused_links;
