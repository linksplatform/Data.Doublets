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

        [Fact]
        public void DecimalMinValueTest()
        {
            const decimal @decimal = decimal.MinValue;
            var links = CreateLinks();
            TLink negativeNumberMarker = links.Create();
            AddressToRawNumberConverter<TLink> addressToRawNumberConverter = new();
            RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            BalancedVariantConverter<TLink> balancedVariantConverter = new(links);
            BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter = new(links, addressToRawNumberConverter, balancedVariantConverter, negativeNumberMarker);
            RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter = new(links, numberToAddressConverter, negativeNumberMarker);
            DecimalToRationalConverter<TLink> decimalToRationalConverter = new(links, bigIntegerToRawNumberSequenceConverter);
            RationalToDecimalConverter<TLink> rationalToDecimalConverter = new(links, rawNumberSequenceToBigIntegerConverter);
            var rationalNumber = decimalToRationalConverter.Convert(@decimal);
            var decimalFromRational = rationalToDecimalConverter.Convert(rationalNumber);
           Assert.Equal(@decimal, decimalFromRational);
        }
        
        [Fact]
        public void DecimalMaxValueTest()
        {
            const decimal @decimal = decimal.MaxValue;
            var links = CreateLinks();
            TLink negativeNumberMarker = links.Create();
            AddressToRawNumberConverter<TLink> addressToRawNumberConverter = new();
            RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            BalancedVariantConverter<TLink> balancedVariantConverter = new(links);
            BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter = new(links, addressToRawNumberConverter, balancedVariantConverter, negativeNumberMarker);
            RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter = new(links, numberToAddressConverter, negativeNumberMarker);
            DecimalToRationalConverter<TLink> decimalToRationalConverter = new(links, bigIntegerToRawNumberSequenceConverter);
            RationalToDecimalConverter<TLink> rationalToDecimalConverter = new(links, rawNumberSequenceToBigIntegerConverter);
            var rationalNumber = decimalToRationalConverter.Convert(@decimal);
            var decimalFromRational = rationalToDecimalConverter.Convert(rationalNumber);
            Assert.Equal(@decimal, decimalFromRational);
        }
        
        [Fact]
        public void DecimalTest()
        {
            const decimal @decimal = 1234.56789M;
            var links = CreateLinks();
            TLink negativeNumberMarker = links.Create();
            AddressToRawNumberConverter<TLink> addressToRawNumberConverter = new();
            RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            BalancedVariantConverter<TLink> balancedVariantConverter = new(links);
            BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter = new(links, addressToRawNumberConverter, balancedVariantConverter, negativeNumberMarker);
            RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter = new(links, numberToAddressConverter, negativeNumberMarker);
            DecimalToRationalConverter<TLink> decimalToRationalConverter = new(links, bigIntegerToRawNumberSequenceConverter);
            RationalToDecimalConverter<TLink> rationalToDecimalConverter = new(links, rawNumberSequenceToBigIntegerConverter);
            var rationalNumber = decimalToRationalConverter.Convert(@decimal);
            var decimalFromRational = rationalToDecimalConverter.Convert(rationalNumber);
            Assert.Equal(@decimal, decimalFromRational);
        }
    }
}
