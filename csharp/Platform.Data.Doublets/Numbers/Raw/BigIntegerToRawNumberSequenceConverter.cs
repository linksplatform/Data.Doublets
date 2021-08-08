using System.Collections.Generic;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Numbers;
using Platform.Reflection;
using Platform.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Numbers.Raw
{
    public class BigIntegerToRawNumberSequenceConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<BigInteger, TLink>
        where TLink : struct
    {
        public static readonly TLink MaximumValue = NumericType<TLink>.MaxValue;
        public static readonly TLink BitMask = Bit.ShiftRight(MaximumValue, 1);
        public readonly IConverter<TLink> AddressToNumberConverter;
        public readonly IConverter<IList<TLink>, TLink> ListToSequenceConverter;
        public readonly TLink NegativeNumberMarker;

        public BigIntegerToRawNumberSequenceConverter(ILinks<TLink> links, IConverter<TLink> addressToNumberConverter, IConverter<IList<TLink>,TLink> listToSequenceConverter, TLink negativeNumberMarker) : base(links)
        {
            AddressToNumberConverter = addressToNumberConverter;
            ListToSequenceConverter = listToSequenceConverter;
            NegativeNumberMarker = negativeNumberMarker;
        }

        private List<TLink> GetRawNumberParts(BigInteger bigInteger)
        {
            List<TLink> rawNumbers = new();
            BigInteger currentBigInt = bigInteger;
            do
            {
                var bigIntBytes = currentBigInt.ToByteArray();
                var bigIntWithBitMask = Bit.And(bigIntBytes.ToStructure<TLink>(), BitMask);
                var rawNumber = AddressToNumberConverter.Convert(bigIntWithBitMask);
                rawNumbers.Add(rawNumber);
                currentBigInt >>= 63;
            }
            while (currentBigInt > 0);
            return rawNumbers;
        }

        public TLink Convert(BigInteger bigInteger)
        {
            var sign = bigInteger.Sign;
            var number = GetRawNumberParts(sign == -1 ? BigInteger.Negate(bigInteger) : bigInteger);
            var numberSequence = ListToSequenceConverter.Convert(number);
            return sign == -1 ? _links.GetOrCreate(NegativeNumberMarker, numberSequence) : numberSequence;
        }
    }
}
