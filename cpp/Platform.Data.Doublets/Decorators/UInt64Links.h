namespace Platform::Data::Doublets::Decorators
{
    class UInt64Links : public LinksDisposableDecoratorBase<std::uint64_t>
    {
        public: UInt64Links(ILinks<std::uint64_t> &storage) : base(storage) { }

        public: std::uint64_t Create(IList<std::uint64_t> &restriction) { return this->decorated().CreatePoint(); }

        public: std::uint64_t Update(IList<std::uint64_t> &restriction, IList<std::uint64_t> &substitution)
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

        public: void Delete(IList<std::uint64_t> &restriction)
        {
            auto linkIndex = restriction[_constants.IndexPart];
            storage.EnforceResetValues(linkIndex);
            this->facade().DeleteAllUsages(linkIndex);
            storage.Delete(linkIndex);
        }
    };
}
