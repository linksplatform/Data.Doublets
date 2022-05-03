#![feature(try_trait_v2)]
#![feature(default_free_fn)]

pub use cascade_unique_resolver::CascadeUniqueResolver;
pub use cascade_usages_resolver::CascadeUsagesResolver;
pub use non_null_deletion_resolver::NonNullDeletionResolver;
pub use unique_resolver::UniqueResolver;
pub use unique_validator::UniqueValidator;
pub use usages_validator::UsagesValidator;

//mod ilinks_decorator;
mod cascade_unique_resolver;
mod cascade_usages_resolver;
mod non_null_deletion_resolver;
mod unique_resolver;
mod unique_validator;
mod usages_validator;
