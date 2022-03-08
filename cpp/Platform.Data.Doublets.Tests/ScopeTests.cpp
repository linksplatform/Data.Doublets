namespace Platform::Data::Doublets::Tests
{
    TEST_CLASS(ScopeTests)
    {
        public: TEST_METHOD(SingleDependencyTest)
        {
            using (auto scope = Scope())
            {
                scope.IncludeAssemblyOf<IMemory>();
                auto instance = scope.Use<IDirectMemory>();
                Assert.IsType<HeapResizableDirectMemory>(instance);
            }
        }

        public: TEST_METHOD(CascadeDependencyTest)
        {
            using (auto scope = Scope())
            {
                scope.Include<TemporaryFileMappedResizableDirectMemory>();
                scope.Include<UInt64UnitedMemoryLinks>();
                auto instance = scope.Use<ILinks<std::uint64_t>>();
                Assert.IsType<UInt64UnitedMemoryLinks>(instance);
            }
        }

        public: static void FullAutoResolutionTest()
        {
            using (auto scope = Scope(autoInclude: true, autoExplore: true))
            {
                auto instance = scope.Use<UInt64Links>();
                Assert.IsType<UInt64Links>(instance);
            }
        }

        public: TEST_METHOD(TypeParametersTest)
        {
            using (auto scope = Scope<Types<HeapResizableDirectMemory, UnitedMemoryLinks<std::uint64_t>>>())
            {
                auto storage = scope.Use<ILinks<std::uint64_t>>();
                Assert.IsType<UnitedMemoryLinks<std::uint64_t>>(storage);
            }
        }
    };
}
