using System.Numerics;
using Platform.Data.Doublets.Numbers.Raw;
using Xunit;

namespace Platform.Data.Doublets.Tests.Numbers.Raw
{
    public class BigIntegerTests
    {
        public void Test()
        {
            BigInteger bigInt = new(1);
            BigIntegerToRawNumberSequenceConverter bigIntegerToRawNumberSequenceConverter = new();
            RawNumberSequenceToBigIntegerConverter rawNumberSequenceToBigIntegerConverter = new();
            var bigIntSequence = bigIntegerToRawNumberSequenceConverter.Convert(bigInt);
            var bigIntFromSequence = rawNumberSequenceToBigIntegerConverter.Convert(bigIntSequence);
            Assert.Equal(bigInt, bigIntFromSequence);
        }
    }
}
