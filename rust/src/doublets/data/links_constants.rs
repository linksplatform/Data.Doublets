use std::ops::RangeInclusive;

use num_traits::{one, zero, One, Zero};

use crate::doublets::data::hybrid::Hybrid;
use crate::doublets::data::ilinks::IGenericLinks;
use crate::num::LinkType;
use std::default::default;

#[derive(Clone, Eq, PartialEq)]
pub struct LinksConstants<T: LinkType> {
    pub index_part: T,
    pub source_part: T,
    pub target_part: T,
    pub r#break: T,
    pub null: T,
    pub r#continue: T,
    pub skip: T,
    pub any: T,
    pub itself: T,
    pub internal_range: RangeInclusive<T>,
    pub external_range: Option<RangeInclusive<T>>,
}

// TODO: use Options style
impl<T: LinkType> LinksConstants<T> {
    fn default_target_part() -> T {
        T::one() + one()
    }

    pub fn full_new(
        target_part: T,
        internal: RangeInclusive<T>,
        external: Option<RangeInclusive<T>>,
    ) -> Self {
        let one = one();
        let two = one + one;
        let three = two + one;
        let four = three + one;
        Self {
            index_part: zero(),
            source_part: one,
            target_part,
            r#break: default(),
            null: default(),
            r#continue: *internal.end(),
            skip: *internal.end() - one,
            any: *internal.end() - two,
            itself: *internal.end() - three,
            internal_range: *internal.start()..=*internal.end() - four,
            external_range: external,
        }
    }

    // TODO: enough for now
    pub fn via_external(target_part: T, external: bool) -> Self {
        Self::full_new(
            target_part,
            Self::default_internal(external),
            Self::default_external(external),
        )
    }

    pub fn via_ranges(internal: RangeInclusive<T>, external: Option<RangeInclusive<T>>) -> Self {
        Self::full_new(Self::default_target_part(), internal, external)
    }

    pub fn via_only_external(external: bool) -> Self {
        Self::via_external(Self::default_target_part(), external)
    }

    pub fn new() -> Self {
        Self::via_only_external(false)
    }

    fn default_internal(external: bool) -> RangeInclusive<T> {
        if external {
            one()..=Hybrid::half()
        } else {
            one()..=T::MAX
        }
    }

    fn default_external(external: bool) -> Option<RangeInclusive<T>> {
        if external {
            Some(Hybrid::external_zero()..=T::MAX)
        } else {
            None
        }
    }

    // TODO: Extensions
    pub fn is_internal_reference(&self, address: T) -> bool {
        self.internal_range.contains(&address)
    }

    pub fn is_external_reference(&self, address: T) -> bool {
        self.external_range
            .clone()
            .map_or(false, |range| range.contains(&address))
    }

    fn is_reference(&self, address: T) -> bool {
        self.is_internal_reference(address) || self.is_external_reference(address)
    }
}
