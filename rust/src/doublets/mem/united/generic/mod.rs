pub(crate) use links_recursionless_size_balanced_tree_base::{*};
pub(crate) use links_size_balanced_tree_base::{*};
pub(crate) use links_sources_size_balanced_tree::{*};
pub(crate) use links_targets_size_balanced_tree::{*};
pub(crate) use sources_recursionless_size_balanced_tree::{*};
pub(crate) use targets_recursionless_size_balanced_tree::{*};
pub(crate) use unused_links::{*};

// TODO: move
pub trait UpdatePointers {
    fn update_pointers(&mut self, links: *mut u8, header: *mut u8);
}

mod links_size_balanced_tree_base;
mod links_sources_size_balanced_tree;
mod links_targets_size_balanced_tree;
mod unused_links;
mod links_recursionless_size_balanced_tree_base;
mod sources_recursionless_size_balanced_tree;
mod targets_recursionless_size_balanced_tree;

