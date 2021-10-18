use std::default::default;
use crate::doublets::ILinks;
use crate::num::LinkType;
use crate::sequences::criterion::Criterion;
use crate::sequences::freq::counters::{Counter, SymbolFreq};

#[derive(Debug, Clone)]
pub struct MarkedSymbolFreq<T: LinkType, C: Criterion<T> + Clone> {
    base: SymbolFreq<T>,
    criterion: C, // TODO: rename to `marked_mather`
}

impl<T: LinkType, C: Criterion<T> + Clone> MarkedSymbolFreq<T, C> {
    pub fn new(criterion: C, sequence: T, symbol: T) -> Self {
        Self {
            base: SymbolFreq::new(sequence, symbol),
            criterion,
        }
    }
}

impl<T: LinkType, C: Criterion<T> + Clone> Counter<T> for MarkedSymbolFreq<T, C> {
    fn count<Links: ILinks<T>>(&mut self, links: &Links, _: ()) -> T {
        if !self.criterion.is_match(links, self.base.sequence) {
            default()
        } else {
            self.base.count(links, ())
        }
    }
}
