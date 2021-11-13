#![feature(box_syntax)]

use std::ffi::{CStr, CString, OsStr, OsString};
use std::path::Path;
use std::ptr;
use std::ptr::drop_in_place;
use std::slice;

use doublets::doublets::{data::LinksConstants, ILinks, ILinksExtensions, mem::united::Links};
use doublets::mem::FileMappedMem;
use doublets::num::LinkType;
use libc::c_char;
use libc::c_void;

type UnitedLinks<T> = Links<T, FileMappedMem>;

unsafe fn new_united_links<T: LinkType>(path: *const c_char) -> *mut c_void {
    let path = CStr::from_ptr(path);
    let path = path.to_str().unwrap();
    let path = OsStr::new(path);
    let mem = FileMappedMem::new(path).unwrap();
    let links = box UnitedLinks::<T>::with_constants(mem, LinksConstants::via_only_external(true));
    Box::into_raw(links) as *mut c_void
}

#[no_mangle]
unsafe extern "C" fn NewUnitedLinks_Uint8(path: *const c_char) -> *mut c_void {
    new_united_links::<u8>(path)
}

#[no_mangle]
unsafe extern "C" fn NewUnitedLinks_Uint16(path: *const c_char) -> *mut c_void {
    new_united_links::<u16>(path)
}

#[no_mangle]
unsafe extern "C" fn NewUnitedLinks_Uint32(path: *const c_char) -> *mut c_void {
    new_united_links::<u32>(path)
}

#[no_mangle]
unsafe extern "C" fn NewUnitedLinks_Uint64(path: *const c_char) -> *mut c_void {
    new_united_links::<u64>(path)
}

unsafe fn drop_united_links<T: LinkType>(this: *mut c_void) {
    let links = &mut *(this as *mut UnitedLinks<T>);
    drop_in_place(links)
}

#[no_mangle]
unsafe extern "C" fn DropUnitedLinks_Uint8(this: *mut c_void) -> () {
    drop_united_links::<u8>(this)
}

#[no_mangle]
unsafe extern "C" fn DropUnitedLinks_Uint16(this: *mut c_void) -> () {
    drop_united_links::<u16>(this)
}

#[no_mangle]
unsafe extern "C" fn DropUnitedLinks_Uint32(this: *mut c_void) -> () {
    drop_united_links::<u32>(this)
}

#[no_mangle]
unsafe extern "C" fn DropUnitedLinks_Uint64(this: *mut c_void) -> () {
    drop_united_links::<u64>(this)
}

unsafe fn create_united<T: LinkType>(this: *mut c_void) -> T {
    let links = &mut *(this as *mut UnitedLinks<T>);
    links.create()
}

#[no_mangle]
unsafe extern "C" fn CreateUnited_Uint8(this: *mut c_void) -> u8 {
    create_united::<u8>(this)
}

#[no_mangle]
unsafe extern "C" fn CreateUnited_Uint16(this: *mut c_void) -> u16 {
    create_united::<u16>(this)
}

#[no_mangle]
unsafe extern "C" fn CreateUnited_Uint32(this: *mut c_void) -> u32 {
    create_united::<u32>(this)
}

#[no_mangle]
unsafe extern "C" fn CreateUnited_Uint64(this: *mut c_void) -> u64 {
    create_united::<u64>(this)
}

#[repr(C)]
struct Link<T: LinkType> {
    index: T,
    source: T,
    target: T,
}

type EachCallback<T> = extern "C" fn(Link<T>) -> T;

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
        _ => panic!("UB")
    }
}

#[no_mangle]
unsafe extern "C" fn EachUnited_Uint8(
    this: *mut c_void,
    callback: EachCallback<u8>,
    query: *const u8,
    len: usize,
) -> u8 {
    each_united::<u8>(
        this,
        callback,
        query,
        len,
    )
}

#[no_mangle]
unsafe extern "C" fn EachUnited_Uint16(
    this: *mut c_void,
    callback: EachCallback<u16>,
    query: *const u16,
    len: usize,
) -> u16 {
    each_united::<u16>(
        this,
        callback,
        query,
        len,
    )
}

#[no_mangle]
unsafe extern "C" fn EachUnited_Uint32(
    this: *mut c_void,
    callback: EachCallback<u32>,
    query: *const u32,
    len: usize,
) -> u32 {
    each_united::<u32>(
        this,
        callback,
        query,
        len,
    )
}

#[no_mangle]
unsafe extern "C" fn EachUnited_Uint64(
    this: *mut c_void,
    callback: EachCallback<u64>,
    query: *const u64,
    len: usize,
) -> u64 {
    each_united::<u64>(
        this,
        callback,
        query,
        len,
    )
}

unsafe fn count_united<T: LinkType>(this: *mut c_void) -> T {
    let links = &mut *(this as *mut UnitedLinks<T>);
    links.count()
}

#[no_mangle]
unsafe extern "C" fn CountUnited_Uint8(this: *mut c_void) -> u8 {
    count_united::<u8>(this)
}

#[no_mangle]
unsafe extern "C" fn CountUnited_Uint16(this: *mut c_void) -> u16 {
    count_united::<u16>(this)
}

#[no_mangle]
unsafe extern "C" fn CountUnited_Uint32(this: *mut c_void) -> u32 {
    count_united::<u32>(this)
}

#[no_mangle]
unsafe extern "C" fn CountUnited_Uint64(this: *mut c_void) -> u64 {
    count_united::<u64>(this)
}

unsafe fn update_united<T: LinkType>(this: *mut c_void, index: T, source: T, target: T) -> T {
    let links = &mut *(this as *mut UnitedLinks<T>);
    links.update(index, source, target)
}

#[no_mangle]
unsafe extern "C" fn UpdateUnited_Uint8(this: *mut c_void, index: u8, source: u8, target: u8) -> u8 {
    update_united::<u8>(this, index, source, target)
}

#[no_mangle]
unsafe extern "C" fn UpdateUnited_Uint16(
    this: *mut c_void,
    index: u16,
    source: u16,
    target: u16,
) -> u16 {
    update_united::<u16>(this, index, source, target)
}

#[no_mangle]
unsafe extern "C" fn UpdateUnited_Uint32(
    this: *mut c_void,
    index: u32,
    source: u32,
    target: u32,
) -> u32 {
    update_united::<u32>(this, index, source, target)
}

#[no_mangle]
unsafe extern "C" fn UpdateUnited_Uint64(
    this: *mut c_void,
    index: u64,
    source: u64,
    target: u64,
) -> u64 {
    update_united::<u64>(this, index, source, target)
}

unsafe fn delete_united<T: LinkType>(this: *mut c_void, index: T) -> T {
    let links = &mut *(this as *mut UnitedLinks<T>);
    links.delete(index)
}

#[no_mangle]
unsafe extern "C" fn DeleteUnited_Uint8(this: *mut c_void, index: u8) -> u8 {
    delete_united::<u8>(this, index)
}

#[no_mangle]
unsafe extern "C" fn DeleteUnited_Uint16(this: *mut c_void, index: u16) -> u16 {
    delete_united::<u16>(this, index)
}

#[no_mangle]
unsafe extern "C" fn DeleteUnited_Uint32(this: *mut c_void, index: u32) -> u32 {
    delete_united::<u32>(this, index)
}

#[no_mangle]
unsafe extern "C" fn DeleteUnited_Uint64(this: *mut c_void, index: u64) -> u64 {
    delete_united::<u64>(this, index)
}

#[test]
fn its_work() {}