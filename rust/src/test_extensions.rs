use crate::doublets::data::{IGenericLinks, IGenericLinksExtensions};
use crate::doublets::ILinks;
use crate::doublets::ILinksExtensions;
use crate::doublets::Link;
use crate::num::LinkType;
use num_traits::{one, zero};
use rand::Rng;
use std::default::default;

pub trait ILinksTestExtensions<T: LinkType>: ILinks<T> + ILinksExtensions<T> {
    fn test_crud(&mut self) {
        let constants = self.constants();

        assert_eq!(self.count(), zero());

        let address = self.create();
        // TODO: expect
        let mut link: Link<T> = self.get_generic_link(address).unwrap().collect();

        assert_eq!(link.len(), 3);
        assert_eq!(link.index, address);
        assert_eq!(link.source, constants.null);
        assert_eq!(link.target, constants.null);
        assert_eq!(self.count(), one());

        self.update(address, address, address);

        // TODO: expect
        link = self.get_generic_link(address).unwrap().collect();
        assert_eq!(link.source, address);
        assert_eq!(link.target, address);

        let updated = self.update(address, constants.null, constants.null);
        assert_eq!(updated, address);
        // TODO: expect
        link = self.get_generic_link(address).unwrap().collect();
        assert_eq!(link.source, constants.null);
        assert_eq!(link.target, constants.null);

        self.delete(address);
        assert_eq!(self.count(), zero());
    }

    fn test_random_creations_and_deletions(&mut self, per_cycle: usize) {

        for n in 1..per_cycle {
            let mut created = 0;
            let mut deleted = 0;
            for _ in 0..n {
                let count = self.count().as_();
                let create_point: bool = rand::random();
                if count >= 2 && create_point {
                    let address = 1..=count;
                    let source = rand::thread_rng().gen_range(address.clone());
                    let target = rand::thread_rng().gen_range(address);
                    let result = self.get_or_create(
                        T::from_usize(source).unwrap(),
                        T::from_usize(target).unwrap()
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

        self.each(|link| {
            println!("{:?}->{:?}", link.source, link.target);
            self.constants().r#continue
        });
    }
}

impl<T: LinkType, All: ILinks<T>> ILinksTestExtensions<T> for All {}
