using System;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Unsafe;

namespace Platform.Data.Doublets.Numbers.Raw
{
    public class RawNumberSequenceToBigIntegerConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<TLink, BigInteger>
    where TLink : struct
    {
        private readonly IConverter<TLink, TLink> _numberToAddressConverter;

        public RawNumberSequenceToBigIntegerConverter(ILinks<TLink> links, IConverter<TLink, TLink> numberToAddressConverter) : base(links)
        {
            _numberToAddressConverter = numberToAddressConverter;
        }

        public BigInteger Convert(TLink bigInt)
        {
            var convertedBigInt = _numberToAddressConverter.Convert(bigInt);
            var convertedBigIntBytes = convertedBigInt.ToBytes();
            return new BigInteger(convertedBigIntBytes);
        }      
    }
}
