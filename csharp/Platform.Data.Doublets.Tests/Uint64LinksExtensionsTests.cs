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
    /// <summary>
    /// <para>
    /// Represents the uint 64 links extensions tests.
    /// </para>
    /// <para></para>
    /// </summary>
    public class Uint64LinksExtensionsTests
    {
        /// <summary>
        /// <para>
        /// Creates the links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>A links of t link</para>
        /// <para></para>
        /// </returns>
        public static ILinks<TLink> CreateLinks() => CreateLinks<TLink>(new Platform.IO.TemporaryFile());

        /// <summary>
        /// <para>
        /// Creates the links using the specified data db filename.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLink">
        /// <para>The link.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="dataDBFilename">
        /// <para>The data db filename.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A links of t link</para>
        /// <para></para>
        /// </returns>
        public static ILinks<TLink> CreateLinks<TLink>(string dataDBFilename)
        {
            var linksConstants = new LinksConstants<TLink>(enableExternalReferencesSupport: true);
            return new UnitedMemoryLinks<TLink>(new FileMappedResizableDirectMemory(dataDBFilename), UnitedMemoryLinks<TLink>.DefaultLinksSizeStep, linksConstants, IndexTreeType.Default);
        }
        /// <summary>
        /// <para>
        /// Tests that format structure with external reference test.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public void FormatStructureWithExternalReferenceTest()
        {
            ILinks<TLink> links = CreateLinks();
            TLink zero = default;
            var one = Arithmetic.Increment(zero);
            var markerIndex = one;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            var numberMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(ref markerIndex));
            AddressToRawNumberConverter<TLink> addressToNumberConverter = new();
            var numberAddress = addressToNumberConverter.Convert(1);
            var numberLink = links.GetOrCreate(numberMarker, numberAddress);
            var linkNotation = links.FormatStructure(numberLink, link => link.IsFullPoint(), true);
            Assert.Equal("(3:(2:1 2) 18446744073709551615)", linkNotation);
        }
    }
}
