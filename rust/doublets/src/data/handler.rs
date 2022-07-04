use crate::Link;
use data::Flow;
use num::LinkType;
use std::{marker::PhantomData, ops::Try};

pub trait Handler<T, R>: FnMut(Link<T>, Link<T>) -> R
where
    T: LinkType,
    R: Try<Output = ()>,
{
}
impl<T, R, All> Handler<T, R> for All
where
    T: LinkType,
    R: Try<Output = ()>,
    All: FnMut(Link<T>, Link<T>) -> R,
{
}

pub struct FuseHandler<T, H, R>
where
    T: LinkType,
    H: Handler<T, R>,
    R: Try<Output = ()>,
{
    handler: H,
    done: bool,
    _marker1: PhantomData<R>,
    _marker2: PhantomData<T>,
}

impl<T, F, R> FuseHandler<T, F, R>
where
    T: LinkType,
    F: FnMut(Link<T>, Link<T>) -> R,
    R: Try<Output = ()>,
{
    pub fn new(handler: F) -> Self {
        FuseHandler {
            handler,
            done: false,
            _marker1: PhantomData,
            _marker2: PhantomData,
        }
    }
}

impl<T, H, R> From<H> for FuseHandler<T, H, R>
where
    T: LinkType,
    H: Handler<T, R>,
    R: Try<Output = ()>,
{
    fn from(handler: H) -> Self {
        Self::new(handler)
    }
}

impl<T, H, R> FnOnce<(Link<T>, Link<T>)> for FuseHandler<T, H, R>
where
    H: FnMut(Link<T>, Link<T>) -> R,
    R: Try<Output = ()>,
    T: LinkType,
{
    type Output = Flow;

    extern "rust-call" fn call_once(self, args: (Link<T>, Link<T>)) -> Self::Output {
        self.handler.call_once(args).branch().into()
    }
}

impl<T, H, R> FnMut<(Link<T>, Link<T>)> for FuseHandler<T, H, R>
where
    T: LinkType,
    H: Handler<T, R>,
    R: Try<Output = ()>,
{
    extern "rust-call" fn call_mut(&mut self, args: (Link<T>, Link<T>)) -> Self::Output {
        if !self.done {
            let result = self.handler.call_mut(args);
            if result.branch().is_break() {
                self.done = false;
                Flow::Break
            } else {
                Flow::Continue
            }
        } else {
            Flow::Break
        }
    }
}
