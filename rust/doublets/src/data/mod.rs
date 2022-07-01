mod doublet;
mod error;
mod handler;
mod link;
mod traits;

pub use doublet::Doublet;
pub use error::LinksError;
pub use handler::{FuseHandler, Handler};
pub use link::Link;
pub use traits::Doublets;

#[cfg(feature = "data")]
pub use data::*;
