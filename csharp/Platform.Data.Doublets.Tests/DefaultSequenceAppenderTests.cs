using System.Collections.Generic;
using Platform.Collections.Stacks;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Sequences.HeightProviders;
using Platform.Data.Numbers.Raw;
using Platform.Interfaces;
using Platform.Memory;
using Platform.Numbers;
using Xunit;
using Xunit.Abstractions;
using TLink = System.UInt64;

namespace Platform.Data.Doublets.Tests
{
    public class DefaultSequenceAppenderTests
    {
        private readonly ITestOutputHelper _output;

        public DefaultSequenceAppenderTests(ITestOutputHelper output)
        {
            _output = output;
        }
        public static ILinks<TLink> CreateLinks() => CreateLinks<TLink>(new IO.TemporaryFile());

        public static ILinks<TLink> CreateLinks<TLink>(string dataDBFilename)
        {
            var linksConstants = new LinksConstants<TLink>(enableExternalReferencesSupport: true);
            return new UnitedMemoryLinks<TLink>(new FileMappedResizableDirectMemory(dataDBFilename), UnitedMemoryLinks<TLink>.DefaultLinksSizeStep, linksConstants, IndexTreeType.Default);
        }
        
        public class ValueCriterionMatcher<TLink> : ICriterionMatcher<TLink>
        {
            public readonly ILinks<TLink> Links;
            public readonly TLink Marker;
            public ValueCriterionMatcher(ILinks<TLink> links, TLink marker)
            {
                Links = links;
                Marker = marker;
            }

            public bool IsMatched(TLink link) => EqualityComparer<TLink>.Default.Equals(Links.GetSource(link), Marker);
        }

        [Fact]
        public void AppendArrayBug()
        {
            ILinks<TLink> links = CreateLinks();
            TLink zero = default;
            var markerIndex = Arithmetic.Increment(zero);
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            var sequence = links.Create();
            sequence = links.Update(sequence, meaningRoot, sequence);
            var appendant = links.Create();
            appendant = links.Update(appendant, meaningRoot, appendant);
            ValueCriterionMatcher<TLink> valueCriterionMatcher = new(links, meaningRoot);
            DefaultSequenceRightHeightProvider<ulong> defaultSequenceRightHeightProvider = new(links, valueCriterionMatcher);
            DefaultSequenceAppender<TLink> defaultSequenceAppender = new(links, new DefaultStack<ulong>(), defaultSequenceRightHeightProvider);
            var newArray = defaultSequenceAppender.Append(sequence, appendant);
            var output = links.FormatStructure(newArray, link => link.IsFullPoint(), true);
            Assert.Equal("(4:(2:1 2) (3:1 3))", output);
        }
    }
}
