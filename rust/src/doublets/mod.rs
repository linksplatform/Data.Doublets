pub use doublet::Doublet;
pub use doublets::Doublets;
pub use doublets::ILinksExtensions;
pub use flow::Flow;
pub use handler::{Handler, StoppedHandler};
pub use link::Link;

pub use doublets::Result;
pub use error::LinksError;

pub mod data;
pub mod decorators;
mod doublet;
mod doublets;
mod error;
mod flow;
mod handler;
mod link;
pub mod mem;
