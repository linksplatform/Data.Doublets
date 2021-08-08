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
    public class DecimalToRationalConverter<TLink> : LinksDecoratorBase<TLink>, IConverter<decimal, TLink>
    where TLink: struct
    {
        public readonly BigIntegerToRawNumberSequenceConverter<TLink> BigIntegerToRawNumberSequenceConverter;
        public readonly UncheckedConverter<int, TLink> UncheckedConverter = UncheckedConverter<int, TLink>.Default;

        public DecimalToRationalConverter(ILinks<TLink> links, BigIntegerToRawNumberSequenceConverter<TLink> bigIntegerToRawNumberSequenceConverter) : base(links)
        {
            BigIntegerToRawNumberSequenceConverter = bigIntegerToRawNumberSequenceConverter;
        }

        public TLink Convert(decimal @decimal)
        {
            var decimalAsString = @decimal.ToString();
            var dotPosition = decimalAsString.IndexOf('.');
            var decimalWithoutDots = decimalAsString;
            int digitsAfterDot = 0;
            if (dotPosition != -1)
            {
                decimalWithoutDots = decimalWithoutDots.Remove(dotPosition, 1);
                digitsAfterDot = decimalAsString.Length - 1 - dotPosition;
            }
            BigInteger denominator = new(System.Math.Pow(10, digitsAfterDot));
            BigInteger numerator = BigInteger.Parse(decimalWithoutDots);
            BigInteger greatestCommonDivisor = new(0);
            while (greatestCommonDivisor != 1)
            {
                greatestCommonDivisor = BigInteger.GreatestCommonDivisor(numerator, denominator);
                numerator /= greatestCommonDivisor;
                denominator /= greatestCommonDivisor;
            }
            var numeratorLink = BigIntegerToRawNumberSequenceConverter.Convert(numerator);
            var denominatorLink = BigIntegerToRawNumberSequenceConverter.Convert(denominator);
            return _links.GetOrCreate(numeratorLink, denominatorLink);
        }
    }
}
