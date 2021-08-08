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
        
        [Fact]
        public void TestPositiveValue()
        {
            var links = CreateLinks();
            BigInteger bigInteger = new(decimal.MaxValue);
            TLink negativeNumberMarker = links.Create();
            AddressToRawNumberConverter<TLink> addressToRawNumberConverter = new();
            RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            BalancedVariantConverter<TLink> listToSequenceConverter = new(links);
            BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter = new(links, addressToRawNumberConverter, listToSequenceConverter, negativeNumberMarker);
            RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter = new(links, numberToAddressConverter, negativeNumberMarker);
            var bigIntSequence = bigIntegerToRawNumberSequenceConverter.Convert(bigInteger);
            var bigIntFromSequence = rawNumberSequenceToBigIntegerConverter.Convert(bigIntSequence);
            Assert.Equal(bigInteger, bigIntFromSequence);
        }
        
        [Fact]
        public void TestNegativeValue()
        {
            var links = CreateLinks();
            BigInteger bigInteger = new(decimal.MinValue);
            TLink negativeNumberMarker = links.Create();
            AddressToRawNumberConverter<TLink> addressToRawNumberConverter = new();
            RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            BalancedVariantConverter<TLink> listToSequenceConverter = new(links);
            BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter = new(links, addressToRawNumberConverter, listToSequenceConverter, negativeNumberMarker);
            RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter = new(links, numberToAddressConverter, negativeNumberMarker);
            var bigIntSequence = bigIntegerToRawNumberSequenceConverter.Convert(bigInteger);
            var bigIntFromSequence = rawNumberSequenceToBigIntegerConverter.Convert(bigIntSequence);
            Assert.Equal(bigInteger, bigIntFromSequence);
        }
        
        [Fact]
        public void TestZeroValue()
        {
            var links = CreateLinks();
            BigInteger bigInteger = new(0);
            TLink negativeNumberMarker = links.Create();
            AddressToRawNumberConverter<TLink> addressToRawNumberConverter = new();
            RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            BalancedVariantConverter<TLink> listToSequenceConverter = new(links);
            BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter = new(links, addressToRawNumberConverter, listToSequenceConverter, negativeNumberMarker);
            RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter = new(links, numberToAddressConverter, negativeNumberMarker);
            var bigIntSequence = bigIntegerToRawNumberSequenceConverter.Convert(bigInteger);
            var bigIntFromSequence = rawNumberSequenceToBigIntegerConverter.Convert(bigIntSequence);
            Assert.Equal(bigInteger, bigIntFromSequence);
        }
        
        [Fact]
        public void TestOneValue()
        {
            var links = CreateLinks();
            BigInteger bigInteger = new(1);
            TLink negativeNumberMarker = links.Create();
            AddressToRawNumberConverter<TLink> addressToRawNumberConverter = new();
            RawNumberToAddressConverter<TLink> numberToAddressConverter = new();
            BalancedVariantConverter<TLink> listToSequenceConverter = new(links);
            BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter = new(links, addressToRawNumberConverter, listToSequenceConverter, negativeNumberMarker);
            RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter = new(links, numberToAddressConverter, negativeNumberMarker);
            var bigIntSequence = bigIntegerToRawNumberSequenceConverter.Convert(bigInteger);
            var bigIntFromSequence = rawNumberSequenceToBigIntegerConverter.Convert(bigIntSequence);
            Assert.Equal(bigInteger, bigIntFromSequence);
        }
    }
}
