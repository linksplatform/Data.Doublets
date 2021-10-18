use crate::doublets::{Doublet, ILinks, ILinksExtensions};
use crate::num::LinkType;
use crate::sequences::freq::counters::Counter;
use num_traits::one;
use std::collections::HashMap;
use std::default::default;

pub struct FreqCache<T: LinkType, C: Counter<T, T> + Clone> {
    cache: HashMap<Doublet<T>, T>,
    counter: C,
}

impl<T: LinkType, C: Counter<T, T> + Clone> FreqCache<T, C> {
    pub fn new(counter: C) -> Self {
        Self {
            cache: HashMap::with_capacity(4096),
            counter,
        }
    }

    pub fn get_freq(&mut self, doublet: Doublet<T>) -> T {
        *self.cache.entry(doublet).or_default()
    }

    pub fn inc<Links: ILinks<T>>(&mut self, links: &Links, doublet: Doublet<T>) -> T {
        let mut counter = self.counter.clone();
        let val = *self
            .cache
            .entry(doublet)
            .and_modify(|e| *e = *e + one())
            .or_insert_with_key(|doublet| {
                let link = links.search_or(doublet.source, doublet.target, default());
                let mut freq = one();
                if link != default() {
                    freq = freq + counter.count(links, link);
                }
                return freq;
            });
        self.counter = counter;
        return val;
    }
}
