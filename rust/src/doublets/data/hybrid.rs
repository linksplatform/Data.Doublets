use crate::num::{LinkType, ToSigned};
use num_traits::{AsPrimitive, one, abs, zero, ToPrimitive};

pub struct Hybrid<T: LinkType> {
    value: T
}

impl<T: LinkType> Hybrid<T> {
    pub fn new(value: T, external: bool) -> Self {
        Self { value: Self::extend_value(value, external) }
    }

    fn extend_value(value: T, external: bool) -> T {
        if value == zero() && external {
            Self::external_zero()
        }
        else
        {
            let absolute = value; 
            if external { T::MAX - absolute } else { absolute }
        }
    }

    pub fn half() -> T {
        T::MAX / T::from_i8(2).unwrap()
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
        let abs: usize = if self.value == Self::external_zero() {
            zero()
        } else {
            abs(self.signed())
        }.as_();
        T::from_usize(abs).unwrap()
    }

    pub fn as_usize(&self) -> usize {
        self.value.as_()
    }

    pub fn as_value(&self) -> T {
        self.value
    }
}