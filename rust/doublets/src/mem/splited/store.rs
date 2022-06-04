use std::default::default;
use std::error::Error;
use std::mem::size_of;
use std::ops::Try;

use num_traits::{one, zero};

use crate::mem::splited::generic::{
    ExternalSourcesRecursionlessTree, ExternalTargetsRecursionlessTree, InternalSourcesLinkedList,
    InternalSourcesRecursionlessTree, InternalTargetsRecursionlessTree, UnusedLinks,
};
use crate::mem::splited::{DataPart, IndexPart};
use crate::mem::united::UpdatePointersSplit;
use crate::mem::{ILinksListMethods, ILinksTreeMethods, LinksHeader, UpdatePointers};
use crate::{Doublets, Link, LinksError};
use data::LinksConstants;
use data::{Flow, Links, ReadHandler, ToQuery, WriteHandler};
use mem::RawMem;
use methods::RelativeCircularDoublyLinkedList;
use num::LinkType;

pub struct Store<
    T: LinkType,
    MD: RawMem,
    MI: RawMem,
    IS: ILinksTreeMethods<T> + UpdatePointersSplit = InternalSourcesRecursionlessTree<T>,
    ES: ILinksTreeMethods<T> + UpdatePointersSplit = ExternalSourcesRecursionlessTree<T>,
    IT: ILinksTreeMethods<T> + UpdatePointersSplit = InternalTargetsRecursionlessTree<T>,
    ET: ILinksTreeMethods<T> + UpdatePointersSplit = ExternalTargetsRecursionlessTree<T>,
    UL: ILinksListMethods<T> + UpdatePointers = UnusedLinks<T>,
> {
    data_mem: MD,
    index_mem: MI,

    data_step: usize,
    index_step: usize,

    constants: LinksConstants<T>,

    internal_sources: IS,
    external_sources: ES,
    internal_targets: IT,
    external_targets: ET,

    sources_list: InternalSourcesLinkedList<T>,
    unused: UL,
}

impl<
        T: LinkType,
        MD: RawMem,
        MI: RawMem,
        IS: ILinksTreeMethods<T> + UpdatePointersSplit,
        ES: ILinksTreeMethods<T> + UpdatePointersSplit,
        IT: ILinksTreeMethods<T> + UpdatePointersSplit,
        ET: ILinksTreeMethods<T> + UpdatePointersSplit,
        UL: ILinksListMethods<T> + UpdatePointers,
    > Store<T, MD, MI, IS, ES, IT, ET, UL>
{
    const USE_LIST: bool = false;
    const SIZE_STEP: usize = 1024_usize.pow(2) * 64;
    const HEADER_SIZE: usize = std::mem::size_of::<LinksHeader<T>>();
    const DATA_SIZE: usize = std::mem::size_of::<DataPart<T>>();
    const INDEX_SIZE: usize = std::mem::size_of::<IndexPart<T>>();

    // TODO: create Options
    pub fn with_constants(
        data_mem: MD,
        index_mem: MI,
        constants: LinksConstants<T>,
    ) -> Result<Store<T, MD, MI>, LinksError<T>> {
        let data = data_mem.ptr();
        let index = index_mem.ptr();
        let header = index_mem.ptr();

        let internal_sources = InternalSourcesRecursionlessTree::new(
            constants.clone(),
            data.as_mut_ptr(),
            index.as_mut_ptr(),
            header.as_mut_ptr(),
        );
        let external_sources = ExternalSourcesRecursionlessTree::new(
            constants.clone(),
            data.as_mut_ptr(),
            index.as_mut_ptr(),
            header.as_mut_ptr(),
        );

        let internal_targets = InternalTargetsRecursionlessTree::new(
            constants.clone(),
            data.as_mut_ptr(),
            index.as_mut_ptr(),
            header.as_mut_ptr(),
        );
        let external_targets = ExternalTargetsRecursionlessTree::new(
            constants.clone(),
            data.as_mut_ptr(),
            index.as_mut_ptr(),
            header.as_mut_ptr(),
        );

        let sources_list = InternalSourcesLinkedList::new(
            constants.clone(),
            data.as_mut_ptr(),
            index.as_mut_ptr(),
            header.as_mut_ptr(),
        );
        let unused = UnusedLinks::new(data.as_mut_ptr(), index.as_mut_ptr());

        let mut new = Store {
            data_mem,
            index_mem,
            data_step: Self::SIZE_STEP,
            index_step: Self::SIZE_STEP * 4,
            constants,
            internal_sources,
            external_sources,
            internal_targets,
            external_targets,
            sources_list,
            unused,
        };

        new.init()?;
        Ok(new)
    }

    pub fn new(data_mem: MD, index_mem: MI) -> Result<Store<T, MD, MI>, LinksError<T>> {
        Self::with_constants(data_mem, index_mem, default())
    }

    fn mut_from_mem<Output: Sized, Memory: RawMem>(
        mem: &Memory,
        index: usize,
    ) -> Option<&mut Output> {
        let mut ptr = mem.ptr();
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

    fn get_from_mem<Output: Sized, M: RawMem>(mem: &M, index: usize) -> Option<&Output> {
        Self::mut_from_mem(mem, index).map(|v| &*v)
    }

    fn get_header(&self) -> &LinksHeader<T> {
        Self::get_from_mem(&self.index_mem, 0).expect("Header should be in index memory")
    }

    fn mut_header(&mut self) -> &mut LinksHeader<T> {
        Self::mut_from_mem(&self.index_mem, 0).expect("Header should be in index memory")
    }

    fn get_data_part(&self, index: T) -> &DataPart<T> {
        Self::get_from_mem(&self.data_mem, index.as_()).expect("Data part should be in data memory")
    }

    unsafe fn get_data_unchecked(&self, index: T) -> &DataPart<T> {
        Self::get_from_mem(&self.data_mem, index.as_()).unwrap_unchecked()
    }

    fn mut_data_part(&mut self, index: T) -> &mut DataPart<T> {
        Self::mut_from_mem(&self.data_mem, index.as_()).expect("Data part should be in data memory")
    }

    fn get_index_part(&self, index: T) -> &IndexPart<T> {
        Self::get_from_mem(&self.index_mem, index.as_())
            .expect("Index part should be in index memory")
    }

    fn mut_index_part(&mut self, index: T) -> &mut IndexPart<T> {
        Self::mut_from_mem(&self.index_mem, index.as_())
            .expect("Index part should be in index memory")
    }

    fn update_pointers(&mut self) {
        let data = self.data_mem.ptr().as_mut_ptr();
        let index = self.index_mem.ptr().as_mut_ptr();
        let header = self.index_mem.ptr().as_mut_ptr();

        self.internal_sources.update_pointers(data, index, header);
        self.external_sources.update_pointers(data, index, header);
        self.internal_targets.update_pointers(data, index, header);
        self.external_targets.update_pointers(data, index, header);
        self.sources_list.update_pointers(data, index, header);
        self.unused.update_pointers(data, index);
    }

    fn align(mut to_align: usize, target: usize) -> usize {
        debug_assert!(to_align >= target);

        // TODO: optimize this `if`
        if to_align % target != 0 {
            to_align = ((to_align / target) * target) + target;
        }
        to_align
    }

    fn init(&mut self) -> Result<(), LinksError<T>> {
        if self.index_mem.allocated() < Self::HEADER_SIZE {
            self.index_mem.alloc(Self::HEADER_SIZE)?;
        }
        self.update_pointers();

        let header = *self.get_header();
        let allocated = header.allocated.as_();

        let mut data_capacity = allocated * Self::DATA_SIZE;
        data_capacity = data_capacity.max(self.data_mem.occupied());
        data_capacity = data_capacity.max(self.data_mem.allocated());
        data_capacity = data_capacity.max(self.data_step);

        let mut index_capacity = allocated * Self::INDEX_SIZE;
        index_capacity = index_capacity.max(self.index_mem.occupied());
        index_capacity = index_capacity.max(self.index_mem.allocated());
        index_capacity = index_capacity.max(self.index_step);

        data_capacity = Self::align(data_capacity, self.data_step);
        index_capacity = Self::align(index_capacity, self.index_step);

        self.data_mem.alloc(data_capacity)?;
        self.index_mem.alloc(index_capacity)?;
        self.update_pointers();

        let allocated = header.allocated.as_();
        self.data_mem
            .occupy(allocated * Self::DATA_SIZE + Self::DATA_SIZE)?;
        self.index_mem
            .occupy(allocated * Self::INDEX_SIZE + Self::INDEX_SIZE)?;

        self.mut_header().reserved =
            T::from((self.data_mem.allocated() - Self::DATA_SIZE) / Self::DATA_SIZE).unwrap();
        Ok(())
    }

    fn total(&self) -> T {
        let header = self.get_header();
        header.allocated - header.free
    }

    pub(crate) fn is_unused(&self, link: T) -> bool {
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

    pub(crate) fn is_virtual(&self, link: T) -> bool {
        link > self.get_header().allocated && !self.constants.is_external(link)
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

    fn try_each_by_core<F, R>(&self, handler: &mut F, restrictions: impl ToQuery<T>) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let restriction = restrictions.to_query();

        if restriction.len() == 0 {
            for index in one()..=self.get_header().allocated {
                if let Some(link) = self.get_link(index) {
                    handler(link)?;
                }
            }
            return R::from_output(());
        }

        let constants = self.constants.clone();
        let any = constants.any;
        let _continue = constants.r#continue;
        let index = restriction[constants.index_part.as_()];
        if restriction.len() == 1 {
            return if index == any {
                self.try_each_by_core(handler, [])
            } else if let Some(link) = self.get_link(index) {
                handler(link)
            } else {
                R::from_output(())
            };
        }
        //
        if restriction.len() == 2 {
            let value = restriction[1]; // TODO: Hmm... `const` position?
            return if index == any {
                if value == any {
                    self.try_each_by_core(handler, [])
                } else {
                    self.try_each_by_core(handler, [index, value, any])?;
                    self.try_each_by_core(handler, [index, any, value])
                }
            } else {
                if let Some(link) = self.get_link(index) {
                    if value == any {
                        handler(link)
                    } else {
                        if (link.source, link.target) == (value, value) {
                            handler(link)
                        } else {
                            R::from_output(())
                        }
                    }
                } else {
                    R::from_output(())
                }
            };
        }
        //
        if restriction.len() == 3 {
            let source = restriction[constants.source_part.as_()];
            let target = restriction[constants.target_part.as_()];
            //
            return if index == any {
                if (source, target) == (any, any) {
                    self.try_each_by_core(handler, [])
                } else if source == any {
                    if constants.is_external(target) {
                        self.external_targets.each_usages(target, handler)
                    } else {
                        self.internal_targets.each_usages(target, handler)
                    }
                } else if target == any {
                    if constants.is_external(source) {
                        self.external_sources.each_usages(source, handler)
                    } else {
                        if Self::USE_LIST {
                            self.sources_list.each_usages(source, handler)
                        } else {
                            self.internal_sources.each_usages(source, handler)
                        }
                    }
                } else {
                    let link = if let Some(_) = &constants.external_range {
                        if constants.is_external(source) && constants.is_external(target) {
                            self.external_sources.search(source, target)
                        } else if constants.is_external(source) {
                            self.internal_targets.search(source, target)
                        } else if constants.is_external(target) {
                            if Self::USE_LIST {
                                self.external_sources.search(source, target)
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        } else {
                            if Self::USE_LIST
                                || self.internal_sources.count_usages(source)
                                    > self.internal_targets.count_usages(target)
                            {
                                self.internal_targets.search(source, target)
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        }
                    } else {
                        if Self::USE_LIST
                            || self.internal_sources.count_usages(source)
                                > self.internal_targets.count_usages(target)
                        {
                            self.internal_targets.search(source, target)
                        } else {
                            self.internal_sources.search(source, target)
                        }
                    };
                    return if link == constants.null {
                        R::from_output(())
                    } else {
                        let link = self.get_link(link).unwrap();
                        handler(link)
                    };
                }
            } else {
                if let Some(link) = self.get_link(index) {
                    if (source, target) == (any, any) {
                        handler(unsafe { self.get_link_unchecked(index) })
                    } else {
                        if source != any && target != any {
                            if (link.source, link.target) == (source, target) {
                                handler(link)
                            } else {
                                R::from_output(())
                            }
                        } else {
                            if source != any {
                                if (link.source, link.target) == (source, source) {
                                    handler(link)
                                } else {
                                    R::from_output(())
                                }
                            } else {
                                if (link.source, link.target) == (target, target) {
                                    handler(link)
                                } else {
                                    R::from_output(())
                                }
                            }
                        }
                    }
                } else {
                    R::from_output(())
                }
            };
        }
        todo!()
    }
}

impl<
        T: LinkType,
        MD: RawMem,
        MI: RawMem,
        IS: ILinksTreeMethods<T> + UpdatePointersSplit,
        ES: ILinksTreeMethods<T> + UpdatePointersSplit,
        IT: ILinksTreeMethods<T> + UpdatePointersSplit,
        ET: ILinksTreeMethods<T> + UpdatePointersSplit,
        UL: ILinksListMethods<T> + UpdatePointers,
    > Doublets<T> for Store<T, MD, MI, IS, ES, IT, ET, UL>
{
    fn constants(&self) -> LinksConstants<T> {
        self.constants.clone()
    }

    fn count_by(&self, query: impl ToQuery<T>) -> T {
        let query = query.to_query();

        if query.len() == 0 {
            return self.total();
        }

        let constants = self.constants();
        let any = constants.any;
        let index = query[constants.index_part.as_()];
        if query.len() == 1 {
            return if index == any {
                self.total()
            } else {
                if self.exists(index) {
                    one()
                } else {
                    zero()
                }
            };
        }

        if query.len() == 2 {
            let value = query[1]; // TODO: Hmm... `const` position?
            return if index == any {
                if value == any {
                    self.total()
                } else if constants.is_external(value) {
                    self.external_sources.count_usages(value)
                        + self.external_targets.count_usages(value)
                } else {
                    if Self::USE_LIST {
                        self.sources_list.count_usages(value)
                            + self.internal_targets.count_usages(value)
                    } else {
                        self.internal_sources.count_usages(value)
                            + self.internal_targets.count_usages(value)
                    }
                }
            } else {
                if !self.exists(index) {
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
                }
            };
        }

        if query.len() == 3 {
            let source = query[constants.source_part.as_()];
            let target = query[constants.target_part.as_()];

            return if index == any {
                if (source, target) == (any, any) {
                    self.total()
                } else if source == any {
                    if constants.is_external(target) {
                        self.external_targets.count_usages(target)
                    } else {
                        self.internal_targets.count_usages(target)
                    }
                } else if constants.is_external(source) {
                    self.external_sources.count_usages(source)
                } else if target == any {
                    if Self::USE_LIST {
                        self.sources_list.count_usages(source)
                    } else {
                        self.internal_sources.count_usages(source)
                    }
                } else {
                    let link = if let Some(_) = &constants.external_range {
                        if constants.is_external(source) && constants.is_external(target) {
                            self.external_sources.search(source, target)
                        } else if constants.is_external(source) {
                            self.internal_targets.search(source, target)
                        } else if constants.is_external(target) {
                            if Self::USE_LIST {
                                self.external_sources.search(source, target)
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        } else {
                            if Self::USE_LIST
                                || self.internal_sources.count_usages(source)
                                    > self.internal_targets.count_usages(target)
                            {
                                self.internal_targets.search(source, target)
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        }
                    } else {
                        if Self::USE_LIST
                            || self.internal_sources.count_usages(source)
                                > self.internal_targets.count_usages(target)
                        {
                            self.internal_targets.search(source, target)
                        } else {
                            self.internal_sources.search(source, target)
                        }
                    };
                    return if link == constants.null {
                        zero()
                    } else {
                        one()
                    };
                }
            } else {
                if !self.exists(index) {
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
                    } else {
                        if source != any {
                            if (link.source, link.target) == (source, source) {
                                one()
                            } else {
                                zero()
                            }
                        } else {
                            if (link.source, link.target) == (target, target) {
                                one()
                            } else {
                                zero()
                            }
                        }
                    }
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

            // TODO: if header.allocated >= header.reserved - one() {
            if self.index_mem.allocated() < self.index_mem.occupied() + Self::INDEX_SIZE {
                self.data_mem
                    .alloc(self.data_mem.allocated() + self.data_step)?;
                self.index_mem
                    .alloc(self.index_mem.allocated() + self.index_step)?;
                self.update_pointers();
                // let reserved = self.data_mem.allocated();
                let reserved = self.index_mem.allocated();
                let header = self.mut_header();
                // header.reserved = T::from_usize(reserved / Self::DATA_SIZE).unwrap()
                header.reserved = T::from_usize(reserved / Self::INDEX_SIZE).unwrap()
            }
            let header = self.mut_header();
            header.allocated = header.allocated + one();
            free = header.allocated;
            self.data_mem
                .occupy(self.data_mem.occupied() + Self::DATA_SIZE)?;
            self.index_mem
                .occupy(self.index_mem.occupied() + Self::INDEX_SIZE)?;
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
        self.try_each_by_core(&mut handler, restrictions.to_query())
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
        let new_source = replacement[1];
        let new_target = replacement[2];

        let constants = self.constants();
        let null = constants.null;

        let link = self.get_data_part(index);
        let source = link.source;
        let target = link.target;
        let old_source = source;
        let old_target = target;

        if source != null {
            if constants.is_external(source) {
                let temp = &mut self.mut_header().root_as_source as *mut T;
                self.external_sources.detach(unsafe { &mut *temp }, index)
            } else {
                if Self::USE_LIST {
                    self.sources_list.detach(source, index);
                } else {
                    let temp = &mut self.mut_index_part(source).root_as_source as *mut T;
                    self.internal_sources.detach(unsafe { &mut *temp }, index);
                }
            }
        }

        if target != null {
            if constants.is_external(target) {
                let temp = &mut self.mut_header().root_as_target as *mut T;
                self.external_targets.detach(unsafe { &mut *temp }, index)
            } else {
                let temp = &mut self.mut_index_part(target).root_as_target as *mut T;
                self.internal_targets.detach(unsafe { &mut *temp }, index);
            }
        }

        let link = self.mut_data_part(index);
        if link.source != new_source {
            link.source = new_source;
        }
        if link.target != new_target {
            link.target = new_target;
        }
        let source = link.source;
        let target = link.target;

        if source != null {
            if constants.is_external(source) {
                let temp = &mut self.mut_header().root_as_source as *mut T;
                self.external_sources.attach(unsafe { &mut *temp }, index)
            } else {
                if Self::USE_LIST {
                    self.sources_list.attach_as_last(source, index);
                } else {
                    let temp = &mut self.mut_index_part(source).root_as_source as *mut T;
                    self.internal_sources.attach(unsafe { &mut *temp }, index);
                }
            }
        }

        if target != null {
            if constants.is_external(target) {
                let temp = &mut self.mut_header().root_as_target as *mut T;
                self.external_targets.attach(unsafe { &mut *temp }, index)
            } else {
                let temp = &mut self.mut_index_part(target).root_as_target as *mut T;
                self.internal_targets.attach(unsafe { &mut *(temp) }, index);
            }
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
        let (source, target) = if let Some(link) = self.get_link(index) {
            (link.source, link.target)
        } else {
            return Err(LinksError::NotExists(index));
        };
        if (source, target) != (zero(), zero()) {
            self.update(index, zero(), zero())?;
        }

        // TODO: move to `delete_core`
        let header = self.get_header();
        let link = index;
        if link < header.allocated {
            self.unused.attach_as_first(link)
        } else if link == header.allocated {
            self.data_mem
                .occupy(self.data_mem.occupied() - Self::DATA_SIZE)?;
            self.index_mem
                .occupy(self.index_mem.occupied() - Self::INDEX_SIZE)?;

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
                self.data_mem.occupy(used_mem - Self::DATA_SIZE)?;

                let used_mem = self.index_mem.occupied();
                self.index_mem.occupy(used_mem - Self::INDEX_SIZE)?;
            }
        } else {
            // must be unreachable
            panic!("unexpected link index")
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

impl<
        T: LinkType,
        MD: RawMem,
        MI: RawMem,
        IS: ILinksTreeMethods<T> + UpdatePointersSplit,
        ES: ILinksTreeMethods<T> + UpdatePointersSplit,
        IT: ILinksTreeMethods<T> + UpdatePointersSplit,
        ET: ILinksTreeMethods<T> + UpdatePointersSplit,
        UL: ILinksListMethods<T> + UpdatePointers,
    > Links<T> for Store<T, MD, MI, IS, ES, IT, ET, UL>
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

unsafe impl<
        T: LinkType,
        MD: RawMem,
        MI: RawMem,
        IS: ILinksTreeMethods<T> + UpdatePointersSplit,
        ES: ILinksTreeMethods<T> + UpdatePointersSplit,
        IT: ILinksTreeMethods<T> + UpdatePointersSplit,
        ET: ILinksTreeMethods<T> + UpdatePointersSplit,
        UL: ILinksListMethods<T> + UpdatePointers,
    > Sync for Store<T, MD, MI, IS, ES, IT, ET, UL>
{
}

unsafe impl<
        T: LinkType,
        MD: RawMem,
        MI: RawMem,
        IS: ILinksTreeMethods<T> + UpdatePointersSplit,
        ES: ILinksTreeMethods<T> + UpdatePointersSplit,
        IT: ILinksTreeMethods<T> + UpdatePointersSplit,
        ET: ILinksTreeMethods<T> + UpdatePointersSplit,
        UL: ILinksListMethods<T> + UpdatePointers,
    > Send for Store<T, MD, MI, IS, ES, IT, ET, UL>
{
}
