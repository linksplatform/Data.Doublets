use crate::ILinks;
use doublets::LinksConstants;
use num::LinkType;

// TODO:
//   _______ ____  _____   ____
//  |__   __/ __ \|  __ \ / __ \
//     | | | |  | | |  | | |  | | (_)
//     | | | |  | | |  | | |  | |
//     | | | |__| | |__| | |__| |
//     |_|  \____/|_____/ \____/  (_)
//  create proc-macro cargo package for easily implement decorator for trait

////  impl<T: LinkType> ILinks for $T {
////      fn constants(&self) -> LinksConstants<T> {
////          self.links().constants()
////      }
//
////      fn count_generic<L>(&self, restrictions: L) -> T
//       where
////          L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
/////     {
////          self.links().count_generic(restrictions)
////      }
//
////      fn each_generic<F, L>(&self, handler: F, restrictions: L) -> T
/////     where
/////         F: FnMut(&[T]) -> T,
////          L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
//\      {
/////         self.links().each_generic(handler, restrictions)
/////     }
//
////      fn create_generic<L>(&mut self, restrictions: L) -> T
/////     where
/////         L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
/////     {
/////         self.links().create_generic(restrictions)
/////     }
/////
/////     fn update_generic<Lr, Ls>(&mut self, restrictions: Lr, substitution: Ls) -> T
/////     where
/////         Lr: IntoIterator<Item = T, IntoIter: ExactSizeIterator>,
/////         Ls: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
/////     {
/////         self.links().update_generic(restrictions, substitution)
/////     }
/////
/////     fn delete_generic<L>(&mut self, restrictions: L)
/////     where
/////         L: IntoIterator<Item = T, IntoIter: ExactSizeIterator>
/////     {
/////         self.links().update_generic(restrictions)
/////     }
///// }
