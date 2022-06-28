use num::LinkType;

#[derive(Debug, Default, Eq, PartialEq, Hash, Clone)]
pub struct DataPart<T: LinkType> {
    pub source: T,
    pub target: T,
}
