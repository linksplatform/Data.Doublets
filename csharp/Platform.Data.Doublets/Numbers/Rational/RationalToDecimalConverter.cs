using System.Collections.Generic;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Numbers;
using Platform.Reflection;
using Platform.Unsafe;
using System;
using System.Text;
using Platform.Data.Doublets.Numbers.Raw;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Numbers.Rational
{
    public class RationalToDecimalConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<TLink, decimal>
    where TLink: struct
    {
        public readonly RawNumberSequenceToBigIntegerConverter<TLink> RawNumberSequenceToBigIntegerConverter;

        public RationalToDecimalConverter(ILinks<TLink> links, RawNumberSequenceToBigIntegerConverter<TLink> rawNumberSequenceToBigIntegerConverter) : base(links)
        {
            RawNumberSequenceToBigIntegerConverter = rawNumberSequenceToBigIntegerConverter;
        }


        public decimal Convert(TLink rationalNumber)
        {
            var numerator = (decimal)RawNumberSequenceToBigIntegerConverter.Convert(_links.GetSource(rationalNumber));
            var denominator = (decimal)RawNumberSequenceToBigIntegerConverter.Convert(_links.GetTarget(rationalNumber));
            return (decimal)(numerator / denominator);
        }
    }
}
