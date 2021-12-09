use std::ptr::Unique;
use std::sync::{Arc, Mutex, RwLock};
use std::sync::mpsc::channel;
use std::thread;
use std::time::Instant;

use rand::Rng;

use crate::doublets::{ILinks, ILinksExtensions, Link};
use crate::doublets::data::IGenericLinks;
use crate::doublets::mem::{splited, united};
use crate::mem::ResizeableMem;
use crate::num::LinkType;
use crate::tests::make_links;
use crate::tests::make_mem;

#[test]
fn basic_sync() {
    let mem = make_mem();
    let mut links = make_links(mem).unwrap();

    let base_links = Arc::new(RwLock::new(links));

    let mut threads = vec![];

    for _ in 0..100 {
        let links = Arc::clone(&base_links);
        let thread = thread::spawn(move || {
            for _ in 0..10000 {
                let mut links = links.write().unwrap();
                (*links).create_point().unwrap();
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

#[test]
fn super_read() {
    let mem = make_mem();
    let mut links = make_links(mem).unwrap();

    let instant = Instant::now();

    for _ in 0..1000000 {
        let source = rand::thread_rng().gen_range(1..=1000);
        let target = rand::thread_rng().gen_range(1..=1000);
        links.get_or_create(source, target).unwrap();
    }

    println!("links count: {}", links.count());
    println!("created time: {:?}", instant.elapsed());

    let synced = RwLock::new(links);
    let links_arc = Arc::new(synced);

    let instant = Instant::now();

    let mut threads = vec![];
    for _ in 0..1000 {
        let links = Arc::clone(&links_arc);
        let thread = thread::spawn(move || {
            let links = links.read().unwrap();

            let mut data = vec![];
            let r#continue = links.constants().r#continue;
            links.each(|link| {
                data.push(link);
                r#continue
            });
        });
        threads.push(thread);
    }

    {
        drop(threads);
        let links = links_arc.read().unwrap();
        println!("links count: {}", links.count());
        println!("read time: {:?}", instant.elapsed());
    }

    let mut threads = vec![];
    for _ in 0..1000 {
        let links = Arc::clone(&links_arc);
        let thread = thread::spawn(move || {
            let links = links.write().unwrap();

            let mut data = vec![];
            let r#continue = links.constants().r#continue;
            links.each(|link| {
                data.push(link);
                r#continue
            });
        });
        threads.push(thread);
    }

    {
        drop(threads);
        let links = links_arc.write().unwrap();
        println!("links count: {}", links.count());
        println!("write as read time: {:?}", instant.elapsed());
    }
}
