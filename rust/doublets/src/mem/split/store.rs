use std::{cmp::Ordering, default::default, error::Error, mem::transmute, ptr::NonNull};

use num_traits::{one, zero};

use crate::{
    mem::{
        split::{
            DataPart, ExternalSourcesRecursionlessTree, ExternalTargetsRecursionlessTree,
            IndexPart, InternalSourcesLinkedList, InternalSourcesRecursionlessTree,
            InternalTargetsRecursionlessTree, UnusedLinks,
        },
        LinksHeader, LinksTree, SplitList, SplitTree, SplitUpdateMem,
    },
    Doublets, Link, Links, LinksError, ReadHandler, WriteHandler,
};
use data::{Flow, Flow::Continue, LinksConstants, ToQuery};
use mem::{RawMem, DEFAULT_PAGE_SIZE};
use methods::RelativeCircularLinkedList;
use num::LinkType;

pub struct Store<
    T: LinkType,
    MD: RawMem<DataPart<T>>,
    MI: RawMem<IndexPart<T>>,
    IS: SplitTree<T> = InternalSourcesRecursionlessTree<T>,
    ES: SplitTree<T> = ExternalSourcesRecursionlessTree<T>,
    IT: SplitTree<T> = InternalTargetsRecursionlessTree<T>,
    ET: SplitTree<T> = ExternalTargetsRecursionlessTree<T>,
    UL: SplitList<T> = UnusedLinks<T>,
> {
    data_mem: MD,
    index_mem: MI,

    data_ptr: NonNull<[DataPart<T>]>,
    index_ptr: NonNull<[IndexPart<T>]>,

    data_step: usize,
    index_step: usize,

    constants: LinksConstants<T>,

    internal_sources: IS,
    pub external_sources: ES,
    internal_targets: IT,
    pub external_targets: ET,

    sources_list: InternalSourcesLinkedList<T>,
    unused: UL,
}

impl<
    T: LinkType,
    MD: RawMem<DataPart<T>>,
    MI: RawMem<IndexPart<T>>,
    IS: SplitTree<T>,
    ES: SplitTree<T>,
    IT: SplitTree<T>,
    ET: SplitTree<T>,
    UL: SplitList<T>,
> Store<T, MD, MI, IS, ES, IT, ET, UL>
{
    const USE_LIST: bool = false;
    const SIZE_STEP: usize = 2_usize.pow(20);

    // TODO: create Options
    pub fn with_constants(
        data_mem: MD,
        index_mem: MI,
        constants: LinksConstants<T>,
    ) -> Result<Store<T, MD, MI>, LinksError<T>> {
        let dangling_data = NonNull::slice_from_raw_parts(NonNull::dangling(), 0);
        let dangling_index = NonNull::slice_from_raw_parts(NonNull::dangling(), 0);

        let internal_sources =
            InternalSourcesRecursionlessTree::new(constants.clone(), dangling_data, dangling_index);
        let external_sources =
            ExternalSourcesRecursionlessTree::new(constants.clone(), dangling_data, dangling_index);

        let internal_targets =
            InternalTargetsRecursionlessTree::new(constants.clone(), dangling_data, dangling_index);
        let external_targets =
            ExternalTargetsRecursionlessTree::new(constants.clone(), dangling_data, dangling_index);

        let sources_list =
            InternalSourcesLinkedList::new(constants.clone(), dangling_data, dangling_index);
        let unused = UnusedLinks::new(dangling_data, dangling_index);

        let mut new = Store {
            data_mem,
            index_mem,
            data_ptr: dangling_data,
            index_ptr: dangling_index,
            data_step: Self::SIZE_STEP,
            index_step: Self::SIZE_STEP,
            constants,
            internal_sources,
            external_sources,
            internal_targets,
            external_targets,
            sources_list,
            unused,
        };

        // SAFETY: Without this, the code will become unsafe
        unsafe {
            new.init()?;
        }
        Ok(new)
    }

    pub fn new(data_mem: MD, index_mem: MI) -> Result<Store<T, MD, MI>, LinksError<T>> {
        Self::with_constants(data_mem, index_mem, default())
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

    fn get_from_mem<'a, U>(ptr: NonNull<[U]>, index: usize) -> Option<&'a U> {
        Self::mut_from_mem(ptr, index).map(|v| &*v)
    }

    fn get_header(&self) -> &LinksHeader<T> {
        // SAFETY: `LinksHeader` and `IndexPart` layout are equivalent
        unsafe {
            Self::get_from_mem(self.index_ptr, 0)
                .map(|x| transmute(x))
                .expect("Header should be in index memory")
        }
    }

    fn mut_header(&mut self) -> &mut LinksHeader<T> {
        // SAFETY: `LinksHeader` and `IndexPart` layout are equivalent
        unsafe {
            Self::mut_from_mem(self.index_ptr, 0)
                .map(|x| transmute(x))
                .expect("Header should be in index memory")
        }
    }

    pub fn get_data_part(&self, index: T) -> &DataPart<T> {
        Self::get_from_mem(self.data_ptr, index.as_()).expect("Data part should be in data memory")
    }

    unsafe fn get_data_unchecked(&self, index: T) -> &DataPart<T> {
        Self::get_from_mem(self.data_ptr, index.as_()).unwrap_unchecked()
    }

    fn mut_data_part(&mut self, index: T) -> &mut DataPart<T> {
        Self::mut_from_mem(self.data_ptr, index.as_()).expect("Data part should be in data memory")
    }

    pub fn get_index_part(&self, index: T) -> &IndexPart<T> {
        Self::get_from_mem(self.index_ptr, index.as_())
            .expect("Index part should be in index memory")
    }

    fn mut_index_part(&mut self, index: T) -> &mut IndexPart<T> {
        Self::mut_from_mem(self.index_ptr, index.as_())
            .expect("Index part should be in index memory")
    }

    unsafe fn mut_source_header_root(&mut self) -> *mut T {
        &mut self.mut_header().root_as_source
    }

    unsafe fn mut_target_header_root(&mut self) -> *mut T {
        &mut self.mut_header().root_as_target
    }

    unsafe fn mut_source_root(&mut self, link: T) -> *mut T {
        &mut self.mut_index_part(link).root_as_source
    }

    unsafe fn mut_target_root(&mut self, link: T) -> *mut T {
        &mut self.mut_index_part(link).root_as_target
    }

    unsafe fn detach_internal_source_unchecked(&mut self, root: *mut T, index: T) {
        self.internal_sources.detach(&mut *root, index)
    }

    unsafe fn detach_internal_target_unchecked(&mut self, root: *mut T, index: T) {
        self.internal_targets.detach(&mut *root, index)
    }

    unsafe fn attach_internal_source_unchecked(&mut self, root: *mut T, index: T) {
        self.internal_sources.attach(&mut *root, index)
    }

    unsafe fn attach_internal_target_unchecked(&mut self, root: *mut T, index: T) {
        self.internal_targets.attach(&mut *root, index)
    }

    unsafe fn detach_external_source_unchecked(&mut self, root: *mut T, index: T) {
        self.external_sources.detach(&mut *root, index)
    }

    unsafe fn detach_external_target_unchecked(&mut self, root: *mut T, index: T) {
        self.external_targets.detach(&mut *root, index)
    }

    unsafe fn attach_external_source_unchecked(&mut self, root: *mut T, index: T) {
        self.external_sources.attach(&mut *root, index)
    }

    unsafe fn attach_external_target_unchecked(&mut self, root: *mut T, index: T) {
        self.external_targets.attach(&mut *root, index)
    }

    unsafe fn detach_internal_source(&mut self, root: T, index: T) {
        let root = self.mut_source_root(root);
        self.detach_internal_source_unchecked(root, index);
    }

    unsafe fn detach_internal_target(&mut self, root: T, index: T) {
        let root = self.mut_target_root(root);
        self.detach_internal_target_unchecked(root, index);
    }

    unsafe fn attach_internal_source(&mut self, root: T, index: T) {
        let root = self.mut_source_root(root);
        self.attach_internal_source_unchecked(root, index);
    }

    unsafe fn attach_internal_target(&mut self, root: T, index: T) {
        let root = self.mut_target_root(root);
        self.attach_internal_target_unchecked(root, index);
    }

    unsafe fn detach_external_source(&mut self, index: T) {
        let root = self.mut_source_header_root();
        self.detach_external_source_unchecked(root, index);
    }

    unsafe fn detach_external_target(&mut self, index: T) {
        let root = self.mut_target_header_root();
        self.detach_external_target_unchecked(root, index);
    }

    unsafe fn attach_external_source(&mut self, index: T) {
        let root = self.mut_source_header_root();
        self.attach_external_source_unchecked(root, index);
    }

    unsafe fn attach_external_target(&mut self, index: T) {
        let root = self.mut_target_header_root();
        self.attach_external_target_unchecked(root, index);
    }

    fn update_mem(&mut self, data: NonNull<[DataPart<T>]>, index: NonNull<[IndexPart<T>]>) {
        self.data_ptr = data;
        self.index_ptr = index;

        self.internal_sources.update_mem(data, index);
        self.external_sources.update_mem(data, index);
        self.internal_targets.update_mem(data, index);
        self.external_targets.update_mem(data, index);
        self.sources_list.update_mem(data, index);
        self.unused.update_mem(data, index);
    }

    fn align(mut to_align: usize, target: usize) -> usize {
        debug_assert!(to_align >= target);

        // TODO: optimize this `if`
        if to_align % target != 0 {
            to_align = ((to_align / target) * target) + target;
        }
        to_align
    }

    unsafe fn init(&mut self) -> Result<(), LinksError<T>> {
        let data = NonNull::from(self.data_mem.alloc(DEFAULT_PAGE_SIZE)?);
        let index = NonNull::from(self.index_mem.alloc(DEFAULT_PAGE_SIZE)?);
        self.update_mem(data, index);

        let header = self.get_header().clone();
        let allocated = header.allocated.as_();

        let mut data_capacity = allocated;
        data_capacity = data_capacity.max(self.data_mem.allocated());
        data_capacity = data_capacity.max(self.data_step);

        let mut index_capacity = allocated;
        index_capacity = index_capacity.max(self.index_mem.allocated());
        index_capacity = index_capacity.max(self.index_step);

        data_capacity = Self::align(data_capacity, self.data_step);
        index_capacity = Self::align(index_capacity, self.index_step);

        let data = NonNull::from(self.data_mem.alloc(data_capacity)?);
        let index = NonNull::from(self.index_mem.alloc(index_capacity)?);
        self.update_mem(data, index);

        let allocated = header.allocated.as_();
        self.data_mem.occupy(allocated + 1)?;
        self.index_mem.occupy(allocated + 1)?;

        self.mut_header().reserved = T::from(self.data_mem.allocated() - 1).unwrap();
        Ok(())
    }

    fn total(&self) -> T {
        let header = self.get_header();
        header.allocated - header.free
    }

    pub fn is_unused(&self, link: T) -> bool {
        let header = self.get_header();
        if link <= header.allocated && header.first_free != link {
            // TODO: May be this check is not needed
            let index = self.get_index_part(link);
            let data = self.get_data_part(link);
            index.size_as_target.is_zero() && !data.source.is_zero()
        } else {
            true
        }
    }

    //fn is_non_

    pub fn is_virtual(&self, link: T) -> bool {
        self.is_unused(link)
    }

    pub fn exists(&self, link: T) -> bool {
        let constants = self.constants();
        let header = self.get_header();

        // TODO: use attributes expressions feature
        // TODO: use `Range::contains`
        link >= *constants.internal_range.start()
            && link <= header.allocated
            && !self.is_unused(link)
    }

    // SAFETY: must be link exists
    unsafe fn get_link_unchecked(&self, index: T) -> Link<T> {
        debug_assert!(self.exists(index));

        let raw = self.get_data_unchecked(index);
        Link::new(index, raw.source, raw.target)
    }

    fn try_each_by_core(&self, handler: ReadHandler<T>, query: &[T]) -> Flow {
        let query = query.to_query();

        if query.len() == 0 {
            for index in one()..=self.get_header().allocated {
                if let Some(link) = self.get_link(index) {
                    handler(link)?;
                }
            }
            return Flow::Continue;
        }

        let constants = self.constants.clone();
        let any = constants.any;
        let _continue = constants.r#continue;
        let index = query[constants.index_part.as_()];
        if query.len() == 1 {
            return if index == any {
                self.try_each_by_core(handler, &[])
            } else if let Some(link) = self.get_link(index) {
                handler(link)
            } else {
                Flow::Continue
            };
        }
        //
        if query.len() == 2 {
            let value = query[1];
            return if index == any {
                if value == any {
                    self.try_each_by_core(handler, &[])
                } else {
                    self.try_each_by_core(handler, &[index, value, any])?;
                    self.try_each_by_core(handler, &[index, any, value])
                }
            } else if let Some(link) = self.get_link(index) {
                if value == any || link.source == value || link.target == value {
                    handler(link)
                } else {
                    Flow::Continue
                }
            } else {
                Flow::Continue
            };
        }
        //
        if query.len() == 3 {
            let source = query[constants.source_part.as_()];
            let target = query[constants.target_part.as_()];
            let is_virtual_source = self.is_virtual(source);
            let is_virtual_target = self.is_virtual(target);

            return if index == any {
                if (source, target) == (any, any) {
                    self.try_each_by_core(handler, &[])
                } else if source == any {
                    if is_virtual_target {
                        self.external_targets.each_usages(target, handler)
                    } else {
                        self.internal_targets.each_usages(target, handler)
                    }
                } else if target == any {
                    if is_virtual_source {
                        self.external_sources.each_usages(source, handler)
                    } else if Self::USE_LIST {
                        self.sources_list.each_usages(source, handler)
                    } else {
                        self.internal_sources.each_usages(source, handler)
                    }
                } else {
                    let link = if true {
                        if is_virtual_source && is_virtual_target {
                            self.external_sources.search(source, target)
                        } else if is_virtual_source {
                            self.internal_targets.search(source, target)
                        } else if is_virtual_target {
                            if Self::USE_LIST {
                                self.external_sources.search(source, target)
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        } else if Self::USE_LIST
                            || self.internal_sources.count_usages(source)
                                > self.internal_targets.count_usages(target)
                        {
                            self.internal_targets.search(source, target)
                        } else {
                            self.internal_sources.search(source, target)
                        }
                    } else if Self::USE_LIST
                        || self.internal_sources.count_usages(source)
                            > self.internal_targets.count_usages(target)
                    {
                        self.internal_targets.search(source, target)
                    } else {
                        self.internal_sources.search(source, target)
                    };
                    return if link == constants.null {
                        Flow::Continue
                    } else {
                        // SAFETY: link 100% exists
                        let link = unsafe { self.get_link(link).unwrap_unchecked() };
                        handler(link)
                    };
                }
            } else if let Some(link) = self.get_link(index) {
                if (source, target) == (any, any) {
                    handler(link)
                } else if source != any && target != any {
                    if (link.source, link.target) == (source, target) {
                        handler(link)
                    } else {
                        Flow::Continue
                    }
                } else if source == any {
                    if link.target == target {
                        handler(link)
                    } else {
                        Flow::Continue
                    }
                } else if target == any {
                    if link.source == source {
                        handler(link)
                    } else {
                        Flow::Continue
                    }
                } else {
                    Flow::Continue
                }
            } else {
                Flow::Continue
            };
        }
        todo!()
    }

    fn resolve_danglind_internal(&mut self, index: T) {
        let any = self.constants.any;
        for link in self
            .each_iter([any, index, any])
            .filter(|link| link.index != index)
        {
            unsafe {
                self.detach_internal_source(index, link.index);
                self.attach_external_source(link.index);
            }
        }

        for link in self
            .each_iter([any, any, index])
            .filter(|link| link.index != index)
            .filter(|link| !link.is_full())
        {
            unsafe {
                self.detach_internal_target(index, link.index);
                self.attach_external_target(link.index);
            }
        }
    }

    fn resolve_danglind_external(&mut self, free: T) {
        let any = self.constants().any;
        for link in self
            .each_iter([any, free, any])
            .filter(|link| link.index != free)
        {
            unsafe {
                self.detach_external_source(link.index);
                self.attach_internal_source(free, link.index);
            }
        }

        for link in self
            .each_iter([any, any, free])
            .filter(|link| link.index != free)
            .filter(|link| !link.is_full())
        {
            unsafe {
                self.detach_external_target(link.index);
                self.attach_internal_target(free, link.index);
            }
        }
    }
}

impl<
    T: LinkType,
    MD: RawMem<DataPart<T>>,
    MI: RawMem<IndexPart<T>>,
    IS: SplitTree<T>,
    ES: SplitTree<T>,
    IT: SplitTree<T>,
    ET: SplitTree<T>,
    UL: SplitList<T>,
> Links<T> for Store<T, MD, MI, IS, ES, IT, ET, UL>
{
    fn constants(&self) -> &LinksConstants<T> {
        &self.constants
    }

    fn count_links(&self, query: &[T]) -> T {
        if query.is_empty() {
            return self.total();
        }

        let constants = self.constants();
        let any = constants.any;
        let index = query[constants.index_part.as_()];
        if query.len() == 1 {
            return if index == any {
                self.total()
            } else if self.exists(index) {
                one()
            } else {
                zero()
            };
        }

        if query.len() == 2 {
            let value = query[1];
            let is_virtual_val = self.is_virtual(value);

            return if index == any {
                if value == any {
                    self.total()
                } else if is_virtual_val {
                    self.external_sources.count_usages(value)
                        + self.external_targets.count_usages(value)
                } else if Self::USE_LIST {
                    self.sources_list.count_usages(value)
                        + self.internal_targets.count_usages(value)
                } else {
                    self.internal_sources.count_usages(value)
                        + self.internal_targets.count_usages(value)
                }
            } else if !self.exists(index) {
                zero()
            } else if value == any {
                one()
            } else {
                let stored = self.get_data_part(index);
                if (stored.source, stored.target) == (value, value) {
                    one()
                } else {
                    zero()
                }
            };
        }

        if query.len() == 3 {
            let source = query[constants.source_part.as_()];
            let target = query[constants.target_part.as_()];

            let is_virtual_source = self.is_virtual(source);
            let is_virtual_target = self.is_virtual(target);

            return if index == any {
                if (source, target) == (any, any) {
                    self.total()
                } else if source == any {
                    if is_virtual_target {
                        self.external_targets.count_usages(target)
                    } else {
                        self.internal_targets.count_usages(target)
                    }
                } else if is_virtual_source {
                    self.external_sources.count_usages(source)
                } else if target == any {
                    if Self::USE_LIST {
                        self.sources_list.count_usages(source)
                    } else {
                        self.internal_sources.count_usages(source)
                    }
                } else {
                    let link = if true {
                        if is_virtual_source && is_virtual_target {
                            self.external_sources.search(source, target)
                        } else if is_virtual_source {
                            self.internal_targets.search(source, target)
                        } else if is_virtual_target {
                            if Self::USE_LIST {
                                self.external_sources.search(source, target)
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        } else if Self::USE_LIST
                            || self.internal_sources.count_usages(source)
                                > self.internal_targets.count_usages(target)
                        {
                            self.internal_targets.search(source, target)
                        } else {
                            self.internal_sources.search(source, target)
                        }
                    } else if Self::USE_LIST
                        || self.internal_sources.count_usages(source)
                            > self.internal_targets.count_usages(target)
                    {
                        self.internal_targets.search(source, target)
                    } else {
                        self.internal_sources.search(source, target)
                    };
                    return if link == constants.null {
                        zero()
                    } else {
                        one()
                    };
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

    fn create_links(
        &mut self,
        _query: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, LinksError<T>> {
        let constants = self.constants().clone();
        let header = self.get_header();
        let mut free = header.first_free;
        if free == constants.null {
            let max_inner = *constants.internal_range.end();
            if header.allocated >= max_inner {
                return Err(LinksError::LimitReached(max_inner));
            }

            // TODO: if header.allocated >= header.reserved - one() {
            if self.index_mem.allocated() < self.index_mem.occupied() + 1 {
                let data = NonNull::from(
                    self.data_mem
                        .alloc(self.data_mem.allocated() + self.data_step)?,
                );
                let index = NonNull::from(
                    self.index_mem
                        .alloc(self.index_mem.allocated() + self.index_step)?,
                );
                self.update_mem(data, index);
                // let reserved = self.data_mem.allocated();
                let reserved = self.index_mem.allocated();
                let header = self.mut_header();
                // header.reserved = T::from_usize(reserved / Self::DATA_SIZE).unwrap()
                header.reserved = T::from_usize(reserved).unwrap()
            }
            let header = self.mut_header();
            header.allocated = header.allocated + one();
            free = header.allocated;
            self.data_mem.occupy(self.data_mem.occupied() + 1)?;
            self.index_mem.occupy(self.index_mem.occupied() + 1)?;
        } else {
            self.unused.detach(free)
        }

        self.resolve_danglind_external(free);

        Ok(handler(
            Link::nothing(),
            Link::new(free, T::zero(), T::zero()),
        ))
    }

    fn each_links(&self, query: &[T], handler: ReadHandler<T>) -> Flow {
        self.try_each_by_core(handler, query)
    }

    fn update_links(
        &mut self,
        query: &[T],
        change: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, LinksError<T>> {
        let index = query[0];
        let new_source = change[1];
        let new_target = change[2];

        let link = self.try_get_link(index)?;

        if link.source != zero() {
            // SAFETY: Here index attach to source
            unsafe {
                if self.is_virtual(link.source) {
                    self.detach_external_source(index);
                } else if Self::USE_LIST {
                    // self.sources_list.attach_as_last(source, index);
                    todo!()
                } else {
                    self.detach_internal_source(link.source, index);
                }
            }
        }

        if link.target != zero() {
            // SAFETY: Here index attach to target
            unsafe {
                if self.is_virtual(link.target) {
                    self.detach_external_target(index);
                } else {
                    self.detach_internal_target(link.target, index);
                }
            }
        }

        let virtual_source = self.is_virtual(new_source);
        let virtual_target = self.is_virtual(new_target);
        let place = self.mut_data_part(index);
        place.source = new_source;
        place.target = new_target;
        let place = place.clone();

        if place.source != zero() {
            // SAFETY: Here index attach to source
            unsafe {
                if virtual_source {
                    self.attach_external_source(index);
                } else if Self::USE_LIST {
                    // self.sources_list.attach_as_last(source, index);
                    todo!()
                } else {
                    self.attach_internal_source(place.source, index);
                }
            }
        }

        if place.target != zero() {
            // SAFETY: Here index attach to target
            unsafe {
                if virtual_target {
                    self.attach_external_target(index);
                } else {
                    self.attach_internal_target(place.target, index);
                }
            }
        }

        Ok(handler(link, Link::new(index, place.source, place.target)))
    }

    fn delete_links(
        &mut self,
        query: &[T],
        handler: WriteHandler<T>,
    ) -> Result<Flow, LinksError<T>> {
        let index = query[0];
        let link = self.try_get_link(index)?;

        self.resolve_danglind_internal(index);

        self.update(index, zero(), zero())?;

        // TODO: move to `delete_core`
        let header = self.get_header();

        match index.cmp(&header.allocated) {
            Ordering::Less => self.unused.attach_as_first(index),
            Ordering::Greater => unreachable!(),
            Ordering::Equal => {
                self.data_mem.occupy(self.data_mem.occupied() - 1)?;
                self.index_mem.occupy(self.index_mem.occupied() - 1)?;

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
                    // TODO: create extension `update_used`

                    let used_mem = self.data_mem.occupied();
                    self.data_mem.occupy(used_mem - 1)?;

                    let used_mem = self.index_mem.occupied();
                    self.index_mem.occupy(used_mem - 1)?;
                }
            }
        }
        Ok(handler(
            Link::new(index, link.source, link.target),
            Link::nothing(),
        ))
    }
}

impl<
    T: LinkType,
    MD: RawMem<DataPart<T>>,
    MI: RawMem<IndexPart<T>>,
    IS: SplitTree<T>,
    ES: SplitTree<T>,
    IT: SplitTree<T>,
    ET: SplitTree<T>,
    UL: SplitList<T>,
> Doublets<T> for Store<T, MD, MI, IS, ES, IT, ET, UL>
{
    fn get_link(&self, index: T) -> Option<Link<T>> {
        if self.exists(index) {
            Some(unsafe { self.get_link_unchecked(index) })
        } else {
            None
        }
    }
}

unsafe impl<
    T: LinkType,
    MD: RawMem<DataPart<T>>,
    MI: RawMem<IndexPart<T>>,
    IS: SplitTree<T>,
    ES: SplitTree<T>,
    IT: SplitTree<T>,
    ET: SplitTree<T>,
    UL: SplitList<T>,
> Sync for Store<T, MD, MI, IS, ES, IT, ET, UL>
{
}

unsafe impl<
    T: LinkType,
    MD: RawMem<DataPart<T>>,
    MI: RawMem<IndexPart<T>>,
    IS: SplitTree<T>,
    ES: SplitTree<T>,
    IT: SplitTree<T>,
    ET: SplitTree<T>,
    UL: SplitList<T>,
> Send for Store<T, MD, MI, IS, ES, IT, ET, UL>
{
}
