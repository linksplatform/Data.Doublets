use num::LinkType;

#[derive(Debug, Default, Eq, PartialEq, Hash, Clone)]
pub struct DataPart<T: LinkType> {
    pub(crate) source: T,
    pub(crate) target: T,
}
