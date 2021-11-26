pub use doublet::Doublet;
pub use ilinks::ILinks;
pub use ilinks::ILinksExtensions;
pub use link::Link;

pub use ilinks::Result;
pub use error::LinksError;

pub mod data;
pub mod mem;
mod ilinks;
mod link;
pub mod decorators;
mod doublet;
mod error;
