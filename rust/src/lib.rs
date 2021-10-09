#![feature(step_trait)]
#![feature(associated_type_bounds)]
#![feature(default_free_fn)]
#![feature(box_syntax)]

pub mod doublets;
pub mod mem;
pub mod methods;
pub mod num;
pub mod test_extensions;

#[cfg(test)]
mod tests {
    use std::collections::{HashMap, HashSet};
    use std::mem::size_of_val;
    use std::ops::{Not, Range};

    use crate::doublets;
    use crate::doublets::data::converters::{AddrToRaw, RawToAddr};
    use crate::doublets::data::ilinks::IGenericLinks;
    use crate::doublets::data::point::Point;
    use crate::doublets::ilinks::{ILinks, ILinksExtensions};
    use crate::doublets::link::Link;
    use crate::doublets::mem::united::generic::links_size_balanced_tree_base::LinksSizeBalancedTreeBaseAbstract;
    use crate::doublets::mem::united::links::Links;
    use crate::mem::file_mapped_mem::FileMappedMem;
    use crate::mem::heap_mem::HeapMem;
    use crate::mem::mem::{Mem, ResizeableMem};
    use crate::methods::trees::size_balanced_tree::{SizeBalancedTree, SizeBalancedTreeMethods};
    use crate::test_extensions::ILinksTestExtensions;
    use num_traits::{zero, AsPrimitive};
    use rand::Rng;
    use crate::doublets::mem::united::generic::links_sources_size_balanced_tree::LinksSourcesSizeBalancedTree;

    #[test]
    fn it_works() {
        unsafe {
            let mut mem = HeapMem::new();
            //let mut mem = FileMappedMem::new("db.links").unwrap();
            mem.reserve_mem(2048 * 1000);
            unsafe {
                let ptr = mem.get_ptr();
                for i in 0..100 {
                    let ptr = ptr as *mut usize;
                    let ptr = ptr.add(i);
                    *ptr = i
                }

                {
                    let ptr = ptr as *mut usize;
                    *(ptr.offset(10)) = 2281337177013;
                }

                for i in 0..100 {
                    let ptr = ptr as *mut usize;
                    println!("{}", *(ptr.offset(i as isize)))
                }
            }
        }
    }

    #[test]
    fn t1() {
        let x = 12;
        unsafe {
            *(&x as *const i32 as *mut i32) = 13;
        }
        println!("{}", x);
    }

    #[test]
    fn szb_tree() {
        unsafe {
            let mut tree = SizeBalancedTree::new();
            let root = &mut tree.root as *mut usize;
            let maximum_operations_per_cycle = 2000;
            let mut added = HashSet::<usize>::new();
            let mut current_count = 0;

            use std::time::Instant;
            let now = Instant::now();

            for N in 1..maximum_operations_per_cycle {
                for i in 0..N {
                    let node = (rand::random::<usize>() % N) + 1;
                    if !added.contains(&node) {
                        added.insert(node);
                        tree.attach(&mut *root, node);
                        current_count += 1;
                        assert_eq!(current_count, tree.len())
                    }
                }
                for i in 1..N + 1 {
                    let node = (rand::random::<usize>() % N) + 1;
                    if tree.contains(node, *root) {
                        tree.detach(&mut *root, node);
                        current_count -= 1;
                        assert_eq!(current_count, tree.len());
                        added.remove(&node);
                    }
                }
            }

            let elapsed = now.elapsed();
            println!("Elapsed: {:.2?}", elapsed);
            println!("Size: {}", tree.len());
        }
    }

    #[test]
    fn point() {
        let x: Link<_> = (2_u32..10).into_iter().collect();
        println!("link is: {:?}", x);

        for item in &x {
            println!("{}", item);
        }

        println!("is full point: {}", Point::is_full(&x));
        println!("is full point: {}", Point::is_full(x.clone()));
        println!("is full point: {}", Point::is_full(&vec![1, 2, 3, 4][0..2]));

        println!("is partial point: {}", Point::is_partial(&x));
        println!("is partial point: {}", Point::is_partial(x.clone()));
        println!(
            "is partial point: {}",
            Point::is_partial(&vec![1, 2, 1, 4][0..3])
        );

        println!("is full point: {}", Point::is_full(&vec![1; 10][0..5]));
        println!("is full point: {}", Point::is_full(&vec![1, 1, 2, 2][0..3]));
    }

    #[test]
    fn converters() {
        let to_raw = AddrToRaw::new();
        let raw_to = RawToAddr::new();

        let raw = to_raw.convert(12_u32);
        println!("{}", raw);
        println!("{}", to_raw.convert(raw));
        println!("{}", raw_to.convert(raw));
        let ori = raw_to.convert(raw);
        println!("{}", to_raw.convert(ori));
        println!("{}", raw_to.convert(ori));
    }

    #[test]
    fn ilinks() {}

    #[test]
    fn links() {
        std::fs::remove_file("db.links");

        let mem = FileMappedMem::new("db.links").unwrap();
        let mem = HeapMem::new();
        let mut links = Links::<u32, HeapMem>::new(mem);
        let constants = links.constants();
        let any = constants.any;

        let mut root = links.create_point();

        for i in 0..10 {
            let new = links.create_point();
            links.create_and_update(new, root);
        }

        let mut vec = vec![];
        links.each(|link| {
            vec.push(link.clone());
            return constants.r#continue;
        });

        for link in vec {
            let new = links.create_point();
            links.create_and_update(new, link.source);
        }

        links.each(|link| {
            println!("{} -> {}", link.source, link.target);
            return constants.r#continue;
        });

        println!("count: {}", links.count());
    }

    #[test]
    fn links_test() {
        std::fs::remove_file("db.links");

        let mem = FileMappedMem::new("db.links").unwrap();
        let mem = HeapMem::new();
        let mut links = Links::<u32, _>::new(mem);

        for i in 0..1000 {
            links.get_or_create(i, i);
        }

        let constants = links.constants();
        links.each(|link| {
            println!("{} -> {}", link.source, link.target);
            constants.r#continue
        });

        links.delete_all();
        assert_eq!(links.count(), 0);
    }

    #[test]
    fn links_bug() {
        let mut mem = HeapMem::new();
        let mut links = Links::<usize, _>::new(mem);

        links.test_random_creations_and_deletions(10);
    }

}
