pub use crate::data::flow::Flow;
pub use doublet::Doublet;
pub use doublets::Doublets;
pub use doublets::ILinksExtensions;
pub use handler::{Handler, StoppedHandler};
pub use link::Link;

pub use doublets::Result;
pub use error::LinksError;

pub mod decorators;
mod doublet;
mod doublets;
mod error;
mod handler;
mod link;
pub mod mem;
