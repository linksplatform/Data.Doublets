use crate::doublets::data::links_constants::LinksConstants;
use crate::doublets::data::point::Point;
use crate::num::LinkType;
use libc::labs;
use num_traits::zero;
use std::ops::Deref;

pub trait IGenericLinks<T: LinkType> {
    fn constants(&self) -> LinksConstants<T> {
        LinksConstants::new()
    }

    fn count_generic<L>(&self, restrictions: L) -> T
        where
            L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>;

    fn each_generic<F, L>(&self, handler: F, restrictions: L) -> T
        where
            F: FnMut(&[T]) -> T,
            L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>;

    fn create_generic<L>(&mut self, restrictions: L) -> T
        where
            L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>;

    fn update_generic<Lr, Ls>(&mut self, restrictions: Lr, substitution: Ls) -> T
        where
            Lr: IntoIterator<Item=T, IntoIter: ExactSizeIterator>,
            Ls: IntoIterator<Item=T, IntoIter: ExactSizeIterator>;

    fn delete_generic<L>(&mut self, restrictions: L)
        where
            L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>;
}

pub trait IGenericLinksExtensions<T: LinkType>: IGenericLinks<T> {
    fn exist(&self, link: T) -> bool {
        let constants = self.constants();

        if constants.is_external_reference(link) {
            self.count_generic([link]) != zero()
        } else {
            constants.is_internal_reference(link)
        }
    }

    // TODO: maybe use .collect() style
    //  let link: Vec<_> = links.get_link(index)
    fn get_generic_link(&self, link: T) -> Option<Box<dyn ExactSizeIterator<Item=T>>> {
        let constants = self.constants();
        if constants.is_external_reference(link) {
            Some(box Point::new(link, constants.target_part.as_() + 1).into_iter())
        } else {
            let mut slice = None;
            self.each_generic(
                |link| {
                    slice = Some(link.to_vec());
                    return constants.r#break;
                }, [link]);
            // TODO: fix type annotations
            slice.map(|slice| -> Box<dyn ExactSizeIterator<Item=T>> {
                box slice.into_iter()
            })
        }
    }

    fn is_full_point(&self, link: T) -> bool {
        let constants = self.constants();
        if constants.is_external_reference(link) {
            true
        } else {
            assert!(self.exist(link)); // TODO: add message
            Point::is_full(self.get_generic_link(link).unwrap())
        }
    }

    fn is_partial_point(&self, link: T) -> bool {
        let constants = self.constants();
        if constants.is_external_reference(link) {
            true
        } else {
            assert!(self.exist(link)); // TODO: add message
            Point::is_partial(self.get_generic_link(link).unwrap())
        }
    }
}

impl<T: LinkType, All: IGenericLinks<T>> IGenericLinksExtensions<T> for All {}
