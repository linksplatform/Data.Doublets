namespace Platform::Data::Doublets::Tests
{
    using TLinkAddress = std::uint64_t;
    TEST(LinksConstantsTests, ExternalReferencesTest)
    {
        Ranges::Range possibleInternalReferencesRange {1, std::numeric_limits<std::int64_t>::max()};
        Ranges::Range possibleExternalReferencesRange {std::numeric_limits<std::int64_t>::max() + 1UL, std::numeric_limits<TLinkAddress>::max()};
        LinksConstants<TLinkAddress> constants {possibleInternalReferencesRange, possibleExternalReferencesRange};
        Hybrid<TLinkAddress> minimum {1, isExternal: true};
        Hybrid<TLinkAddress> maximum {std::numeric_limits<TLinkAddress>::max(), isExternal: true};
        Expects(constants.IsExternalReference(minimum))
        Expects(constants.IsExternalReference(maximum));
    };
}
