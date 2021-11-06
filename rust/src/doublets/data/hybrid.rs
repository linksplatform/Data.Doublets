use num_traits::{abs, one, Signed, zero};

use crate::num::{LinkType, ToSigned};

#[derive(Debug, Clone, Copy, Hash, PartialOrd, PartialEq, Ord, Eq)]
pub struct Hybrid<T: LinkType> {
    value: T,
}

impl<T: LinkType> Hybrid<T> {
    pub fn new(value: T) -> Self {
        Self::internal(value)
    }

    pub fn external(value: T) -> Self {
        Self {
            value: Self::extend_value(value),
        }
    }

    pub fn internal(value: T) -> Self {
        Self {
            value
        }
    }

    fn extend_value(value: T) -> T {
        if value == zero() {
            Self::external_zero()
        } else {
            T::MAX - value + one()
        }
    }

    pub fn half() -> T {
        T::MAX / T::from_u8(2).unwrap()
    }

    pub fn external_zero() -> T {
        Self::half() - one()
    }

    pub fn is_nothing(&self) -> bool {
        self.value == Self::external_zero() || self.signed() == zero()
    }

    pub fn is_internal(&self) -> bool {
        self.signed() > zero()
    }

    pub fn is_external(&self) -> bool {
        self.value == Self::external_zero() || self.signed() < zero()
    }

    pub fn signed(&self) -> <T as ToSigned>::Type {
        self.value.to_signed()
    }

    pub fn absolute(&self) -> T {
        let abs = if self.value == Self::external_zero() {
            zero()
        } else {
            abs(self.signed())
        }.abs();
        T::from(abs).unwrap()
    }

    pub fn as_value(&self) -> T {
        self.value
    }
}
