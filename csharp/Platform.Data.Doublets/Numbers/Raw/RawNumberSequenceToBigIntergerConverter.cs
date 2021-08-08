using System;
using System.Collections.Generic;
using System.Numerics;
using Platform.Collections.Stacks;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Sequences.Walkers;
using Platform.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Numbers.Raw
{
    public class RawNumberSequenceToBigIntegerConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<TLink, BigInteger>
    where TLink : struct
    {
        public readonly EqualityComparer<TLink> EqualityComparer = EqualityComparer<TLink>.Default;
        private readonly IConverter<TLink, TLink> _numberToAddressConverter;
        private readonly LeftSequenceWalker<TLink> _leftSequenceWalker;
        public readonly TLink NegativeNumberMarker;

        public RawNumberSequenceToBigIntegerConverter(ILinks<TLink> links, IConverter<TLink, TLink> numberToAddressConverter, TLink negativeNumberMarker) : base(links)
        {
            _numberToAddressConverter = numberToAddressConverter;
            _leftSequenceWalker = new(links, new DefaultStack<TLink>());
            NegativeNumberMarker = negativeNumberMarker;
        }

        public BigInteger Convert(TLink bigInteger)
        {
            var sign = 1;
            var bigIntegerSequence = bigInteger;
            if (EqualityComparer.Equals(_links.GetSource(bigIntegerSequence), NegativeNumberMarker))
            {
                sign = -1;
                bigIntegerSequence = _links.GetTarget(bigInteger);
            }
            using var enumerator = _leftSequenceWalker.Walk(bigIntegerSequence).GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new Exception("Raw number sequence cannot be empty.");
            }
            var nextPart = _numberToAddressConverter.Convert(enumerator.Current);
            BigInteger currentBigInt = new(nextPart.ToBytes());
            while (enumerator.MoveNext())
            {
                currentBigInt <<= 63;
                nextPart = _numberToAddressConverter.Convert(enumerator.Current);
                currentBigInt |= new BigInteger(nextPart.ToBytes());
            }
            return sign == 1 ? currentBigInt : BigInteger.Negate(currentBigInt);
        }      
    }
}
