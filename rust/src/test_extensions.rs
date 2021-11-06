use std::default::default;

use num_traits::{one, zero};
use rand::Rng;

use crate::doublets::data::{Hybrid, IGenericLinks, IGenericLinksExtensions};
use crate::doublets::ILinks;
use crate::doublets::ILinksExtensions;
use crate::doublets::Link;
use crate::num::LinkType;

pub trait ILinksTestExtensions<T: LinkType>: ILinks<T> + ILinksExtensions<T> {
    fn test_crud(&mut self) {
        let constants = self.constants();

        assert_eq!(self.count(), zero());

        let address = self.create();
        // TODO: expect
        let mut link: Link<T> = self.get_link(address).unwrap();

        assert_eq!(link.len(), 3);
        assert_eq!(link.index, address);
        assert_eq!(link.source, constants.null);
        assert_eq!(link.target, constants.null);
        assert_eq!(self.count(), one());

        self.update(address, address, address);

        // TODO: expect
        link = self.get_link(address).unwrap();
        assert_eq!(link.source, address);
        assert_eq!(link.target, address);

        let updated = self.update(address, constants.null, constants.null);
        assert_eq!(updated, address);
        // TODO: expect
        link = self.get_link(address).unwrap();
        assert_eq!(link.source, constants.null);
        assert_eq!(link.target, constants.null);

        self.delete(address);
        assert_eq!(self.count(), zero());
    }

    fn test_raw_numbers_crud(&mut self) {
        let links = self;

        let constants = links.constants();

        let n106 = T::from(106_usize);
        let n107 = T::from(char::from_u32(107).unwrap() as usize);
        let n108 = T::from((-108_i32) as usize);

        let h106 = Hybrid::external(n106.unwrap());
        let h107 = Hybrid::new(n107.unwrap());
        let h108 = Hybrid::new(n108.unwrap());

        assert_eq!(h106.absolute().as_(), 106);
        assert_eq!(h107.absolute().as_(), 107);
        assert_eq!(h108.absolute().as_(), 108);

        let address1 = links.create();
        links.update(address1, h106.as_value(), h108.as_value());

        let link = links.get_link(address1).unwrap();
        assert_eq!(link.source, h106.as_value());
        assert_eq!(link.target, h108.as_value());

        let address2 = links.create();
        links.update(address2, address1, h108.as_value());

        let link = links.get_link(address2).unwrap();
        assert_eq!(link.source, address1);
        assert_eq!(link.target, h108.as_value());

        let address3 = links.create();
        links.update(address3, address1, address2);

        let link = links.get_link(address3).unwrap();
        assert_eq!(link.source, address1);
        assert_eq!(link.target, address2);

        let any = constants.any;
        let r#break = constants.r#break;

        let mut result = None;
        links.each_by(|link| {
            result = Some(link.index);
            r#break
        }, [any, h106.as_value(), h108.as_value()]);
        assert_eq!(result, Some(address1));

        let mut result = None;
        links.each_by(|link| {
            result = Some(link.index);
            r#break
        }, [any, h106.as_value(), h107.as_value()]);
        assert_eq!(result, None);

        let updated = links.update(address3, zero(), zero());
        assert_eq!(updated, address3);

        let link = links.get_link(updated).unwrap();
        assert_eq!(link.source, zero());
        assert_eq!(link.target, zero());
        links.delete(updated);

        assert_eq!(links.count(), T::from(2).unwrap());

        let r#continue = links.constants().r#continue;
        let mut result = None;
        links.each(|link| {
            result = Some(link.index);
            r#continue
        });
        assert_eq!(result, Some(address2));
    }

    fn test_random_creations_and_deletions(&mut self, per_cycle: usize) {
        for n in 1..per_cycle {
            let mut created = 0;
            let mut _deleted = 0;
            for _ in 0..n {
                let count = self.count().as_();
                let create_point: bool = rand::random();
                if count >= 2 && create_point {
                    let address = 1..=count;
                    let source = rand::thread_rng().gen_range(address.clone());
                    let target = rand::thread_rng().gen_range(address);
                    let result = self.get_or_create(
                        T::from_usize(source).unwrap(),
                        T::from_usize(target).unwrap(),
                    ).as_();

                    if result > count {
                        created += 1;
                    }
                } else {
                    self.create();
                    created += 1;
                }
                assert_eq!(created, self.count().as_());
            }

            // TODO: maybe add cfg! flag
            //for i in one()..=self.count() {
            //    self.update(i, zero(), zero());
            //}
            self.delete_all();

            assert_eq!(self.count(), zero());
        }
    }
}

impl<T: LinkType, All: ILinks<T>> ILinksTestExtensions<T> for All {}
