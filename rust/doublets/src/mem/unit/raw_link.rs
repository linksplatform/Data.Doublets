use num::LinkType;

#[derive(Debug, Default, PartialEq, Eq, Hash, Clone)]
#[repr(C)]
pub struct LinkPart<T: LinkType> {
    pub(crate) source: T,
    pub(crate) target: T,
    pub(crate) left_as_source: T,
    pub(crate) right_as_source: T,
    pub(crate) size_as_source: T,
    pub(crate) left_as_target: T,
    pub(crate) right_as_target: T,
    pub(crate) size_as_target: T,
}
