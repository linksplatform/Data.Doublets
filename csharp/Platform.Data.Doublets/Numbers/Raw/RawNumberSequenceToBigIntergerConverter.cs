using System;
using System.Linq;
using System.Numerics;
using Platform.Collections.Stacks;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Sequences.Walkers;
using Platform.Unsafe;

namespace Platform.Data.Doublets.Numbers.Raw
{
    public class RawNumberSequenceToBigIntegerConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<TLink, BigInteger>
    where TLink : struct
    {
        private readonly IConverter<TLink, TLink> _numberToAddressConverter;
        private readonly LeftSequenceWalker<TLink> _leftSequenceWalker;

        public RawNumberSequenceToBigIntegerConverter(ILinks<TLink> links, IConverter<TLink, TLink> numberToAddressConverter) : base(links)
        {
            _numberToAddressConverter = numberToAddressConverter;
            _leftSequenceWalker = new(links, new DefaultStack<TLink>());
        }

        public BigInteger Convert(TLink bigInteger)
        {
            var parts = _leftSequenceWalker.Walk(bigInteger);
            var partsEnumerator = parts.GetEnumerator();
            partsEnumerator.MoveNext();
            TLink nextPart = _numberToAddressConverter.Convert(partsEnumerator.Current);
            BigInteger currentBigInt = new(nextPart.ToBytes());
            while (partsEnumerator.MoveNext())
            {
                currentBigInt <<= 63;
                nextPart = _numberToAddressConverter.Convert(partsEnumerator.Current);
                currentBigInt |= new BigInteger(nextPart.ToBytes());
            }
            return currentBigInt;
        }      
    }
}
