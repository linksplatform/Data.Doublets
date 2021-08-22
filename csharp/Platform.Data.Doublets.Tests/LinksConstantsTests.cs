using Xunit;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// <para>
    /// Represents the links constants tests.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class LinksConstantsTests
    {
        /// <summary>
        /// <para>
        /// Tests that external references test.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public static void ExternalReferencesTest()
        {
            LinksConstants<ulong> constants = new LinksConstants<ulong>((1, long.MaxValue), (long.MaxValue + 1UL, ulong.MaxValue));

            //var minimum = new Hybrid<ulong>(0, isExternal: true);
            var minimum = new Hybrid<ulong>(1, isExternal: true);
            var maximum = new Hybrid<ulong>(long.MaxValue, isExternal: true);

            Assert.True(constants.IsExternalReference(minimum));
            Assert.True(constants.IsExternalReference(maximum));
        }
    }
}
