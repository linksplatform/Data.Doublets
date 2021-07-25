using System;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;

namespace Platform.Data.Doublets.Numbers.Raw
{
    public class RawNumberSequenceToBigIntegerConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<TLink>
    {
        private readonly IConverter<TLink> _numberToAddressConverter;

        public RawNumberSequenceToBigIntegerConverter(ILinks<TLink> links, IConverter<TLink> numberToAddressConverter) : base(links)
        {
            _numberToAddressConverter = numberToAddressConverter;
        }

        public TLink Convert(TLink bigInt)
        {
            return _numberToAddressConverter.Convert(bigInt);
        }      
    }
}
