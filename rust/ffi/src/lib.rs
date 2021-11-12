#![feature(box_syntax)]

use std::ffi::{CStr, CString, OsStr, OsString};
use std::path::Path;
use std::ptr;
use std::ptr::drop_in_place;
use std::slice;

use doublets::doublets::{data::LinksConstants, mem::united::Links, ILinks, ILinksExtensions};
use doublets::mem::FileMappedMem;
use doublets::num::LinkType;
use libc::c_char;
use libc::c_void;

type TLink = u64;
type UnitedLinks = Links<TLink, FileMappedMem>;

#[no_mangle]
unsafe extern "C" fn NewUnitedLinks(path: *const c_char) -> *mut c_void {
    let path = CStr::from_ptr(path);
    let path = path.to_str().unwrap(); // TODO: assumes valid string
    let path = OsStr::new(path);
    let mem = FileMappedMem::new(path).unwrap();
    let links = box UnitedLinks::with_constants(mem, LinksConstants::via_only_external(true));

    Box::into_raw(links) as *mut c_void
}

#[no_mangle]
unsafe extern "C" fn DropUnitedLinks(this: *mut c_void) {
    let links = &mut *(this as *mut UnitedLinks);
    drop_in_place(links)
}

#[no_mangle]
unsafe extern "C" fn Create(this: *mut c_void) -> TLink {
    let links = &mut *(this as *mut UnitedLinks);

    links.create()
}

#[repr(C)]
struct Link<T: LinkType> {
    index: T,
    source: T,
    target: T,
}

type EachCallback = extern "C" fn(Link<TLink>) -> TLink;

#[no_mangle]
unsafe extern "C" fn Each(
    this: *mut c_void,
    callback: EachCallback,
    query: *const TLink,
    len: usize,
) -> TLink {
    let links = &mut *(this as *mut UnitedLinks);

    let slice = slice::from_raw_parts(query, len);
    let capture = move |link: doublets::doublets::Link<_>| {
        let new = Link {
            index: link.index,
            source: link.source,
            target: link.target,
        };
        callback(new)
    };

    // TODO: [maybe] bug in trait
    match slice.len() {
        0 => links.each(capture),
        1 => links.each_by(capture, [slice[0]]),
        2 => links.each_by(capture, [slice[0], slice[1]]),
        3 => links.each_by(capture, [slice[0], slice[1], slice[2]]),
        _ => {
            panic!("UB")
        }
    }
}

#[no_mangle]
unsafe extern "C" fn Count(this: *mut c_void) -> TLink {
    let links = &mut *(this as *mut UnitedLinks);

    links.count()
}

#[no_mangle]
unsafe extern "C" fn Update(
    this: *mut c_void,
    index: TLink,
    source: TLink,
    target: TLink,
) -> TLink {
    let links = &mut *(this as *mut UnitedLinks);

    links.update(index, source, target)
}

#[no_mangle]
unsafe extern "C" fn Delete(this: *mut c_void, index: TLink) -> TLink {
    let links = &mut *(this as *mut UnitedLinks);

    links.delete(index)
}
