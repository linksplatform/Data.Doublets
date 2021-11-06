extern crate test;

use test::Bencher;

use crate::methods::NonRecurTree;
use crate::methods::RecursionlessSizeBalancedTreeMethods;
use crate::methods::SizeBalancedTreeBase;
use crate::methods::SizeBalancedTreeMethods;

#[bench]
fn non_recur_tree_hard(b: &mut Bencher) {
    let mut tree = NonRecurTree::new(100000001);
    let root = &mut tree.root as *mut usize;

    let mut last = 1;
    b.iter(|| {
        tree.attach(unsafe { &mut *root }, last);
        last += 1;
    });
}
