use num_traits::zero;
use std::{ops::Try, ptr::NonNull};

use crate::mem::traits::LinksTree;

use crate::mem::{
    split::{
        generic::internal_recursion_less_base::{
            InternalRecursionlessSizeBalancedTreeBase,
            InternalRecursionlessSizeBalancedTreeBaseAbstract,
        },
        DataPart, IndexPart,
    },
    SplitUpdateMem,
};

use crate::{mem::SplitTree, Link};
use data::LinksConstants;
use methods::{NoRecurSzbTree, SzbTree};
use num::LinkType;

pub struct InternalSourcesRecursionlessTree<T: LinkType> {
    base: InternalRecursionlessSizeBalancedTreeBase<T>,
}

impl<T: LinkType> InternalSourcesRecursionlessTree<T> {
    pub fn new(
        constants: LinksConstants<T>,
        data: NonNull<[DataPart<T>]>,
        indexes: NonNull<[IndexPart<T>]>,
    ) -> Self {
        Self {
            base: InternalRecursionlessSizeBalancedTreeBase::new(constants, data, indexes),
        }
    }
}

impl<T: LinkType> SzbTree<T> for InternalSourcesRecursionlessTree<T> {
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
        self.get_key_part(first) < self.get_key_part(second)
    }

    fn first_is_to_the_right_of_second(&self, first: T, second: T) -> bool {
        self.get_key_part(first) > self.get_key_part(second)
    }

    fn clear_node(&mut self, node: T) {
        let link = self.get_mut_index_part(node);
        link.left_as_source = zero();
        link.right_as_source = zero();
        link.size_as_source = zero();
    }
}

impl<T: LinkType> NoRecurSzbTree<T> for InternalSourcesRecursionlessTree<T> {}

fn each_usages_core<T: LinkType, R: Try<Output = ()>, H: FnMut(Link<T>) -> R>(
    _self: &InternalSourcesRecursionlessTree<T>,
    base: T,
    link: T,
    handler: &mut H,
) -> R {
    if link == zero() {
        return R::from_output(());
    }

    each_usages_core(_self, base, _self.get_left_or_default(link), handler)?;
    handler(_self.get_link_value(link))?;
    each_usages_core(_self, base, _self.get_right_or_default(link), handler)?;
    R::from_output(())
}

impl<T: LinkType> LinksTree<T> for InternalSourcesRecursionlessTree<T> {
    fn count_usages(&self, link: T) -> T {
        self.count_usages_core(link)
    }

    fn search(&self, source: T, target: T) -> T {
        self.search_core(self.get_tree_root(source), target)
    }

    fn each_usages<H: FnMut(Link<T>) -> R, R: Try<Output = ()>>(
        &self,
        root: T,
        mut handler: H,
    ) -> R {
        each_usages_core(self, root, self.get_tree_root(root), &mut handler)
    }

    fn detach(&mut self, root: &mut T, index: T) {
        unsafe { NoRecurSzbTree::detach(self, root as *mut _, index) }
    }

    fn attach(&mut self, root: &mut T, index: T) {
        unsafe { NoRecurSzbTree::attach(self, root as *mut _, index) }
    }
}

impl<T: LinkType> SplitUpdateMem<T> for InternalSourcesRecursionlessTree<T> {
    fn update_mem(&mut self, data: NonNull<[DataPart<T>]>, index: NonNull<[IndexPart<T>]>) {
        self.base.data = data;
        self.base.indexes = index;
    }
}

impl<T: LinkType> SplitTree<T> for InternalSourcesRecursionlessTree<T> {}

impl<T: LinkType> InternalRecursionlessSizeBalancedTreeBaseAbstract<T>
    for InternalSourcesRecursionlessTree<T>
{
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

    fn get_tree_root(&self, link: T) -> T {
        self.get_index_part(link).root_as_source
    }

    fn get_base_part(&self, link: T) -> T {
        self.get_data_part(link).source
    }

    fn get_key_part(&self, link: T) -> T {
        self.get_data_part(link).target
    }
}
