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
            public readonly TLink ValueMarker;
            public ValueCriterionMatcher(ILinks<TLink> links, TLink valueMarker)
            {
                Links = links;
                ValueMarker = valueMarker;
            }

            public bool IsMatched(TLink link) => EqualityComparer<TLink>.Default.Equals(Links.GetSource(link), ValueMarker);
        }

        [Fact]
        public void AppendArrayBug()
        {
            ILinks<TLink> links = CreateLinks();
            TLink zero = default;
            var one = Arithmetic.Increment(zero);
            var markerIndex = one;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            var valueMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(ref markerIndex));
            var numberMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(ref markerIndex));
            var arrayMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(ref markerIndex));
            AddressToRawNumberConverter<TLink> addressToNumberConverter = new();
            var numberOneAddress = addressToNumberConverter.Convert(1);
            var numberOneLink = links.GetOrCreate(numberMarker, numberOneAddress);
            var array = links.GetOrCreate(arrayMarker, numberOneLink);
            var arrayValue = links.GetOrCreate(valueMarker, array);
            var numberTwoAddress = addressToNumberConverter.Convert(2);
            var numberTwoLink = links.GetOrCreate(numberMarker, numberTwoAddress);
            var appendant = links.GetOrCreate(valueMarker, numberTwoLink);
            var equalityComparer = EqualityComparer<TLink>.Default;
            ValueCriterionMatcher<TLink> valueCriterionMatcher = new(links, valueMarker);
            DefaultSequenceRightHeightProvider<ulong> defaultSequenceRightHeightProvider = new(links, valueCriterionMatcher);
            DefaultSequenceAppender<TLink> defaultSequenceAppender = new(links, new DefaultStack<ulong>(), defaultSequenceRightHeightProvider);
            var newArray = defaultSequenceAppender.Append(arrayValue, appendant);
            var output = links.FormatStructure(newArray, link => link.IsFullPoint(), true);
        }
    }
}
