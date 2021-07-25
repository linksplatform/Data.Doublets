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
        public readonly int TLinkSize = Structure<TLink>.Size;

        public BigIntegerToRawNumberSequenceConverter(ILinks<TLink> links, IConverter<TLink> addressToNumberConverter) : base(links)
        {
            _addressToNumberConverter = addressToNumberConverter;   
        }

        public TLink Convert(BigInteger bigInt)
        {
            var bigIntAsTLink = ByteArrayExtensions.ToStructure<TLink>(bigInt.ToByteArray());
            return _addressToNumberConverter.Convert(bigIntAsTLink);
        }
    }
}
