use num_traits::{one, zero};
use std::ops::Try;

use crate::data::LinksConstants;
use crate::doublets::mem::ilinks_tree_methods::ILinksTreeMethods;
use crate::doublets::mem::links_header::LinksHeader;
use crate::doublets::mem::splited::generic::external_recursion_less_base::{
    ExternalRecursionlessSizeBalancedTreeBase, ExternalRecursionlessSizeBalancedTreeBaseAbstract,
};
use crate::doublets::mem::splited::{DataPart, IndexPart};
use crate::doublets::mem::united::UpdatePointersSplit;
use crate::doublets::mem::UpdatePointers;
use crate::doublets::Link;
use crate::methods::RecursionlessSizeBalancedTreeMethods;
use crate::methods::SizeBalancedTreeBase;
use crate::num::LinkType;

pub struct ExternalTargetsRecursionlessTree<T: LinkType> {
    base: ExternalRecursionlessSizeBalancedTreeBase<T>,
}

impl<T: LinkType> ExternalTargetsRecursionlessTree<T> {
    pub fn new(
        constants: LinksConstants<T>,
        data: *mut u8,
        indexes: *mut u8,
        header: *mut u8,
    ) -> Self {
        Self {
            base: ExternalRecursionlessSizeBalancedTreeBase::new(constants, data, indexes, header),
        }
    }
}

impl<T: LinkType> SizeBalancedTreeBase<T> for ExternalTargetsRecursionlessTree<T> {
    fn get_left_reference(&self, node: T) -> *const T {
        &self.get_index_part(node).left_as_target as *const _
    }

    fn get_right_reference(&self, node: T) -> *const T {
        &self.get_index_part(node).right_as_target as *const _
    }

    fn get_mut_left_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut_index_part(node).left_as_target as *mut _
    }

    fn get_mut_right_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut_index_part(node).right_as_target as *mut _
    }

    fn get_left(&self, node: T) -> T {
        self.get_index_part(node).left_as_target
    }

    fn get_right(&self, node: T) -> T {
        self.get_index_part(node).right_as_target
    }

    fn get_size(&self, node: T) -> T {
        self.get_index_part(node).size_as_target
    }

    fn set_left(&mut self, node: T, left: T) {
        self.get_mut_index_part(node).left_as_target = left
    }

    fn set_right(&mut self, node: T, right: T) {
        self.get_mut_index_part(node).right_as_target = right
    }

    fn set_size(&mut self, node: T, size: T) {
        self.get_mut_index_part(node).size_as_target = size
    }

    fn first_is_to_the_left_of_second(&self, first: T, second: T) -> bool {
        let first = self.get_data_part(first);
        let second = self.get_data_part(second);
        self.first_is_to_the_left_of_second_4(
            first.source,
            first.target,
            second.source,
            second.target,
        )
    }

    fn first_is_to_the_right_of_second(&self, first: T, second: T) -> bool {
        let first = self.get_data_part(first);
        let second = self.get_data_part(second);
        self.first_is_to_the_right_of_second_4(
            first.source,
            first.target,
            second.source,
            second.target,
        )
    }

    fn clear_node(&mut self, node: T) {
        let link = self.get_mut_index_part(node);
        link.left_as_target = zero();
        link.right_as_target = zero();
        link.size_as_target = zero();
    }
}

impl<T: LinkType> RecursionlessSizeBalancedTreeMethods<T> for ExternalTargetsRecursionlessTree<T> {}

fn each_usages_core<T: LinkType, R: Try<Output = ()>, H: FnMut(Link<T>) -> R>(
    _self: &ExternalTargetsRecursionlessTree<T>,
    base: T,
    link: T,
    handler: &mut H,
) -> R {
    if link == zero() {
        return R::from_output(());
    }
    let link_base_part = _self.get_base_part(link);
    if link_base_part > base {
        each_usages_core(_self, base, _self.get_left_or_default(link), handler)?;
    } else if link_base_part < base {
        each_usages_core(_self, base, _self.get_right_or_default(link), handler)?;
    } else {
        handler(_self.get_link_value(link))?;
        each_usages_core(_self, base, _self.get_left_or_default(link), handler)?;
        each_usages_core(_self, base, _self.get_right_or_default(link), handler)?;
    }
    R::from_output(())
}

impl<T: LinkType> ILinksTreeMethods<T> for ExternalTargetsRecursionlessTree<T> {
    fn count_usages(&self, link: T) -> T {
        let mut root = self.get_tree_root();
        let total = self.get_size(root);
        let mut total_right_ignore = zero();
        while root != zero() {
            let base = self.get_base_part(root);
            if base <= link {
                root = self.get_right_or_default(root);
            } else {
                total_right_ignore = total_right_ignore + (self.get_right_size(root) + one());
                root = self.get_left_or_default(root);
            }
        }
        root = self.get_tree_root();
        let mut total_left_ignore = zero();
        while root != zero() {
            let base = self.get_base_part(root);
            if base >= link {
                root = self.get_left_or_default(root);
            } else {
                total_left_ignore = total_left_ignore + (self.get_left_size(root) + one());
                root = self.get_right_or_default(root);
            }
        }
        total - total_right_ignore - total_left_ignore
    }

    fn search(&self, source: T, target: T) -> T {
        let mut root = self.get_tree_root();
        while root != zero() {
            let root_link = self.get_data_part(root);
            let root_source = root_link.source;
            let root_target = root_link.target;
            if self.first_is_to_the_left_of_second_4(source, target, root_source, root_target) {
                root = self.get_left_or_default(root);
            } else if self.first_is_to_the_right_of_second_4(
                source,
                target,
                root_source,
                root_target,
            ) {
                root = self.get_right_or_default(root);
            } else {
                return root;
            }
        }
        zero()
    }

    fn each_usages<H: FnMut(Link<T>) -> R, R: Try<Output = ()>>(
        &self,
        root: T,
        mut handler: H,
    ) -> R {
        each_usages_core(self, root, self.get_tree_root(), &mut handler)
    }

    fn detach(&mut self, root: &mut T, index: T) {
        unsafe { RecursionlessSizeBalancedTreeMethods::detach(self, root as *mut _, index) }
    }

    fn attach(&mut self, root: &mut T, index: T) {
        unsafe { RecursionlessSizeBalancedTreeMethods::attach(self, root as *mut _, index) }
    }
}

impl<T: LinkType> UpdatePointersSplit for ExternalTargetsRecursionlessTree<T> {
    fn update_pointers(&mut self, data: *mut u8, indexes: *mut u8, header: *mut u8) {
        self.base.data = data;
        self.base.indexes = indexes;
        self.base.header = header;
    }
}

impl<T: LinkType> ExternalRecursionlessSizeBalancedTreeBaseAbstract<T>
    for ExternalTargetsRecursionlessTree<T>
{
    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { &*(self.base.header as *const LinksHeader<T>) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { &mut *(self.base.header as *mut LinksHeader<T>) }
    }

    fn get_index_part(&self, link: T) -> &IndexPart<T> {
        unsafe { &*((self.base.indexes as *const IndexPart<T>).add(link.as_())) }
    }

    fn get_mut_index_part(&mut self, link: T) -> &mut IndexPart<T> {
        unsafe { &mut *((self.base.indexes as *mut IndexPart<T>).add(link.as_())) }
    }

    fn get_data_part(&self, link: T) -> &DataPart<T> {
        unsafe { &mut *((self.base.data as *mut DataPart<T>).add(link.as_())) }
    }

    fn get_mut_data_part(&mut self, link: T) -> &mut DataPart<T> {
        unsafe { &mut *((self.base.data as *mut DataPart<T>).add(link.as_())) }
    }

    fn get_tree_root(&self) -> T {
        self.get_header().root_as_target
    }

    fn get_base_part(&self, link: T) -> T {
        self.get_data_part(link).source
    }

    fn first_is_to_the_left_of_second_4(
        &self,
        first_source: T,
        first_target: T,
        second_source: T,
        second_target: T,
    ) -> bool {
        (first_target < second_target)
            || (first_target == second_target && first_source < second_source)
    }

    fn first_is_to_the_right_of_second_4(
        &self,
        first_source: T,
        first_target: T,
        second_source: T,
        second_target: T,
    ) -> bool {
        (first_target > second_target)
            || (first_target == second_target && first_source > second_source)
    }
}
