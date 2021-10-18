use crate::doublets::ILinks;
use crate::num::LinkType;

pub trait Counter<R: LinkType, A = ()>: Clone {
    fn count<Links: ILinks<R>>(&mut self, links: &Links, _: A) -> R;
}
