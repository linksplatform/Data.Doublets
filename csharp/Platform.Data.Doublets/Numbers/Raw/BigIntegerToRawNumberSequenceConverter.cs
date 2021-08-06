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
        public readonly EqualityComparer<TLink> EqualityComparer = EqualityComparer<TLink>.Default;
        private readonly IConverter<TLink> _addressToNumberConverter;
        private readonly IConverter<IList<TLink>, TLink> _listToSequenceConverter;
        private static readonly TLink _maximumValue = NumericType<TLink>.MaxValue;
        private static readonly TLink _bitMask = Bit.ShiftRight(_maximumValue, 1);
        
        public BigIntegerToRawNumberSequenceConverter(ILinks<TLink> links, IConverter<TLink> addressToNumberConverter, IConverter<IList<TLink>,TLink> listToSequenceConverter) : base(links)
        {
            _addressToNumberConverter = addressToNumberConverter;
            _listToSequenceConverter = listToSequenceConverter;
        }

        private List<TLink> GetRawNumberParts(BigInteger bigInteger)
        {
            List<TLink> rawNumbers = new();
            for (BigInteger currentBigInt = bigInteger; currentBigInt > 0; currentBigInt >>= 63)
            {
                var bigIntBytes = currentBigInt.ToByteArray();
                var bigIntWithBitMask = Bit.And(bigIntBytes.ToStructure<TLink>(), _bitMask);
                var rawNumber = _addressToNumberConverter.Convert(bigIntWithBitMask);
                rawNumbers.Add(rawNumber);
            }
            return rawNumbers;
        }

        public TLink Convert(BigInteger bigInteger)
        {
            var rawNumbers = GetRawNumberParts(bigInteger);
            return _listToSequenceConverter.Convert(rawNumbers);
        }
    }
}
