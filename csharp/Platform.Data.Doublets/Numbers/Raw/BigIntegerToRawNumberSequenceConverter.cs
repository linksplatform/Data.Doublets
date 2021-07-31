using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Numbers;
using Platform.Reflection;
using Platform.Unsafe;


namespace Platform.Data.Doublets.Numbers.Raw
{
    public class BigIntegerToRawNumberSequenceConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<BigInteger, TLink>
    where TLink : struct
    {
        private readonly IConverter<TLink> _addressToNumberConverter;
        public static readonly int BitsStorableInRawNumber = Structure<TLink>.Size - 1;
        private static readonly int _bitsPerRawNumber = NumericType<TLink>.BitsSize - 1;
        private static readonly TLink _maximumValue = NumericType<TLink>.MaxValue;
        private static readonly TLink _bitMask = Bit.ShiftRight(_maximumValue, 1);
        
        public BigIntegerToRawNumberSequenceConverter(ILinks<TLink> links, IConverter<TLink> addressToNumberConverter) : base(links)
        {
            _addressToNumberConverter = addressToNumberConverter;   
        }

        public TLink Convert(BigInteger bigInt)
        {
            var currentBigInt = bigInt;
            var bigIntBytes = currentBigInt.ToByteArray();
            var nextBigInt = currentBigInt >> 63;
            TLink bigIntLink;
            if (nextBigInt > 0)
            {
                TLink nextBigIntLink = Convert(nextBigInt);
                var bigIntWithBitMask = Bit.And(bigIntBytes.ToStructure<TLink>(), _bitMask);
                var convertedBigInt = _addressToNumberConverter.Convert(bigIntWithBitMask);
                bigIntLink = _links.GetOrCreate(convertedBigInt, nextBigIntLink);
            }
            else
            {
                bigIntLink = _addressToNumberConverter.Convert(bigIntBytes.ToStructure<TLink>());
            }
            return bigIntLink;
        }
    }
}
