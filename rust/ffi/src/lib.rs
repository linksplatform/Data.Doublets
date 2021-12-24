#![feature(box_syntax)]
#![feature(thread_local)]
#![feature(try_blocks)]
#![feature(cell_update)]
#![feature(backtrace)]

use std::alloc::{alloc, Layout};
use std::cell::{Cell, RefCell};
use std::ffi::{CStr, CString, OsStr, OsString};
use std::fmt::{Display, Write};
use std::panic::catch_unwind;
use std::path::Path;
use std::ptr;
use std::ptr::{drop_in_place, null_mut};
use std::slice;
use std::time::{Instant, SystemTime, UNIX_EPOCH};

use doublets::doublets::data::Query;
use doublets::doublets::Link as DLink;
use doublets::doublets::{
    data::LinksConstants, mem::united::Links, ILinks, ILinksExtensions, LinksError,
};
use doublets::mem::FileMappedMem;
use doublets::num::LinkType;
use doublets::query;
use ffi_attributes as ffi;
use libc::c_char;
use libc::c_void;
use log::Metadata;
use log::{debug, error, info, trace, warn};
use std::error::Error;
use tracing_subscriber::fmt::format::Format;

type UnitedLinks<T> = Links<T, FileMappedMem>;

fn result_into_log<R, E: Display>(result: Result<R, E>, default: R) -> R {
    match result {
        Ok(r) => r,
        Err(e) => {
            error!("{}", e);
            default
        }
    }
}

fn query_from_raw<T: LinkType>(query: *const T, len: usize) -> Query<'static, T> {
    if !query.is_null() && len != 0 {
        warn!("If `query` is null then `len` must be 0");
    }

    if query.is_null() {
        query![]
    } else {
        Query::new(unsafe { slice::from_raw_parts(query, len) })
    }
}

fn unnul_or_error<'a, Ptr, R>(ptr: *mut Ptr) -> &'a mut R {
    if ptr.is_null() {
        // todo: use std::Backtrace or crates/tracing
        error!(
            "Null pointer at:\n {}",
            std::backtrace::Backtrace::capture()
        );
        panic!("Null pointer");
    } else {
        unsafe { &mut *(ptr as *mut R) }
    }
}

type EachCallback<T> = extern "C" fn(Link<T>) -> T;

type LinksCallback<T> = extern "C" fn(before: Link<T>, after: Link<T>) -> T;

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_New"
)]
fn new_united_links<T: LinkType>(path: *const c_char) -> *mut c_void {
    let result: Result<_, Box<dyn Error>> = try {
        let path = unsafe { CStr::from_ptr(path) }.to_str()?;
        let path = OsStr::new(path);
        let mem = FileMappedMem::new(path)?;
        let links =
            box UnitedLinks::<T>::with_constants(mem, LinksConstants::via_only_external(true))?;
        Box::into_raw(links) as *mut c_void
    };
    result_into_log(result, null_mut())
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Drop"
)]
fn drop_united_links<T: LinkType>(this: *mut c_void) {
    let links: &mut UnitedLinks<T> = unnul_or_error(this);
    unsafe {
        drop_in_place(links);
    }
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Create"
)]
fn create_united<T: LinkType>(this: *mut c_void, query: *const T, len: usize) -> T {
    let result = {
        let links: &mut UnitedLinks<T> = unnul_or_error(this);
        links.create()
    };
    result_into_log(result, T::default())
}
#[repr(C)]
struct Link<T: LinkType> {
    index: T,
    source: T,
    target: T,
}

impl<T: LinkType> From<DLink<T>> for Link<T> {
    fn from(link: DLink<T>) -> Self {
        Link {
            index: link.index,
            source: link.source,
            target: link.target,
        }
    }
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Each"
)]
fn each_united<T: LinkType>(
    this: *mut c_void,
    query: *const T,
    len: usize,
    callback: EachCallback<T>,
) -> T {
    let links: &mut UnitedLinks<T> = unnul_or_error(this);
    let query = query_from_raw(query, len);
    let handler = move |link: DLink<_>| callback(link.into());
    links.each_by(query, handler)
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
    let links: &mut UnitedLinks<T> = unnul_or_error(this);
    let query = query_from_raw(query, len);
    links.count_by(query)
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Update"
)]
unsafe fn update_united<T: LinkType>(
    this: *mut c_void,
    restrictions: *const T,
    len_r: usize,
    substitutuion: *const T,
    len_s: usize,
    callback: LinksCallback<T>,
) -> T {
    let restrictions = query_from_raw(restrictions, len_r);
    let substitutuion = query_from_raw(substitutuion, len_s);
    let result = {
        let links: &mut UnitedLinks<T> = unnul_or_error(this);
        let handler =
            move |before: DLink<T>, after: DLink<T>| callback(before.into(), after.into());
        links.update_by_with(restrictions, substitutuion, handler)
    };
    result_into_log(result, T::default())
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*UnitedMemoryLinks_Delete"
)]
unsafe fn delete_united<T: LinkType>(
    this: *mut c_void,
    query: *const T,
    len: usize,
    callback: LinksCallback<T>,
) -> T {
    let query = query_from_raw(query, len);
    let result = {
        let links: &mut UnitedLinks<T> = unnul_or_error(this);
        let handler =
            move |after: DLink<_>, before: DLink<_>| callback(after.into(), before.into());
        links.delete_by_with(query, handler)
    };
    result_into_log(result, T::default())
}

#[repr(C)]
pub struct SharedLogger {
    formatter: for<'a> extern "C" fn(&'a log::Record<'_>),
}

impl log::Log for SharedLogger {
    fn enabled(&self, _: &Metadata) -> bool {
        true
    }
    fn log(&self, record: &log::Record) {
        (self.formatter)(record)
    }
    fn flush(&self) {}
}

pub fn build_shared_logger() -> SharedLogger {
    extern "C" fn formatter(r: &log::Record<'_>) {
        tracing_log::format_trace(r).unwrap()
    }
    SharedLogger { formatter }
}
#[no_mangle]
pub extern "C" fn setup_shared_logger(logger: SharedLogger) {
    log::set_max_level(log::STATIC_MAX_LEVEL);

    let subscriber = tracing_subscriber::fmt::fmt()
        .with_max_level(tracing::Level::TRACE)
        .finish();
    if let Err(err) = tracing::subscriber::set_global_default(subscriber) {
        log::warn!("subscriber error: {}", err)
    }

    if let Err(err) = log::set_boxed_logger(Box::new(logger)) {
        log::warn!("{}", err)
    }
}

mod tests {
    use super::*;

    fn init_log() {
        let logger = build_shared_logger();
        setup_shared_logger(logger);
    }

    #[test]
    fn error_log() {
        init_log();

        trace!("trace");
        debug!("debug");
        info!("info");
        warn!("warn");
        error!("error");
    }
}
