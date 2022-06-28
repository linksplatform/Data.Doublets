use num_traits::{one, zero};
use std::{mem::transmute, ops::Try, ptr::NonNull};

use crate::mem::{
    links_header::LinksHeader,
    links_traits::LinksTree,
    splited::{
        generic::external_recursion_less_base::{
            ExternalRecursionlessSizeBalancedTreeBase,
            ExternalRecursionlessSizeBalancedTreeBaseAbstract,
        },
        DataPart, IndexPart,
    },
};

use crate::{
    mem::{SplitTree, SplitUpdateMem},
    Link,
};
use data::LinksConstants;
use methods::{NoRecurSzbTree, SzbTree};
use num::LinkType;

pub struct ExternalSourcesRecursionlessTree<T: LinkType> {
    base: ExternalRecursionlessSizeBalancedTreeBase<T>,
}

impl<T: LinkType> ExternalSourcesRecursionlessTree<T> {
    pub fn new(
        constants: LinksConstants<T>,
        data: NonNull<[DataPart<T>]>,
        indexes: NonNull<[IndexPart<T>]>,
    ) -> Self {
        Self {
            base: ExternalRecursionlessSizeBalancedTreeBase::new(constants, data, indexes),
        }
    }
}

impl<T: LinkType> SzbTree<T> for ExternalSourcesRecursionlessTree<T> {
    fn get_left_reference(&self, node: T) -> *const T {
        &self.get_index_part(node).left_as_source as *const _
    }

    fn get_right_reference(&self, node: T) -> *const T {
        &self.get_index_part(node).right_as_source as *const _
    }

    fn get_mut_left_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut_index_part(node).left_as_source as *mut _
    }

    fn get_mut_right_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut_index_part(node).right_as_source as *mut _
    }

    fn get_left(&self, node: T) -> T {
        self.get_index_part(node).left_as_source
    }

    fn get_right(&self, node: T) -> T {
        self.get_index_part(node).right_as_source
    }

    fn get_size(&self, node: T) -> T {
        self.get_index_part(node).size_as_source
    }

    fn set_left(&mut self, node: T, left: T) {
        self.get_mut_index_part(node).left_as_source = left
    }

    fn set_right(&mut self, node: T, right: T) {
        self.get_mut_index_part(node).right_as_source = right
    }

    fn set_size(&mut self, node: T, size: T) {
        self.get_mut_index_part(node).size_as_source = size
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
        link.left_as_source = zero();
        link.right_as_source = zero();
        link.size_as_source = zero();
    }
}

impl<T: LinkType> NoRecurSzbTree<T> for ExternalSourcesRecursionlessTree<T> {}

fn each_usages_core<T: LinkType, R: Try<Output = ()>, H: FnMut(Link<T>) -> R>(
    _self: &ExternalSourcesRecursionlessTree<T>,
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

impl<T: LinkType> LinksTree<T> for ExternalSourcesRecursionlessTree<T> {
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
        unsafe { NoRecurSzbTree::detach(self, root as *mut _, index) }
    }

    fn attach(&mut self, root: &mut T, index: T) {
        unsafe { NoRecurSzbTree::attach(self, root as *mut _, index) }
    }
}

impl<T: LinkType> SplitUpdateMem<T> for ExternalSourcesRecursionlessTree<T> {
    fn update_mem(&mut self, data: NonNull<[DataPart<T>]>, indexes: NonNull<[IndexPart<T>]>) {
        self.base.indexes = indexes;
        self.base.data = data;
    }
}

impl<T: LinkType> SplitTree<T> for ExternalSourcesRecursionlessTree<T> {}

impl<T: LinkType> ExternalRecursionlessSizeBalancedTreeBaseAbstract<T>
    for ExternalSourcesRecursionlessTree<T>
{
    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { transmute(&self.base.indexes.as_ref()[0]) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { transmute(&mut self.base.indexes.as_mut()[0]) }
    }

    fn get_index_part(&self, link: T) -> &IndexPart<T> {
        unsafe { &self.base.indexes.as_ref()[link.as_()] }
    }

    fn get_mut_index_part(&mut self, link: T) -> &mut IndexPart<T> {
        unsafe { &mut self.base.indexes.as_mut()[link.as_()] }
    }

    fn get_data_part(&self, link: T) -> &DataPart<T> {
        unsafe { &self.base.data.as_ref()[link.as_()] }
    }

    fn get_mut_data_part(&mut self, link: T) -> &mut DataPart<T> {
        unsafe { &mut self.base.data.as_mut()[link.as_()] }
    }

    fn get_tree_root(&self) -> T {
        self.get_header().root_as_source
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
        (first_source < second_source)
            || (first_source == second_source && first_target < second_target)
    }

    fn first_is_to_the_right_of_second_4(
        &self,
        first_source: T,
        first_target: T,
        second_source: T,
        second_target: T,
    ) -> bool {
        (first_source > second_source)
            || (first_source == second_source && first_target > second_target)
    }
}
