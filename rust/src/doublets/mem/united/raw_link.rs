use crate::num::LinkType;

#[derive(PartialEq, Eq, Hash, Clone, Debug, Copy)]
pub struct RawLink<T: LinkType> {
    pub source: T,
    pub target: T,
    pub left_as_source: T,
    pub right_as_source: T,
    pub size_as_source: T,
    pub left_as_target: T,
    pub right_as_target: T,
    pub size_as_target: T,
}
