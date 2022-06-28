use std::default::default;
use std::marker::PhantomData;

use crate::mem::ilinks_tree_methods::ILinksTreeMethods;
use crate::mem::links_header::LinksHeader;
use crate::mem::united::raw_link::RawLink;
use crate::Link;
use data::LinksConstants;
use methods::NoRecurSzbTree;
use num::LinkType;

// TODO: why is there so much duplication in OOP!!! FIXME
pub struct LinksRecursionlessSizeBalancedTreeBase<T: LinkType> {
    pub links: *mut u8,
    pub header: *mut u8,
    pub r#break: T,
    pub r#continue: T,

    _phantom: PhantomData<T>,
}

impl<T: LinkType> LinksRecursionlessSizeBalancedTreeBase<T> {
    pub fn new(constants: LinksConstants<T>, links: *mut u8, header: *mut u8) -> Self {
        Self {
            links,
            header,
            r#break: constants.r#break,
            r#continue: constants.r#continue,
            _phantom: default(),
        }
    }
}

pub trait LinkRecursionlessSizeBalancedTreeBaseAbstract<T: LinkType>:
    NoRecurSzbTree<T> + ILinksTreeMethods<T>
{
    fn get_header(&self) -> &LinksHeader<T>;

    fn get_mut_header(&mut self) -> &mut LinksHeader<T>;

    fn get_link(&self, link: T) -> &RawLink<T>;

    fn get_mut_link(&mut self, link: T) -> &mut RawLink<T>;

    fn get_tree_root(&self) -> T;

    fn get_base_part(&self, link: T) -> T;

    // TODO: rename
    fn first_is_to_the_left_of_second_4(
        &self,
        source: T,
        target: T,
        root_source: T,
        root_target: T,
    ) -> bool;

    fn first_is_to_the_right_of_second_4(
        &self,
        source: T,
        target: T,
        root_source: T,
        root_target: T,
    ) -> bool;

    fn get_link_value(&self, index: T) -> Link<T> {
        let link = self.get_link(index);
        Link::new(index, link.source, link.target)
    }
}
