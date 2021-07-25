using System;
using System.Collections.Generic;
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
            var bigIntBytes = bigInt.ToByteArray();
            List<TLink> bigIntPartsAsTLink = new (bigIntBytes.Length / TLinkSize + 1);
            byte[] linkBytes = new byte[TLinkSize];
            /* for (int i = 0; i < bigIntBytes.Length; i++)
            {
                if (i % TLinkSize == 0)
                {
                    var bigIntPartAsTLink = ByteArrayExtensions.ToStructure<TLink>(linkBytes.ToArray());
                    bigIntPartsAsTLink.Add(bigIntPartAsTLink);
                    Array.Clear(linkBytes, 0, linkBytes.Length);
                }
                linkBytes[i % TLinkSize] = linkBytes[i];
            }
            // var bigIntAsTLink = ByteArrayExtensions.ToStructure<TLink>(); */
            // Array.Copy(bigIntBytes, 0, linkBytes, 0, TLinkSize);
            return _addressToNumberConverter.Convert(bigIntBytes.ToStructure<TLink>());
        }
    }
}
