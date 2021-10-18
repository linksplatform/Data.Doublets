use crate::num::LinkType;
use std::default::default;
use num_traits::one;
use crate::doublets::data::StoppableWalker;
use crate::doublets::{ILinks, ILinksExtensions};
use crate::sequences::freq::counters::Counter;

#[derive(Debug, Clone)]
pub struct SymbolFreq<T: LinkType> {
    pub(in crate::sequences::freq::counters) sequence: T,
    pub(in crate::sequences::freq::counters) symbol: T,
    pub(in crate::sequences::freq::counters) total: T,
}

impl<T: LinkType> SymbolFreq<T> {
    pub fn new(sequence: T, symbol: T) -> Self {
        Self {
            sequence,
            symbol,
            total: default(),
        }
    }
}

impl<T: LinkType> Counter<T> for SymbolFreq<T> {
    fn count<Links: ILinks<T>>(&mut self, links: &Links, _: ()) -> T {
        if self.total != default() {
            self.total
        } else {
            let mut total = default();
            StoppableWalker::walk_right(
                self.sequence,
                |index| links.get_link(index).unwrap().source, // TODO: expect error
                |index| links.get_link(index).unwrap().target, // TODO: expect error
                |index| index == self.symbol || links.is_partial_point(index),
                |index| { if index == self.symbol { total = total + one(); } true }
            );
            self.total = total;
            return self.total;
        }
    }
}
