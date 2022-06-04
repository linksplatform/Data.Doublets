use data::LinksConstants;

use num::LinkType;
pub(crate) use sources_recursionless_size_balanced_tree::*;
pub(crate) use targets_recursionless_size_balanced_tree::*;
pub(crate) use unused_links::*;

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

pub trait UpdatePointersSplit {
    fn update_pointers(&mut self, data: *mut u8, indexes: *mut u8, header: *mut u8);
}

mod links_recursionless_size_balanced_tree_base;
mod links_size_balanced_tree_base;
mod links_sources_size_balanced_tree;
mod links_targets_size_balanced_tree;
mod sources_recursionless_size_balanced_tree;
mod targets_recursionless_size_balanced_tree;
mod unused_links;
