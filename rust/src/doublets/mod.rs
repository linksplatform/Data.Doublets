pub use doublet::Doublet;
pub use ilinks::ILinks;
pub use ilinks::ILinksExtensions;
pub use link::Link;

pub use error::LinksError;
pub use ilinks::Result;

pub mod data;
pub mod decorators;
mod doublet;
mod error;
mod ilinks;
mod link;
pub mod mem;
