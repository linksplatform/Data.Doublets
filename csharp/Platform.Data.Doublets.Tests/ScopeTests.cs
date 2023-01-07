using Xunit;
using Platform.Scopes;
using Platform.Memory;
using Platform.Data.Doublets.Decorators;
using Platform.Reflection;
using Platform.Data.Doublets.Memory.United.Generic;
using TLinkAddress = System.UInt64;

namespace Platform.Data.Doublets.Tests
{
    public static class ScopeTests
    {
        [Fact]
        public static void SingleDependencyTest()
        {
            using (var scope = new Scope())
            {
                scope.IncludeAssemblyOf<IMemory>();
                var instance = scope.Use<IDirectMemory>();
                Assert.IsType<HeapResizableDirectMemory>(instance);
            }
        }

        [Fact]
        public static void CascadeDependencyTest()
        {
            using (var scope = new Scope())
            {
                scope.Include<TemporaryFileMappedResizableDirectMemory>();
                scope.Include<UnitedMemoryLinks<TLinkAddress>>();
                var instance = scope.Use<ILinks<TLinkAddress>>();
                Assert.IsType<UnitedMemoryLinks<TLinkAddress>>(instance);
            }
        }

        [Fact(Skip = "Would be fixed later.")]
        public static void FullAutoResolutionTest()
        {
            using (var scope = new Scope(autoInclude: true, autoExplore: true))
            {
                var instance = scope.Use<CombinedDecorator<TLinkAddress>>();
                Assert.IsType<TLinkAddress>(instance);
            }
        }

        [Fact]
        public static void TypeParametersTest()
        {
            using (var scope = new Scope<Types<HeapResizableDirectMemory, UnitedMemoryLinks<TLinkAddress>>>())
            {
                var links = scope.Use<ILinks<TLinkAddress>>();
                Assert.IsType<UnitedMemoryLinks<TLinkAddress>>(links);
            }
        }
    }
}
