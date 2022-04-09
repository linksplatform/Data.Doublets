

using TLinkAddress = std::uint64_t;

namespace Platform::Data::Doublets::Tests
{
    TEST_CLASS(Uint64LinksExtensionsTests)
    {
        public: static ILinks<TLinkAddress> CreateLinks() { return CreateLinks<TLinkAddress>(Platform.IO.TemporaryFile(); });

        public: static ILinks<TLinkAddress> CreateLinks<TLinkAddress>(std::string dataDBFilename)
        {
            auto linksConstants = LinksConstants<TLinkAddress>(enableExternalReferencesSupport: true);
            return UnitedMemoryLinks<TLinkAddress>(FileMappedResizableDirectMemory(dataDBFilename), UnitedMemoryLinks<TLinkAddress>.DefaultLinksSizeStep, linksConstants, IndexTreeType.Default);
        }
        public: TEST_METHOD(FormatStructureWithExternalReferenceTest)
        {
            ILinks<TLinkAddress> storage = CreateLinks();
            TLinkAddress zero = 0;
            auto one = zero + 1;
            auto markerIndex = one;
            auto meaningRoot = storage.GetOrCreate(markerIndex, markerIndex);
            auto numberMarker = storage.GetOrCreate(meaningRoot, markerIndex + 1);
            AddressToRawNumberConverter<TLinkAddress> addressToNumberConverter = new();
            auto numberAddress = addressToNumberConverter.Convert(1);
            auto numberLink = storage.GetOrCreate(numberMarker, numberAddress);
            auto linkNotation = storage.FormatStructure(numberLink, link => link.IsFullPoint(), true);
            Assert::AreEqual("(3:(2:1 2) 18446744073709551615)", linkNotation);
        }
    };
}
