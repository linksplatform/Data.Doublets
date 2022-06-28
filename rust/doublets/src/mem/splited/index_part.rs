use num::LinkType;

#[derive(Debug, Default, Eq, PartialEq, Hash, Clone)]
pub struct IndexPart<T: LinkType> {
    pub root_as_source: T,
    pub left_as_source: T,
    pub right_as_source: T,
    pub size_as_source: T,

    pub root_as_target: T,
    pub left_as_target: T,
    pub right_as_target: T,
    pub size_as_target: T,
}
