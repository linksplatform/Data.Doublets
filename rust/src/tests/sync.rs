use crate::tests::make_mem;
use crate::tests::make_links;
use crate::doublets::{ILinksExtensions, Link};
use crate::doublets::data::IGenericLinks;
use std::sync::{Arc, Mutex, RwLock};
use std::sync::mpsc::channel;
use std::thread;
use crate::num::LinkType;
use crate::mem::ResizeableMem;
use crate::doublets::mem::united;
use std::ptr::Unique;

// TODO: use safe slice
unsafe impl<T: LinkType, M: ResizeableMem> Send for united::Links<T, M> {}
unsafe impl<T: LinkType, M: ResizeableMem> Sync for united::Links<T, M> {}

#[test]
fn basic_sync() {
    let mem = make_mem();
    let mut links = make_links(mem);

    let base_links = Arc::new(RwLock::new(links));

    let mut threads = vec![];

    for _ in 0..100 {
        let links = Arc::clone(&base_links);
        let thread = thread::spawn(move || {
            for _ in 0..10000 {
                let mut links = links.write().unwrap();
                (*links).create_point();
            }
        });
        threads.push(thread);
    }

    for thread in threads {
        thread.join().unwrap();
    }

    println!("{:?}", base_links.read().unwrap().count());
    //assert_eq!(10, base_links.write().unwrap().count());
}
