use crate::{
    mem::{
        header::LinksHeader,
        traits::UnitList,
        unit::{
            LinkPart, LinksSourcesRecursionlessSizeBalancedTree,
            LinksTargetsRecursionlessSizeBalancedTree, UnusedLinks,
        },
        UnitTree,
    },
    Doublets, Link, LinksError,
};
use data::{Flow, Flow::Continue, Links, LinksConstants, ReadHandler, ToQuery, WriteHandler};
use leak_slice::LeakSliceExt;
use mem::{RawMem, DEFAULT_PAGE_SIZE};
use num::LinkType;
use num_traits::{one, zero};
use smallvec::SmallVec;
use std::{
    cmp,
    cmp::Ordering,
    error::Error,
    mem::{size_of, transmute},
    ops::{ControlFlow, Try},
    ptr::NonNull,
};

pub struct Store<
    T: LinkType,
    M: RawMem<LinkPart<T>>,
    TS: UnitTree<T> = LinksSourcesRecursionlessSizeBalancedTree<T>,
    TT: UnitTree<T> = LinksTargetsRecursionlessSizeBalancedTree<T>,
    TU: UnitList<T> = UnusedLinks<T>,
> {
    mem: M,
    mem_ptr: NonNull<[LinkPart<T>]>,
    reserve_step: usize,
    constants: LinksConstants<T>,

    sources: TS,
    targets: TT,
    unused: TU,
}

impl<T: LinkType, M: RawMem<LinkPart<T>>, TS: UnitTree<T>, TT: UnitTree<T>, TU: UnitList<T>>
    Store<T, M, TS, TT, TU>
{
    pub const SIZE_STEP: usize = 2_usize.pow(16);

    pub fn new(mem: M) -> Result<Store<T, M>, LinksError<T>> {
        Self::with_constants(mem, LinksConstants::new())
    }

    pub fn with_constants(
        mem: M,
        constants: LinksConstants<T>,
    ) -> Result<Store<T, M>, LinksError<T>> {
        let dangling_mem = NonNull::slice_from_raw_parts(NonNull::dangling(), 0);
        let sources =
            LinksSourcesRecursionlessSizeBalancedTree::new(constants.clone(), dangling_mem);
        let targets =
            LinksTargetsRecursionlessSizeBalancedTree::new(constants.clone(), dangling_mem);
        let unused = UnusedLinks::new(dangling_mem);
        let mut new = Store::<
            T,
            M,
            LinksSourcesRecursionlessSizeBalancedTree<T>,
            LinksTargetsRecursionlessSizeBalancedTree<T>,
            UnusedLinks<T>,
        > {
            mem,
            mem_ptr: dangling_mem,
            reserve_step: Self::SIZE_STEP,
            constants,
            sources,
            targets,
            unused,
        };

        // SAFETY: Without this, the code will become unsafe
        new.init()?;
        Ok(new)
    }

    fn init(&mut self) -> Result<(), LinksError<T>> {
        let mem = NonNull::from(self.mem.alloc(DEFAULT_PAGE_SIZE)?);
        self.update_mem(mem);

        let header = self.get_header().clone();
        let capacity = cmp::max(self.reserve_step, header.allocated.as_());
        let mem = self.mem.alloc(capacity)?.leak();
        self.update_mem(mem);

        self.mem.occupy(header.allocated.as_() + 1)?;

        let reserved = self.mem.allocated();

        let header = self.mut_header();
        header.reserved = T::from_usize(reserved - 1).unwrap();
        Ok(())
    }

    fn mut_from_mem<'a, U>(mut ptr: NonNull<[U]>, index: usize) -> Option<&'a mut U> {
        if index < ptr.len() {
            // SAFETY: `ptr` is non-dangling slice
            Some(unsafe {
                let slice = ptr.as_mut();
                &mut slice[index]
            })
        } else {
            None
        }
    }

    fn get_from_mem<'a, U>(mem: NonNull<[U]>, index: usize) -> Option<&'a U> {
        Self::mut_from_mem(mem, index).map(|v| &*v)
    }

    fn get_header(&self) -> &LinksHeader<T> {
        // SAFETY: `LinksHeader` and `IndexPart` layout are equivalent
        unsafe {
            Self::get_from_mem(self.mem_ptr, 0)
                .map(|x| transmute(x))
                .expect("Header should be in index memory")
        }
    }

    fn mut_header(&mut self) -> &mut LinksHeader<T> {
        // SAFETY: `LinksHeader` and `IndexPart` layout are equivalent
        unsafe {
            Self::mut_from_mem(self.mem_ptr, 0)
                .map(|x| transmute(x))
                .expect("Header should be in index memory")
        }
    }

    fn get_link_part(&self, index: T) -> &LinkPart<T> {
        Self::get_from_mem(self.mem_ptr, index.as_()).expect("Data part should be in data memory")
    }

    unsafe fn get_link_part_unchecked(&self, index: T) -> &LinkPart<T> {
        Self::get_from_mem(self.mem_ptr, index.as_()).unwrap_unchecked()
    }

    fn mut_link_part(&mut self, index: T) -> &mut LinkPart<T> {
        Self::mut_from_mem(self.mem_ptr, index.as_()).expect("Data part should be in data memory")
    }

    fn get_total(&self) -> T {
        let header = self.get_header();
        header.allocated - header.free
    }

    fn is_unused(&self, link: T) -> bool {
        let header = self.get_header();
        if link <= header.allocated && header.first_free != link {
            let link = unsafe { self.get_link_part_unchecked(link) };
            link.size_as_source == zero() && link.source != zero()
        } else {
            true
        }
    }

    fn exists(&self, link: T) -> bool {
        let constants = self.constants();
        let header = self.get_header();
        
        link >= *constants.internal_range.start()
            && link <= header.allocated
            && !self.is_unused(link)
    }

    fn update_mem(&mut self, mem: NonNull<[LinkPart<T>]>) {
        self.mem_ptr = mem;
        self.targets.update_mem(mem);
        self.sources.update_mem(mem);
        self.unused.update_mem(mem);
    }

    unsafe fn get_link_unchecked(&self, index: T) -> Link<T> {
        debug_assert!(self.exists(index));

        let raw = self.get_link_part_unchecked(index);
        Link::new(index, raw.source, raw.target)
    }

    fn each_core<F, R>(&self, handler: &mut F, restriction: impl ToQuery<T>) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let restriction = restriction.to_query();
        let constants = self.constants();
        
        if restriction.len() == 0 {
            for index in T::one()..self.get_header().allocated + one() {
                if let Some(link) = self.get_link(index) {
                    handler(link)?;
                }
            }
            return R::from_output(());
        }
        
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
                        ControlFlow::Continue(_output) => {
                            self.each_core(handler, [index, value, any])
                        }
                        ControlFlow::Break(residual) => R::from_residual(residual),
                    }
                }
            } else if let Some(link) = self.get_link(index) {
                if value == any || link.source == value || link.target == value {
                    handler(link)
                } else {
                    R::from_output(())
                }
            } else {
                R::from_output(())
            };
        }

        if restriction.len() == 3 {
            let source = restriction[constants.source_part.as_()];
            let target = restriction[constants.target_part.as_()];

            return if index == any {
                if (source, target) == (any, any) {
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
                }
            } else if let Some(link) = self.get_link(index) {
                if (target, source) == (any, any) {
                    handler(link) // TODO: add (x * *) search test
                } else if target != any && source != any {
                    if (source, target) == (link.source, link.target) {
                        handler(link)
                    } else {
                        R::from_output(())
                    }
                } else if source == any {
                    if link.target == target {
                        handler(link)
                    } else {
                        R::from_output(())
                    }
                } else if target == any {
                    if link.source == source {
                        handler(link)
                    } else {
                        R::from_output(())
                    }
                } else {
                    R::from_output(())
                }
            } else {
                R::from_output(())
            };
        }
        todo!()
    }

    pub fn iter<'a>(&'a self) -> impl Iterator<Item = Link<T>> + 'a {
        crate::generator! {
            for index in one()..=self.get_header().allocated {
                if let Some(link) = self.get_link(index) {
                    yield link;
                }
            }
        }
    }

    pub fn each_iter(&self, query: impl ToQuery<T>) -> impl Iterator<Item = Link<T>> {
        let count = self.count_by(query.to_query());
        // todo: wait const generics feature
        //  64 / (8 * 3) = 2
        let mut vec = SmallVec::<[_; 2]>::with_capacity(count.as_());

        self.try_each_by(query, |link| {
            vec.push(link);
            Continue
        });

        vec.into_iter()
    }
}

impl<T: LinkType, M: RawMem<LinkPart<T>>, TS: UnitTree<T>, TT: UnitTree<T>, TU: UnitList<T>>
    Doublets<T> for Store<T, M, TS, TT, TU>
{
    fn constants(&self) -> LinksConstants<T> {
        self.constants.clone()
    }

    fn count_by(&self, query: impl ToQuery<T>) -> T {
        let query = query.to_query();

        if query.len() == 0 {
            return self.get_total();
        };

        let constants = self.constants();
        let any = constants.any;
        let index = query[constants.index_part.as_()];

        if query.len() == 1 {
            return if index == any {
                self.get_total()
            } else if self.exists(index) {
                one()
            } else {
                zero()
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
            } else if !self.exists(index) {
                zero()
            } else if (source, target) == (any, any) {
                one()
            } else {
                let link = unsafe { self.get_link_unchecked(index) };
                if source != any && target != any {
                    if (link.source, link.target) == (source, target) {
                        one()
                    } else {
                        zero()
                    }
                } else if source == any {
                    if link.target == target { one() } else { zero() }
                } else if target == any {
                    if link.source == source { one() } else { zero() }
                } else {
                    zero()
                }
            };
        }
        todo!()
    }

    fn create_by_with<F, R>(
        &mut self,
        _query: impl ToQuery<T>,
        mut handler: F,
    ) -> Result<R, LinksError<T>>
    where
        F: FnMut(Link<T>, Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let constants = self.constants();
        let header = self.get_header();
        let mut free = header.first_free;
        if free == constants.null {
            let max_inner = *constants.internal_range.end();
            if header.allocated >= max_inner {
                return Err(LinksError::LimitReached(max_inner));
            }

            if header.allocated >= header.reserved - one() {
                let mem = self
                    .mem
                    .alloc(self.mem.allocated() + self.reserve_step)?
                    .leak();
                self.update_mem(mem);
                let reserved = self.mem.allocated();
                let header = self.mut_header();
                header.reserved = T::from_usize(reserved).unwrap()
            }
            let header = self.mut_header();
            header.allocated = header.allocated + one();
            free = header.allocated;
            self.mem.occupy(self.mem.occupied() + 1)?;
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

        let constants = self.constants();
        let null = constants.null;

        let link = self.get_link_part(index).clone();
        if link.source != null {
            let temp = &mut self.mut_header().root_as_source as *mut T;
            unsafe { self.sources.detach(&mut *temp, index) };
        }

        let link = self.get_link_part(index).clone();
        if link.target != null {
            let temp = &mut self.mut_header().root_as_target as *mut T;
            unsafe { self.targets.detach(&mut *temp, index) };
        }

        let link = self.mut_link_part(index);
        if link.source != source {
            link.source = source;
        }
        if link.target != target {
            link.target = target;
        }

        let link = self.mut_link_part(index).clone();
        if link.source != null {
            let temp = &mut self.mut_header().root_as_source as *mut T;
            unsafe { self.sources.attach(&mut *temp, index) };
        }

        let link = self.mut_link_part(index).clone();
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
        if (source, target) != (zero(), zero()) {
            self.update(index, zero(), zero())?;
        }

        let header = self.get_header();
        let link = index;

        match link.cmp(&header.allocated) {
            Ordering::Less => self.unused.attach_as_first(link),
            Ordering::Greater => unreachable!(),
            Ordering::Equal => {
                self.mem.occupy(self.mem.occupied() - 1)?;

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
                    let used_mem = self.mem.occupied();
                    self.mem.occupy(used_mem - 1)?;
                }
            }
        }

        Ok(handler(Link::new(index, source, target), Link::nothing()))
    }

    fn get_link(&self, index: T) -> Option<Link<T>> {
        if self.constants.is_external(index) {
            Some(Link::point(index))
        } else if self.exists(index) {
            // SAFETY: links is exists
            Some(unsafe { self.get_link_unchecked(index) })
        } else {
            None
        }
    }
}

impl<T: LinkType, M: RawMem<LinkPart<T>>, TS: UnitTree<T>, TT: UnitTree<T>, TU: UnitList<T>>
    Links<T> for Store<T, M, TS, TT, TU>
{
    fn constants_links(&self) -> LinksConstants<T> {
        self.constants()
    }

    fn count_links(&self, query: &[T]) -> T {
        self.count_by(query)
    }

    fn create_links(
        &mut self,
        query: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, Box<dyn Error>> {
        self.create_by_with(query, |before, after| handler(&before[..], &after[..]))
            .map_err(|err| err.into())
    }

    fn each_links(&self, query: &[T], handler: ReadHandler<T>) -> Result<Flow, Box<dyn Error>> {
        Ok(self.try_each_by(query, |link| handler(&link[..])))
    }

    fn update_links(
        &mut self,
        query: &[T],
        replacement: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, Box<dyn Error>> {
        self.update_by_with(query, replacement, |before, after| {
            handler(&before[..], &after[..])
        })
        .map_err(|err| err.into())
    }

    fn delete_links(
        &mut self,
        query: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, Box<dyn Error>> {
        self.delete_by_with(query, |before, after| handler(&before[..], &after[..]))
            .map_err(|err| err.into())
    }
}

unsafe impl<T: LinkType, M: RawMem<LinkPart<T>>, TS: UnitTree<T>, TT: UnitTree<T>, TU: UnitList<T>>
    Sync for Store<T, M, TS, TT, TU>
{
}

unsafe impl<T: LinkType, M: RawMem<LinkPart<T>>, TS: UnitTree<T>, TT: UnitTree<T>, TU: UnitList<T>>
    Send for Store<T, M, TS, TT, TU>
{
}
