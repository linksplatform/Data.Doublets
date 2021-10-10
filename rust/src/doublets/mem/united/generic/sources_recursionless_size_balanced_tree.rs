use crate::doublets::mem::united::generic::links_recursionless_size_balanced_tree_base::{LinksRecursionlessSizeBalancedTreeBase, LinkRecursionlessSizeBalancedTreeBaseAbstract};
use crate::num::LinkType;
use crate::doublets::data::links_constants::LinksConstants;
use crate::methods::trees::recursuionless_size_balanced_tree::RecursionlessSizeBalancedTreeMethods;
use crate::doublets::mem::ilinks_tree_methods::ILinksTreeMethods;
use crate::doublets::mem::united::generic::UpdatePointers;
use crate::doublets::mem::links_header::LinksHeader;
use crate::doublets::mem::united::raw_link::RawLink;
use num_traits::{zero, one};
use crate::methods::trees::size_balanced_tree_base::SizeBalancedTreeBase;

pub struct LinksSourcesRecursionlessSizeBalancedTree<T: LinkType> {
    base: LinksRecursionlessSizeBalancedTreeBase<T>,
}

impl<T: LinkType> LinksSourcesRecursionlessSizeBalancedTree<T> {
    pub fn new(constants: LinksConstants<T>, links: *mut u8, header: *mut u8) -> Self {
        Self {
            base: LinksRecursionlessSizeBalancedTreeBase::new(constants, links, header),
        }
    }
}

impl<T: LinkType> SizeBalancedTreeBase<T> for LinksSourcesRecursionlessSizeBalancedTree<T> {
    fn get_left_reference(&self, node: T) -> *const T {
        &self.get_link(node).left_as_source as *const _
    }

    fn get_right_reference(&self, node: T) -> *const T {
        &self.get_link(node).right_as_source as *const _
    }

    fn get_mut_left_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut_link(node).left_as_source as *mut _
    }

    fn get_mut_right_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut_link(node).right_as_source as *mut _
    }

    fn get_left(&self, node: T) -> T {
        self.get_link(node).left_as_source
    }

    fn get_right(&self, node: T) -> T {
        self.get_link(node).right_as_source
    }

    fn get_size(&self, node: T) -> T {
        self.get_link(node).size_as_source
    }

    fn set_left(&mut self, node: T, left: T) {
        self.get_mut_link(node).left_as_source = left
    }

    fn set_right(&mut self, node: T, right: T) {
        self.get_mut_link(node).right_as_source = right
    }

    fn set_size(&mut self, node: T, size: T) {
        self.get_mut_link(node).size_as_source = size
    }

    fn first_is_to_the_left_of_second(&self, first: T, second: T) -> bool {
        let first = self.get_link(first);
        let second = self.get_link(second);
        self.first_is_to_the_left_of_second_4(first.source, first.target, second.source, second.target)
    }

    fn first_is_to_the_right_of_second(&self, first: T, second: T) -> bool {
        let first = self.get_link(first);
        let second = self.get_link(second);
        self.first_is_to_the_right_of_second_4(first.source, first.target, second.source, second.target)
    }

    fn clear_node(&mut self, node: T) {
        let link = self.get_mut_link(node);
        link.left_as_source = zero();
        link.right_as_source = zero();
        link.size_as_source = zero();
    }
}

impl<T: LinkType> RecursionlessSizeBalancedTreeMethods<T> for LinksSourcesRecursionlessSizeBalancedTree<T> {
}

fn each_usages_core<T: LinkType, H: FnMut(&[T]) -> T>(_self: &LinksSourcesRecursionlessSizeBalancedTree<T>, base: T, link: T, handler: &mut H) -> T {
    let r#continue = _self.base.r#continue;
    if (link == zero())
    {
        return r#continue;
    }
    let linkBasePart = _self.get_base_part(link);
    let r#break = _self.base.r#break;
    if (linkBasePart > base)
    {
        if each_usages_core(_self, base, _self.get_left_or_default(link), handler) == r#break
        {
            return r#break;
        }
    }
    else if linkBasePart < base
    {
        if  each_usages_core(_self, base, _self.get_right_or_default(link), handler) == r#break
        {
            return r#break;
        }
    }
    else
    {
        if handler(_self.get_link_value(link).as_slice()) == r#break
        {
            return r#break;
        }
        if each_usages_core(_self, base, _self.get_left_or_default(link), handler) == r#break
        {
            return r#break;
        }
        if each_usages_core(_self, base, _self.get_right_or_default(link), handler) == r#break
        {
            return r#break;
        }
    }
    return r#continue;
}

impl<T: LinkType> ILinksTreeMethods<T> for LinksSourcesRecursionlessSizeBalancedTree<T> {
    fn count_usages(&self, link: T) -> T {
        let mut root = self.get_tree_root();
        let total = self.get_size(root);
        let mut totalRightIgnore = zero();
        while (root != zero())
        {
            let base = self.get_base_part(root);
            if ((base <= link))
            {
                root = self.get_right_or_default(root);
            }
            else
            {
                totalRightIgnore = totalRightIgnore + (self.get_right_size(root) + one());
                root = self.get_left_or_default(root);
            }
        }
        root = self.get_tree_root();
        let mut totalLeftIgnore = zero();
        while (root != zero())
        {
            let base = self.get_base_part(root);
            if (base >= link)
            {
                root = self.get_left_or_default(root);
            }
            else
            {
                totalLeftIgnore = totalLeftIgnore + (self.get_left_size(root) + one());
                root = self.get_right_or_default(root);
            }
        }
        return total - totalRightIgnore - totalLeftIgnore;
    }

    fn search(&self, source: T, target: T) -> T {
        let mut root = self.get_tree_root();
        while root != zero()
        {
            let rootLink = self.get_link(root);
            let rootSource = rootLink.source;
            let rootTarget = rootLink.target;
            if self.first_is_to_the_left_of_second_4(source, target, rootSource, rootTarget)
            {
                root = self.get_left_or_default(root);
            }
            else if self.first_is_to_the_right_of_second_4(source, target, rootSource, rootTarget)
            {
                root = self.get_right_or_default(root);
            }
            else
            {
                return root;
            }
        }
        return zero();
    }

    fn each_usages<H: FnMut(&[T]) -> T>(&self, base: T, mut handler: H) -> T {
        each_usages_core(self, base, self.get_tree_root(), &mut handler)
    }

    fn detach(&mut self, root: &mut T, index: T) {
        unsafe { RecursionlessSizeBalancedTreeMethods::detach(self, root as *mut _, index) }
    }

    fn attach(&mut self, root: &mut T, index: T) {
        unsafe { RecursionlessSizeBalancedTreeMethods::attach(self, root as *mut _, index) }
    }
}

impl<T: LinkType> UpdatePointers for LinksSourcesRecursionlessSizeBalancedTree<T> {
    fn update_pointers(&mut self, links: *mut u8, header: *mut u8) {
        self.base.links = links;
        self.base.header = header;
    }
}

impl<T: LinkType> LinkRecursionlessSizeBalancedTreeBaseAbstract<T> for LinksSourcesRecursionlessSizeBalancedTree<T> {
    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { &*(self.base.header as *const LinksHeader<T>) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { &mut *(self.base.header as *mut LinksHeader<T>) }
    }

    fn get_link(&self, link: T) -> &RawLink<T> {
        unsafe { &*((self.base.links as *const RawLink<T>).offset(link.as_() as isize)) }
    }

    fn get_mut_link(&mut self, link: T) -> &mut RawLink<T> {
        unsafe { &mut *(self.base.links as *mut RawLink<T>).offset(link.as_() as isize) }
    }

    fn get_tree_root(&self) -> T {
        self.get_header().root_as_source
    }

    fn get_base_part(&self, link: T) -> T {
        self.get_link(link).source
    }

    fn first_is_to_the_left_of_second_4(&self, firstSource: T, firstTarget: T, secondSource: T, secondTarget: T) -> bool {
        (firstSource < secondSource) || (firstSource == secondSource && firstTarget < secondTarget)
    }

    fn first_is_to_the_right_of_second_4(&self, firstSource: T, firstTarget: T, secondSource: T, secondTarget: T) -> bool {
        (firstSource > secondSource) || (firstSource == secondSource && firstTarget > secondTarget)
    }
}
