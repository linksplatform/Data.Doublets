use num::LinkType;

#[derive(Debug, Default, Eq, PartialEq, Hash, Clone)]
pub struct IndexPart<T: LinkType> {
    pub(crate) root_as_source: T,
    pub(crate) left_as_source: T,
    pub(crate) right_as_source: T,
    pub(crate) size_as_source: T,

    pub(crate) root_as_target: T,
    pub(crate) left_as_target: T,
    pub(crate) right_as_target: T,
    pub(crate) size_as_target: T,
}
