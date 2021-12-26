use crate::doublets::Link;
use crate::LinkType;
use std::ops::Try;

trait Handler<T: LinkType, R: Try<Output = ()>> = FnMut(Link<T>, Link<T>) -> R;
