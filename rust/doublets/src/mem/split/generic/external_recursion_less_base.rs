use crate::{
    mem::{
        header::LinksHeader,
        split::{DataPart, IndexPart},
        traits::LinksTree,
    },
    Link,
};
use data::LinksConstants;
use methods::NoRecurSzbTree;
use num::LinkType;
use std::ptr::NonNull;

// TODO: why is there so much duplication in OOP!!! FIXME
pub struct ExternalRecursionlessSizeBalancedTreeBase<T: LinkType> {
    pub data: NonNull<[DataPart<T>]>,
    pub indexes: NonNull<[IndexPart<T>]>,
    pub r#break: T,
    pub r#continue: T,
}

impl<T: LinkType> ExternalRecursionlessSizeBalancedTreeBase<T> {
    pub fn new(
        constants: LinksConstants<T>,
        data: NonNull<[DataPart<T>]>,
        indexes: NonNull<[IndexPart<T>]>,
    ) -> Self {
        Self {
            data,
            indexes,
            r#break: constants.r#break,
            r#continue: constants.r#continue,
        }
    }
}

pub trait ExternalRecursionlessSizeBalancedTreeBaseAbstract<T: LinkType>:
    NoRecurSzbTree<T> + LinksTree<T>
{
    fn get_header(&self) -> &LinksHeader<T>;

    fn get_mut_header(&mut self) -> &mut LinksHeader<T>;

    fn get_index_part(&self, link: T) -> &IndexPart<T>;

    fn get_mut_index_part(&mut self, link: T) -> &mut IndexPart<T>;

    fn get_data_part(&self, link: T) -> &DataPart<T>;

    fn get_mut_data_part(&mut self, link: T) -> &mut DataPart<T>;

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
        let link = self.get_data_part(index);
        Link::new(index, link.source, link.target)
    }
}
