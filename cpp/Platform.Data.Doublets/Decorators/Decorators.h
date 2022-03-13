using namespace Platform::Interfaces;
using namespace Platform::Data::Doublets::Decorators;

template<typename TLinksStorage>
using LinksDecoratedWithAutomaticUniquenessAndUsagesResolution = Platform::Interfaces::Decorated<TLinksStorage, LinksCascadeUniquenessAndUsagesResolver, NonNullContentsLinkDeletionResolver, LinksCascadeUsagesResolver>;
