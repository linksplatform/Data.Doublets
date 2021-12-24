pub use doublet::Doublet;
pub use flow::Flow;
pub use ilinks::ILinks;
pub use ilinks::ILinksExtensions;
pub use link::Link;

pub use error::LinksError;
pub use ilinks::Result;

pub mod data;
pub mod decorators;
mod doublet;
mod error;
mod flow;
mod ilinks;
mod link;
pub mod mem;
