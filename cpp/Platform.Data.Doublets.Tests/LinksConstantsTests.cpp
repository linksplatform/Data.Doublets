namespace Platform::Data::Doublets::Tests
{
    using TLinkAddress = std::uint64_t;
    TEST(LinksConstantsTests, ExternalReferencesTest)
    {
        using namespace Platform::Ranges;
        constexpr Ranges::Range possibleInternalReferencesRange {1, std::numeric_limits<TLinkAddress>::max()};
        constexpr Ranges::Range possibleExternalReferencesRange {std::numeric_limits<TLinkAddress>::max() + 1UL, std::numeric_limits<TLinkAddress>::max()};
        constexpr LinksConstants<TLinkAddress> constants {possibleInternalReferencesRange, possibleExternalReferencesRange};
        auto isExternal {true};
        Hybrid<TLinkAddress> minimum {1, isExternal};
        Hybrid<TLinkAddress> maximum {std::numeric_limits<TLinkAddress>::max(), true};
        bool isMinimumExternalReference = IsExternalReference<TLinkAddress, constants>(minimum.Value);
        bool isMaximumExternalReference = IsExternalReference<TLinkAddress, constants>(minimum.Value);
        ASSERT_TRUE(isMinimumExternalReference);
        ASSERT_TRUE(isMaximumExternalReference);
    };
}
