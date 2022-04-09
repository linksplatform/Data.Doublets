

using TLink = std::uint32_t;

namespace Platform::Data::Doublets::Decorators
{
    class UInt32Links : public LinksDisposableDecoratorBase<TLink>
    {
        public: UInt32Links(ILinks<TLink> &storage) : base(storage) { }

        public: TLink Create(const  LinkType& restriction) override { return this->decorated().CreatePoint(); }

        public: TLink Update(const  LinkType& restriction, const LinkType& substitution) override
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
                return this->facade().MergeAndDelete(updatedLink, existedLink);
            }
        }

        public: void Delete(const  LinkType& restriction) override
        {
            auto linkIndex = restriction[_constants.IndexPart];
            storage.EnforceResetValues(linkIndex);
            this->facade().DeleteAllUsages(linkIndex);
            storage.Delete(linkIndex);
        }
    };
}
