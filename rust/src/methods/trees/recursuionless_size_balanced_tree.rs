use std::default::default;
use std::mem::size_of;
use crate::num::Num;
use crate::methods::trees::size_balanced_tree_base::SizeBalancedTreeBase;
use num_traits::{zero, one};
use crate::methods::SizeBalancedTreeMethods;

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
            let leftSize = self.get_size_or_zero(*left);
            let right = self.get_mut_right_reference(*root);
            let rightSize = self.get_size_or_zero(*right);
            if self.first_is_to_the_left_of_second(node, *root)
            {
                if *left == zero()
                {
                    self.inc_size(*root);
                    self.set_size(node, one());
                    *left = node;
                    return;
                }
                if self.first_is_to_the_left_of_second(node, *left)
                {
                    if ((leftSize + one()) > rightSize)
                    {
                        self.right_rotate(root);
                    } else {
                        self.inc_size(*root);
                        root = left;
                    }
                } else {
                    let leftRightSize = self.get_size_or_zero(self.get_right(*left));
                    if ((leftRightSize + one()) > rightSize)
                    {
                        if (leftRightSize == zero() && rightSize == zero())
                        {
                            self.set_left(node, *left);
                            self.set_right(node, *root);
                            self.set_size(node, leftSize + one() + one());
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
                if (*right == zero())
                {
                    self.inc_size(*root);
                    self.set_size(node, one());
                    *right = node;
                    return;
                }
                if (self.first_is_to_the_right_of_second(node, *right))
                {
                    if ((rightSize + one()) > leftSize)
                    {
                        self.left_rotate(root);
                    } else {
                        self.inc_size(*root);
                        root = right;
                    }
                } else {
                    let rightLeftSize = self.get_size_or_zero(self.get_left(*right));
                    if ((rightLeftSize + one()) > leftSize)
                    {
                        if (rightLeftSize == zero() && leftSize == zero())
                        {
                            self.set_left(node, *root);
                            self.set_right(node, *right);
                            self.set_size(node, rightSize + one() + one());
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
        while (true)
        {
            let left = self.get_mut_left_reference(*root);
            let leftSize = self.get_size_or_zero(*left);
            let right = self.get_mut_right_reference(*root);
            let rightSize = self.get_size_or_zero(*right);
            if (self.first_is_to_the_left_of_second(node, *root))
            {
                let decrementedLeftSize = leftSize - one();
                if (self.get_size_or_zero(self.get_right_or_default(*right)) > decrementedLeftSize)
                {
                    self.left_rotate(root);
                }
                else if (self.get_size_or_zero(self.get_left_or_default(*right)) > decrementedLeftSize)
                {
                    self.right_rotate(right);
                    self.left_rotate(root);
                }
                else
                {
                    self.dec_size(*root);
                    root = left;
                }
            }
            else if (self.first_is_to_the_right_of_second(node, *root))
            {
                let decrementedRightSize = rightSize - one();
                if (self.get_size_or_zero(self.get_left_or_default(*left)) > decrementedRightSize)
                {
                    self.right_rotate(root);
                }
                else if (self.get_size_or_zero(self.get_right_or_default(*left)) > decrementedRightSize)
                {
                    self.left_rotate(left);
                    self.right_rotate(root);
                }
                else
                {
                    self.dec_size(*root);
                    root = right;
                }
            }
            else
            {
                if (leftSize > zero() && rightSize > zero())
                {
                    let mut replacement = zero();
                    if (leftSize > rightSize)
                    {
                        replacement = self.get_rightest(*left);
                        self.detach_core(left, replacement);
                    }
                    else
                    {
                        replacement = self.get_leftest(*right);
                        self.detach_core(right, replacement);
                    }
                    self.set_left(replacement, *left);
                    self.set_right(replacement, *right);
                    self.set_size(replacement, leftSize + rightSize);
                    *root = replacement;
                }
                else if (leftSize > zero())
                {
                    *root = *left;
                }
                else if (rightSize > zero())
                {
                    *root = *right;
                }
                else
                {
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
        unsafe fn is_zero(ptr: *const usize, mut bytes: isize) -> bool {
            let ptr = ptr as *const i8;
            (0..bytes).all(|i| *(ptr.offset(i)) == 0)
        }

        unsafe { is_zero(self.get(node) as *mut TreeElement<T> as *const usize, size_of::<TreeElement<T>>() as isize) }
    }

    fn get(&self, node: T) -> &mut TreeElement<T> {
        // TODO: lol stupid implementation
        unsafe { &mut *(&self.elements[node.as_()] as *const TreeElement<T> as *mut TreeElement<T>) }
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
        &mut self.get(node).left as *mut T
    }

    fn get_mut_right_reference(&mut self, node: T) -> *mut T {
        &mut self.get(node).right as *mut T
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
        self.get(node).left = left
    }

    fn set_right(&mut self, node: T, right: T) {
        self.get(node).right = right
    }

    fn set_size(&mut self, node: T, size: T) {
        self.get(node).size = size
    }

    fn first_is_to_the_left_of_second(&self, first: T, second: T) -> bool {
        first < second
    }

    fn first_is_to_the_right_of_second(&self, first: T, second: T) -> bool {
        first > second
    }
}
