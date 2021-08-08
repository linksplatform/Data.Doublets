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
            int dotPosition = decimalAsString.IndexOf('.');
            string decimalWithoutDots = decimalAsString;
            if (dotPosition == -1)
            {
                dotPosition = 0;
            }
            else
            {
                decimalWithoutDots = decimalWithoutDots.Remove(dotPosition, 1);
            }
            BigInteger numerator = BigInteger.Parse(decimalWithoutDots);
            var numeratorLink = BigIntegerToRawNumberSequenceConverter.Convert(numerator);
            var dotPositionLink = BigIntegerToRawNumberSequenceConverter.AddressToNumberConverter.Convert(UncheckedConverter.Convert(dotPosition));
            return _links.GetOrCreate(numeratorLink, dotPositionLink);
        }

        /* public TLink Convert(decimal @decimal)
        {
            var decimalAsString = @decimal.ToString();
            var isDigitAfterDot = false;
            int digitsAfterDot = 0;
            StringBuilder stringBuilder = new(decimalAsString.Length);
            foreach (var @char in decimalAsString)
            {
                if (@char == '.')
                {
                    isDigitAfterDot = true;
                    continue;
                }
                if (isDigitAfterDot)
                {
                    digitsAfterDot++;
                }
                stringBuilder.Append(@char);
            }
            var decimalWithoutDots = stringBuilder.ToString();
            BigInteger denominator = new(System.Math.Pow(10, digitsAfterDot));
            BigInteger numerator = BigInteger.Parse(decimalWithoutDots);
            BigInteger greatestCommonDivisor;
            do
            {
                greatestCommonDivisor = BigInteger.GreatestCommonDivisor(numerator, denominator);
                numerator /= greatestCommonDivisor;
                denominator /= greatestCommonDivisor;
            }
            while (greatestCommonDivisor != 1);
            var numeratorLink = BigIntegerToRawNumberSequenceConverter.Convert(numerator);
            var denominatorLink = BigIntegerToRawNumberSequenceConverter.Convert(denominator);
            return _links.GetOrCreate(numeratorLink, denominatorLink);
        } */
    }
}
