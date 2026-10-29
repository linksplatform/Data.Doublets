namespace Platform::Data::Doublets::Tests
{
    template <typename T>
    struct TrackedHelper : T
    {
        inline static int Instances = 0;

        template <typename... TArgs>
        explicit TrackedHelper(TArgs&&... args) : T(std::forward<TArgs>(args)...)
        {
            ++Instances;
        }

        ~TrackedHelper()
        {
            --Instances;
        }
    };

    TEST(OwnershipTests, UnitedHelpersAreReleasedOnGrowthAndDestruction)
    {
        using namespace Memory::United::Generic;
        using Options = LinksOptions<std::uint64_t>;
        using Sources = TrackedHelper<LinksSourcesSizeBalancedTreeMethods<Options>>;
        using Targets = TrackedHelper<LinksTargetsSizeBalancedTreeMethods<Options>>;
        using Unused = TrackedHelper<UnusedLinksListMethods<Options>>;
        {
            UnitedMemoryLinks<Options, Platform::Memory::HeapResizableDirectMemory, Sources, Targets, Unused> storage{
                Platform::Memory::HeapResizableDirectMemory{}, 512};
            for (int i = 0; i < 128; ++i)
                Create(storage);
            EXPECT_EQ(1, Sources::Instances);
            EXPECT_EQ(1, Targets::Instances);
            EXPECT_EQ(1, Unused::Instances);
        }
        EXPECT_EQ(0, Sources::Instances);
        EXPECT_EQ(0, Targets::Instances);
        EXPECT_EQ(0, Unused::Instances);
    }

    TEST(OwnershipTests, SplitHelpersAreReleasedOnGrowthAndDestruction)
    {
        using namespace Memory::Split::Generic;
        using Options = LinksOptions<std::uint64_t>;
        using Sources = TrackedHelper<InternalLinksSourcesSizeBalancedTreeMethods<Options>>;
        using List = TrackedHelper<InternalLinksSourcesLinkedListMethods<Options>>;
        using Targets = TrackedHelper<InternalLinksTargetsSizeBalancedTreeMethods<Options>>;
        using ExternalSources = TrackedHelper<ExternalLinksSourcesSizeBalancedTreeMethods<Options>>;
        using ExternalTargets = TrackedHelper<ExternalLinksTargetsSizeBalancedTreeMethods<Options>>;
        using Unused = TrackedHelper<UnusedLinksListMethods<Options>>;
        {
            SplitMemoryLinks<Options, Platform::Memory::HeapResizableDirectMemory, Sources, List, Targets,
                             ExternalSources, ExternalTargets, Unused>
                storage{Platform::Memory::HeapResizableDirectMemory{}, Platform::Memory::HeapResizableDirectMemory{},
                        4};
            for (int i = 0; i < 128; ++i)
                Create(storage);
            EXPECT_EQ(1, Sources::Instances);
            EXPECT_EQ(0, List::Instances);
            EXPECT_EQ(1, Targets::Instances);
            EXPECT_EQ(1, ExternalSources::Instances);
            EXPECT_EQ(1, ExternalTargets::Instances);
            EXPECT_EQ(1, Unused::Instances);
        }
        EXPECT_EQ(0, Sources::Instances);
        EXPECT_EQ(0, List::Instances);
        EXPECT_EQ(0, Targets::Instances);
        EXPECT_EQ(0, ExternalSources::Instances);
        EXPECT_EQ(0, ExternalTargets::Instances);
        EXPECT_EQ(0, Unused::Instances);
    }

    TEST(OwnershipTests, SplitGrowthRespectsBothBufferCapacities)
    {
        using Storage = Memory::Split::Generic::SplitMemoryLinks<LinksOptions<std::uint64_t>,
                                                                 Platform::Memory::HeapResizableDirectMemory>;
        const std::pair<std::uint64_t, std::uint64_t> reservations[] = {{4096, 4096}, {8192, 4096}, {4096, 8192}};
        for (auto capacities : reservations)
        {
            Storage storage{Platform::Memory::HeapResizableDirectMemory{capacities.first},
                            Platform::Memory::HeapResizableDirectMemory{capacities.second}, 4};
            for (int i = 0; i < 128; ++i)
            {
                Create(storage);
                EXPECT_LE(storage._dataMemory.UsedCapacity(), storage._dataMemory.ReservedCapacity());
                EXPECT_LE(storage._indexMemory.UsedCapacity(), storage._indexMemory.ReservedCapacity());
            }
            EXPECT_EQ(128, Count(storage));
        }
    }
} // namespace Platform::Data::Doublets::Tests
