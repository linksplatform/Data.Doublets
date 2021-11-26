use std::backtrace::Backtrace;
use std::io;
use thiserror::Error;
use crate::doublets::{Doublet, Link};
use crate::num::LinkType;

#[derive(Error, Debug)]
pub enum LinksError<T: LinkType> {
    #[error("link [{0}] does not exist.")]
    NotExists(T),

    #[error("link [{0}] has dependencies")]
    HasDeps(Link<T>),

    #[error("link [{0}] already exists")]
    AlreadyExists(Doublet<T>),

    #[error("limit for the number of links in the storage has been reached ({0})")]
    LimitReached(T),

    #[error("unable to allocate memory for links storage")]
    AllocFailed(#[from] #[backtrace] io::Error),
}
