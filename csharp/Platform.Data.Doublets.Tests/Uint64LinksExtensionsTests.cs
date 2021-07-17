using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Numbers.Raw;
using Platform.Memory;
using Platform.Numbers;
using Xunit;
using Xunit.Abstractions;
using TLink = System.UInt64;

namespace Platform.Data.Doublets.Tests
{
    public class Uint64LinksExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public Uint64LinksExtensionsTests(ITestOutputHelper output)
        {
            this._output = output;
        }
        public static ILinks<TLink> CreateLinks() => CreateLinks<TLink>(new Platform.IO.TemporaryFile());

        public static ILinks<TLink> CreateLinks<TLink>(string dataDBFilename)
        {
            var linksConstants = new LinksConstants<TLink>(enableExternalReferencesSupport: true);
            return new UnitedMemoryLinks<TLink>(new FileMappedResizableDirectMemory(dataDBFilename), UnitedMemoryLinks<TLink>.DefaultLinksSizeStep, linksConstants, IndexTreeType.Default);
        }
        [Fact]
        public void BugTest()
        {
            // Trying to create link "(43:(7:1 7) (42:(10:1 10) (41:7 (40:(9:1 9) 0))))"
            ILinks<TLink> links = CreateLinks();
            TLink zero = default;
            var one = Arithmetic.Increment(zero);
            var markerIndex = one;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            var numberMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(ref markerIndex)); // 9
            AddressToRawNumberConverter<TLink> addressToNumberConverter = new();
            // RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            var numberAddress = addressToNumberConverter.Convert(1); // Address of 0
            var numberLink = links.GetOrCreate(numberMarker, numberAddress); // 40
            var output = ((ILinks<ulong>)(object)links).FormatStructure((ulong)(object)numberLink, link => link.IsFullPoint(), true);
            _output.WriteLine(output);
        }
    }
}
