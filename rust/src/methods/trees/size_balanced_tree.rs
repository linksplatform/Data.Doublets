use std::borrow::{Borrow, BorrowMut};
use std::mem::size_of;
use std::slice::SliceIndex;

use num_traits::{one, zero, AsPrimitive, PrimInt, Unsigned};

use crate::num::Num;

pub trait SizeBalancedTreeMethods<T: Num> {
    // TODO: split get_... and get_mut...
    fn get_left_reference(&self, node: T) -> *const T;
    fn get_right_reference(&self, node: T) -> *const T;

    fn get_mut_left_reference(&mut self, node: T) -> *mut T;
    fn get_mut_right_reference(&mut self, node: T) -> *mut T;

    fn get_left(&self, node: T) -> T;
    fn get_right(&self, node: T) -> T;

    fn get_size(&self, node: T) -> T;

    fn set_left(&mut self, node: T, left: T);
    fn set_right(&mut self, node: T, right: T);

    fn set_size(&mut self, node: T, size: T);

    fn inc_size(&mut self, node: T) {
        self.set_size(node, self.get_size(node) + T::one());
    }

    fn dec_size(&mut self, node: T) {
        self.set_size(node, self.get_size(node) - T::one());
    }

    fn first_is_to_the_left_of_second(&self, first: T, second: T) -> bool;
    fn first_is_to_the_right_of_second(&self, first: T, second: T) -> bool;

    fn clear_node(&mut self, node: T) {
        self.set_left(node, T::zero());
        self.set_right(node, T::zero());
        self.set_size(node, T::zero());
    }

    fn fix_size(&mut self, node: T) {
        self.set_size(
            node,
            self.get_size(self.get_left(node)) + self.get_size(self.get_right(node)) + T::one(),
        )
    }

    // fn fix_sizes(&self, node: T)
    // {
    //     if node == T::zero() {
    //         return;
    //     }
    //     self.fix_sizes(self.get_left(node));
    //     self.fix_sizes(self.get_right(node));
    //     self.fix_sizes(node);
    // }

    unsafe fn left_rotate(&mut self, root: *mut T) {
        *root = self.left_rotate_core(*root)
    }

    fn left_rotate_core(&mut self, root: T) -> T {
        let right = self.get_right(root);
        self.set_right(root, self.get_left(right));
        self.set_left(right, root);
        self.set_size(right, self.get_size(root));
        self.fix_size(root);
        right
    }

    unsafe fn right_rotate(&mut self, root: *mut T) {
        *root = self.right_rotate_core(*root)
    }

    fn right_rotate_core(&mut self, root: T) -> T {
        let left = self.get_left(root);
        self.set_left(root, self.get_right(left));
        self.set_right(left, root);
        self.set_size(left, self.get_size(root));
        self.fix_size(root);
        left
    }

    fn get_rightest(&self, mut current: T) -> T {
        let mut current_right = self.get_right(current);
        while current_right != T::zero() {
            current = current_right;
            current_right = self.get_right(current);
        }
        current
    }

    fn get_leftest(&self, mut current: T) -> T {
        let mut current_left = self.get_left(current);
        while current_left != T::zero() {
            current = current_left;
            current_left = self.get_left(current);
        }
        current
    }

    fn contains(&self, node: T, mut root: T) -> bool {
        while root != T::zero() {
            if self.first_is_to_the_left_of_second(node, root) {
                root = self.get_left(root);
            } else if self.first_is_to_the_right_of_second(node, root) {
                root = self.get_right(root);
            } else {
                return true;
            }
        }
        false
    }

    unsafe fn left_maintain(&mut self, root: *mut T) {
        if *root == T::zero() {
            return;
        }

        let left_root = self.get_left(*root);
        if left_root != T::zero() {
            let right_root = self.get_right(*root);
            let right_root_size = self.get_size(right_root);
            let left_left_root = self.get_left(left_root);
            // TODO: use capture
            if left_left_root != T::zero()
                && (right_root == T::zero() || self.get_size(left_left_root) > right_root_size)
            {
                self.right_rotate(&mut *root);
            } else {
                let left_right_root = self.get_right(left_root);
                if left_right_root != T::zero()
                    && (right_root == T::zero() || self.get_size(left_right_root) > right_root_size)
                {
                    let temp = self.get_mut_left_reference(*root);
                    self.left_rotate(&mut *temp);
                    self.right_rotate(&mut *root);
                    return; // TODO: WARNING
                } else {
                    return;
                }
            }
            let left = self.get_mut_left_reference(*root);
            self.left_maintain(&mut *left);
            let right = self.get_mut_right_reference(*root);
            self.right_maintain(&mut *right);
            self.left_maintain(root);
            self.right_maintain(root);
        }
    }

    unsafe fn right_maintain(&mut self, root: *mut T) {
        if *root == T::zero() {
            return;
        }

        let right_root = self.get_right(*root);
        if right_root != T::zero() {
            let left_root = self.get_left(*root);
            let left_root_size = self.get_size(left_root);
            let right_right_root = self.get_right(right_root);
            // TODO: use capture
            if right_right_root != T::zero()
                && (left_root == T::zero() || self.get_size(right_right_root) > left_root_size)
            {
                self.left_rotate(&mut *root);
            } else {
                let right_left_root = self.get_left(right_root);
                if right_left_root != T::zero()
                    && (left_root == T::zero() || self.get_size(right_left_root) > left_root_size)
                {
                    let temp = self.get_mut_right_reference(*root);
                    self.right_rotate(temp);
                    self.left_rotate(root);
                    return;
                } else {
                    return;
                }
            }
            let left = self.get_mut_left_reference(*root);
            self.left_maintain(&mut *left);
            let right = self.get_mut_right_reference(*root);
            self.right_maintain(&mut *right);
            self.left_maintain(root);
            self.right_maintain(root);
        }
    }

    unsafe fn attach_core(&mut self, root: *mut T, node: T) {
        if *root == zero() {
            *root = node;
            self.inc_size(*root);
        } else {
            self.inc_size(*root);
            if self.first_is_to_the_left_of_second(node, *root) {
                let left = self.get_mut_left_reference(*root);
                self.attach_core(left, node);
                self.left_maintain(root);
            } else {
                let right = self.get_mut_right_reference(*root);
                self.attach_core(right, node);
                self.right_maintain(root);
            }
        }
    }

    unsafe fn detach_core(&mut self, root: *mut T, detached: T) {
        let mut current = root;
        let mut parent = root;
        let mut replacement = T::zero();

        while *current != detached {
            self.dec_size(*current);
            if self.first_is_to_the_left_of_second(detached, *current) {
                parent = current;
                current = self.get_mut_left_reference(*current);
            } else if self.first_is_to_the_right_of_second(detached, *current) {
                parent = current;
                current = self.get_mut_right_reference(*current);
            } else {
                println!("{:?} -- {:?}", detached, *current,);
                // TODO: return error
                //self.clear_node(*root);
                //*root = zero();
                //return;
                panic!("duplicate link found in the tree");
            }
        }

        let detached_left = self.get_left(detached);
        let mut node = self.get_right(detached);
        if detached_left != T::zero() && node != T::zero() {
            let leftest = self.get_leftest(node);
            let temp = self.get_mut_right_reference(detached);
            self.detach_core(temp, leftest);
            self.set_left(leftest, detached_left);
            node = self.get_right(detached);
            if node != T::zero() {
                self.set_right(leftest, node);
                self.set_size(
                    leftest,
                    self.get_size(detached_left) + self.get_size(node) + T::one(),
                )
            } else {
                self.set_size(leftest, self.get_size(detached_left) + T::one());
            }
            replacement = leftest;
        } else if detached_left != T::zero() {
            replacement = detached_left;
        } else if node != T::zero() {
            replacement = node;
        }
        if *root == detached {
            *root = replacement;
        } else if self.get_left(*parent) == detached {
            self.set_left(*parent, replacement);
        } else if self.get_right(*parent) == detached {
            self.set_right(*parent, replacement);
        }
        self.clear_node(detached);
    }

    fn attach(&mut self, root: &mut T, node: T) {
        if *root == T::zero() {
            self.set_size(node, T::one());
            *root = node;
            return;
        }
        unsafe { self.attach_core(root, node) }
    }

    fn detach(&mut self, root: &mut T, node: T) {
        unsafe { self.detach_core(root, node) }
    }
}
