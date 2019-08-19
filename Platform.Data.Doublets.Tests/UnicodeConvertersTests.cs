using Platform.Data.Doublets.Incrementers;
using Platform.Data.Doublets.Numbers.Unary;
using Platform.Data.Doublets.PropertyOperators;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Data.Doublets.Sequences.Walkers;
using Platform.Data.Doublets.Unicode;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class UnicodeConvertersTests
    {
        [Fact]
        public static void CharAndUnicodeSymbolConvertersTest()
        {
            using (var scope = new TempLinksTestScope())
            {
                var links = scope.Links;

                var itself = links.Constants.Itself;

                var meaningRoot = links.CreatePoint();
                var one = links.CreateAndUpdate(meaningRoot, itself);
                var unicodeSymbolMarker = links.CreateAndUpdate(meaningRoot, itself);

                var powerOf2ToUnaryNumberConverter = new PowerOf2ToUnaryNumberConverter<ulong>(links, one);
                var addressToUnaryNumberConverter = new AddressToUnaryNumberConverter<ulong>(links, powerOf2ToUnaryNumberConverter);
                var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<ulong>(links, addressToUnaryNumberConverter, unicodeSymbolMarker);

                var originalCharacter = 'H';

                var characterLink = charToUnicodeSymbolConverter.Convert(originalCharacter);

                var unaryNumberToAddressConverter = new UnaryNumberToAddressOrOperationConverter<ulong>(links, powerOf2ToUnaryNumberConverter);
                var unicodeSymbolCriterionMatcher = new UnicodeSymbolCriterionMatcher<ulong>(links, unicodeSymbolMarker);
                var unicodeSymbolToCharConverter = new UnicodeSymbolToCharConverter<ulong>(links, unaryNumberToAddressConverter, unicodeSymbolCriterionMatcher);

                var resultingCharacter = unicodeSymbolToCharConverter.Convert(characterLink);

                Assert.Equal(originalCharacter, resultingCharacter);
            }
        }

        [Fact]
        public static void StringAndUnicodeSequenceConvertersTest()
        {
            using (var scope = new TempLinksTestScope())
            {
                var links = scope.Links;

                var itself = links.Constants.Itself;

                var meaningRoot = links.CreatePoint();
                var unaryOne = links.CreateAndUpdate(meaningRoot, itself);
                var unicodeSymbolMarker = links.CreateAndUpdate(meaningRoot, itself);
                var unicodeSequenceMarker = links.CreateAndUpdate(meaningRoot, itself);
                var frequencyMarker = links.CreateAndUpdate(meaningRoot, itself);
                var frequencyPropertyMarker = links.CreateAndUpdate(meaningRoot, itself);

                var powerOf2ToUnaryNumberConverter = new PowerOf2ToUnaryNumberConverter<ulong>(links, unaryOne);
                var addressToUnaryNumberConverter = new AddressToUnaryNumberConverter<ulong>(links, powerOf2ToUnaryNumberConverter);
                var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<ulong>(links, addressToUnaryNumberConverter, unicodeSymbolMarker);

                var unaryNumberToAddressConverter = new UnaryNumberToAddressOrOperationConverter<ulong>(links, powerOf2ToUnaryNumberConverter);
                var unaryNumberIncrementer = new UnaryNumberIncrementer<ulong>(links, unaryOne);
                var frequencyIncrementer = new FrequencyIncrementer<ulong>(links, frequencyMarker, unaryOne, unaryNumberIncrementer);
                var frequencyPropertyOperator = new PropertyOperator<ulong>(links, frequencyPropertyMarker, frequencyMarker);
                var index = new FrequencyIncrementingSequenceIndex<ulong>(links, frequencyPropertyOperator, frequencyIncrementer);
                var linkToItsFrequencyNumberConverter = new LinkToItsFrequencyNumberConveter<ulong>(links, frequencyPropertyOperator, unaryNumberToAddressConverter);
                var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<ulong>(links, linkToItsFrequencyNumberConverter);
                var optimalVariantConverter = new OptimalVariantConverter<ulong>(links, sequenceToItsLocalElementLevelsConverter);

                var stringToUnicodeSymbolConverter = new StringToUnicodeSequenceConverter<ulong>(links, charToUnicodeSymbolConverter, index, optimalVariantConverter, unicodeSequenceMarker);

                var originalString = "Hello";

                var unicodeSequenceLink = stringToUnicodeSymbolConverter.Convert(originalString);
                
                var unicodeSymbolCriterionMatcher = new UnicodeSymbolCriterionMatcher<ulong>(links, unicodeSymbolMarker);
                var unicodeSymbolToCharConverter = new UnicodeSymbolToCharConverter<ulong>(links, unaryNumberToAddressConverter, unicodeSymbolCriterionMatcher);

                var unicodeSequenceCriterionMatcher = new UnicodeSequenceCriterionMatcher<ulong>(links, unicodeSequenceMarker);

                var sequenceWalker = new LeveledSequenceWalker<ulong>(links, unicodeSymbolCriterionMatcher.IsMatched);

                var unicodeSequenceToStringConverter = new UnicodeSequenceToStringConverter<ulong>(links, unicodeSequenceCriterionMatcher, sequenceWalker, unicodeSymbolToCharConverter);

                var resultingString = unicodeSequenceToStringConverter.Convert(unicodeSequenceLink);

                Assert.Equal(originalString, resultingString);
            }
        }
    }
}
