use std::collections::HashSet;
use crate::methods::SizeBalancedTree;
use crate::methods::SizeBalancedTreeMethods;
use crate::methods::SizeBalancedTreeBase;
use crate::methods::RecursionlessSizeBalancedTreeMethods;

#[test]
// TODO: move to `bench`
fn szb_tree() {
    unsafe {
        let mut tree = SizeBalancedTree::new();
        let root = &mut tree.root as *mut usize;

        for i in 1..=1000000 {
            tree.attach(&mut *root, i);
        }
    }
}