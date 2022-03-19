use crate::data::Links;
use crate::data::LinksConstants;
use crate::data::ToQuery;
use crate::doublets;
use crate::doublets::link::Link;
use crate::doublets::mem::links_header::LinksHeader;
use crate::doublets::mem::united::LinksTargetsRecursionlessSizeBalancedTree;
use crate::doublets::mem::united::RawLink;
use crate::doublets::mem::united::UnusedLinks;
use crate::doublets::mem::united::{LinksSourcesRecursionlessSizeBalancedTree, NewList, NewTree};
use crate::doublets::mem::ILinksTreeMethods;
use crate::doublets::mem::{ILinksListMethods, UpdatePointers};
use crate::doublets::Result;
use crate::doublets::{Doublets, LinksError};
use crate::mem::{Mem, ResizeableMem};
use crate::num::LinkType;
use num_traits::{one, zero};
use std::default::default;
use std::mem::size_of;
use std::ops::{ControlFlow, Try};

pub struct Store<
    T: LinkType,
    M: ResizeableMem,
    TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers = LinksSourcesRecursionlessSizeBalancedTree<T>,
    TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers = LinksTargetsRecursionlessSizeBalancedTree<T>,
    TU: ILinksListMethods<T> + NewList<T> + UpdatePointers = UnusedLinks<T>,
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
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    > Store<T, M, TS, TT, TU>
{
    pub const LINK_SIZE: usize = size_of::<RawLink<T>>();
    pub const HEADER_SIZE: usize = size_of::<LinksHeader<T>>();
    pub const MAGIC_CONSTANT: usize = 1024 * 1024;
    pub const DEFAULT_LINKS_SIZE_STEP: usize = Self::LINK_SIZE * Self::MAGIC_CONSTANT;

    pub fn new(mem: M) -> Result<Store<T, M>, LinksError<T>> {
        Self::with_constants(mem, LinksConstants::new())
    }

    pub fn with_constants(
        mem: M,
        constants: LinksConstants<T>,
    ) -> Result<Store<T, M>, LinksError<T>> {
        let links = mem.get_ptr().as_mut_ptr();
        let header = links;
        let sources =
            LinksSourcesRecursionlessSizeBalancedTree::new(constants.clone(), links, header);
        let targets =
            LinksTargetsRecursionlessSizeBalancedTree::new(constants.clone(), links, header);
        let unused = UnusedLinks::new(links, header);
        let mut new = Store::<
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

        new.init()?;
        Ok(new)
    }

    fn init(&mut self) -> Result<(), LinksError<T>> {
        if self.mem.reserved_mem() < self.reserve_step {
            self.mem.reserve_mem(self.reserve_step)?;
        }
        self.update_pointers();

        let header = *self.get_header();
        self.mem
            .use_mem(Self::HEADER_SIZE + header.allocated.as_() * Self::LINK_SIZE)?;

        let reserved = if let Some(min) =
            (self.constants.internal_range.end().as_() + 1).checked_mul(Self::LINK_SIZE)
        {
            self.mem.reserved_mem().min(min)
        } else {
            self.mem.reserved_mem()
        };

        let header = self.mut_header();
        header.reserved = T::from_usize((reserved - Self::HEADER_SIZE) / Self::LINK_SIZE).unwrap();
        Ok(())
    }

    fn mut_from_mem<Output: Sized, Memory: Mem>(mem: &Memory, index: usize) -> Option<&mut Output> {
        let mut ptr = mem.get_ptr();
        let sizeof = size_of::<Output>();
        if index * sizeof < ptr.len() {
            Some(unsafe {
                let slice = ptr.as_mut();
                let (_, slice, _) = slice.align_to_mut();
                &mut slice[index]
            })
        } else {
            None
        }
    }

    fn get_from_mem<Output: Sized, Memory: Mem>(mem: &Memory, index: usize) -> Option<&Output> {
        Self::mut_from_mem(mem, index).map(|v| &*v)
    }

    fn get_header(&self) -> &LinksHeader<T> {
        Self::get_from_mem(&self.mem, 0).expect("Header should be in index memory")
    }

    fn mut_header(&mut self) -> &mut LinksHeader<T> {
        Self::mut_from_mem(&self.mem, 0).expect("Header should be in index memory")
    }

    fn get_raw_link(&self, index: T) -> &RawLink<T> {
        Self::get_from_mem(&self.mem, index.as_()).expect("Data part should be in data memory")
    }

    unsafe fn get_raw_link_unchecked(&self, index: T) -> &RawLink<T> {
        Self::get_from_mem(&self.mem, index.as_()).unwrap_unchecked()
    }

    fn mut_raw_link(&mut self, index: T) -> &mut RawLink<T> {
        Self::mut_from_mem(&self.mem, index.as_()).expect("Data part should be in data memory")
    }

    fn get_total(&self) -> T {
        let header = self.get_header();
        header.allocated - header.free
    }

    fn is_unused(&self, link: T) -> bool {
        let header = self.get_header();
        if link <= header.allocated && header.first_free != link {
            let link = unsafe { self.get_raw_link_unchecked(link) };
            link.size_as_source == zero() && link.source != zero()
        } else {
            true
        }
    }

    fn exists(&self, link: T) -> bool {
        let constants = self.constants_links();
        let header = self.get_header();

        // TODO: use `Range::contains`
        link >= *constants.internal_range.start()
            && link <= header.allocated
            && !self.is_unused(link)
    }

    fn update_pointers(&mut self) {
        let ptr = self.mem.get_ptr().as_mut_ptr();
        self.targets.update_pointers(ptr, ptr);
        self.sources.update_pointers(ptr, ptr);
        self.unused.update_pointers(ptr, ptr);
    }

    unsafe fn get_link_unchecked(&self, index: T) -> Link<T> {
        debug_assert!(self.exists(index));

        let raw = self.get_raw_link_unchecked(index);
        Link::new(index, raw.source, raw.target)
    }

    fn each_core<F, R>(&self, handler: &mut F, restriction: impl ToQuery<T>) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let restriction = restriction.to_query();

        let constants = self.constants_links();
        let r#break = constants.r#break;

        if restriction.len() == 0 {
            for index in T::one()..self.get_header().allocated + one() {
                if let Some(link) = self.get_link(index) {
                    handler(link)?;
                }
            }
            return R::from_output(());
        }

        let r#continue = constants.r#continue;
        let any = constants.any;
        let index = restriction[constants.index_part.as_()];

        if restriction.len() == 1 {
            return if index == any {
                self.each_core(handler, [])
            } else if let Some(link) = self.get_link(index) {
                handler(link)
            } else {
                R::from_output(())
            };
        }

        if restriction.len() == 2 {
            let value = restriction[1];
            return if index == any {
                if value == any {
                    self.each_core(handler, [])
                } else {
                    match self.each_core(handler, [index, value, any]).branch() {
                        ControlFlow::Continue(output) => {
                            self.each_core(handler, [index, value, any])
                        }
                        ControlFlow::Break(residual) => R::from_residual(residual),
                    }
                }
            } else {
                if let Some(link) = self.get_link(index) {
                    if value == any {
                        return handler(link);
                    } else if link.source == value || link.target == value {
                        return handler(link);
                    } else {
                        R::from_output(())
                    }
                } else {
                    R::from_output(())
                }
            };
        }

        if restriction.len() == 3 {
            let source = restriction[constants.source_part.as_()];
            let target = restriction[constants.target_part.as_()];

            if index == any {
                return if (source, target) == (any, any) {
                    self.each_core(handler, [])
                } else if source == any {
                    self.targets.each_usages(target, handler)
                } else if target == any {
                    self.sources.each_usages(source, handler)
                } else {
                    let link = self.sources.search(source, target);
                    if let Some(link) = self.get_link(link) {
                        handler(link)
                    } else {
                        R::from_output(())
                    }
                };
            } else {
                let link;
                if let Some(_link) = self.get_link(index) {
                    link = _link;
                } else {
                    link = Link::nothing();
                    return R::from_output(());
                }
                if (target, source) == (any, any) {
                    return handler(link); // TODO: add (x * *) search test
                }
                if target != any && source != any {
                    return if (source, target) == (link.source, link.target) {
                        handler(link)
                    } else {
                        R::from_output(())
                    };
                }

                let mut value = default();
                if source == any {
                    value = target;
                }
                if target == any {
                    value = source;
                }
                return if (link.source == value) || (link.target == value) {
                    handler(link)
                } else {
                    R::from_output(())
                };
            }
        }
        todo!()
    }
}

impl<
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    > doublets::Doublets<T> for Store<T, M, TS, TT, TU>
{
    fn constants(&self) -> LinksConstants<T> {
        self.constants.clone()
    }

    fn count_by(&self, query: impl ToQuery<T>) -> T {
        let query = query.to_query();

        if query.len() == 0 {
            return self.get_total();
        };

        let constants = self.constants_links();
        let any = constants.any;
        let index = query[constants.index_part.as_()];

        if query.len() == 1 {
            return if index == any {
                self.get_total()
            } else {
                if self.exists(index) {
                    one()
                } else {
                    zero()
                }
            };
        }

        if query.len() == 2 {
            let value = query[1];
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

                return if let Some(stored) = self.get_link(index) {
                    if stored.source == value || stored.target == value {
                        one()
                    } else {
                        zero()
                    }
                } else {
                    zero()
                };
            };
        }

        if query.len() == 3 {
            let source = query[constants.source_part.as_()];
            let target = query[constants.target_part.as_()];

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
                let link;
                if let Some(_link) = self.get_link(index) {
                    link = _link;
                } else {
                    link = Link::nothing();
                    return zero();
                }
                if (target, source) == (any, any) {
                    return self.get_total();
                }
                if target != any && source != any {
                    return if (source, target) == (link.source, link.target) {
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
                if (link.source == value) || (link.target == value) {
                    one()
                } else {
                    zero()
                }
            };
        }
        todo!()
    }

    fn create_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let constants = self.constants_links();
        let header = self.get_header();
        let mut free = header.first_free;
        if free == constants.null {
            let max_inner = *constants.internal_range.end();
            if header.allocated >= max_inner {
                return Err(LinksError::LimitReached(max_inner));
            }

            if header.allocated >= header.reserved - one() {
                self.mem
                    .reserve_mem(self.mem.reserved_mem() + self.reserve_step)?;
                self.update_pointers();
                let reserved = self.mem.reserved_mem();
                let header = self.mut_header();
                header.reserved = T::from_usize(reserved / Self::LINK_SIZE).unwrap()
            }
            let header = self.mut_header();
            header.allocated = header.allocated + one();
            free = header.allocated;
            self.mem.use_mem(self.mem.used_mem() + Self::LINK_SIZE)?;
        } else {
            self.unused.detach(free)
        }
        Ok(handler(
            Link::nothing(),
            Link::new(free, T::zero(), T::zero()),
        ))
    }

    fn try_each_by<F, R>(&self, restrictions: impl ToQuery<T>, mut handler: F) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.each_core(&mut handler, restrictions.to_query())
    }

    fn update_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        replacement: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let query = query.to_query();
        let replacement = replacement.to_query();

        let index = query[0];
        let source = replacement[1];
        let target = replacement[2];
        let old_source = source;
        let old_target = target;

        let constants = self.constants_links();
        let null = constants.null;

        let link = *self.mut_raw_link(index);
        if link.source != null {
            let temp = &mut self.mut_header().root_as_source as *mut T;
            unsafe { self.sources.detach(&mut *temp, index) };
        }

        let link = *self.mut_raw_link(index);
        if link.target != null {
            let temp = &mut self.mut_header().root_as_target as *mut T;
            unsafe { self.targets.detach(&mut *temp, index) };
        }

        let link = self.mut_raw_link(index);
        link.source = source;
        link.target = target;

        let link = *self.mut_raw_link(index);
        if link.source != null {
            let temp = &mut self.mut_header().root_as_source as *mut T;
            unsafe { self.sources.attach(&mut *temp, index) };
        }

        let link = *self.mut_raw_link(index);
        if link.target != null {
            let temp = &mut self.mut_header().root_as_target as *mut T;
            unsafe { self.targets.attach(&mut *temp, index) };
        }

        Ok(handler(
            Link::new(index, old_source, old_target),
            Link::new(index, source, target),
        ))
    }

    fn delete_by_with<F, R>(
        &mut self,
        query: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let query = query.to_query();

        let index = query[0];
        // TODO: use method style - remove .get_link
        let (source, target) = if let Some(link) = self.get_link(index) {
            (link.source, link.target)
        } else {
            return Err(LinksError::NotExists(index));
        };
        self.update(index, zero(), zero())?;

        let header = self.get_header();
        let link = index;
        if link < header.allocated {
            self.unused.attach_as_first(link)
        } else if link == header.allocated {
            self.mem.use_mem(self.mem.used_mem() - Self::LINK_SIZE)?;

            let allocated = self.get_header().allocated;
            let header = self.mut_header();
            header.allocated = allocated - one();

            loop {
                let allocated = self.get_header().allocated;
                if !(allocated > zero() && self.is_unused(allocated)) {
                    break;
                }
                self.unused.detach(allocated);
                self.mut_header().allocated = allocated - one();
                let used_mem = self.mem.used_mem();
                self.mem.use_mem(used_mem - Self::LINK_SIZE)?;
            }
        }
        Ok(handler(Link::new(index, source, target), Link::nothing()))
    }

    fn get_link(&self, index: T) -> Option<Link<T>> {
        if self.constants.is_external(index) {
            Some(Link::point(index))
        } else if self.exists(index) {
            Some(unsafe { self.get_link_unchecked(index) })
        } else {
            None
        }
    }
}
