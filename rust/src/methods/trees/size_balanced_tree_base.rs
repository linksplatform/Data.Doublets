use num_traits::{one, zero};

use crate::num::Num;

pub trait SizeBalancedTreeBase<T: Num> {
    fn get_mut_left_reference(&mut self, node: T) -> *mut T;

    fn get_mut_right_reference(&mut self, node: T) -> *mut T;

    fn get_left_reference(&self, node: T) -> *const T;

    fn get_right_reference(&self, node: T) -> *const T;

    fn get_left(&self, node: T) -> T;

    fn get_right(&self, node: T) -> T;

    fn get_size(&self, node: T) -> T;

    fn set_left(&mut self, node: T, left: T);

    fn set_right(&mut self, node: T, right: T);

    fn set_size(&mut self, node: T, size: T);

    fn first_is_to_the_left_of_second(&self, first: T, second: T) -> bool;

    fn first_is_to_the_right_of_second(&self, first: T, second: T) -> bool;

    fn get_left_or_default(&self, node: T) -> T {
        if node == zero() {
            zero()
        } else {
            self.get_left(node)
        }
    }

    fn get_right_or_default(&self, node: T) -> T {
        if node == zero() {
            zero()
        } else {
            self.get_right(node)
        }
    }

    fn get_size_or_zero(&self, node: T) -> T {
        if node == zero() {
            zero()
        } else {
            self.get_size(node)
        }
    }

    fn inc_size(&mut self, node: T) {
        self.set_size(node, self.get_size(node) + one());
    }

    fn dec_size(&mut self, node: T) {
        self.set_size(node, self.get_size(node) - one());
    }

    fn get_left_size(&self, node: T) -> T {
        self.get_size_or_zero(self.get_left_or_default(node))
    }

    fn get_right_size(&self, node: T) -> T {
        self.get_size_or_zero(self.get_right_or_default(node))
    }

    fn fix_size(&mut self, node: T) {
        self.set_size(
            node,
            (self.get_left_size(node) + self.get_right_size(node)) + one(),
        );
    }

    unsafe fn left_rotate(&mut self, root: *mut T) {
        *root = self.left_rotate_core(*root);
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
        *root = self.right_rotate_core(*root);
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
        while current_right != zero() {
            current = current_right;
            current_right = self.get_right(current);
        }
        current
    }

    fn get_leftest(&self, mut current: T) -> T {
        let mut current_left = self.get_left(current);
        while current_left != zero() {
            current = current_left;
            current_left = self.get_left(current);
        }
        current
    }

    fn get_next(&self, node: T) -> T {
        self.get_leftest(self.get_right(node))
    }

    fn get_previous(&self, node: T) -> T {
        self.get_rightest(self.get_left(node))
    }

    fn contains(&self, node: T, mut root: T) -> bool {
        while root != zero() {
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

    fn clear_node(&mut self, node: T) {
        self.set_left(node, zero());
        self.set_right(node, zero());
        self.set_size(node, zero());
    }
}
