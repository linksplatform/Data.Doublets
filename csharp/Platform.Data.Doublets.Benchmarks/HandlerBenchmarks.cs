using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Platform.Delegates;
using TLinkAddress = System.UInt64;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    [SimpleJob]
    [MemoryDiagnoser]
    public class HandlerBenchmarks
    {
        private static ILinks<TLinkAddress> _links;
        private static TLinkAddress _continueConstant;
        private static TLinkAddress _breakConstant;
        private static HeapResizableDirectMemory _dataMemory;
        private static ReadHandler<TLinkAddress>? _nullHandler;
        private static ReadHandler<TLinkAddress> _defaultHandler;
        private static ReadHandler<TLinkAddress> _actualHandler;

        [Params(10000, 100000)]
        public static int N;

        [GlobalSetup]
        public static void Setup()
        {
            _dataMemory = new HeapResizableDirectMemory();
            _links = new UnitedMemoryLinks<TLinkAddress>(_dataMemory).DecorateWithAutomaticUniquenessAndUsagesResolution();
            _continueConstant = _links.Constants.Continue;
            _breakConstant = _links.Constants.Break;
            
            // Setup handlers
            _nullHandler = null;
            _defaultHandler = default;
            _actualHandler = (link) => _continueConstant; // Actual handler that just returns Continue
            
            // Create some test data
            for (int i = 0; i < 1000; i++)
            {
                var link = _links.Create();
                _links.Update(link, (TLinkAddress)(i % 10 + 1), (TLinkAddress)(i % 5 + 1));
            }
        }

        [GlobalCleanup]
        public static void Cleanup() 
        {
            _dataMemory.Dispose();
        }

        // Function that gets the handler and checks if it's null or default
        private static TLinkAddress HandleNullableAndDefault(IList<TLinkAddress>? link, ReadHandler<TLinkAddress>? handler)
        {
            if (handler == null || handler == default)
            {
                return _continueConstant;
            }
            return handler(link);
        }

        // Function that gets the handler and checks if it's null
        private static TLinkAddress HandleNullable(IList<TLinkAddress>? link, ReadHandler<TLinkAddress>? handler)
        {
            if (handler == null)
            {
                return _continueConstant;
            }
            return handler(link);
        }

        // Function that just calls the handler (non-nullable)
        private static TLinkAddress HandleNonNullable(IList<TLinkAddress>? link, ReadHandler<TLinkAddress> handler)
        {
            if (handler == default)
            {
                return _continueConstant;
            }
            return handler(link);
        }

        [Benchmark]
        public void NullableAndDefaultHandlerWithNull()
        {
            var query = new Link<TLinkAddress>(_links.Constants.Any, _links.Constants.Any, _links.Constants.Any);
            for (int i = 0; i < N; i++)
            {
                _links.Each(link => HandleNullableAndDefault(link, _nullHandler), query);
            }
        }

        [Benchmark]
        public void NullableAndDefaultHandlerWithDefault()
        {
            var query = new Link<TLinkAddress>(_links.Constants.Any, _links.Constants.Any, _links.Constants.Any);
            for (int i = 0; i < N; i++)
            {
                _links.Each(link => HandleNullableAndDefault(link, _defaultHandler), query);
            }
        }

        [Benchmark]
        public void NullableHandlerWithNull()
        {
            var query = new Link<TLinkAddress>(_links.Constants.Any, _links.Constants.Any, _links.Constants.Any);
            for (int i = 0; i < N; i++)
            {
                _links.Each(link => HandleNullable(link, _nullHandler), query);
            }
        }

        [Benchmark]
        public void NullableHandlerWithDefault()
        {
            var query = new Link<TLinkAddress>(_links.Constants.Any, _links.Constants.Any, _links.Constants.Any);
            for (int i = 0; i < N; i++)
            {
                _links.Each(link => HandleNullable(link, _defaultHandler), query);
            }
        }

        [Benchmark]
        public void NonNullableHandlerWithDefault()
        {
            var query = new Link<TLinkAddress>(_links.Constants.Any, _links.Constants.Any, _links.Constants.Any);
            for (int i = 0; i < N; i++)
            {
                _links.Each(link => HandleNonNullable(link, _defaultHandler), query);
            }
        }

        [Benchmark]
        public void DirectHandlerCall()
        {
            var query = new Link<TLinkAddress>(_links.Constants.Any, _links.Constants.Any, _links.Constants.Any);
            for (int i = 0; i < N; i++)
            {
                _links.Each(_actualHandler, query);
            }
        }
    }
}