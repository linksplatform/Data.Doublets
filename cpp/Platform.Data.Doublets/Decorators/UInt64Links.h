namespace Platform::Data::Doublets::Decorators
{
    class UInt64Links : public LinksDisposableDecoratorBase<std::uint64_t>
    {
        public: UInt64Links(ILinks<std::uint64_t> &storage) : base(storage) { }

        public: std::uint64_t Create(IList<std::uint64_t> &restrictions) override { return _links.CreatePoint(); }

        public: std::uint64_t Update(IList<std::uint64_t> &restrictions, IList<std::uint64_t> &substitution) override
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

        public: void Delete(IList<std::uint64_t> &restrictions) override
        {
            auto linkIndex = restrictions[_constants.IndexPart];
            auto storage = _links;
            storage.EnforceResetValues(linkIndex);
            _facade.DeleteAllUsages(linkIndex);
            storage.Delete(linkIndex);
        }
    };
}
