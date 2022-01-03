#![feature(step_trait)]
#![feature(associated_type_bounds)]
#![feature(default_free_fn)]
#![feature(box_syntax)]
#![feature(duration_constants)]
#![feature(with_options)]
#![feature(option_result_unwrap_unchecked)]
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
#![feature(in_band_lifetimes)]
#![feature(const_fn_trait_bound)]
#![feature(cow_is_borrowed)]
#![feature(control_flow_enum)]
#![feature(type_alias_impl_trait)]
#![feature(trait_alias)]
#![feature(unboxed_closures)]
#![feature(slice_ptr_len)]
#![feature(nonnull_slice_from_raw_parts)]
#![feature(layout_for_ptr)]

use crate::doublets::mem::united::{Links, NewList, NewTree, UpdatePointersSplit};
use crate::doublets::mem::{splited, ILinksListMethods, ILinksTreeMethods, UpdatePointers};
use crate::mem::ResizeableMem;
use crate::num::LinkType;

pub mod bench;
pub mod doublets;
pub mod mem;
pub mod methods;
pub mod num;
pub mod test_extensions;
pub mod tests;

unsafe impl<
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    > Sync for Links<T, M, TS, TT, TU>
{
}

unsafe impl<
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    > Send for Links<T, M, TS, TT, TU>
{
}

unsafe impl<
        T: LinkType,
        MD: ResizeableMem,
        MI: ResizeableMem,
        IS: ILinksTreeMethods<T> + UpdatePointersSplit,
        ES: ILinksTreeMethods<T> + UpdatePointersSplit,
        IT: ILinksTreeMethods<T> + UpdatePointersSplit,
        ET: ILinksTreeMethods<T> + UpdatePointersSplit,
        UL: ILinksListMethods<T> + UpdatePointers,
    > Sync for splited::Links<T, MD, MI, IS, ES, IT, ET, UL>
{
}

unsafe impl<
        T: LinkType,
        MD: ResizeableMem,
        MI: ResizeableMem,
        IS: ILinksTreeMethods<T> + UpdatePointersSplit,
        ES: ILinksTreeMethods<T> + UpdatePointersSplit,
        IT: ILinksTreeMethods<T> + UpdatePointersSplit,
        ET: ILinksTreeMethods<T> + UpdatePointersSplit,
        UL: ILinksListMethods<T> + UpdatePointers,
    > Send for splited::Links<T, MD, MI, IS, ES, IT, ET, UL>
{
}

pub mod prelude {
    pub use crate::doublets::data::{AddrToRaw, LinksConstants, Query, RawToAddr};
    pub use crate::doublets::{Doublet, Flow, Link, LinksError};
    pub use crate::num::LinkType;
}
