

using TLink = std::uint32_t;

namespace Platform::Data::Doublets::Decorators
{
    class UInt32Links : public LinksDisposableDecoratorBase<TLink>
    {
        public: UInt32Links(ILinks<TLink> &storage) : base(storage) { }

        public: TLink Create(CList auto&&restrictions) override { return _links.CreatePoint(); }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
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
            auto storage = _links;
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
                return _facade.MergeAndDelete(updatedLink, existedLink);
            }
        }

        public: void Delete(CList auto&&restrictions) override
        {
            auto linkIndex = restrictions[_constants.IndexPart];
            auto storage = _links;
            storage.EnforceResetValues(linkIndex);
            _facade.DeleteAllUsages(linkIndex);
            storage.Delete(linkIndex);
        }
    };
}
