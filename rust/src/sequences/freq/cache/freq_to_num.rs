use crate::doublets::Doublet;
use crate::num::LinkType;
use crate::sequences::criterion::Criterion;
use crate::sequences::freq::cache::FreqCache;
use crate::sequences::freq::counters::Counter;

pub struct FreqToNum<T: LinkType, C: Counter<T, T> + Clone> {
    cache: FreqCache<T, C>
}

impl<T: LinkType, C: Counter<T, T> + Clone> FreqToNum<T, C> {
    pub fn new(cache: FreqCache<T, C>) -> Self {
        Self { cache }
    }

    // TODO: create `Converter` trait
    pub fn convert(&mut self, doublet: Doublet<T>) -> T {
        self.cache.get_freq(doublet)
    }
}
