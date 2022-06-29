// Copyright from https://gist.github.com/DutchGhost/5a6712f3d8b611ccbed6a9f120929e90

use std::{
    ops::{Generator, GeneratorState},
    pin::Pin,
};

/// A wrapper struct around Generators,
/// providing a safe implementation of the [`Iterator`] trait.
pub struct GenIter<G>(Option<G>);

impl<G> Unpin for GenIter<G> {}

impl<G: Generator + Unpin> GenIter<G> {
    /// Creates a new `GenIter` instance from a generator.
    /// The returned instance can be iterated over,
    /// consuming the generator.
    #[inline]
    pub fn new(gen: G) -> Self {
        Self(Some(gen))
    }
}

impl<G: Generator + Unpin> Iterator for GenIter<G> {
    type Item = G::Yield;

    #[inline]
    fn next(&mut self) -> Option<Self::Item> {
        Pin::new(self).next()
    }
}

impl<G: Generator + Unpin> GenIter<G> {
    /// Creates a new `GenIter` instance from a generator.
    ///
    /// The returned instance can be iterated over,
    /// consuming the generator.
    ///
    /// # Safety
    /// This function is marked unsafe,
    /// because the caller must ensure the generator is in a valid state.
    /// A valid state means that the generator has not been moved ever since it's creation.
    #[inline]
    pub unsafe fn new_unchecked(gen: G) -> Self {
        Self(Some(gen))
    }
}

impl<G: Generator> Iterator for Pin<&mut GenIter<G>> {
    type Item = G::Yield;

    fn next(&mut self) -> Option<Self::Item> {
        let this: Pin<&mut GenIter<G>> = self.as_mut();

        // This should be safe.
        // this Iterator implementation is on a Pin<&mut GenIter<G>> where G: Generator.
        // In order to acquire such a Pin<&mut GenIter<G>> if G does *NOT* implement Unpin,
        // the unsafe `new_unchecked` function from the Pin type must be used anyway.
        //
        // Note that if G: Unpin, the Iterator implementation of GenIter<G> itself is used,
        // which just creates a Pin safely, and then delegates to this implementation.
        let gen: Pin<&mut Option<G>> = unsafe { this.map_unchecked_mut(|geniter| &mut geniter.0) };

        let gen: Option<Pin<&mut G>> = Option::as_pin_mut(gen);

        match gen.map(|g| g.resume(())) {
            Some(GeneratorState::Yielded(y)) => Some(y),
            Some(GeneratorState::Complete(_)) => {
                self.set(GenIter(None));
                None
            }
            None => None,
        }
    }
}

/// Creates a new instance of a `GenIter` with the provided generator `$x`.
/// # Examples
/// ```
/// #![feature(generators, generator_trait)]
///
/// use doublets::generator;
///
/// let x = 10;
/// let mut iter = generator! {
///     let r = &x;
///
///     for i in 0..5u32 {
///         yield i * *r
///     }
/// };
/// ```
#[macro_export]
macro_rules! generator {
    ($($x:tt)*) => {

        // Safe, the Generator is directly passed into new_unchecked,
        // so it has not been moved
        unsafe {
            $crate::data::GenIter::new_unchecked( || {
                $($x)*
            })
        }
    };
}
