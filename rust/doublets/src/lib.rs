#![feature(step_trait)]
#![feature(associated_type_bounds)]
#![feature(default_free_fn)]
#![feature(box_syntax)]
#![feature(duration_constants)]
#![feature(test)]
#![feature(ptr_internals)]
#![feature(allocator_api)]
#![feature(slice_ptr_get)]
#![feature(try_blocks)]
#![feature(backtrace)]
#![feature(libstd_sys_internals)]
#![feature(try_trait_v2)]
#![feature(fn_traits)]
#![feature(bench_black_box)]
#![feature(cow_is_borrowed)]
#![feature(control_flow_enum)]
#![feature(type_alias_impl_trait)]
#![feature(trait_alias)]
#![feature(unboxed_closures)]
#![feature(slice_ptr_len)]
#![feature(nonnull_slice_from_raw_parts)]
#![feature(layout_for_ptr)]
#![feature(generic_const_exprs)]

pub mod data;
pub mod mem;
pub mod num;
pub mod test_extensions;
pub use crate::mem::splited;
pub use crate::mem::united;

pub(crate) use crate::data::LinksError;
pub use crate::data::{Doublet, Doublets, Handler, Link, StoppedHandler};

pub type Error<T> = LinksError<T>;
