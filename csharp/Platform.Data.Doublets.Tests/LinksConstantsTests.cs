using Xunit;
using Platform.Ranges;

namespace Platform.Data.Doublets.Tests
{
    public static class LinksConstantsTests
    {
        // TODO: Replace with Range.PositiveInt64 when available in Platform.Ranges
        // This represents the range of positive 64-bit integers: [1, long.MaxValue]
        private static readonly Range<ulong> PositiveInt64 = new Range<ulong>(1UL, (ulong)long.MaxValue);

        [Fact]
        public static void ExternalReferencesTest()
        {
            LinksConstants<ulong> constants = new LinksConstants<ulong>(PositiveInt64, new Range<ulong>((ulong)long.MaxValue + 1UL, ulong.MaxValue));

            //var minimum = new Hybrid<ulong>(0, isExternal: true);
            var minimum = new Hybrid<ulong>(1, isExternal: true);
            var maximum = new Hybrid<ulong>(long.MaxValue, isExternal: true);

            Assert.True(constants.IsExternalReference(minimum));
            Assert.True(constants.IsExternalReference(maximum));
        }
    }
}
