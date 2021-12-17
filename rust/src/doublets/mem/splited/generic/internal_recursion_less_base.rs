use std::default::default;
use std::marker::PhantomData;

use num_traits::zero;

use crate::doublets::data::LinksConstants;
use crate::doublets::link::Link;
use crate::doublets::mem::ilinks_tree_methods::ILinksTreeMethods;
use crate::doublets::mem::links_header::LinksHeader;
use crate::doublets::mem::splited::{DataPart, IndexPart};
use crate::methods::RecursionlessSizeBalancedTreeMethods;
use crate::methods::SizeBalancedTreeMethods;
use crate::num::LinkType;

// TODO: why is there so much duplication in OOP!!! FIXME
pub struct InternalRecursionlessSizeBalancedTreeBase<T: LinkType> {
    pub data: *mut u8,
    pub indexes: *mut u8,
    pub header: *mut u8,
    pub r#break: T,
    pub r#continue: T,
}

impl<T: LinkType> InternalRecursionlessSizeBalancedTreeBase<T> {
    pub fn new(
        constants: LinksConstants<T>,
        data: *mut u8,
        indexes: *mut u8,
        header: *mut u8,
    ) -> Self {
        Self {
            data,
            indexes,
            header,
            r#break: constants.r#break,
            r#continue: constants.r#continue,
        }
    }
}

pub trait InternalRecursionlessSizeBalancedTreeBaseAbstract<T: LinkType>:
    RecursionlessSizeBalancedTreeMethods<T> + ILinksTreeMethods<T>
{
    fn get_index_part(&self, link: T) -> &IndexPart<T>;

    fn get_mut_index_part(&mut self, link: T) -> &mut IndexPart<T>;

    fn get_data_part(&self, link: T) -> &DataPart<T>;

    fn get_mut_data_part(&mut self, link: T) -> &mut DataPart<T>;

    fn get_tree_root(&self, link: T) -> T;

    fn get_base_part(&self, link: T) -> T;

    fn get_key_part(&self, link: T) -> T;

    fn get_link_value(&self, index: T) -> Link<T> {
        let link = self.get_data_part(index);
        Link::new(index, link.source, link.target)
    }

    fn search_core(&self, mut root: T, key: T) -> T {
        while root != zero() {
            let root_key = self.get_key_part(root);
            if key < root_key {
                root = self.get_left_or_default(root);
            } else if key > root_key {
                root = self.get_right_or_default(root);
            } else {
                return root;
            }
        }
        zero()
    }

    fn count_usages_core(&self, link: T) -> T {
        self.get_size_or_zero(self.get_tree_root(link))
    }
}
