use crate::doublets::{ILinks, ILinksExtensions};
use crate::num::LinkType;
use crate::sequences::criterion::Criterion;
use std::default::default;

#[derive(Debug, Clone)]
pub struct MarkedMatch<T: LinkType> {
    sequence_marker: T,
}

impl<T: LinkType> MarkedMatch<T> {
    pub fn new(sequence_marker: T) -> Self {
        Self { sequence_marker }
    }
}

impl<T: LinkType> Criterion<T> for MarkedMatch<T> {
    fn is_match<Links: ILinks<T>>(&self, links: &Links, candidate: T) -> bool {
        // TODO:
        //  - expect error
        //  - create `get_source`
        links.get_link(candidate).unwrap().source == self.sequence_marker
            || links.search_or(self.sequence_marker, candidate, default()) != default()
    }
}
