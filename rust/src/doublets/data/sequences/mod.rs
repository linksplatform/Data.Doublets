use crate::num::LinkType;
use std::collections::VecDeque;

pub struct StoppableWalker;

impl StoppableWalker {
    pub fn walk_right<T, FS, FT, FE, FV>(
        sequence: T,
        get_source: FS,
        get_target: FT,
        is_element: FE,
        mut visit: FV,
    ) -> bool
    where
        T: LinkType,
        FS: Fn(T) -> T,
        FT: Fn(T) -> T,
        FE: Fn(T) -> bool,
        FV: FnMut(T) -> bool,
    {
        let mut stack = VecDeque::new();
        let mut element = sequence;
        if is_element(element) {
            return visit(element);
        }

        loop {
            if is_element(element) {
                if stack.len() == 0 {
                    return true;
                }
                element = stack.pop_back().unwrap(); // TODO: 100% `Some`
                let source = get_source(element);
                let target = get_target(element);
                if is_element(source) && !visit(source) || is_element(target) && !visit(target) {
                    return false;
                }
                element = target;
            } else {
                stack.push_back(element);
                element = get_source(element);
            }
        }
    }

    pub fn walk_left<T, FS, FT, FE, FV>(
        sequence: T,
        get_source: FS,
        get_target: FT,
        is_element: FE,
        mut visit: FV,
    ) -> bool
    where
        T: LinkType,
        FS: Fn(T) -> T,
        FT: Fn(T) -> T,
        FE: Fn(T) -> bool,
        FV: FnMut(T) -> bool,
    {
        let mut stack = VecDeque::new();
        let mut element = sequence;
        if is_element(element) {
            return visit(element);
        }

        loop {
            if is_element(element) {
                if stack.len() == 0 {
                    return true;
                }
                element = stack.pop_back().unwrap(); // TODO: 100% `Some`
                let source = get_source(element);
                let target = get_target(element);
                if is_element(target) && !visit(target) || is_element(source) && !visit(source) {
                    return false;
                }
                element = target;
            } else {
                stack.push_back(element);
                element = get_target(element);
            }
        }
    }
}
