use crate::num::LinkType;
use crate::doublets::ilinks::{ILinks, ILinksExtensions};
use num_traits::{zero, one};
use crate::doublets::link::Link;
use std::default::default;
use crate::doublets::data::ilinks::IGenericLinks;
use crate::doublets::data::ilinks::IGenericLinksExtensions;
use rand::Rng;

pub trait ILinksTestExtensions<T: LinkType>: ILinks<T> + ILinksExtensions<T> {
    fn test_crud(&mut self) {
        let constants = self.constants();

        assert_eq!(self.count(), zero());

        let address = self.create();
        let mut link: Link<T> = self.get_link(address).collect();

        assert_eq!(link.len(), 3);
        assert_eq!(link.index, address);
        assert_eq!(link.source, constants.null);
        assert_eq!(link.target, constants.null);
        assert_eq!(self.count(), one());

        self.update(address, address, address);

        link = self.get_link(address).collect();
        assert_eq!(link.source, address);
        assert_eq!(link.target, address);

        let updated = self.update(address, constants.null, constants.null);
        assert_eq!(updated, address);
        link = self.get_link(address).collect();
        assert_eq!(link.source, constants.null);
        assert_eq!(link.target, constants.null);

        self.delete(address);
        assert_eq!(self.count(), zero());
    }

    fn test_random_creations_and_deletions(&mut self, per_cycle: usize) {
        for n in  1..per_cycle {
            let mut created = 0;
            let mut deleted = 0;

            for i in 0..n {
                let count = self.count().as_();
                let create_point: bool = rand::random();
                if count >= 2 && create_point {
                    let address = 1..=count;
                    let source = rand::thread_rng().gen_range(address.clone());
                    let target = rand::thread_rng().gen_range(address);
                    //let result = self.get_or_create(
                    //    T::from_usize(source).unwrap(),
                    //    T::from_usize(target).unwrap()
                    //).as_();
                    let result = self.create_point().as_();

                    if result > count {
                        created += 1;
                        println!("create point: {}, count: {:?}", created, self.count());
                    }
                } else {
                    self.create();
                    created += 1;
                }
                assert_eq!(created, self.count().as_());
            }
            self.delete_all();
        }
    }
}

impl<T: LinkType, All: ILinks<T>> ILinksTestExtensions<T> for All {}
