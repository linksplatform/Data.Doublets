#![feature(box_syntax)]
#![feature(thread_local)]
#![feature(try_blocks)]
#![feature(cell_update)]

use std::alloc::{alloc, Layout};
use std::cell::{Cell, RefCell};
use std::ffi::{CStr, CString, OsStr, OsString};
use std::fmt::Write;
use std::panic::catch_unwind;
use std::path::Path;
use std::ptr;
use std::ptr::{drop_in_place, null_mut};
use std::slice;
use std::time::{SystemTime, UNIX_EPOCH};

use doublets::doublets::{
    data::LinksConstants, mem::united::Links, ILinks, ILinksExtensions, LinksError,
};
use doublets::mem::FileMappedMem;
use doublets::num::LinkType;
use ffi_attributes as ffi;
use libc::c_char;
use libc::c_void;

type UnitedLinks<T> = Links<T, FileMappedMem>;

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_New"
)]
unsafe fn new_united_links<T: LinkType>(path: *const c_char) -> *mut c_void {
    let result: Result<_, LinksError<T>> = try {
        let path = CStr::from_ptr(path);
        let path = path.to_str().unwrap();
        let path = OsStr::new(path);
        let mem = FileMappedMem::new(path)?;
        let links =
            box UnitedLinks::<T>::with_constants(mem, LinksConstants::via_only_external(true))?;
        Box::into_raw(links) as *mut c_void
    };
    result.unwrap_or(null_mut())
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Drop"
)]
unsafe fn drop_united_links<T: LinkType>(this: *mut c_void) {
    let links = &mut *(this as *mut UnitedLinks<T>);
    drop_in_place(links);
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Create"
)]
unsafe fn create_united<T: LinkType>(this: *mut c_void) -> T {
    let result = {
        let links = &mut *(this as *mut UnitedLinks<T>);
        links.create()
    };
    result.unwrap_or(T::zero())
}
#[repr(C)]
struct Link<T: LinkType> {
    index: T,
    source: T,
    target: T,
}

type EachCallback<T> = extern "C" fn(Link<T>) -> T;

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Each"
)]
unsafe fn each_united<T: LinkType>(
    this: *mut c_void,
    callback: EachCallback<T>,
    query: *const T,
    len: usize,
) -> T {
    let links = &mut *(this as *mut UnitedLinks<T>);
    let slice = slice::from_raw_parts(query, len);
    let capture = move |link: doublets::doublets::Link<_>| {
        let new = Link {
            index: link.index,
            source: link.source,
            target: link.target,
        };
        callback(new)
    };
    match slice.len() {
        0 => links.each(capture),
        1 => links.each_by(capture, [slice[0]]),
        2 => links.each_by(capture, [slice[0], slice[1]]),
        3 => links.each_by(capture, [slice[0], slice[1], slice[2]]),
        _ => panic!("UB"),
    }
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Count"
)]
unsafe fn count_united<T: LinkType>(this: *mut c_void, query: *const T, len: usize) -> T {
    let links = &mut *(this as *mut UnitedLinks<T>);
    let slice = slice::from_raw_parts(query, len);
    match slice.len() {
        0 => links.count(),
        1 => links.count_by([slice[0]]),
        2 => links.count_by([slice[0], slice[1]]),
        3 => links.count_by([slice[0], slice[1], slice[2]]),
        _ => panic!("UB"),
    }
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Update"
)]
unsafe fn update_united<T: LinkType>(this: *mut c_void, index: T, source: T, target: T) -> T {
    let result = {
        let links = &mut *(this as *mut UnitedLinks<T>);
        links.update(index, source, target)
    };
    result.unwrap_or(T::zero())
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Delete"
)]
unsafe fn delete_united<T: LinkType>(this: *mut c_void, index: T) -> T {
    let result = {
        let links = &mut *(this as *mut UnitedLinks<T>);
        links.delete(index)
    };
    result.unwrap_or(T::zero())
}
