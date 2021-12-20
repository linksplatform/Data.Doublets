use std::borrow::BorrowMut;
use std::default::default;
use std::ops::Try;

use crate::{doublets, query};
use num_traits::{one, zero};

use crate::doublets::data::ToQuery;
use crate::doublets::data::{LinksConstants, Query};
use crate::doublets::mem::splited::generic::{
    ExternalSourcesRecursionlessTree, ExternalTargetsRecursionlessTree, InternalSourcesLinkedList,
    InternalSourcesRecursionlessTree, InternalTargetsRecursionlessTree, UnusedLinks,
};
use crate::doublets::mem::splited::{DataPart, IndexPart};
use crate::doublets::mem::united::UpdatePointersSplit;
use crate::doublets::mem::{ILinksListMethods, ILinksTreeMethods, LinksHeader, UpdatePointers};
use crate::doublets::Result;
use crate::doublets::{ILinks, Link, LinksError};
use crate::mem::ResizeableMem;
use crate::methods::RelativeCircularDoublyLinkedList;
use crate::num::LinkType;

pub struct Links<
    T: LinkType,
    MD: ResizeableMem,
    MI: ResizeableMem,
    IS: ILinksTreeMethods<T> + UpdatePointersSplit = InternalSourcesRecursionlessTree<T>,
    ES: ILinksTreeMethods<T> + UpdatePointersSplit = ExternalSourcesRecursionlessTree<T>,
    IT: ILinksTreeMethods<T> + UpdatePointersSplit = InternalTargetsRecursionlessTree<T>,
    ET: ILinksTreeMethods<T> + UpdatePointersSplit = ExternalTargetsRecursionlessTree<T>,
    UL: ILinksListMethods<T> + UpdatePointers = UnusedLinks<T>,
> {
    pub data_mem: MD,
    pub index_mem: MI,

    pub data_step: usize,
    pub index_step: usize,

    pub constants: LinksConstants<T>,

    pub internal_sources: IS,
    pub external_sources: ES,
    pub internal_targets: IT,
    pub external_targets: ET,

    pub sources_list: InternalSourcesLinkedList<T>,
    pub unused: UL,
}

impl<
        T: LinkType,
        MD: ResizeableMem,
        MI: ResizeableMem,
        IS: ILinksTreeMethods<T> + UpdatePointersSplit,
        ES: ILinksTreeMethods<T> + UpdatePointersSplit,
        IT: ILinksTreeMethods<T> + UpdatePointersSplit,
        ET: ILinksTreeMethods<T> + UpdatePointersSplit,
        UL: ILinksListMethods<T> + UpdatePointers,
    > Links<T, MD, MI, IS, ES, IT, ET, UL>
{
    const SIZE_STEP: usize = 1024_usize.pow(2) * 64;
    const HEADER_SIZE: usize = std::mem::size_of::<LinksHeader<T>>();
    const DATA_SIZE: usize = std::mem::size_of::<DataPart<T>>();
    const INDEX_SIZE: usize = std::mem::size_of::<IndexPart<T>>();

    // TODO: create Options
    pub fn with_constants(
        data_mem: MD,
        index_mem: MI,
        constants: LinksConstants<T>,
    ) -> Result<Links<T, MD, MI>, LinksError<T>> {
        let data = data_mem.get_ptr();
        let index = index_mem.get_ptr();
        let header = index_mem.get_ptr();

        let internal_sources =
            InternalSourcesRecursionlessTree::new(constants.clone(), data, index, header);
        let external_sources =
            ExternalSourcesRecursionlessTree::new(constants.clone(), data, index, header);

        let internal_targets =
            InternalTargetsRecursionlessTree::new(constants.clone(), data, index, header);
        let external_targets =
            ExternalTargetsRecursionlessTree::new(constants.clone(), data, index, header);

        let sources_list = InternalSourcesLinkedList::new(constants.clone(), data, index, header);
        let unused = UnusedLinks::new(data, index);

        let mut new = Links {
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

    pub fn new(data_mem: MD, index_mem: MI) -> Result<Links<T, MD, MI>, LinksError<T>> {
        Self::with_constants(data_mem, index_mem, default())
    }

    pub fn get_header(&self) -> &LinksHeader<T> {
        unsafe { &*(self.index_mem.get_ptr() as *const LinksHeader<T>) }
    }

    pub fn mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { &mut *(self.index_mem.get_ptr() as *mut LinksHeader<T>) }
    }

    pub fn get_data_part(&self, index: T) -> &DataPart<T> {
        unsafe { &*((self.data_mem.get_ptr() as *const DataPart<T>).add(index.as_())) }
    }

    pub fn mut_data_part(&mut self, index: T) -> &mut DataPart<T> {
        unsafe { &mut *((self.data_mem.get_ptr() as *mut DataPart<T>).add(index.as_())) }
    }

    pub fn get_index_part(&self, index: T) -> &IndexPart<T> {
        unsafe { &*((self.index_mem.get_ptr() as *const IndexPart<T>).add(index.as_())) }
    }

    pub fn mut_index_part(&mut self, index: T) -> &mut IndexPart<T> {
        unsafe { &mut *((self.index_mem.get_ptr() as *mut IndexPart<T>).add(index.as_())) }
    }

    fn update_pointers(&mut self) {
        let data = self.data_mem.get_ptr();
        let index = self.index_mem.get_ptr();
        let header = self.index_mem.get_ptr();

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
        //if to_align % target != 0 {
        //    to_align = ((to_align / target) * target) + target;
        //}
        to_align
    }

    fn init(&mut self) -> Result<(), LinksError<T>> {
        if self.index_mem.reserved_mem() < Self::HEADER_SIZE {
            self.index_mem.reserve_mem(Self::HEADER_SIZE)?;
        }
        self.update_pointers();

        let header = *self.get_header();
        let allocated = header.allocated.as_();

        let mut data_capacity = allocated * Self::DATA_SIZE;
        data_capacity = data_capacity.max(self.data_mem.used_mem());
        data_capacity = data_capacity.max(self.data_mem.reserved_mem());
        data_capacity = data_capacity.max(self.data_step);

        let mut index_capacity = allocated * Self::INDEX_SIZE;
        index_capacity = index_capacity.max(self.index_mem.used_mem());
        index_capacity = index_capacity.max(self.index_mem.reserved_mem());
        index_capacity = index_capacity.max(self.index_step);

        data_capacity = Self::align(data_capacity, self.data_step);
        index_capacity = Self::align(index_capacity, self.index_step);

        self.data_mem.reserve_mem(data_capacity)?;
        self.index_mem.reserve_mem(index_capacity)?;
        self.update_pointers();

        let allocated = header.allocated.as_();
        self.data_mem
            .use_mem(allocated * Self::DATA_SIZE + Self::DATA_SIZE)?;
        self.index_mem
            .use_mem(allocated * Self::INDEX_SIZE + Self::INDEX_SIZE)?;

        self.mut_header().reserved =
            T::from((self.data_mem.reserved_mem() - Self::DATA_SIZE) / Self::DATA_SIZE).unwrap();
        Ok(())
    }

    fn total(&self) -> T {
        let header = self.get_header();
        header.allocated - header.free
    }

    pub(crate) fn is_unused(&self, link: T) -> bool {
        if self.get_header().first_free != link {
            // TODO: May be this check is not needed
            let index = self.get_index_part(link);
            let data = self.get_data_part(link);
            index.size_as_target.is_zero() && !data.source.is_zero()
        } else {
            true
        }
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

    fn get_link_unchecked(&self, index: T) -> Link<T> {
        debug_assert!(self.exists(index));

        let raw = self.get_data_part(index);
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
                // TODO: 100% `Some`
                //  exist(`link`) => get_link(`link`) is Some
                if let Some(link) = self.get_link(index) {
                    handler(link)?;
                }
            }
            return R::from_output(());
        }

        let constants = self.constants.clone();
        let any = constants.any;
        let r#continue = constants.r#continue;
        let index = restriction[constants.index_part.as_()];
        if restriction.len() == 1 {
            return if index == any {
                self.try_each_by_core(handler, [])
            } else if !self.exists(index) {
                R::from_output(())
            } else {
                handler(self.get_link_unchecked(index)) // TODO: 100% `Some`
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
                if !self.exists(index) {
                    R::from_output(())
                } else if value == any {
                    handler(self.get_link_unchecked(index))
                } else {
                    let stored = self.get_data_part(index);
                    if (stored.source, stored.target) == (value, value) {
                        handler(self.get_link_unchecked(index))
                    } else {
                        R::from_output(())
                    }
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
                    if constants.is_external_reference(target) {
                        self.external_targets.each_usages(target, handler)
                    } else {
                        self.internal_targets.each_usages(target, handler)
                    }
                } else if target == any {
                    if constants.is_external_reference(source) {
                        self.external_sources.each_usages(source, handler)
                    } else {
                        if false {
                            todo!()
                        } else {
                            self.internal_sources.each_usages(source, handler)
                        }
                    }
                } else {
                    let mut link = if let Some(_) = &constants.external_range {
                        if constants.is_external_reference(source)
                            && constants.is_external_reference(target)
                        {
                            self.external_sources.search(source, target)
                        } else if constants.is_external_reference(source) {
                            self.internal_targets.search(source, target)
                        } else if constants.is_external_reference(target) {
                            if false {
                                todo!()
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        } else {
                            if false
                            /*|| todo!() */
                            {
                                self.internal_targets.search(source, target)
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        }
                    } else {
                        if false
                        /*|| todo!() */
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
                if !self.exists(index) {
                    R::from_output(())
                } else if (source, target) == (any, any) {
                    let link = self.get_link_unchecked(index);
                    handler(link)
                } else {
                    let link = self.get_link_unchecked(index);
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
            };
        }
        todo!()
    }
}

impl<
        T: LinkType,
        MD: ResizeableMem,
        MI: ResizeableMem,
        IS: ILinksTreeMethods<T> + UpdatePointersSplit,
        ES: ILinksTreeMethods<T> + UpdatePointersSplit,
        IT: ILinksTreeMethods<T> + UpdatePointersSplit,
        ET: ILinksTreeMethods<T> + UpdatePointersSplit,
        UL: ILinksListMethods<T> + UpdatePointers,
    > ILinks<T> for Links<T, MD, MI, IS, ES, IT, ET, UL>
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
                } else if constants.is_external_reference(value) {
                    self.external_sources.count_usages(value)
                        + self.external_targets.count_usages(value)
                } else {
                    if false {
                        todo!()
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
                    if constants.is_external_reference(target) {
                        self.external_targets.count_usages(target)
                    } else {
                        self.internal_targets.count_usages(target)
                    }
                } else if constants.is_external_reference(source) {
                    self.external_sources.count_usages(source)
                } else if target == any {
                    if false {
                        todo!()
                    } else {
                        self.internal_sources.count_usages(source)
                    }
                } else {
                    let mut link = if let Some(_) = &constants.external_range {
                        if constants.is_external_reference(source)
                            && constants.is_external_reference(target)
                        {
                            self.external_sources.search(source, target)
                        } else if constants.is_external_reference(source) {
                            self.internal_targets.search(source, target)
                        } else if constants.is_external_reference(target) {
                            if false {
                                todo!()
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        } else {
                            if false
                            /*|| todo!() */
                            {
                                self.internal_targets.search(source, target)
                            } else {
                                self.internal_sources.search(source, target)
                            }
                        }
                    } else {
                        if false
                        /*|| todo!() */
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
                    let link = self.get_link_unchecked(index);
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

    fn create_by(&mut self, query: impl ToQuery<T>) -> Result<T> {
        let constants = self.constants();
        let header = self.get_header();
        let mut free = header.first_free;
        if free == constants.null {
            let max_inner = *constants.internal_range.end();
            if header.allocated >= max_inner {
                return Err(LinksError::LimitReached(max_inner));
            }

            // TODO: if header.allocated >= header.reserved - one() {
            if self.index_mem.reserved_mem() < self.index_mem.used_mem() + Self::INDEX_SIZE {
                self.data_mem
                    .reserve_mem(self.data_mem.reserved_mem() + self.data_step)?;
                self.index_mem
                    .reserve_mem(self.index_mem.reserved_mem() + self.index_step)?;
                self.update_pointers();
                // let reserved = self.data_mem.reserved_mem();
                let reserved = self.index_mem.reserved_mem();
                let header = self.mut_header();
                // header.reserved = T::from_usize(reserved / Self::DATA_SIZE).unwrap()
                header.reserved = T::from_usize(reserved / Self::INDEX_SIZE).unwrap()
            }
            let header = self.mut_header();
            header.allocated = header.allocated + one();
            free = header.allocated;
            self.data_mem
                .use_mem(self.data_mem.used_mem() + Self::DATA_SIZE)?;
            self.index_mem
                .use_mem(self.index_mem.used_mem() + Self::INDEX_SIZE)?;
        } else {
            self.unused.detach(free)
        }
        Ok(free)
    }

    fn try_each_by<F, R>(&self, restrictions: impl ToQuery<T>, mut handler: F) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        self.try_each_by_core(&mut handler, restrictions.to_query())
    }

    fn update_by(&mut self, query: impl ToQuery<T>, replacement: impl ToQuery<T>) -> Result<T> {
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

        if source != null {
            if constants.is_external_reference(source) {
                let temp = &mut self.mut_header().root_as_source as *mut T;
                self.external_sources.detach(unsafe { &mut *temp }, index)
            } else {
                if false {
                    self.sources_list.detach(source, index);
                } else {
                    let temp = &mut self.mut_index_part(source).root_as_source as *mut T;
                    self.internal_sources.detach(unsafe { &mut *temp }, index);
                }
            }
        }

        if target != null {
            if constants.is_external_reference(target) {
                let temp = &mut self.mut_header().root_as_target as *mut T;
                self.external_targets.detach(unsafe { &mut *temp }, index)
            } else {
                let temp = &mut self.mut_index_part(target).root_as_target as *mut T;
                self.internal_targets.detach(unsafe { &mut *temp }, index);
            }
        }

        let link = self.mut_data_part(index);
        link.source = new_source;
        link.target = new_target;
        let source = link.source;
        let target = link.target;

        if source != null {
            if constants.is_external_reference(source) {
                let temp = &mut self.mut_header().root_as_source as *mut T;
                self.external_sources.attach(unsafe { &mut *temp }, index)
            } else {
                if false {
                    self.sources_list.attach_as_last(source, index);
                } else {
                    let temp = &mut self.mut_index_part(source).root_as_source as *mut T;
                    self.internal_sources.attach(unsafe { &mut *temp }, index);
                }
            }
        }

        if target != null {
            if constants.is_external_reference(target) {
                let temp = &mut self.mut_header().root_as_target as *mut T;
                self.external_targets.attach(unsafe { &mut *temp }, index)
            } else {
                let temp = &mut self.mut_index_part(target).root_as_target as *mut T;
                self.internal_targets.attach(unsafe { &mut *(temp) }, index);
            }
        }

        Ok(index)
    }

    fn delete_by(&mut self, query: impl ToQuery<T>) -> Result<T> {
        let query = query.to_query();

        let index = query[0];

        if !self.exists(index) {
            return Err(LinksError::NotExists(index));
        }
        self.update(index, zero(), zero())?;

        // TODO: move to `delete_core`
        let header = self.get_header();
        let link = index;
        if link < header.allocated || true {
            self.unused.attach_as_first(link)
        } else if link == header.allocated {
            self.data_mem
                .use_mem(self.data_mem.used_mem() - Self::DATA_SIZE)?;
            self.index_mem
                .use_mem(self.index_mem.used_mem() - Self::INDEX_SIZE)?;

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

                let used_mem = self.data_mem.used_mem();
                self.data_mem.use_mem(used_mem - Self::DATA_SIZE)?;

                let used_mem = self.index_mem.used_mem();
                self.index_mem.use_mem(used_mem - Self::INDEX_SIZE)?;
            }
            //*self.get_mut_header() = header;
        }
        Ok(index)
    }

    fn get_link(&self, index: T) -> Option<Link<T>> {
        if self.exists(index) {
            let raw = self.get_data_part(index);
            Some(Link::new(index, raw.source, raw.target))
        } else {
            None
        }
    }
}
