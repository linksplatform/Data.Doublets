using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;

namespace Platform.Data.Doublets.Numbers.Raw
{
    public class RawNumberSequenceToBigIntegerConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<TLink, BigInteger>
    {
        public RawNumberSequenceToBigIntegerConverter(ILinks<TLink> links) : base(links) { }

        public BigInteger Convert(TLink bigInt)
        {
            
        }      
    }
}
