using System.Collections.Generic;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Numbers.Rational;
using Platform.Data.Doublets.Numbers.Raw;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Numbers.Raw;
using Platform.Memory;
using Xunit;
using TLink = System.UInt64;

namespace Platform.Data.Doublets.Tests
{
    public class RationalNumbersTests
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
                new object[]{ decimal.MaxValue },
                new object[]{ decimal.MinValue },
                new object[]{ 1234.56789M }
            };
        
        [Theory]
        [MemberData(nameof(Data))]
        public void Test(decimal @decimal)
        {
            var links = CreateLinks();
            AddressToRawNumberConverter<TLink> addressToRawNumberConverter = new();
            RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            BalancedVariantConverter<TLink> balancedVariantConverter = new(links);
            BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter = new(links, addressToRawNumberConverter, balancedVariantConverter);
            RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter = new(links, numberToAddressConverter);
            DecimalToRationalConverter<TLink> decimalToRationalConverter = new(links, bigIntegerToRawNumberSequenceConverter);
            RationalToDecimalConverter<TLink> rationalToDecimalConverter = new(links, rawNumberSequenceToBigIntegerConverter);
            var rationalNumber = decimalToRationalConverter.Convert(@decimal);
            var decimalFromRational = rationalToDecimalConverter.Convert(rationalNumber);
           Assert.Equal(@decimal, decimalFromRational);
        }
    }
}
