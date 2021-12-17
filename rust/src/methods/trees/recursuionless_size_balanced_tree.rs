use std::default::default;
use std::mem::size_of;

use num_traits::{one, zero};

use crate::methods::trees::size_balanced_tree_base::SizeBalancedTreeBase;
use crate::methods::SizeBalancedTreeMethods;
use crate::num::Num;

pub trait RecursionlessSizeBalancedTreeMethods<T: Num>: SizeBalancedTreeBase<T> {
    unsafe fn attach(&mut self, root: *mut T, node: T) {
        if *root == zero() {
            self.set_size(node, one());
            *root = node;
            return;
        }
        self.attach_core(root, node);
    }
    unsafe fn detach(&mut self, root: *mut T, node: T) {
        self.detach_core(root, node);
    }

    unsafe fn attach_core(&mut self, mut root: *mut T, node: T) {
        loop {
            let left = self.get_mut_left_reference(*root);
            let left_size = self.get_size_or_zero(*left);
            let right = self.get_mut_right_reference(*root);
            let right_size = self.get_size_or_zero(*right);
            if self.first_is_to_the_left_of_second(node, *root) {
                if *left == zero() {
                    self.inc_size(*root);
                    self.set_size(node, one());
                    *left = node;
                    return;
                }
                if self.first_is_to_the_left_of_second(node, *left) {
                    if (left_size + one()) > right_size {
                        self.right_rotate(root);
                    } else {
                        self.inc_size(*root);
                        root = left;
                    }
                } else {
                    let left_right_size = self.get_size_or_zero(self.get_right(*left));
                    if (left_right_size + one()) > right_size {
                        if left_right_size == zero() && right_size == zero() {
                            self.set_left(node, *left);
                            self.set_right(node, *root);
                            self.set_size(node, left_size + one() + one());
                            self.set_left(*root, zero());
                            self.set_size(*root, one());
                            *root = node;
                            return;
                        }
                        self.left_rotate(left);
                        self.right_rotate(root);
                    } else {
                        self.inc_size(*root);
                        root = left;
                    }
                }
            } else {
                if *right == zero() {
                    self.inc_size(*root);
                    self.set_size(node, one());
                    *right = node;
                    return;
                }
                if self.first_is_to_the_right_of_second(node, *right) {
                    if (right_size + one()) > left_size {
                        self.left_rotate(root);
                    } else {
                        self.inc_size(*root);
                        root = right;
                    }
                } else {
                    let right_left_size = self.get_size_or_zero(self.get_left(*right));
                    if (right_left_size + one()) > left_size {
                        if right_left_size == zero() && left_size == zero() {
                            self.set_left(node, *root);
                            self.set_right(node, *right);
                            self.set_size(node, right_size + one() + one());
                            self.set_right(*root, zero());
                            self.set_size(*root, one());
                            *root = node;
                            return;
                        }
                        self.right_rotate(right);
                        self.left_rotate(root);
                    } else {
                        self.inc_size(*root);
                        root = right;
                    }
                }
            }
        }
    }

    unsafe fn detach_core(&mut self, mut root: *mut T, node: T) {
        loop {
            let left = self.get_mut_left_reference(*root);
            let left_size = self.get_size_or_zero(*left);
            let right = self.get_mut_right_reference(*root);
            let right_size = self.get_size_or_zero(*right);
            if self.first_is_to_the_left_of_second(node, *root) {
                let decremented_left_size = left_size - one();
                if self.get_size_or_zero(self.get_right_or_default(*right)) > decremented_left_size
                {
                    self.left_rotate(root);
                } else if self.get_size_or_zero(self.get_left_or_default(*right))
                    > decremented_left_size
                {
                    self.right_rotate(right);
                    self.left_rotate(root);
                } else {
                    self.dec_size(*root);
                    root = left;
                }
            } else if self.first_is_to_the_right_of_second(node, *root) {
                let decremented_right_size = right_size - one();
                if self.get_size_or_zero(self.get_left_or_default(*left)) > decremented_right_size {
                    self.right_rotate(root);
                } else if self.get_size_or_zero(self.get_right_or_default(*left))
                    > decremented_right_size
                {
                    self.left_rotate(left);
                    self.right_rotate(root);
                } else {
                    self.dec_size(*root);
                    root = right;
                }
            } else {
                if left_size > zero() && right_size > zero() {
                    let replacement;
                    if left_size > right_size {
                        replacement = self.get_rightest(*left);
                        self.detach_core(left, replacement);
                    } else {
                        replacement = self.get_leftest(*right);
                        self.detach_core(right, replacement);
                    }
                    self.set_left(replacement, *left);
                    self.set_right(replacement, *right);
                    self.set_size(replacement, left_size + right_size);
                    *root = replacement;
                } else if left_size > zero() {
                    *root = *left;
                } else if right_size > zero() {
                    *root = *right;
                } else {
                    *root = zero();
                }
                self.clear_node(node);
                return;
            }
        }
    }
}

#[derive(Default, Debug, Clone, Copy)]
pub struct TreeElement<T: Num> {
    pub size: T,
    pub left: T,
    pub right: T,
}

pub struct NonRecurTree<T: Num> {
    pub elements: Vec<TreeElement<T>>,
    _allocated: T,
    pub root: T,
}

impl<T: Num> NonRecurTree<T> {
    pub fn new(len: usize) -> Self {
        let mut mem = vec![];
        mem.resize(len, default());
        Self {
            elements: mem,
            _allocated: T::one(),
            root: T::zero(),
        }
    }

    pub fn alloc(&mut self) -> T {
        let new = self._allocated;
        if self.is_empty(new) {
            self._allocated = self._allocated + T::one();
            return new;
        }
        // TODO: return error
        panic!("allocated tree element is not empty");
    }

    pub fn dealloc(&mut self, mut node: T) {
        while self._allocated != T::one() && self.is_empty(node) {
            let last = self._allocated - T::one();
            if last == node {
                self._allocated = last;
                node = node - T::one();
            } else {
                return;
            }
        }
    }

    pub fn len(&self) -> T {
        self.get_size(self.root)
    }

    pub fn is_empty(&self, node: T) -> bool {
        unsafe fn is_zero(ptr: *const usize, bytes: isize) -> bool {
            let ptr = ptr as *const i8;
            (0..bytes).all(|i| *(ptr.offset(i)) == 0)
        }

        unsafe {
            is_zero(
                self.get(node) as *const TreeElement<T> as *const usize,
                size_of::<TreeElement<T>>() as isize,
            )
        }
    }

    fn get(&self, node: T) -> &TreeElement<T> {
        &self.elements[node.as_()]
    }

    fn get_mut(&mut self, node: T) -> &mut TreeElement<T> {
        &mut self.elements[node.as_()]
    }
}

impl<T: Num> SizeBalancedTreeMethods<T> for NonRecurTree<T> {
    fn get_left_reference(&self, node: T) -> *const T {
        &self.get(node).left as *const T
    }

    fn get_right_reference(&self, node: T) -> *const T {
        &self.get(node).right as *const T
    }

    fn get_mut_left_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut(node).left as *mut T
    }

    fn get_mut_right_reference(&mut self, node: T) -> *mut T {
        &mut self.get_mut(node).right as *mut T
    }

    fn get_left(&self, node: T) -> T {
        self.get(node).left
    }

    fn get_right(&self, node: T) -> T {
        self.get(node).right
    }

    fn get_size(&self, node: T) -> T {
        self.get(node).size
    }

    fn set_left(&mut self, node: T, left: T) {
        self.get_mut(node).left = left
    }

    fn set_right(&mut self, node: T, right: T) {
        self.get_mut(node).right = right
    }

    fn set_size(&mut self, node: T, size: T) {
        self.get_mut(node).size = size
    }

    fn first_is_to_the_left_of_second(&self, first: T, second: T) -> bool {
        first < second
    }

    fn first_is_to_the_right_of_second(&self, first: T, second: T) -> bool {
        first > second
    }
}
