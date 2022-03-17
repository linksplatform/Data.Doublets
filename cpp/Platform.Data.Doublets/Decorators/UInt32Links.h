

using TLink = std::uint32_t;

namespace Platform::Data::Doublets::Decorators
{
    class UInt32Links : public LinksDisposableDecoratorBase<TLink>
    {
        public: UInt32Links(ILinks<TLink> &storage) : base(storage) { }

        public: TLink Create(CArray<TLinkAddress> auto&& restrictions) override { return this->decorated().CreatePoint(); }

        public: TLink Update(CArray<TLinkAddress> auto&& restrictions, CArray<TLinkAddress> auto&& substitution) override
        {
            auto constants = _constants;
            auto indexPartConstant = constants.IndexPart;
            auto sourcePartConstant = constants.SourcePart;
            auto targetPartConstant = constants.TargetPart;
            auto nullConstant = constants.Null;
            auto itselfConstant = constants.Itself;
            auto existedLink = nullConstant;
            auto updatedLink = restrictions[indexPartConstant];
            auto newSource = substitution[sourcePartConstant];
            auto newTarget = substitution[targetPartConstant];
            auto storage = this->decorated();
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
                return this->facade().MergeAndDelete(updatedLink, existedLink);
            }
        }

        public: void Delete(CArray<TLinkAddress> auto&& restrictions) override
        {
            auto linkIndex = restrictions[_constants.IndexPart];
            auto storage = this->decorated();
            storage.EnforceResetValues(linkIndex);
            this->facade().DeleteAllUsages(linkIndex);
            storage.Delete(linkIndex);
        }
    };
}
