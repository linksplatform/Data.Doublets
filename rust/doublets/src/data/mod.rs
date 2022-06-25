mod doublet;
mod error;
mod generator;
mod handler;
mod link;
mod traits;

pub use doublet::Doublet;
pub use error::LinksError;
pub use generator::GenIter;
pub use handler::{Handler, StoppedHandler};
pub use link::Link;
pub use traits::Doublets;

#[cfg(feature = "data")]
pub use data::*;
