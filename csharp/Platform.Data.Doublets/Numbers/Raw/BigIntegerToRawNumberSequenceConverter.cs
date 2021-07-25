using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;

namespace Platform.Data.Doublets.Numbers.Raw
{
    public class BigIntegerToRawNumberSequenceConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<BigInteger, TLink>
    {
        public TLink Convert(BigInteger bigInt)
        {
            
        }
    }
}
