using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using TLinkAddress = System.UInt64;

#pragma warning disable CA1822 // Mark members as static

namespace Platform.Data.Doublets.Benchmarks
{
    /// <summary>
    /// Benchmarks comparing Split vs United memory implementation approaches for the same algorithms.
    /// This benchmark compares performance characteristics between split and united memory architectures.
    /// </summary>
    [SimpleJob]
    [MemoryDiagnoser]
    public class MemoryImplementationsBenchmarks
    {
        private ILinks<TLinkAddress> _splitMemoryLinks;
        private ILinks<TLinkAddress> _unitedMemoryLinks;
        private HeapResizableDirectMemory _splitDataMemory;
        private HeapResizableDirectMemory _splitIndexMemory;
        private HeapResizableDirectMemory _unitedMemory;

        [Params(100, 1000, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            // Setup Split Memory implementation
            _splitDataMemory = new HeapResizableDirectMemory();
            _splitIndexMemory = new HeapResizableDirectMemory();
            _splitMemoryLinks = new SplitMemoryLinks<TLinkAddress>(_splitDataMemory, _splitIndexMemory, 
                SplitMemoryLinks<TLinkAddress>.DefaultLinksSizeStep)
                .DecorateWithAutomaticUniquenessAndUsagesResolution();

            // Setup United Memory implementation  
            _unitedMemory = new HeapResizableDirectMemory();
            _unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(_unitedMemory, UnitedMemoryLinks<TLinkAddress>.DefaultLinksSizeStep,
                Platform.Singletons.Default<LinksConstants<TLinkAddress>>.Instance, IndexTreeType.SizeBalancedTree)
                .DecorateWithAutomaticUniquenessAndUsagesResolution();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _splitDataMemory?.Dispose();
            _splitIndexMemory?.Dispose();
            _unitedMemory?.Dispose();
        }

        [Benchmark]
        public TLinkAddress SplitMemory_CreateLinks()
        {
            var links = _splitMemoryLinks;
            var firstLink = links.CreatePoint();
            for (int i = 0; i < N; i++)
            {
                var link = links.Create();
                links.Update(link, firstLink, link);
            }
            return firstLink;
        }

        [Benchmark]
        public TLinkAddress UnitedMemory_CreateLinks()
        {
            var links = _unitedMemoryLinks;
            var firstLink = links.CreatePoint();
            for (int i = 0; i < N; i++)
            {
                var link = links.Create();
                links.Update(link, firstLink, link);
            }
            return firstLink;
        }

        [Benchmark]
        public TLinkAddress SplitMemory_SearchLinks()
        {
            var links = _splitMemoryLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            return links.Count(query);
        }

        [Benchmark]
        public TLinkAddress UnitedMemory_SearchLinks()
        {
            var links = _unitedMemoryLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            return links.Count(query);
        }

        [Benchmark]
        public void SplitMemory_UpdateLinks()
        {
            var links = _splitMemoryLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            links.Each(link =>
            {
                if (link != null && link.Count >= 3)
                {
                    links.Update(link[0], link[1], link[2]);
                }
                return links.Constants.Continue;
            }, query);
        }

        [Benchmark]
        public void UnitedMemory_UpdateLinks()
        {
            var links = _unitedMemoryLinks;
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            links.Each(link =>
            {
                if (link != null && link.Count >= 3)
                {
                    links.Update(link[0], link[1], link[2]);
                }
                return links.Constants.Continue;
            }, query);
        }

        [Benchmark]
        public void SplitMemory_DeleteLinks()
        {
            var links = _splitMemoryLinks;
            var linksToDelete = new List<TLinkAddress>();
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            
            // Collect first 10 links to delete
            var count = 0;
            links.Each(link =>
            {
                if (link != null && count < 10)
                {
                    linksToDelete.Add(link[0]);
                    count++;
                }
                return count < 10 ? links.Constants.Continue : links.Constants.Break;
            }, query);

            // Delete collected links
            foreach (var linkToDelete in linksToDelete)
            {
                if (links.Exists(linkToDelete))
                {
                    links.Delete(linkToDelete);
                }
            }
        }

        [Benchmark]
        public void UnitedMemory_DeleteLinks()
        {
            var links = _unitedMemoryLinks;
            var linksToDelete = new List<TLinkAddress>();
            var any = links.Constants.Any;
            var query = new Link<TLinkAddress>(any, 1UL, any);
            
            // Collect first 10 links to delete
            var count = 0;
            links.Each(link =>
            {
                if (link != null && count < 10)
                {
                    linksToDelete.Add(link[0]);
                    count++;
                }
                return count < 10 ? links.Constants.Continue : links.Constants.Break;
            }, query);

            // Delete collected links
            foreach (var linkToDelete in linksToDelete)
            {
                if (links.Exists(linkToDelete))
                {
                    links.Delete(linkToDelete);
                }
            }
        }
    }
}