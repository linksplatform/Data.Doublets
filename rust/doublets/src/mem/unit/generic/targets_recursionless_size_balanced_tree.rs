use num_traits::{one, zero};
use std::{mem::transmute, ops::Try, ptr::NonNull};

use crate::{
    mem::{
        header::LinksHeader,
        unit::{
            generic::{
                LinkRecursionlessSizeBalancedTreeBaseAbstract,
                LinksRecursionlessSizeBalancedTreeBase,
            },
            raw_link::LinkPart,
        },
        LinksTree, UnitTree, UnitUpdateMem,
    },
    Link,
};
use data::{Flow, LinksConstants};
use methods::{NoRecurSzbTree, SzbTree};
use num::LinkType;

pub struct LinksTargetsRecursionlessSizeBalancedTree<T: LinkType> {
    base: LinksRecursionlessSizeBalancedTreeBase<T>,
}

impl<T: LinkType> LinksTargetsRecursionlessSizeBalancedTree<T> {
    pub fn new(constants: LinksConstants<T>, mem: NonNull<[LinkPart<T>]>) -> Self {
        Self {
            base: LinksRecursionlessSizeBalancedTreeBase::new(constants, mem),
        }
    }
}

impl<T: LinkType> SzbTree<T> for LinksTargetsRecursionlessSizeBalancedTree<T> {
    fn get_left_reference(&self, node: T) -> *const T {
        &self.get_link(node).left_as_target as *const _
    }

    fn get_right_reference(&self, node: T) -> *const T {
        &self.get_link(node).right_as_target as *const _
    }

    fn get_mut_left_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut_link(node).left_as_target as *mut _
    }

    fn get_mut_right_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut_link(node).right_as_target as *mut _
    }

    fn get_left(&self, node: T) -> T {
        self.get_link(node).left_as_target
    }

    fn get_right(&self, node: T) -> T {
        self.get_link(node).right_as_target
    }

    fn get_size(&self, node: T) -> T {
        self.get_link(node).size_as_target
    }

    fn set_left(&mut self, node: T, left: T) {
        self.get_mut_link(node).left_as_target = left
    }

    fn set_right(&mut self, node: T, right: T) {
        self.get_mut_link(node).right_as_target = right
    }

    fn set_size(&mut self, node: T, size: T) {
        self.get_mut_link(node).size_as_target = size
    }

    fn first_is_to_the_left_of_second(&self, first: T, second: T) -> bool {
        let first = self.get_link(first);
        let second = self.get_link(second);
        self.first_is_to_the_left_of_second_4(
            first.source,
            first.target,
            second.source,
            second.target,
        )
    }

    fn first_is_to_the_right_of_second(&self, first: T, second: T) -> bool {
        let first = self.get_link(first);
        let second = self.get_link(second);
        self.first_is_to_the_right_of_second_4(
            first.source,
            first.target,
            second.source,
            second.target,
        )
    }

    fn clear_node(&mut self, node: T) {
        let link = self.get_mut_link(node);
        link.left_as_target = zero();
        link.right_as_target = zero();
        link.size_as_target = zero();
    }
}

impl<T: LinkType> NoRecurSzbTree<T> for LinksTargetsRecursionlessSizeBalancedTree<T> {}

fn each_usages_core<T: LinkType, H: FnMut(Link<T>) -> Flow + ?Sized>(
    _self: &LinksTargetsRecursionlessSizeBalancedTree<T>,
    base: T,
    link: T,
    handler: &mut H,
) -> Flow {
    if link == zero() {
        return Flow::Continue;
    }
    let link_base_part = _self.get_base_part(link);
    let _break = _self.base.r#break;
    if link_base_part > base {
        each_usages_core(_self, base, _self.get_left_or_default(link), handler)?;
    } else if link_base_part < base {
        each_usages_core(_self, base, _self.get_right_or_default(link), handler)?;
    } else {
        handler(_self.get_link_value(link))?;
        each_usages_core(_self, base, _self.get_left_or_default(link), handler)?;
        each_usages_core(_self, base, _self.get_right_or_default(link), handler)?;
    }
    Flow::Continue
}

impl<T: LinkType> LinksTree<T> for LinksTargetsRecursionlessSizeBalancedTree<T> {
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
            let root_link = self.get_link(root);
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

    fn each_usages<H: FnMut(Link<T>) -> Flow + ?Sized>(&self, root: T, handler: &mut H) -> Flow {
        each_usages_core(self, root, self.get_tree_root(), handler)
    }

    fn detach(&mut self, root: &mut T, index: T) {
        unsafe { NoRecurSzbTree::detach(self, root as *mut _, index) }
    }

    fn attach(&mut self, root: &mut T, index: T) {
        unsafe { NoRecurSzbTree::attach(self, root as *mut _, index) }
    }
}

impl<T: LinkType> UnitUpdateMem<T> for LinksTargetsRecursionlessSizeBalancedTree<T> {
    fn update_mem(&mut self, mem: NonNull<[LinkPart<T>]>) {
        self.base.mem = mem;
    }
}

impl<T: LinkType> LinkRecursionlessSizeBalancedTreeBaseAbstract<T>
    for LinksTargetsRecursionlessSizeBalancedTree<T>
{
    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { transmute(&self.base.mem.as_ref()[0]) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { transmute(&mut self.base.mem.as_mut()[0]) }
    }

    fn get_link(&self, link: T) -> &LinkPart<T> {
        unsafe { &self.base.mem.as_ref()[link.as_()] }
    }

    fn get_mut_link(&mut self, link: T) -> &mut LinkPart<T> {
        unsafe { &mut self.base.mem.as_mut()[link.as_()] }
    }

    fn get_tree_root(&self) -> T {
        self.get_header().root_as_target
    }

    fn get_base_part(&self, link: T) -> T {
        self.get_link(link).target
    }

    fn first_is_to_the_left_of_second_4(
        &self,
        first_source: T,
        first_target: T,
        second_source: T,
        second_target: T,
    ) -> bool {
        first_target < second_target
            || (first_target == second_target && first_source < second_source)
    }

    fn first_is_to_the_right_of_second_4(
        &self,
        first_source: T,
        first_target: T,
        second_source: T,
        second_target: T,
    ) -> bool {
        first_target > second_target
            || (first_target == second_target && first_source > second_source)
    }
}

impl<T: LinkType> UnitTree<T> for LinksTargetsRecursionlessSizeBalancedTree<T> {}
