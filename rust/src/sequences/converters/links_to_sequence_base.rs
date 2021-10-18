use crate::doublets::ILinks;
use crate::num::LinkType;

pub trait LinksToSequence<T: LinkType> {
    fn convert<L, Links>(&mut self, links: &mut Links, source: L) -> T
    where
        L: IntoIterator<Item=T, IntoIter: ExactSizeIterator>,
        Links: ILinks<T>;
}
