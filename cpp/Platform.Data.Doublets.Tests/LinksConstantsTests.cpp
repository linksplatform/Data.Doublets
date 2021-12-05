namespace Platform::Data::Doublets::Tests
{
    TEST_CLASS(LinksConstantsTests)
    {
        public: TEST_METHOD(ExternalReferencesTest)
        {
            LinksConstants<std::uint64_t> constants = LinksConstants<std::uint64_t>((1, std::numeric_limits<std::int64_t>::max()), (std::numeric_limits<std::int64_t>::max() + 1UL, std::numeric_limits<std::uint64_t>::max()));

            auto minimum = Hybrid<std::uint64_t>(1, isExternal: true);
            auto maximum = Hybrid<std::uint64_t>(std::numeric_limits<std::int64_t>::max(), isExternal: true);

            Assert::IsTrue(constants.IsExternalReference(minimum));
            Assert::IsTrue(constants.IsExternalReference(maximum));
        }
    };
}
