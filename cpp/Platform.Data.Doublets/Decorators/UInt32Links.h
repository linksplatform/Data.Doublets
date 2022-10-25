

using TLinkAddress = std::uint32_t;

namespace Platform::Data::Doublets::Decorators
{
    class UInt32Links : public LinksDisposableDecoratorBase<TLinkAddress>
    {
        public: UInt32Links(ILinks<TLinkAddress> &storage) : base(storage) { }

        public: TLinkAddress Create(const  LinkType& restriction) { return this->decorated().TDecorated::CreatePoint(); }

        public: TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution)
        {
            auto constants = _constants;
            auto indexPartConstant = constants.IndexPart;
            auto sourcePartConstant = constants.SourcePart;
            auto targetPartConstant = constants.TargetPart;
            auto nullConstant = constants.Null;
            auto itselfConstant = constants.Itself;
            auto existedLink = nullConstant;
            auto updatedLink = restriction[indexPartConstant];
            auto newSource = substitution[sourcePartConstant];
            auto newTarget = substitution[targetPartConstant];
            if (newSource != itselfConstant && newTarget != itselfConstant)
            {
                existedLink = storage.SearchOrDefault(newSource, newTarget);
            }
            if (existedLink == nullConstant)
            {
                auto before = storage.GetLink(updatedLink);
                if (before[sourcePartConstant] != newSource || before[targetPartConstant] != newTarget)
                {
                    storage.Update(updatedLink, newSource == itselfConstant ? updatedLink : newSource,
                                              newTarget == itselfConstant ? updatedLink : newTarget);
                }
                return updatedLink;
            }
            else
            {
                return this->facade().TFacade::MergeAndDelete(updatedLink, existedLink);
            }
        }

        public: void Delete(const  LinkType& restriction)
        {
            auto linkIndex = restriction[_constants.IndexPart];
            storage.EnforceResetValues(linkIndex);
            this->facade().TFacade::DeleteAllUsages(linkIndex);
            storage.Delete(linkIndex);
        }
    };
}
