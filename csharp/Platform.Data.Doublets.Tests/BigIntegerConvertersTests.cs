using System.Collections.Generic;
using System.Numerics;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Numbers.Raw;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Numbers.Raw;
using Platform.Memory;
using Xunit;
using TLink = System.UInt64;

namespace Platform.Data.Doublets.Tests
{
    public class BigIntegerConvertersTests
    {
        public ILinks<TLink> CreateLinks() => CreateLinks<TLink>(new IO.TemporaryFile());

        public ILinks<TLink> CreateLinks<TLink>(string dataDbFilename)
        {
            var linksConstants = new LinksConstants<TLink>(enableExternalReferencesSupport: true);
            return new UnitedMemoryLinks<TLink>(new FileMappedResizableDirectMemory(dataDbFilename), UnitedMemoryLinks<TLink>.DefaultLinksSizeStep, linksConstants, IndexTreeType.Default);
        }
        
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[]{ new BigInteger(decimal.MaxValue) },
                new object[]{ new BigInteger(decimal.MinValue) },
                new object[]{ new BigInteger(1234.56789M) }
            };
        
        [Theory]
        [MemberData(nameof(Data))]
        public void Test(BigInteger bigInt)
        {
            var links = CreateLinks();
            AddressToRawNumberConverter<TLink> addressToRawNumberConverter = new();
            RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            BalancedVariantConverter<TLink> listToSequenceConverter = new(links);
            BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter = new(links, addressToRawNumberConverter, listToSequenceConverter);
            RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter = new(links, numberToAddressConverter);
            var bigIntSequence = bigIntegerToRawNumberSequenceConverter.Convert(bigInt);
            var bigIntFromSequence = rawNumberSequenceToBigIntegerConverter.Convert(bigIntSequence);
            Assert.Equal(bigInt, bigIntFromSequence);
        }
    }
}
