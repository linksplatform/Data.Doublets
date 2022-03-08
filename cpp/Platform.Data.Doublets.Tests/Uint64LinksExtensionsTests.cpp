

using TLink = std::uint64_t;

namespace Platform::Data::Doublets::Tests
{
    TEST_CLASS(Uint64LinksExtensionsTests)
    {
        public: static ILinks<TLink> CreateLinks() { return CreateLinks<TLink>(Platform.IO.TemporaryFile(); });

        public: static ILinks<TLink> CreateLinks<TLink>(std::string dataDBFilename)
        {
            auto linksConstants = LinksConstants<TLink>(enableExternalReferencesSupport: true);
            return UnitedMemoryLinks<TLink>(FileMappedResizableDirectMemory(dataDBFilename), UnitedMemoryLinks<TLink>.DefaultLinksSizeStep, linksConstants, IndexTreeType.Default);
        }
        public: TEST_METHOD(FormatStructureWithExternalReferenceTest)
        {
            ILinks<TLink> storage = CreateLinks();
            TLink zero = 0;
            auto one = zero + 1;
            auto markerIndex = one;
            auto meaningRoot = storage.GetOrCreate(markerIndex, markerIndex);
            auto numberMarker = storage.GetOrCreate(meaningRoot, markerIndex + 1);
            AddressToRawNumberConverter<TLink> addressToNumberConverter = new();
            auto numberAddress = addressToNumberConverter.Convert(1);
            auto numberLink = storage.GetOrCreate(numberMarker, numberAddress);
            auto linkNotation = storage.FormatStructure(numberLink, link => link.IsFullPoint(), true);
            Assert::AreEqual("(3:(2:1 2) 18446744073709551615)", linkNotation);
        }
    };
}
