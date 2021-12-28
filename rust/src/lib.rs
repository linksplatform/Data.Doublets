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

//#[cfg(test)]
//mod tests {
//    use std::collections::{HashMap, HashSet};
//    use std::mem::size_of_val;
//    use std::ops::{Not, Range};
//
//    use crate::doublets;
//    use crate::doublets::data::{AddrToRaw, RawToAddr};
//    use crate::doublets::data::IGenericLinks;
//    use crate::doublets::data::Point;
//    use crate::doublets::{ILinks};
//    use crate::doublets::ILinksExtensions;
//    use crate::doublets::Link;
//    use crate::doublets::mem::united::LinksSizeBalancedTreeBaseAbstract;
//    use crate::doublets::mem::united::Links;
//    use crate::mem::FileMappedMem;
//    use crate::mem::HeapMem;
//    use crate::mem::{Mem, ResizeableMem};
//    use crate::methods::{SizeBalancedTree, SizeBalancedTreeMethods};
//    use crate::test_extensions::ILinksTestExtensions;
//    use num_traits::{zero, AsPrimitive};
//    use rand::{Rng, thread_rng};
//    use crate::doublets::mem::united::LinksSourcesSizeBalancedTree;
//    use std::time::Instant;
//
//
//
//    #[test]
//    fn it_works() {
//
//        unsafe {
//            let mut mem = HeapMem::new().unwrap();
//            //let mut mem = FileMappedMem::new("db.links").unwrap();
//            mem.reserve_mem(2048 * 1000);
//            unsafe {
//                let ptr = mem.get_ptr();
//                for i in 0..100 {
//                    let ptr = ptr as *mut usize;
//                    let ptr = ptr.add(i);
//                    *ptr = i
//                }
//
//                {
//                    let ptr = ptr as *mut usize;
//                    *(ptr.offset(10)) = 2281337177013;
//                }
//
//                for i in 0..100 {
//                    let ptr = ptr as *mut usize;
//                    println!("{}", *(ptr.offset(i as isize)))
//                }
//            }
//        }
//    }
//
//    #[test]
//    fn t1() {
//        let x = 12;
//        unsafe {
//            *(&x as *const i32 as *mut i32) = 13;
//        }
//        println!("{}", x);
//    }
//
//    #[test]
//    fn szb_tree() {
//        unsafe {
//            let mut tree = SizeBalancedTree::new();
//            let root = &mut tree.root as *mut usize;
//            let maximum_operations_per_cycle = 2000;
//            let mut added = HashSet::<usize>::new();
//            let mut current_count = 0;
//
//            use std::time::Instant;
//            let now = Instant::now();
//
//            for N in 1..maximum_operations_per_cycle {
//                for i in 0..N {
//                    let node = (rand::random::<usize>() % N) + 1;
//                    if !added.contains(&node) {
//                        added.insert(node);
//                        tree.attach(&mut *root, node);
//                        current_count += 1;
//                        assert_eq!(current_count, tree.len())
//                    }
//                }
//                for i in 1..N + 1 {
//                    let node = (rand::random::<usize>() % N) + 1;
//                    if tree.contains(node, *root) {
//                        tree.detach(&mut *root, node);
//                        current_count -= 1;
//                        assert_eq!(current_count, tree.len());
//                        added.remove(&node);
//                    }
//                }
//            }
//
//            let elapsed = now.elapsed();
//            println!("Elapsed: {:.2?}", elapsed);
//            println!("Size: {}", tree.len());
//        }
//    }
//
//    #[test]
//    fn point() {
//        let x: Link<_> = (2_u32..10).into_iter().collect();
//        println!("link is: {:?}", x);
//
//        for item in &x {
//            println!("{}", item);
//        }
//
//        println!("is full point: {}", Point::is_full(&x));
//        println!("is full point: {}", Point::is_full(x.clone()));
//        println!("is full point: {}", Point::is_full(&vec![1, 2, 3, 4][0..2]));
//
//        println!("is partial point: {}", Point::is_partial(&x));
//        println!("is partial point: {}", Point::is_partial(x.clone()));
//        println!(
//            "is partial point: {}",
//            Point::is_partial(&vec![1, 2, 1, 4][0..3])
//        );
//
//        println!("is full point: {}", Point::is_full(&vec![1; 10][0..5]));
//        println!("is full point: {}", Point::is_full(&vec![1, 1, 2, 2][0..3]));
//    }
//
//    #[test]
//    fn converters() {
//        let to_raw = AddrToRaw::new();
//        let raw_to = RawToAddr::new();
//
//        let raw = to_raw.convert(12_u32);
//        println!("{}", raw);
//        println!("{}", to_raw.convert(raw));
//        println!("{}", raw_to.convert(raw));
//        let ori = raw_to.convert(raw);
//        println!("{}", to_raw.convert(ori));
//        println!("{}", raw_to.convert(ori));
//    }
//
//    #[test]
//    fn ilinks() {}
//
//    #[test]
//    fn links() {
//        std::fs::remove_file("db.links");
//
//        let mem = FileMappedMem::new("db.links").unwrap();
//        let mem = HeapMem::new().unwrap();
//        let mut links = Links::<u32, HeapMem>::new(mem);
//        let constants = links.constants();
//        let any = constants.any;
//
//        let mut root = links.create_point();
//
//        for i in 0..10 {
//            let new = links.create_point();
//            links.create_and_update(new, root);
//        }
//
//        let mut vec = vec![];
//        links.each(|link| {
//            vec.push(link.clone());
//            return constants.r#continue;
//        });
//
//        for link in vec {
//            let new = links.create_point();
//            links.create_and_update(new, link.source);
//        }
//
//        links.each(|link| {
//            println!("{} -> {}", link.source, link.target);
//            return constants.r#continue;
//        });
//
//        println!("count: {}", links.count());
//    }
//
//    #[test]
//    fn links_test() {
//        std::fs::remove_file("db.links");
//
//        let mem = FileMappedMem::new("db.links").unwrap();
//        let mem = HeapMem::new().unwrap();
//        let mut links = Links::<u32, _>::new(mem);
//
//        for i in 0..1000 {
//            links.get_or_create(i, i);
//        }
//
//        let constants = links.constants();
//        links.each(|link| {
//            println!("{} -> {}", link.source, link.target);
//            constants.r#continue
//        });
//
//        links.delete_all();
//        assert_eq!(links.count(), 0);
//    }
//
//    #[test]
//    fn test_crud() {
//        let mem = HeapMem::new().unwrap();
//        let mut links = Links::<u32, _>::new(mem);
//
//        links.test_crud();
//    }
//
//    #[test]
//    fn links_bug() {
//        let mut mem = HeapMem::new().unwrap();
//        let mut links = Links::<usize, _>::new(mem);
//
//        for i in 0..100 {
//            links.create_point();
//        }
//        println!("{}", links.count());
//
//        let mut to_delete = vec![];
//        let constants = links.constants;
//        links.each(|link| {
//            to_delete.push(link.index);
//            constants.r#continue
//        });
//
//        for index in to_delete {
//            links.update(index, 0, 0);
//            links.delete(index);
//        }
//
//        links.delete_all();
//        println!("{}", links.count());
//
//
//        for i in 0..100 {
//            links.create_point();
//        }
//        println!("{}", links.count());
//
//        links.test_random_creations_and_deletions(100);
//    }
//
//
//    #[test]
//    fn debug() {
//        std::fs::remove_file("дб.ссылкс");
//        //let mut mem = FileMappedMem::new("дб.ссылкс").unwrap();
//        let mut mem = HeapMem::new().unwrap();
//        let mut links = Links::<usize, _>::new(mem);
//        let mut links = UniqueResolver::new(links);
//
//        let instant = Instant::now();
//
//        let any = links.constants().any;
//        println!("count {}", links.count());
//        println!("elapsed {:?}", instant.elapsed());
//    }
//
//    use std::sync::{Arc, Mutex};
//    use std::thread;
//    use std::io::Write;
//
//
//    #[test]
//    fn synchronized_links() {
//        std::fs::remove_file("db.links").unwrap();
//
//        let mut mem = HeapMem::new().unwrap();
//        let mut mem = FileMappedMem::new("db.links").unwrap();
//        let mut links = Links::<usize, _>::new(mem);
//
//        let mut out = std::fs::File::with_options()
//            .create_new(true)
//            .write(true)
//            .open("../out.csv").unwrap();
//
//        let range = 1..=1000;
//        for i in 1..=100000 {
//            let source = rand::thread_rng().gen_range(range.clone());
//            let target = rand::thread_rng().gen_range(range.clone());
//            links.get_or_create(source, target);
//        }
//
//        links.each(|link| {
//            out.write_fmt(format_args!("{}, {}\n", link.source, link.target));
//            links.constants().r#continue
//        });
//    }
//
//    use crate::doublets::decorators::UniqueResolver;
//
//    #[test]
//    fn unique_resolver() {
//        let mem = HeapMem::new().unwrap();
//        let links = Links::<usize, _>::new(mem);
//
//        let mut links = UniqueResolver::new(links);
//        links.create_and_update(1, 1);
//        links.create_and_update(1, 2);
//        links.create_and_update(2, 1);
//        links.create_and_update(2, 2);
//
//        links.create_and_update(1, 1);
//        links.create_and_update(1, 2);
//        links.create_and_update(2, 1);
//        links.create_and_update(2, 2);
//
//        links.each(|link| {
//            println!("{}", link);
//            links.constants().r#continue
//        });
//    }
//
//    use crate::doublets::decorators::UniqueValidator;
//
//    #[test]
//    fn unique_validator() {
//        let mem = HeapMem::new().unwrap();
//        let links = Links::<usize, _>::new(mem);
//
//        let mut links = UniqueValidator::new(links);
//        links.create_and_update(1, 1);
//        links.create_and_update(1, 2);
//        links.create_and_update(2, 1);
//        links.create_and_update(2, 2);
//
//        // TODO: assertion
//        links.create_and_update(2, 2);
//
//        links.each(|link| {
//            println!("{}", link);
//            links.constants().r#continue
//        });
//    }
//}
//
