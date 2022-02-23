namespace Platform::Data::Doublets::CriterionMatchers
{
    template <typename ...> class TargetMatcher;
    template <typename TLink> class TargetMatcher<TLink> : public LinksOperatorBase<TLink>, ICriterionMatcher<TLink>
    {
        private: TLink _targetToMatch = 0;

        public: TargetMatcher(ILinks<TLink> &storage, TLink targetToMatch) : base(storage) { return _targetToMatch = targetToMatch; }

        public: bool IsMatched(TLink link) { return _links.GetTarget(link) == _targetToMatch; }
    };
}
