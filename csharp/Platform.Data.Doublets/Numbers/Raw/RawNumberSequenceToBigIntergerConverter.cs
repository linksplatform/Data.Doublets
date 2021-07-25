using System;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;

namespace Platform.Data.Doublets.Numbers.Raw
{
    public class RawNumberSequenceToBigIntegerConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<TLink, BigInteger>
    {
        private readonly IConverter<TLink, BigInteger> _numberToAddressConverter;

        public RawNumberSequenceToBigIntegerConverter(ILinks<TLink> links, IConverter<TLink, BigInteger> numberToAddressConverter) : base(links)
        {
            _numberToAddressConverter = numberToAddressConverter;
        }

        public BigInteger Convert(TLink bigInt)
        {
            return _numberToAddressConverter.Convert(bigInt);
        }      
    }
}
