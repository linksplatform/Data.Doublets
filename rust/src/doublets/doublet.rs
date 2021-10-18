use std::fmt::{Debug, Display, Formatter};
use crate::num::LinkType;

#[derive(Debug, Eq, PartialEq, Hash, Clone)]
pub struct Doublet<T: LinkType> {
    pub source: T,
    pub target: T,
}

impl<T: LinkType> Doublet<T> {
    pub fn new(source: T, target: T) -> Self {
        Self { source, target }
    }
}

impl<T: LinkType> Display for Doublet<T> {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}->{}", self.source, self.target)
    }
}
