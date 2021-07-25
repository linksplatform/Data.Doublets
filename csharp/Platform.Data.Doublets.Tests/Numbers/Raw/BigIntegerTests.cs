using System.Numerics;
using Platform.Data.Doublets.Numbers.Raw;
using Xunit;

namespace Platform.Data.Doublets.Tests.Numbers.Raw
{
    public class BigIntegerTests
    {
        public void Test()
        {
            BigInteger bigInt = new(123456789123456789);
            BigIntegerToRawNumberSequenceConverter bigIntegerToRawNumberSequenceConverter = new();
            RawSequenceToBigIntegerConverter rawSequenceToBigIntegerConverter = new();
            var bigIntSequence = bigIntegerToRawNumberSequenceConverter.Convert(bigInt);
            var bigIntFromSequence = rawSequenceToBigIntegerConverter.Convert(bigIntSequence);
            Assert.Equal(bigInt, bigIntFromSequence);
        }
    }
}
