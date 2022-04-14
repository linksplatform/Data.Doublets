use num::LinkType;

#[derive(PartialEq, Eq, Hash, Clone, Debug, Copy)]
pub struct LinksHeader<T: LinkType> {
    pub allocated: T,
    pub reserved: T,
    pub free: T,
    pub first_free: T,
    pub root_as_source: T,
    pub root_as_target: T,
    pub last_free: T,

    __reserved_8: T,
}
