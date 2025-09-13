using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class LinkTests
    {
        [Fact]
        public static void ImplicitConversionToTupleTest()
        {
            var link = new Link<ulong>(1, 2, 3);
            
            (ulong, ulong, ulong) tuple = link;
            
            Assert.Equal(1UL, tuple.Item1);
            Assert.Equal(2UL, tuple.Item2);
            Assert.Equal(3UL, tuple.Item3);
        }

        [Fact]
        public static void ImplicitConversionFromTupleTest()
        {
            var tuple = (1UL, 2UL, 3UL);
            
            Link<ulong> link = tuple;
            
            Assert.Equal(1UL, link.Index);
            Assert.Equal(2UL, link.Source);
            Assert.Equal(3UL, link.Target);
        }

        [Fact]
        public static void TupleRoundTripConversionTest()
        {
            var originalLink = new Link<ulong>(10, 20, 30);
            
            (ulong, ulong, ulong) tuple = originalLink;
            Link<ulong> convertedBackLink = tuple;
            
            Assert.Equal(originalLink.Index, convertedBackLink.Index);
            Assert.Equal(originalLink.Source, convertedBackLink.Source);
            Assert.Equal(originalLink.Target, convertedBackLink.Target);
            Assert.Equal(originalLink, convertedBackLink);
        }

        [Fact]
        public static void TupleConversionWithDifferentNumericTypesTest()
        {
            var intLink = new Link<uint>(1, 2, 3);
            
            (uint, uint, uint) intTuple = intLink;
            
            Assert.Equal(1U, intTuple.Item1);
            Assert.Equal(2U, intTuple.Item2);
            Assert.Equal(3U, intTuple.Item3);
        }
    }
}