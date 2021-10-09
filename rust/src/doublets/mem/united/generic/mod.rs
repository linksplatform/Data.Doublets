// TODO: move
pub trait UpdatePointers {
    fn update_pointers(&mut self, links: *mut u8, header: *mut u8);
}

pub mod links_size_balanced_tree_base;
pub mod links_sources_size_balanced_tree;
pub mod links_targets_size_balanced_tree;
pub mod unused_links;
