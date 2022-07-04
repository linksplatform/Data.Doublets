#![feature(backtrace)]
#![feature(fn_traits)]
#![feature(generators)]
#![feature(try_trait_v2)]
#![feature(default_free_fn)]
#![feature(unboxed_closures)]
#![feature(nonnull_slice_from_raw_parts)]

pub mod data;
pub mod mem;
pub mod num;
pub mod test_extensions;
pub use self::mem::{parts, split, unit};

pub(crate) use self::data::Error as LinksError;
pub use self::data::{Doublet, Doublets, Error, FuseHandler, Handler, Link};
