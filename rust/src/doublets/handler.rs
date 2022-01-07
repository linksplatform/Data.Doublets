use crate::doublets::Flow;
use crate::doublets::Link;
use crate::LinkType;
use std::marker::PhantomData;
use std::ops::Try;

pub trait Handler<T: LinkType, R: Try<Output = (), Residual: Send> + Send> =
    FnMut(Link<T>, Link<T>) -> R;

pub struct StoppedHandler<T, F, R>
where
    T: LinkType,
    F: FnMut(Link<T>, Link<T>) -> R + Send,
    R: Try<Output = (), Residual: Send> + Send,
{
    handler: F,
    handle: bool,
    _marker1: PhantomData<R>,
    _marker2: PhantomData<T>,
}

impl<T, F, R> StoppedHandler<T, F, R>
where
    T: LinkType,
    F: FnMut(Link<T>, Link<T>) -> R + Send,
    R: Try<Output = (), Residual: Send> + Send,
{
    pub fn new(handler: F) -> Self {
        StoppedHandler {
            handler,
            handle: true,
            _marker1: PhantomData,
            _marker2: PhantomData,
        }
    }
}

impl<T, F, R> From<F> for StoppedHandler<T, F, R>
where
    T: LinkType,
    F: FnMut(Link<T>, Link<T>) -> R + Send,
    R: Try<Output = (), Residual: Send> + Send,
{
    fn from(handler: F) -> Self {
        Self::new(handler)
    }
}

impl<T, F, R> FnOnce<(Link<T>, Link<T>)> for StoppedHandler<T, F, R>
where
    F: FnMut(Link<T>, Link<T>) -> R + Send,
    R: Try<Output = (), Residual: Send> + Send,
    T: LinkType,
{
    type Output = Flow;

    extern "rust-call" fn call_once(self, args: (Link<T>, Link<T>)) -> Self::Output {
        self.handler.call_once(args).branch().into()
    }
}

impl<T, F, R> FnMut<(Link<T>, Link<T>)> for StoppedHandler<T, F, R>
where
    T: LinkType,
    F: FnMut(Link<T>, Link<T>) -> R + Send,
    R: Try<Output = (), Residual: Send> + Send,
{
    extern "rust-call" fn call_mut(&mut self, args: (Link<T>, Link<T>)) -> Self::Output {
        if self.handle {
            let result: R = self.handler.call_mut(args);
            if result.branch().is_break() {
                self.handle = false;
                Flow::Break
            } else {
                Flow::Continue
            }
        } else {
            Flow::Break
        }
    }
}
