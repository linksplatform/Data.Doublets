use std::default::default;
use std::marker::PhantomData;
use std::mem::size_of;
use std::ops::{ControlFlow, Index, Try};

use num_traits::{one, zero};
use smallvec::SmallVec;

use crate::doublets::data::LinksConstants;
use crate::doublets::data::ToQuery;
use crate::doublets::data::{IGenericLinks, Query};
use crate::doublets::link::Link;
use crate::doublets::mem::links_header::LinksHeader;
use crate::doublets::mem::united::LinksSourcesSizeBalancedTree;
use crate::doublets::mem::united::LinksTargetsRecursionlessSizeBalancedTree;
use crate::doublets::mem::united::LinksTargetsSizeBalancedTree;
use crate::doublets::mem::united::RawLink;
use crate::doublets::mem::united::UnusedLinks;
use crate::doublets::mem::united::{LinksSourcesRecursionlessSizeBalancedTree, NewList, NewTree};
use crate::doublets::mem::ILinksTreeMethods;
use crate::doublets::mem::{ILinksListMethods, UpdatePointers};
use crate::doublets::Result;
use crate::doublets::{data, ILinks, LinksError};
use crate::mem::FileMappedMem;
use crate::mem::{Mem, ResizeableMem};
use crate::num::LinkType;
use crate::{doublets, query};

// TODO: use `_=_` instead of `_ = _`
pub struct Links<
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
    > Links<T, M, TS, TT, TU>
{
    ///
    ///                    Create a builder for building `Links`.
    ///                    On the builder, call `.mem(...)`, `.reserve_step(...)`, `.constants(...)`, `.sources(...)`, `.targets(...)`, `.unused(...)` to set the values of the fields.
    ///                    Finally, call `.build()` to create the instance of `Links`.
    ///
    #[allow(dead_code, clippy::default_trait_access)]
    pub fn with_options() -> LinksBuilder<((), (), (), (), (), ()), T, M, TS, TT, TU> {
        LinksBuilder {
            fields: ((), (), (), (), (), ()),
            phantom: ::core::default::Default::default(),
        }
    }
}

#[must_use]
#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub struct LinksBuilder<TypedBuilderFields, T: LinkType, M: ResizeableMem, TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers = LinksSourcesRecursionlessSizeBalancedTree<T>, TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers = LinksTargetsRecursionlessSizeBalancedTree<T>, TU: ILinksListMethods<T> + NewList<T> + UpdatePointers = UnusedLinks<T>> {
    fields: TypedBuilderFields,
    phantom: (::core::marker::PhantomData<T>, ::core::marker::PhantomData<M>, ::core::marker::PhantomData<TS>, ::core::marker::PhantomData<TT>, ::core::marker::PhantomData<TU>),
}

impl<
        TypedBuilderFields,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    > Clone for LinksBuilder<TypedBuilderFields, T, M, TS, TT, TU>
where
    TypedBuilderFields: Clone,
{
    #[allow(clippy::default_trait_access)]
    fn clone(&self) -> Self {
        Self {
            fields: self.fields.clone(),
            phantom: ::core::default::Default::default(),
        }
    }
}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub trait LinksBuilder_Optional<T> {
    fn into_value<F: FnOnce() -> T>(self, default: F) -> T;
}

impl<T> LinksBuilder_Optional<T> for () {
    fn into_value<F: FnOnce() -> T>(self, default: F) -> T {
        default()
    }
}

impl<T> LinksBuilder_Optional<T> for (T,) {
    fn into_value<F: FnOnce() -> T>(self, _: F) -> T {
        self.0
    }
}

#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __targets,
        __sources,
        __constants,
        __reserve_step,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<
        (
            (),
            __reserve_step,
            __constants,
            __sources,
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    >
{
    pub fn mem(
        self,
        mem: M,
    ) -> LinksBuilder<
        (
            (M,),
            __reserve_step,
            __constants,
            __sources,
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        let mem = (mem,);
        let (_, reserve_step, constants, sources, targets, unused) = self.fields;
        LinksBuilder {
            fields: (mem, reserve_step, constants, sources, targets, unused),
            phantom: self.phantom,
        }
    }
}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub enum LinksBuilder_Error_Repeated_field_mem {}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __targets,
        __sources,
        __constants,
        __reserve_step,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<
        (
            (M,),
            __reserve_step,
            __constants,
            __sources,
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    >
{
    #[deprecated(note = "Repeated field mem")]
    pub fn mem(
        self,
        _: LinksBuilder_Error_Repeated_field_mem,
    ) -> LinksBuilder<
        (
            (M,),
            __reserve_step,
            __constants,
            __sources,
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        self
    }
}

#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __targets,
        __sources,
        __constants,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    > LinksBuilder<(__mem, (), __constants, __sources, __targets, __unused), T, M, TS, TT, TU>
{
    pub fn reserve_step(
        self,
        reserve_step: usize,
    ) -> LinksBuilder<
        (__mem, (usize,), __constants, __sources, __targets, __unused),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        let reserve_step = (reserve_step,);
        let (mem, _, constants, sources, targets, unused) = self.fields;
        LinksBuilder {
            fields: (mem, reserve_step, constants, sources, targets, unused),
            phantom: self.phantom,
        }
    }
}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub enum LinksBuilder_Error_Repeated_field_reserve_step {}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __targets,
        __sources,
        __constants,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<(__mem, (usize,), __constants, __sources, __targets, __unused), T, M, TS, TT, TU>
{
    #[deprecated(note = "Repeated field reserve_step")]
    pub fn reserve_step(
        self,
        _: LinksBuilder_Error_Repeated_field_reserve_step,
    ) -> LinksBuilder<
        (__mem, (usize,), __constants, __sources, __targets, __unused),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        self
    }
}

#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __targets,
        __sources,
        __reserve_step,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    > LinksBuilder<(__mem, __reserve_step, (), __sources, __targets, __unused), T, M, TS, TT, TU>
{
    pub fn constants(
        self,
        constants: LinksConstants<T>,
    ) -> LinksBuilder<
        (
            __mem,
            __reserve_step,
            (LinksConstants<T>,),
            __sources,
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        let constants = (constants,);
        let (mem, reserve_step, _, sources, targets, unused) = self.fields;
        LinksBuilder {
            fields: (mem, reserve_step, constants, sources, targets, unused),
            phantom: self.phantom,
        }
    }
}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub enum LinksBuilder_Error_Repeated_field_constants {}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __targets,
        __sources,
        __reserve_step,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<
        (
            __mem,
            __reserve_step,
            (LinksConstants<T>,),
            __sources,
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    >
{
    #[deprecated(note = "Repeated field constants")]
    pub fn constants(
        self,
        _: LinksBuilder_Error_Repeated_field_constants,
    ) -> LinksBuilder<
        (
            __mem,
            __reserve_step,
            (LinksConstants<T>,),
            __sources,
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        self
    }
}

#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __targets,
        __constants,
        __reserve_step,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<(__mem, __reserve_step, __constants, (), __targets, __unused), T, M, TS, TT, TU>
{
    pub fn sources(
        self,
        sources: TS,
    ) -> LinksBuilder<
        (
            __mem,
            __reserve_step,
            __constants,
            (TS,),
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        let sources = (sources,);
        let (mem, reserve_step, constants, _, targets, unused) = self.fields;
        LinksBuilder {
            fields: (mem, reserve_step, constants, sources, targets, unused),
            phantom: self.phantom,
        }
    }
}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub enum LinksBuilder_Error_Repeated_field_sources {}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __targets,
        __constants,
        __reserve_step,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<
        (
            __mem,
            __reserve_step,
            __constants,
            (TS,),
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    >
{
    #[deprecated(note = "Repeated field sources")]
    pub fn sources(
        self,
        _: LinksBuilder_Error_Repeated_field_sources,
    ) -> LinksBuilder<
        (
            __mem,
            __reserve_step,
            __constants,
            (TS,),
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        self
    }
}

#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __sources,
        __constants,
        __reserve_step,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<(__mem, __reserve_step, __constants, __sources, (), __unused), T, M, TS, TT, TU>
{
    pub fn targets(
        self,
        targets: TT,
    ) -> LinksBuilder<
        (
            __mem,
            __reserve_step,
            __constants,
            __sources,
            (TT,),
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        let targets = (targets,);
        let (mem, reserve_step, constants, sources, _, unused) = self.fields;
        LinksBuilder {
            fields: (mem, reserve_step, constants, sources, targets, unused),
            phantom: self.phantom,
        }
    }
}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub enum LinksBuilder_Error_Repeated_field_targets {}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused,
        __sources,
        __constants,
        __reserve_step,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<
        (
            __mem,
            __reserve_step,
            __constants,
            __sources,
            (TT,),
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    >
{
    #[deprecated(note = "Repeated field targets")]
    pub fn targets(
        self,
        _: LinksBuilder_Error_Repeated_field_targets,
    ) -> LinksBuilder<
        (
            __mem,
            __reserve_step,
            __constants,
            __sources,
            (TT,),
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        self
    }
}

#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __targets,
        __sources,
        __constants,
        __reserve_step,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<(__mem, __reserve_step, __constants, __sources, __targets, ()), T, M, TS, TT, TU>
{
    pub fn unused(
        self,
        unused: TU,
    ) -> LinksBuilder<
        (
            __mem,
            __reserve_step,
            __constants,
            __sources,
            __targets,
            (TU,),
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        let unused = (unused,);
        let (mem, reserve_step, constants, sources, targets, _) = self.fields;
        LinksBuilder {
            fields: (mem, reserve_step, constants, sources, targets, unused),
            phantom: self.phantom,
        }
    }
}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub enum LinksBuilder_Error_Repeated_field_unused {}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __targets,
        __sources,
        __constants,
        __reserve_step,
        __mem,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<
        (
            __mem,
            __reserve_step,
            __constants,
            __sources,
            __targets,
            (TU,),
        ),
        T,
        M,
        TS,
        TT,
        TU,
    >
{
    #[deprecated(note = "Repeated field unused")]
    pub fn unused(
        self,
        _: LinksBuilder_Error_Repeated_field_unused,
    ) -> LinksBuilder<
        (
            __mem,
            __reserve_step,
            __constants,
            __sources,
            __targets,
            (TU,),
        ),
        T,
        M,
        TS,
        TT,
        TU,
    > {
        self
    }
}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub enum LinksBuilder_Error_Missing_required_field_mem {}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, missing_docs, clippy::panic)]
impl<
        __unused,
        __targets,
        __sources,
        __constants,
        __reserve_step,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<
        (
            (),
            __reserve_step,
            __constants,
            __sources,
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    >
{
    #[deprecated(note = "Missing required field mem")]
    pub fn build(
        self,
        _: LinksBuilder_Error_Missing_required_field_mem,
    ) -> Links<T, M, TS, TT, TU> {
        {
            ::std::rt::begin_panic("explicit panic")
        };
    }
}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, non_snake_case)]
pub enum LinksBuilder_Error_Missing_required_field_constants {}

#[doc(hidden)]
#[allow(dead_code, non_camel_case_types, missing_docs, clippy::panic)]
impl<
        __unused,
        __targets,
        __sources,
        __reserve_step,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    > LinksBuilder<((M,), __reserve_step, (), __sources, __targets, __unused), T, M, TS, TT, TU>
{
    #[deprecated(note = "Missing required field constants")]
    pub fn build(
        self,
        _: LinksBuilder_Error_Missing_required_field_constants,
    ) -> Links<T, M, TS, TT, TU> {
        {
            ::std::rt::begin_panic("explicit panic")
        };
    }
}

#[allow(dead_code, non_camel_case_types, missing_docs)]
impl<
        __unused: LinksBuilder_Optional<TU>,
        __targets: LinksBuilder_Optional<TT>,
        __sources: LinksBuilder_Optional<TS>,
        __reserve_step: LinksBuilder_Optional<usize>,
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    >
    LinksBuilder<
        (
            (M,),
            __reserve_step,
            (LinksConstants<T>,),
            __sources,
            __targets,
            __unused,
        ),
        T,
        M,
        TS,
        TT,
        TU,
    >
{
    pub const LINK_SIZE: usize = size_of::<RawLink<T>>();
    pub const HEADER_SIZE: usize = size_of::<LinksHeader<T>>();
    pub const MAGIC_CONSTANT: usize = 1024 * 1024;
    pub const DEFAULT_LINKS_SIZE_STEP: usize = Self::LINK_SIZE * Self::MAGIC_CONSTANT;

    #[allow(clippy::default_trait_access)]
    pub fn open(self) -> Result<Links<T, M, TS, TT, TU>, LinksError<T>> {
        let (mem, reserve_step, constants, sources, targets, unused) = self.fields;
        let mem = mem.0;
        let reserve_step =
            LinksBuilder_Optional::into_value(reserve_step, || Self::DEFAULT_LINKS_SIZE_STEP);
        let constants = constants.0;
        let sources = LinksBuilder_Optional::into_value(sources, || {
            let links = mem.get_ptr();
            let header = links;
            TS::new(constants.clone(), links, header)
        });
        let targets = LinksBuilder_Optional::into_value(targets, || {
            let links = mem.get_ptr();
            let header = links;
            TT::new(constants.clone(), links, header)
        });
        let unused = LinksBuilder_Optional::into_value(unused, || {
            let links = mem.get_ptr();
            let header = links;
            TU::new(links, header)
        });
        let mut new = Links {
            mem,
            reserve_step,
            constants,
            sources,
            targets,
            unused,
        };

        new.init()?;
        Ok(new)
    }
}

impl<
        T: LinkType,
        M: ResizeableMem,
        TS: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TT: ILinksTreeMethods<T> + NewTree<T> + UpdatePointers,
        TU: ILinksListMethods<T> + NewList<T> + UpdatePointers,
    > Links<T, M, TS, TT, TU>
{
    pub const LINK_SIZE: usize = size_of::<RawLink<T>>();
    pub const HEADER_SIZE: usize = size_of::<LinksHeader<T>>();
    pub const MAGIC_CONSTANT: usize = 1024 * 1024;
    pub const DEFAULT_LINKS_SIZE_STEP: usize = Self::LINK_SIZE * Self::MAGIC_CONSTANT;

    pub fn new(mem: M) -> Result<Links<T, M>, LinksError<T>> {
        Self::with_constants(mem, LinksConstants::new())
    }

    pub fn with_constants(
        mem: M,
        constants: LinksConstants<T>,
    ) -> Result<Links<T, M>, LinksError<T>> {
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

        let header = self.get_mut_header();
        header.reserved = T::from_usize((reserved - Self::HEADER_SIZE) / Self::LINK_SIZE).unwrap();
        Ok(())
    }

    pub fn get_header(&self) -> &LinksHeader<T> {
        unsafe { &*(self.mem.get_ptr() as *const LinksHeader<T>) }
    }

    fn get_mut_header(&mut self) -> &mut LinksHeader<T> {
        unsafe { &mut *(self.mem.get_ptr() as *mut LinksHeader<T>) }
    }

    fn get_link(&self, link: T) -> &RawLink<T> {
        unsafe { &*((self.mem.get_ptr() as *const RawLink<T>).add(link.as_())) }
    }

    pub fn get_mut_link(&mut self, link: T) -> &mut RawLink<T> {
        unsafe { &mut *((self.mem.get_ptr() as *mut RawLink<T>).add(link.as_())) }
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

    pub fn get_link_unchecked(&self, index: T) -> Link<T> {
        let link = self.get_link(index);
        Link::new(index, link.source, link.target)
    }

    fn each_core<F, R>(&self, handler: &mut F, restriction: impl ToQuery<T>) -> R
    where
        F: FnMut(Link<T>) -> R,
        R: Try<Output = ()>,
    {
        let restriction = restriction.to_query();

        let constants = self.constants();
        let r#break = constants.r#break;

        if restriction.len() == 0 {
            for index in T::one()..self.get_header().allocated + one() {
                let link = self.get_link_unchecked(index);
                if self.exists(index) {
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
            } else if !self.exists(index) {
                R::from_output(())
            } else {
                let link_struct = self.get_link_unchecked(index);
                handler(link_struct)
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
                if !self.exists(index) {
                    return R::from_output(());
                }
                if value == any {
                    let link = self.get_link_unchecked(index);
                    return handler(link);
                }
                let stored = self.get_link(index);
                if stored.source == value || stored.target == value {
                    let link = self.get_link_unchecked(index);
                    return handler(link);
                }
                R::from_output(())
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
                    if link == constants.null {
                        R::from_output(())
                    } else {
                        let link = self.get_link_unchecked(link);
                        handler(link)
                    }
                };
            } else {
                if !self.exists(index) {
                    return R::from_output(());
                }
                if (target, source) == (any, any) {
                    return R::from_output(()); // TODO: get_total
                }
                let stored = self.get_link(index);
                if target != any && source != any {
                    return if (source, target) == (stored.source, stored.target) {
                        let link = self.get_link_unchecked(index);
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
                return if (stored.source == value) || (stored.target == value) {
                    let link = self.get_link_unchecked(index);
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
    > doublets::ILinks<T> for Links<T, M, TS, TT, TU>
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

                let stored = self.get_link(index);
                return if stored.source == value || stored.target == value {
                    one()
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
        let constants = self.constants();
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
                let header = self.get_mut_header();
                header.reserved = T::from_usize(reserved / Self::LINK_SIZE).unwrap()
            }
            let header = self.get_mut_header();
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

        let constants = self.constants();
        let null = constants.null;

        let link = *self.get_mut_link(index);
        if link.source != null {
            let temp = &mut self.get_mut_header().root_as_source as *mut T;
            unsafe { self.sources.detach(&mut *temp, index) };
        }

        let link = *self.get_mut_link(index);
        if link.target != null {
            let temp = &mut self.get_mut_header().root_as_target as *mut T;
            unsafe { self.targets.detach(&mut *temp, index) };
        }

        let link = self.get_mut_link(index);
        link.source = source;
        link.target = target;

        let link = *self.get_mut_link(index);
        if link.source != null {
            let temp = &mut self.get_mut_header().root_as_source as *mut T;
            unsafe { self.sources.attach(&mut *temp, index) };
        }

        let link = *self.get_mut_link(index);
        if link.target != null {
            let temp = &mut self.get_mut_header().root_as_target as *mut T;
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
        let (source, target) = if let Some(link) = ILinks::get_link(self, index) {
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
            let header = self.get_mut_header();
            header.allocated = allocated - one();

            loop {
                let allocated = self.get_header().allocated;
                if !(allocated > zero() && self.is_unused(allocated)) {
                    break;
                }
                self.unused.detach(allocated);
                self.get_mut_header().allocated = allocated - one();
                let used_mem = self.mem.used_mem();
                self.mem.use_mem(used_mem - Self::LINK_SIZE)?;
            }
        }
        Ok(handler(Link::new(index, source, target), Link::nothing()))
    }

    fn get_link(&self, index: T) -> Option<Link<T>> {
        if self.constants.is_external_reference(index) {
            Some(Link::point(index))
        } else if self.exists(index) {
            Some(self.get_link_unchecked(index))
        } else {
            None
        }
    }
}
