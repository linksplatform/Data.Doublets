pub use header::LinksHeader;
pub use traits::{
    LinksList, LinksTree, SplitList, SplitTree, SplitUpdateMem, UnitTree, UnitUpdateMem,
};
mod header;
pub mod split;
mod traits;
pub mod unit;

#[cfg(feature = "mem")]
pub use mem::*;

pub mod parts {
    pub use super::{
        split::{DataPart, IndexPart},
        unit::LinkPart,
    };
}
