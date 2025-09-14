using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using TLinkAddress = System.UInt64;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    /// <summary>
    /// Benchmarks comparing different tree implementation algorithms for the same operations.
    /// This benchmark compares SizeBalancedTree, RecursionlessSizeBalancedTree, and SizedAndThreadedAVLBalancedTree implementations.
    /// </summary>
    [SimpleJob]
    [MemoryDiagnoser]
    public class TreeImplementationsBenchmarks
    {
        private ILinks<TLinkAddress> _sizeBalancedTreeLinks;
        private ILinks<TLinkAddress> _recursionlessSizeBalancedTreeLinks; 
        private ILinks<TLinkAddress> _avlBalancedTreeLinks;
        private HeapResizableDirectMemory _sbtMemory;
        private HeapResizableDirectMemory _rsbtMemory;
        private HeapResizableDirectMemory _avlMemory;

        [Params(100, 1000, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            // Setup Size Balanced Tree implementation
            _sbtMemory = new HeapResizableDirectMemory();
            _sizeBalancedTreeLinks = new UnitedMemoryLinks<TLinkAddress>(_sbtMemory, UnitedMemoryLinks<TLinkAddress>.DefaultLinksSizeStep, 
                Platform.Singletons.Default<LinksConstants<TLinkAddress>>.Instance, IndexTreeType.SizeBalancedTree)
                .DecorateWithAutomaticUniquenessAndUsagesResolution();

            // Setup Recursionless Size Balanced Tree implementation  
            _rsbtMemory = new HeapResizableDirectMemory();
            _recursionlessSizeBalancedTreeLinks = new UnitedMemoryLinks<TLinkAddress>(_rsbtMemory, UnitedMemoryLinks<TLinkAddress>.DefaultLinksSizeStep,
                Platform.Singletons.Default<LinksConstants<TLinkAddress>>.Instance, IndexTreeType.RecursionlessSizeBalancedTree)
                .DecorateWithAutomaticUniquenessAndUsagesResolution();

            // Setup AVL Balanced Tree implementation
            _avlMemory = new HeapResizableDirectMemory();
            _avlBalancedTreeLinks = new UnitedMemoryLinks<TLinkAddress>(_avlMemory, UnitedMemoryLinks<TLinkAddress>.DefaultLinksSizeStep,
                Platform.Singletons.Default<LinksConstants<TLinkAddress>>.Instance, IndexTreeType.SizedAndThreadedAVLBalancedTree)
                .DecorateWithAutomaticUniquenessAndUsagesResolution();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _sbtMemory?.Dispose();
            _rsbtMemory?.Dispose();
            _avlMemory?.Dispose();
        }

        [Benchmark]
        public TLinkAddress SizeBalancedTree_CreateLinks()
        {
            var links = _sizeBalancedTreeLinks;
            var firstLink = links.CreatePoint();
            for (int i = 0; i < N; i++)
            {
                var link = links.Create();
                links.Update(link, firstLink, link);
            }
            return firstLink;
        }

        [Benchmark]
        public TLinkAddress RecursionlessSizeBalancedTree_CreateLinks()
        {
            var links = _recursionlessSizeBalancedTreeLinks;
            var firstLink = links.CreatePoint();
            for (int i = 0; i < N; i++)
            {
                var link = links.Create();
                links.Update(link, firstLink, link);
            }
            return firstLink;
        }

        [Benchmark]
        public TLinkAddress AVLBalancedTree_CreateLinks()
        {
            var links = _avlBalancedTreeLinks;
            var firstLink = links.CreatePoint();
            for (int i = 0; i < N; i++)
            {
                var link = links.Create();
                links.Update(link, firstLink, link);
            }
            return firstLink;
        }

        [Benchmark]
        public TLinkAddress SizeBalancedTree_SearchLinks()
        {
            var links = _sizeBalancedTreeLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            return links.Count(query);
        }

        [Benchmark]
        public TLinkAddress RecursionlessSizeBalancedTree_SearchLinks()
        {
            var links = _recursionlessSizeBalancedTreeLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            return links.Count(query);
        }

        [Benchmark]
        public TLinkAddress AVLBalancedTree_SearchLinks()
        {
            var links = _avlBalancedTreeLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            return links.Count(query);
        }

        [Benchmark]
        public IList<IList<TLinkAddress>?> SizeBalancedTree_EachLinks()
        {
            var links = _sizeBalancedTreeLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            var results = new List<IList<TLinkAddress>?>();
            links.Each(linkResult => { results.Add(linkResult); return links.Constants.Continue; }, query);
            return results;
        }

        [Benchmark]
        public IList<IList<TLinkAddress>?> RecursionlessSizeBalancedTree_EachLinks()
        {
            var links = _recursionlessSizeBalancedTreeLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            var results = new List<IList<TLinkAddress>?>();
            links.Each(linkResult => { results.Add(linkResult); return links.Constants.Continue; }, query);
            return results;
        }

        [Benchmark]
        public IList<IList<TLinkAddress>?> AVLBalancedTree_EachLinks()
        {
            var links = _avlBalancedTreeLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            var results = new List<IList<TLinkAddress>?>();
            links.Each(linkResult => { results.Add(linkResult); return links.Constants.Continue; }, query);
            return results;
        }
    }
}