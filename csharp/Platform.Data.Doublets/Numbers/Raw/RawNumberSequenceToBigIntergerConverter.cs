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
            var firstPart = _numberToAddressConverter.Convert(parts.First());
            BigInteger currentBigInt = new(firstPart.ToBytes());
            foreach (var part in parts.Skip(1))
            {
                currentBigInt <<= 63;
                var nextPart = _numberToAddressConverter.Convert(part);
                currentBigInt = currentBigInt | new BigInteger(nextPart.ToBytes());
            }
            return currentBigInt;
        }      
    }
}
