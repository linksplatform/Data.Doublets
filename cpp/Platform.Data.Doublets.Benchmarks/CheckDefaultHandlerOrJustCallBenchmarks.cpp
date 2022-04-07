namespace Platform::Data::Doublets::Benchmarks
{
    using TLinkAddress = std::uint64_t;
    constexpr auto DefaultHandler = [](TLinkAddress source, TLinkAddress target){return 1;};

    TLinkAddress CheckDefaultHandlerAndCallHandler(TLinkAddress source, TLinkAddress target, auto&& handler)
    {
        if(DefaultHandler == handler)
        {
            return 1;
        }
        else
        {
            return handler(source, target);
        }
    }

    TLinkAddress CallHandler(TLinkAddress source, TLinkAddress target, auto&& handler)
    {
        return handler(source, target);
    }

    static void BM_CheckDefaultHandlerAndSumWithoutOptimization(benchmark::State& state)
    {
        for(auto _ : state)
        {
            for(auto i = 0; i < state.range(0); ++i)
            {
                benchmark::DoNotOptimize(CheckDefaultHandlerAndCallHandler(TLinkAddress{10}, TLinkAddress{10}, [](TLinkAddress source, TLinkAddress target ){return 1;}));
            }
        }
    }

    static void BM_JustCallHandlerWithoutOptimization(benchmark::State& state)
    {
        for(auto _ : state)
        {
            for(auto i = 0; i < state.range(0); ++i)
            {
                benchmark::DoNotOptimize(CallHandler(TLinkAddress{10}, TLinkAddress{10}, [](TLinkAddress source, TLinkAddress target ){return 1;}));
            }
        }
    }

    static void BM_CheckDefaultHandlerAndSum(benchmark::State& state)
    {
        for(auto _ : state)
        {
            for(auto i = 0; i < state.range(0); ++i)
            {
                CheckDefaultHandlerAndCallHandler(TLinkAddress{10}, TLinkAddress{10}, [](TLinkAddress source, TLinkAddress target ){return 1;});
            }
        }
    }

    static void BM_JustCallHandler(benchmark::State& state)
    {
        for(auto _ : state)
        {
            for(auto i = 0; i < state.range(0); ++i)
            {
                CallHandler(TLinkAddress{10}, TLinkAddress{10}, [](TLinkAddress source, TLinkAddress target ){return 1;});
            }
        }
    }

    BENCHMARK(BM_CheckDefaultHandlerAndSumWithoutOptimization)->Arg(1000000);
    BENCHMARK(BM_JustCallHandlerWithoutOptimization)->Arg(1000000);
    BENCHMARK(BM_CheckDefaultHandlerAndSum)->Arg(1000000);
    BENCHMARK(BM_JustCallHandler)->Arg(1000000);

}
