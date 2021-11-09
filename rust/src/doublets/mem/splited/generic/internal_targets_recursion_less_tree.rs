use num_traits::{one, zero};

use crate::doublets::data::LinksConstants;
use crate::doublets::mem::ilinks_tree_methods::ILinksTreeMethods;
use crate::doublets::mem::links_header::LinksHeader;
use crate::doublets::mem::splited::generic::internal_recursion_less_base::{
    InternalRecursionlessSizeBalancedTreeBase, InternalRecursionlessSizeBalancedTreeBaseAbstract,
};
use crate::doublets::mem::splited::{DataPart, IndexPart};
use crate::doublets::mem::united::UpdatePointersSplit;
use crate::doublets::mem::UpdatePointers;
use crate::doublets::Link;
use crate::methods::RecursionlessSizeBalancedTreeMethods;
use crate::methods::SizeBalancedTreeBase;
use crate::num::LinkType;

pub struct InternalTargetsRecursionlessTree<T: LinkType> {
    base: InternalRecursionlessSizeBalancedTreeBase<T>,
}

impl<T: LinkType> InternalTargetsRecursionlessTree<T> {
    pub fn new(
        constants: LinksConstants<T>,
        data: *mut u8,
        indexes: *mut u8,
        header: *mut u8,
    ) -> Self {
        Self {
            base: InternalRecursionlessSizeBalancedTreeBase::new(constants, data, indexes, header),
        }
    }
}

impl<T: LinkType> SizeBalancedTreeBase<T> for InternalTargetsRecursionlessTree<T> {
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
        self.get_key_part(first) < self.get_key_part(second)
    }

    fn first_is_to_the_right_of_second(&self, first: T, second: T) -> bool {
        self.get_key_part(first) > self.get_key_part(second)
    }

    fn clear_node(&mut self, node: T) {
        let link = self.get_mut_index_part(node);
        link.left_as_target = zero();
        link.right_as_target = zero();
        link.size_as_target = zero();
    }
}

impl<T: LinkType> RecursionlessSizeBalancedTreeMethods<T> for InternalTargetsRecursionlessTree<T> {}

fn each_usages_core<T: LinkType, H: FnMut(Link<T>) -> T>(
    _self: &InternalTargetsRecursionlessTree<T>,
    base: T,
    link: T,
    handler: &mut H,
) -> T {
    let r#break = _self.base.r#break;
    let r#continue = _self.base.r#continue;

    if link == zero() {
        return r#continue;
    }

    // TODO: match
    if each_usages_core(_self, base, _self.get_left_or_default(link), handler) == r#break {
        return r#break;
    }
    if handler(_self.get_link_value(link)) == r#break {
        return r#break;
    }
    if each_usages_core(_self, base, _self.get_right_or_default(link), handler) == r#break {
        return r#break;
    }
    r#continue
}

impl<T: LinkType> ILinksTreeMethods<T> for InternalTargetsRecursionlessTree<T> {
    fn count_usages(&self, link: T) -> T {
        self.count_usages_core(link)
    }

    fn search(&self, source: T, target: T) -> T {
        self.search_core(self.get_tree_root(source), target)
    }

    fn each_usages<H: FnMut(Link<T>) -> T>(&self, base: T, mut handler: H) -> T {
        each_usages_core(self, base, self.get_tree_root(base), &mut handler)
    }

    fn detach(&mut self, root: &mut T, index: T) {
        unsafe { RecursionlessSizeBalancedTreeMethods::detach(self, root as *mut _, index) }
    }

    fn attach(&mut self, root: &mut T, index: T) {
        unsafe { RecursionlessSizeBalancedTreeMethods::attach(self, root as *mut _, index) }
    }
}

impl<T: LinkType> UpdatePointersSplit for InternalTargetsRecursionlessTree<T> {
    fn update_pointers(&mut self, data: *mut u8, indexes: *mut u8, header: *mut u8) {
        self.base.data = data;
        self.base.indexes = indexes;
        self.base.header = header;
    }
}

impl<T: LinkType> InternalRecursionlessSizeBalancedTreeBaseAbstract<T>
    for InternalTargetsRecursionlessTree<T>
{
    fn get_index_part(&self, link: T) -> &IndexPart<T> {
        unsafe { &*((self.base.indexes as *mut IndexPart<T>).add(link.as_())) }
    }

    fn get_mut_index_part(&mut self, link: T) -> &mut IndexPart<T> {
        unsafe { &mut *((self.base.indexes as *mut IndexPart<T>).add(link.as_())) }
    }

    fn get_data_part(&self, link: T) -> &DataPart<T> {
        unsafe { &*((self.base.data as *mut DataPart<T>).add(link.as_())) }
    }

    fn get_mut_data_part(&mut self, link: T) -> &mut DataPart<T> {
        unsafe { &mut *((self.base.data as *mut DataPart<T>).add(link.as_())) }
    }

    fn get_tree_root(&self, link: T) -> T {
        self.get_index_part(link).root_as_target
    }

    fn get_base_part(&self, link: T) -> T {
        self.get_data_part(link).target
    }

    fn get_key_part(&self, link: T) -> T {
        self.get_data_part(link).source
    }
}
