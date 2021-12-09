use crate::num::LinkType;

#[derive(Debug, Eq, PartialEq, Hash, Clone)]
pub struct DataPart<T: LinkType> {
    pub source: T,
    pub target: T,
}
