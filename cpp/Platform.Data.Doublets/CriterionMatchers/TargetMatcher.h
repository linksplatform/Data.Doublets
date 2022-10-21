namespace Platform::Data::Doublets::CriterionMatchers
{
    template <typename ...> class TargetMatcher;
    template <std::integral TLinkAddress> class TargetMatcher<TLinkAddress> : public LinksOperatorBase<TLinkAddress>, ICriterionMatcher<TLinkAddress>
    {
        private: TLinkAddress _targetToMatch = 0;

        public: TargetMatcher(ILinks<TLinkAddress> &storage, TLinkAddress targetToMatch) : base(storage) { return _targetToMatch = targetToMatch; }

        public: bool IsMatched(TLinkAddress link) { return _links.GetTarget(link) == _targetToMatch; }
    };
}
