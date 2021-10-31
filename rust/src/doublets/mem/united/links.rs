use std::default::default;
use std::marker::PhantomData;
use std::mem::size_of;
use std::ops::Index;

use num_traits::{one, zero};
use smallvec::SmallVec;

use crate::doublets;
use crate::doublets::data::IGenericLinks;
use crate::doublets::data::LinksConstants;
use crate::doublets::link::Link;
use crate::doublets::mem::links_header::LinksHeader;
use crate::doublets::mem::united::LinksSourcesRecursionlessSizeBalancedTree;
use crate::doublets::mem::united::LinksSourcesSizeBalancedTree;
use crate::doublets::mem::united::LinksTargetsRecursionlessSizeBalancedTree;
use crate::doublets::mem::united::LinksTargetsSizeBalancedTree;
use crate::doublets::mem::united::RawLink;
use crate::doublets::mem::united::UnusedLinks;
use crate::doublets::mem::ILinksListMethods;
use crate::doublets::mem::ILinksTreeMethods;
use crate::doublets::{data, ILinks};
use crate::mem::FileMappedMem;
use crate::mem::{Mem, ResizeableMem};
use crate::num::LinkType;

// TODO: use `_=_` instead of `_ = _`
pub struct Links<
    T: LinkType,
    M: ResizeableMem,
    TS: ILinksTreeMethods<T> = LinksSourcesRecursionlessSizeBalancedTree<T>,
    TT: ILinksTreeMethods<T> = LinksTargetsRecursionlessSizeBalancedTree<T>,
    TU: ILinksListMethods<T> = UnusedLinks<T>,
> {
    pub mem: M,
    pub reserve_step: usize,
    pub constants: LinksConstants<T>,

    pub sources: TS,
    pub targets: TT,
    pub unused: TU,
}

impl<
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T>,
        TT: ILinksTreeMethods<T>,
        TU: ILinksListMethods<T>,
    > Links<T, M, TS, TT, TU>
{
    pub const LINK_SIZE: usize = size_of::<RawLink<T>>();
    pub const HEADER_SIZE: usize = size_of::<LinksHeader<T>>();
    pub const MAGIC_CONSTANT: usize = 1024 * 1024;
    pub const DEFAULT_LINKS_SIZE_STEP: usize = Self::LINK_SIZE * Self::MAGIC_CONSTANT;

    pub fn new(
        mem: M,
    ) -> Links<
        T,
        M,
        LinksSourcesRecursionlessSizeBalancedTree<T>,
        LinksTargetsRecursionlessSizeBalancedTree<T>,
        UnusedLinks<T>,
    > {
        Self::with_constants(mem, LinksConstants::new())
    }

    pub fn with_constants(
        mem: M,
        constants: LinksConstants<T>,
    ) -> Links<
        T,
        M,
        LinksSourcesRecursionlessSizeBalancedTree<T>,
        LinksTargetsRecursionlessSizeBalancedTree<T>,
        UnusedLinks<T>,
    > {
        let links = mem.get_ptr();
        let header = links;
        let sources =
            LinksSourcesRecursionlessSizeBalancedTree::new(constants.clone(), links, header);
        let targets =
            LinksTargetsRecursionlessSizeBalancedTree::new(constants.clone(), links, header);
        let unused = UnusedLinks::new(links, header);
        let mut new = Links::<
            T,
            M,
            LinksSourcesRecursionlessSizeBalancedTree<T>,
            LinksTargetsRecursionlessSizeBalancedTree<T>,
            UnusedLinks<T>,
        > {
            mem,
            reserve_step: Self::DEFAULT_LINKS_SIZE_STEP,
            constants,
            sources,
            targets,
            unused,
        };

        new.init();
        new
    }

    fn init(&mut self) {
        if self.mem.reserved_mem() < self.reserve_step {
            self.mem.reserve_mem(self.reserve_step).unwrap();
        }
        self.update_pointers();

        let header = self.get_header();
        self.mem
            .use_mem(Self::HEADER_SIZE + header.allocated.as_() * Self::LINK_SIZE)
            .unwrap();

        let reserved = self.mem.reserved_mem();
        let header = self.get_mut_header();
        header.reserved = T::from_usize((reserved - Self::HEADER_SIZE) / Self::LINK_SIZE).unwrap();
    }

    fn get_header(&self) -> &LinksHeader<T> {
        unsafe { &*(self.mem.get_ptr() as *const LinksHeader<T>) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { &mut *(self.mem.get_ptr() as *mut LinksHeader<T>) }
    }

    fn get_link(&self, link: T) -> &RawLink<T> {
        unsafe { &*((self.mem.get_ptr() as *const RawLink<T>).offset(link.as_() as isize)) }
    }

    pub fn get_mut_link(&mut self, link: T) -> &mut RawLink<T> {
        unsafe { &mut *((self.mem.get_ptr() as *mut RawLink<T>).offset(link.as_() as isize)) }
    }

    fn get_total(&self) -> T {
        let header = self.get_header();
        if header.free > header.allocated {
            println!("{:?}][{:?}", header.allocated, header.free);
        }
        header.allocated - header.free
    }

    fn is_unused(&self, link: T) -> bool {
        let header = self.get_header();
        if header.first_free != link {
            let link = self.get_link(link);
            link.size_as_source == zero() && link.source != zero()
        } else {
            true
        }
    }

    fn exists(&self, link: T) -> bool {
        let constants = self.constants();
        let header = self.get_header();

        // TODO: use attributes expressions feature
        // TODO: use `Range::contains`
        link >= *constants.internal_range.start()
            && link <= header.allocated
            && !self.is_unused(link)
    }

    fn update_pointers(&mut self) {
        let ptr = self.mem.get_ptr();
        self.targets.update_pointers(ptr, ptr);
        self.sources.update_pointers(ptr, ptr);
        self.unused.update_pointers(ptr, ptr);
    }

    pub fn show_mem(&self) {
        // TODO:
        println!("mem: {:?}", self.mem.get_ptr());
    }

    pub fn get_link_struct(&self, index: T) -> Link<T> {
        let link = self.get_link(index);
        Link::new(index, link.source, link.target)
    }

    fn each_core<F, const L: usize>(&self, handler: &mut F, restrictions: [T; L]) -> T
    where
        F: FnMut(Link<T>) -> T,
    {
        let constants = self.constants();
        let r#break = constants.r#break;

        if restrictions.len() == 0 {
            for index in T::one()..self.get_header().allocated + one() {
                let link = self.get_link_struct(index);
                if self.exists(index) && handler(link) == r#break {
                    return r#break;
                }
            }
            return r#break;
        }

        let r#continue = constants.r#continue;
        let any = constants.any;
        let index = restrictions[constants.index_part.as_()];

        if restrictions.len() == 1 {
            return if index == any {
                self.each_core(handler, [])
            } else if !self.exists(index) {
                r#continue
            } else {
                let link_struct = self.get_link_struct(index);
                handler(link_struct)
            };
        }

        if restrictions.len() == 2 {
            let value = restrictions[1];
            return if index == any {
                if value == any {
                    self.each_core(handler, [])
                } else if self.each_core(handler, [index, value, any]) == r#break {
                    r#break
                } else {
                    self.each_core(handler, [index, any, value])
                }
            } else {
                if !self.exists(index) {
                    return r#continue;
                }
                if value == any {
                    let link = self.get_link_struct(index);
                    return handler(link);
                }
                let stored = self.get_link(index);
                if stored.source == value || stored.target == value {
                    let link = self.get_link_struct(index);
                    return handler(link);
                }
                r#continue
            };
        }

        if restrictions.len() == 3 {
            let source = restrictions[constants.source_part.as_()];
            let target = restrictions[constants.target_part.as_()];

            if index == any {
                return if (source, target) == (any, any) {
                    self.each_core(handler, [])
                } else if source == any {
                    self.targets.each_usages(target, handler)
                } else if target == any {
                    self.sources.each_usages(source, handler)
                } else {
                    let link = self.sources.search(source, target);
                    if link == constants.null {
                        r#continue
                    } else {
                        let link = self.get_link_struct(link);
                        return handler(link);
                    }
                };
            } else {
                if !self.exists(index) {
                    return zero();
                }
                if (target, source) == (any, any) {
                    return self.get_total();
                }
                let stored = self.get_link(index);
                if target != any && source != any {
                    return if (source, target) == (stored.source, stored.target) {
                        let link = self.get_link_struct(index);
                        handler(link)
                    } else {
                        r#continue
                    };
                }

                let mut value = default();
                if source == any {
                    value = target;
                }
                if target == any {
                    value = source;
                }
                return if (stored.source == value) || (stored.target == value) {
                    let link = self.get_link_struct(index);
                    handler(link)
                } else {
                    r#continue
                };
            }
        }

        //return constants.r#break;
        panic!("not supported");
    }
}

impl<
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T>,
        TT: ILinksTreeMethods<T>,
        TU: ILinksListMethods<T>,
    > doublets::ILinks<T> for Links<T, M, TS, TT, TU>
{
    fn constants(&self) -> LinksConstants<T> {
        self.constants.clone()
    }

    fn count_by<const L: usize>(&self, restrictions: [T; L]) -> T {
        if restrictions.len() == 0 {
            return self.get_total();
        };

        let constants = self.constants();
        let any = constants.any;
        let index = restrictions[constants.index_part.as_()];

        if restrictions.len() == 1 {
            return if index == any {
                self.get_total()
            } else {
                // TODO: T::from_usize(self.exists(index) as usize).unwrap()
                if self.exists(index) {
                    one()
                } else {
                    zero()
                }
            };
        }

        if restrictions.len() == 2 {
            let value = restrictions[1];
            return if index == any {
                if value == any {
                    self.get_total()
                } else {
                    self.targets.count_usages(value) + self.sources.count_usages(value)
                }
            } else {
                if !self.exists(index) {
                    return zero();
                }
                if value == any {
                    return one();
                }

                let stored = self.get_link(index);
                return if stored.source == value || stored.target == value {
                    one()
                } else {
                    zero()
                };
            };
        }

        if restrictions.len() == 3 {
            let source = restrictions[constants.source_part.as_()];
            let target = restrictions[constants.target_part.as_()];

            return if index == any {
                if (target, source) == (any, any) {
                    self.get_total()
                } else if source == any {
                    self.targets.count_usages(target)
                } else if target == any {
                    self.sources.count_usages(source)
                } else {
                    let link = self.sources.search(source, target);
                    if link == constants.null {
                        zero()
                    } else {
                        one()
                    }
                }
            } else {
                if !self.exists(index) {
                    return zero();
                }
                if (target, source) == (any, any) {
                    return self.get_total();
                }
                let stored = self.get_link(index);

                if target != any && source != any {
                    return if (source, target) == (stored.source, stored.target) {
                        one()
                    } else {
                        zero()
                    };
                }

                let mut value = default();
                if source == any {
                    value = target;
                }
                if target == any {
                    value = source;
                }
                if (stored.source == value) || (stored.target == value) {
                    one()
                } else {
                    zero()
                }
            };
        }

        panic!("not implemented")
    }

    fn create(&mut self) -> T {
        let constants = self.constants();
        let header = self.get_header();
        let mut free = header.first_free;
        if free == constants.null {
            let max_inner = *constants.internal_range.end();
            if header.allocated > max_inner {
                todo!()
            }

            if header.allocated >= header.reserved - one() {
                self.mem
                    .reserve_mem(self.mem.reserved_mem() + self.reserve_step);
                self.update_pointers();
                let reserved = self.mem.reserved_mem();
                let header = self.get_mut_header();
                header.reserved = T::from_usize(reserved / Self::LINK_SIZE).unwrap()
            }
            let header = self.get_mut_header();
            header.allocated = header.allocated + one();
            free = header.allocated;
            self.mem.use_mem(self.mem.used_mem() + Self::LINK_SIZE);
        } else {
            self.unused.detach(free)
        }
        return free;
    }

    fn each_by<H, const L: usize>(&self, mut handler: H, restrictions: [T; L]) -> T
    where
        H: FnMut(Link<T>) -> T,
    {
        self.each_core(&mut handler, restrictions)
    }

    fn update(&mut self, index: T, source: T, target: T) -> T {
        let constants = self.constants();
        let null = constants.null;

        unsafe {
            let link = self.get_mut_link(index) as *mut RawLink<T>;

            // TODO: memory filled with zeros
            if (*link).source != null {
                let temp = &mut self.get_mut_header().root_as_source as *mut T;
                unsafe { self.sources.detach(&mut *temp, index) };
            }
            if (*link).target != null {
                let temp = &mut self.get_mut_header().root_as_target as *mut T;
                unsafe { self.targets.detach(&mut *temp, index) };
            }

            (*link).source = source;
            (*link).target = target;

            if (*link).source != null {
                let temp = &mut self.get_mut_header().root_as_source as *mut T;
                unsafe { self.sources.attach(&mut *temp, index) };
            }

            if (*link).target != null {
                let temp = &mut self.get_mut_header().root_as_target as *mut T;
                unsafe { self.targets.attach(&mut *temp, index) };
            }
        }

        index
    }

    fn delete(&mut self, index: T) -> T {
        if !self.exists(index) { return default(); }
        // TODO:
        self.update(index, zero(), zero());
        // TODO:

        // TODO: move to `delete_core`

        let constants = self.constants();
        let header = self.get_header();
        let link = index;
        if link < header.allocated {
            self.unused.attach_as_first(link)
        } else if link == header.allocated {
            unsafe {
                let mut header = self.get_mut_header() as *mut LinksHeader<T>;

                self.mem
                    .use_mem(self.mem.used_mem() - Self::LINK_SIZE)
                    .unwrap();

                (*header).allocated = (*header).allocated - one();

                while (*header).allocated > zero() && self.is_unused((*header).allocated) {
                    self.unused.detach((*header).allocated);
                    (*header).allocated = (*header).allocated - one();
                    let used_mem = self.mem.used_mem();
                    self.mem.use_mem(used_mem - Self::LINK_SIZE).unwrap();
                }
            }
            //*self.get_mut_header() = header;
        }
        return index;
    }

    fn get_link(&self, index: T) -> Option<Link<T>> {
        if self.constants.is_external_reference(index) {
            Some(Link::from_once(index))
        } else {
            if self.exists(index) {
                Some(self.get_link_struct(index))
            } else {
                None
            }
        }
    }
}
