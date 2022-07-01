#![feature(try_blocks)]
#![feature(box_syntax)]
#![feature(try_trait_v2)]

use data::Flow;
use std::{
    error::Error,
    ffi::CStr,
    fs::File,
    ops::{RangeInclusive, Try},
    ptr::{drop_in_place, null_mut},
};

use doublets::{
    data::{
        query,
        Flow::{Break, Continue},
        LinksConstants, Query,
    },
    num::LinkType,
};
use libc::{c_char, c_void};
use log::{error, warn};

use doublets::{mem, parts, unit, Doublets};
use ffi_attributes as ffi;

fn result_into_log<R, E: std::fmt::Display>(result: Result<R, E>, default: R) -> R {
    match result {
        Ok(r) => r,
        Err(e) => {
            error!("{}", e);
            default
        }
    }
}

fn query_from_raw<T: LinkType>(query: *const T, len: usize) -> Query<'static, T> {
    if query.is_null() && len != 0 {
        warn!("If `query` is null then `len` must be 0");
    }

    if query.is_null() {
        query![]
    } else {
        Query::new(unsafe { std::slice::from_raw_parts(query, len) })
    }
}

fn unnul_or_error<'a, Ptr, R>(ptr: *mut Ptr) -> &'a mut R {
    if ptr.is_null() {
        // todo: use std::Backtrace or crates/tracing
        error!("Null pointer");
        panic!("Null pointer");
    } else {
        unsafe { &mut *(ptr as *mut R) }
    }
}

// TODO: remove ::mem:: in doublets crate
type UnitedLinks<T> = unit::Store<T, mem::FileMappedMem<parts::LinkPart<T>>>;

// type WrappedLinks<T> = env_decorators::env_type!("JS_LETS_GO", "<T, *>", UnitedLinks<T>);
type WrappedLinks<T> = UnitedLinks<T>;

type EachCallback<T> = extern "C" fn(Link<T>) -> T;

type CUDCallback<T> = extern "C" fn(before: Link<T>, after: Link<T>) -> T;

#[repr(C)]
#[derive(Eq, PartialEq)]
pub struct Range<T: LinkType>(pub T, pub T);

#[repr(C)]
pub struct Constants<T: LinkType> {
    pub index_part: T,
    pub source_part: T,
    pub target_part: T,
    pub null: T,
    pub r#continue: T,
    pub r#break: T,
    pub skip: T,
    pub any: T,
    pub itself: T,
    pub error: T,
    pub internal_range: Range<T>,
    pub external_range: Range<T>,
    pub _opt_marker: bool,
}

impl<T: LinkType> From<LinksConstants<T>> for Constants<T> {
    fn from(c: LinksConstants<T>) -> Self {
        Self {
            index_part: c.index_part,
            source_part: c.source_part,
            target_part: c.target_part,
            null: c.null,
            r#continue: c.r#continue,
            r#break: c.r#break,
            skip: c.skip,
            any: c.any,
            itself: c.itself,
            error: c.error,
            internal_range: Range(*c.internal_range.start(), *c.internal_range.end()),
            // external_range: c.external_range.map(|r| Range(*r.start(), *r.end())),
            external_range: c
                .clone()
                .external_range
                .map_or(Range(T::zero(), T::zero()), |r| Range(*r.start(), *r.end())),
            _opt_marker: c.external_range.is_some(),
        }
    }
}

impl<T: LinkType> Into<LinksConstants<T>> for Constants<T> {
    fn into(self) -> LinksConstants<T> {
        LinksConstants {
            index_part: self.index_part,
            source_part: self.source_part,
            target_part: self.target_part,
            r#break: self.r#break,
            null: self.null,
            r#continue: self.r#continue,
            skip: self.skip,
            any: self.any,
            itself: self.itself,
            error: self.error,
            internal_range: RangeInclusive::new(self.internal_range.0, self.internal_range.1),
            external_range: if self._opt_marker {
                Some(RangeInclusive::new(
                    self.external_range.0,
                    self.external_range.1,
                ))
            } else {
                None
            },
        }
    }
}

type DLink<T> = doublets::Link<T>;

#[repr(C)]
pub struct Link<T: LinkType> {
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
    name = "*Links_New"
)]
fn new_united_links<T: LinkType>(path: *const c_char) -> *mut c_void {
    new_with_constants_united_links::<T>(path, LinksConstants::external().into())
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*Links_NewWithConstants"
)]
fn new_with_constants_united_links<T: LinkType>(
    path: *const c_char,
    constants: Constants<T>,
) -> *mut c_void {
    let result: Result<_, Box<dyn Error>> = try {
        let path = unsafe { CStr::from_ptr(path) }.to_str()?;
        let file = File::options()
            .create(true)
            .read(true)
            .write(true)
            .open(path)?;
        let mem = mem::FileMappedMem::new(file)?;
        let links = box env_decorators::env_value!(
            "JS_LETS_GO",
            UnitedLinks::<T>::with_constants(mem, constants.into())?
        );
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
    name = "*Links_Drop"
)]
unsafe fn drop_united_links<T: LinkType>(this: *mut c_void) {
    let links: &mut WrappedLinks<T> = unnul_or_error(this);
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
    name = "*Links_GetConstants"
)]
unsafe fn get_constants_united_links<T: LinkType>(this: *mut c_void) -> Constants<T> {
    let links: &mut WrappedLinks<T> = unnul_or_error(this);
    links.constants().into()
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*Links_Create"
)]
fn create_united<T: LinkType>(
    this: *mut c_void,
    query: *const T,
    len: usize,
    callback: CUDCallback<T>,
) -> T {
    let links: &mut WrappedLinks<T> = unnul_or_error(this);
    let continue_ = links.constants().r#continue;
    let break_ = links.constants().r#break;
    let result = {
        let query = query_from_raw(query, len);
        let handler = |before: DLink<_>, after: DLink<_>| {
            if callback(before.into(), after.into()) == continue_ {
                Break
            } else {
                Continue
            }
        };
        links.create_by_with(query, handler)
    };
    result_into_log(
        result.map(|flow| {
            if flow.branch().is_continue() {
                continue_
            } else {
                break_
            }
        }),
        links.constants().error,
    )
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*Links_SmartCreate"
)]
fn smart_create_united<T: LinkType>(this: *mut c_void) -> T {
    let links: &mut WrappedLinks<T> = unnul_or_error(this);
    let result = links.create();
    result_into_log(result, links.constants().error)
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*Links_SmartUpdate"
)]
fn smart_update_united<T: LinkType>(this: *mut c_void, index: T, source: T, target: T) -> T {
    let links: &mut WrappedLinks<T> = unnul_or_error(this);
    let result = links.update(index, source, target);
    result_into_log(result, links.constants().error)
}
/*
#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*Links_Each"
)]*/
fn each_united<T: LinkType>(
    this: *mut c_void,
    query: *const T,
    len: usize,
    callback: EachCallback<T>,
) -> T {
    let links: &mut WrappedLinks<T> = unnul_or_error(this);
    let query = query_from_raw(query, len);
    let handler = move |link: DLink<_>| callback(link.into());
    let r#continue = links.constants().r#continue;
    let r#break = links.constants().r#break;
    let result = links.each_by(query, move |link| {
        if handler(link) == r#continue {
            Continue
        } else {
            Break
        }
    });
    match result {
        Continue => r#continue,
        Break => r#break,
    }
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*Links_Count"
)]
unsafe fn count_united<T: LinkType>(this: *mut c_void, query: *const T, len: usize) -> T {
    let links: &mut WrappedLinks<T> = unnul_or_error(this);
    let query = query_from_raw(query, len);
    links.count_by(query)
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*Links_Update"
)]
unsafe fn update_united<T: LinkType>(
    this: *mut c_void,
    restrictions: *const T,
    len_r: usize,
    substitutuion: *const T,
    len_s: usize,
    callback: CUDCallback<T>,
) -> T {
    let restrictions = query_from_raw(restrictions, len_r);
    let substitutuion = query_from_raw(substitutuion, len_s);
    let links: &mut WrappedLinks<T> = unnul_or_error(this);
    let continue_ = links.constants().r#continue;
    let break_ = links.constants().r#break;
    let result = {
        let handler = move |before: DLink<T>, after: DLink<T>| {
            if callback(before.into(), after.into()) == continue_ {
                Continue
            } else {
                Break
            }
        };
        links.update_by_with(restrictions, substitutuion, handler)
    };
    result_into_log(
        result.map(|flow| {
            if flow.branch().is_continue() {
                continue_
            } else {
                break_
            }
        }),
        links.constants().error,
    )
}

#[ffi::specialize_for(
    types = "u8",
    types = "u16",
    types = "u32",
    types = "u64",
    convention = "csharp",
    name = "*Links_Delete"
)]
unsafe fn delete_united<T: LinkType>(
    this: *mut c_void,
    query: *const T,
    len: usize,
    callback: CUDCallback<T>,
) -> T {
    let query = query_from_raw(query, len);
    let links: &mut WrappedLinks<T> = unnul_or_error(this);
    let continue_ = links.constants().r#continue;
    let break_ = links.constants().r#break;
    let result = {
        let handler = move |before: DLink<_>, after: DLink<_>| {
            if callback(before.into(), after.into()) == break_ {
                Break
            } else {
                Continue
            }
        };
        links.delete_by_with(query, handler)
    };
    result_into_log(
        result.map(|flow| {
            if flow.branch().is_continue() {
                continue_
            } else {
                break_
            }
        }),
        links.constants().error,
    )
}

#[repr(C)]
pub struct SharedLogger {
    formatter: for<'a> extern "C" fn(&'a log::Record<'_>),
}

impl log::Log for SharedLogger {
    fn enabled(&self, _: &log::Metadata) -> bool {
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
        warn!("subscriber error: {}", err)
    }

    if let Err(err) = log::set_boxed_logger(Box::new(logger)) {
        warn!("{}", err)
    }
}

#[no_mangle]
pub extern "C" fn init_fmt_logger() {
    let logger = build_shared_logger();
    setup_shared_logger(logger);
}

mod tests {
    use log::{debug, info, trace};

    use super::*;

    #[test]
    fn error_log() {
        let logger = build_shared_logger();
        setup_shared_logger(logger);
        trace!("trace");
        debug!("debug");
        info!("info");
        warn!("warn");
        error!("error");
    }
}
