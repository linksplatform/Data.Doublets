using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Unsafe;


namespace Platform.Data.Doublets.Numbers.Raw
{
    public class BigIntegerToRawNumberSequenceConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<BigInteger, TLink>
    where TLink : struct
    {
        private readonly IConverter<TLink> _addressToNumberConverter;
        public static readonly int BitsStorableInRawNumber = Structure<TLink>.Size - 1;
        
        public BigIntegerToRawNumberSequenceConverter(ILinks<TLink> links, IConverter<TLink> addressToNumberConverter) : base(links)
        {
            _addressToNumberConverter = addressToNumberConverter;   
        }

        public TLink Convert(BigInteger bigInt)
        {
            var currentBigInt = bigInt;
            TLink bigIntLink;
                var bigIntBytes = currentBigInt.ToByteArray();
                var next63Bits = currentBigInt >> 63;
                TLink next63BitsLink;
                if (next63Bits != 0)
                {
                    next63BitsLink = Convert(next63Bits);
                    var currentBigIntLink = _addressToNumberConverter.Convert(bigIntBytes.ToStructure<TLink>());
                    bigIntLink = _links.GetOrCreate(currentBigIntLink, next63BitsLink);
                }
                else
                {
                    bigIntLink = _addressToNumberConverter.Convert(bigIntBytes.ToStructure<TLink>());
                }
                return bigIntLink;
        }
    }
}
