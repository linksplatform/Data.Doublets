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
        private static readonly BigInteger _maximumValue = NumericType<BigInteger>.MaxValue;
        private static readonly BigInteger _bitMask = Bit.ShiftRight(_maximumValue, 1);
        
        public BigIntegerToRawNumberSequenceConverter(ILinks<TLink> links, IConverter<TLink> addressToNumberConverter) : base(links)
        {
            _addressToNumberConverter = addressToNumberConverter;   
        }

        public TLink Convert(BigInteger bigInt)
        {
            var currentBigInt = bigInt;
            var bigIntPart = Bit.And(currentBigInt, _bitMask);
            var bigIntBytes = bigIntPart.ToByteArray();
            var nextBigInt = currentBigInt >> 63;
            TLink bigIntLink;
            if (nextBigInt > 0)
            {
                TLink nextBigIntLink = Convert(nextBigInt);
                var currentBigIntLink = _addressToNumberConverter.Convert(bigIntBytes.ToStructure<TLink>());
                bigIntLink = _links.GetOrCreate(currentBigIntLink, nextBigIntLink);
            }
            else
            {
                bigIntLink = _addressToNumberConverter.Convert(bigIntBytes.ToStructure<TLink>());
            }
            return bigIntLink;
        }
    }
}
