use num_traits::zero;
use std::error::Error;

use crate::doublets::data::links_constants::LinksConstants;
use crate::doublets::data::point::Point;
use crate::doublets::Flow;
use crate::num::LinkType;

// TODO: Create `Error` in `Links` trait
type Result = std::result::Result<Flow, Box<dyn Error>>;

type ReadHandler<'a, T> = &'a dyn FnMut(&[T]) -> Flow;

type WriteHandler<'a, T> = &'a dyn FnMut(&[T], &[T]) -> Flow;

pub trait Links<T: LinkType> {
    fn constants(&self) -> LinksConstants<T>;

    fn count_links(&self, query: &[T]) -> T;

    fn create_links(&mut self, query: &[T], handler: WriteHandler<T>) -> Result;

    fn each_links(&self, query: &[T], handler: ReadHandler<T>) -> Result;

    fn update_links(&mut self, query: &[T], replacement: &[T], handler: WriteHandler<T>) -> Result;

    fn delete_generic(&mut self, query: &[T], handler: WriteHandler<T>) -> Result;
}
