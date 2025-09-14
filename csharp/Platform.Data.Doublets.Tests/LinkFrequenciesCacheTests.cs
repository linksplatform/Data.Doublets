using System;
using System.IO;
using System.Linq;
using Xunit;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Moq;
using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Tests
{
    public class LinkFrequenciesCacheTests
    {
        private readonly Mock<ILinks<ulong>> _mockLinks;
        private readonly Mock<ICounter<ulong, ulong>> _mockCounter;
        private readonly LinkFrequenciesCache<ulong> _cache;

        public LinkFrequenciesCacheTests()
        {
            _mockLinks = new Mock<ILinks<ulong>>();
            _mockCounter = new Mock<ICounter<ulong, ulong>>();
            _cache = new LinkFrequenciesCache<ulong>(_mockLinks.Object, _mockCounter.Object);
        }

        [Fact]
        public void IncrementFrequency_NewDoublet_CreatesNewEntry()
        {
            // Arrange
            ulong source = 1;
            ulong target = 2;
            ulong expectedLink = 10;

            _mockLinks.Setup(x => x.SearchOrDefault(source, target)).Returns(expectedLink);
            _mockCounter.Setup(x => x.Count(expectedLink)).Returns(5UL);

            // Act
            var result = _cache.IncrementFrequency(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6UL, result.Frequency); // 1 + 5 from counter
            Assert.Equal(expectedLink, result.Link);
        }

        [Fact]
        public void IncrementFrequency_ExistingDoublet_IncrementsFrequency()
        {
            // Arrange
            ulong source = 1;
            ulong target = 2;
            ulong expectedLink = 10;

            _mockLinks.Setup(x => x.SearchOrDefault(source, target)).Returns(expectedLink);
            _mockCounter.Setup(x => x.Count(expectedLink)).Returns(5UL);

            // Act - increment twice
            _cache.IncrementFrequency(source, target);
            var result = _cache.IncrementFrequency(source, target);

            // Assert
            Assert.Equal(7UL, result.Frequency); // 1 + 5 + 1
        }

        [Fact]
        public void GetFrequency_ExistingDoublet_ReturnsCorrectFrequency()
        {
            // Arrange
            ulong source = 1;
            ulong target = 2;
            ulong expectedLink = 10;

            _mockLinks.Setup(x => x.SearchOrDefault(source, target)).Returns(expectedLink);
            _mockCounter.Setup(x => x.Count(expectedLink)).Returns(5UL);

            _cache.IncrementFrequency(source, target);

            // Act
            var result = _cache.GetFrequency(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6UL, result.Frequency);
            Assert.Equal(expectedLink, result.Link);
        }

        [Fact]
        public void GetFrequency_NonExistingDoublet_ReturnsNull()
        {
            // Act
            var result = _cache.GetFrequency(999, 888);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void IncrementFrequencies_Sequence_IncrementsAllPairs()
        {
            // Arrange
            var sequence = new List<ulong> { 1, 2, 3, 4 };
            
            _mockLinks.Setup(x => x.SearchOrDefault(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(0UL);
            _mockCounter.Setup(x => x.Count(It.IsAny<ulong>())).Returns(0UL);

            // Act
            _cache.IncrementFrequencies(sequence);

            // Assert
            Assert.Equal(3, _cache.Count); // (1,2), (2,3), (3,4)
            
            var freq12 = _cache.GetFrequency(1, 2);
            var freq23 = _cache.GetFrequency(2, 3);
            var freq34 = _cache.GetFrequency(3, 4);
            
            Assert.NotNull(freq12);
            Assert.NotNull(freq23);
            Assert.NotNull(freq34);
            Assert.Equal(1UL, freq12.Frequency);
            Assert.Equal(1UL, freq23.Frequency);
            Assert.Equal(1UL, freq34.Frequency);
        }

        [Fact]
        public void SerializeToJson_WithData_ReturnsValidJson()
        {
            // Arrange
            _mockLinks.Setup(x => x.SearchOrDefault(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(0UL);
            _mockCounter.Setup(x => x.Count(It.IsAny<ulong>())).Returns(0UL);

            _cache.IncrementFrequency(1, 2);
            _cache.IncrementFrequency(2, 3);

            // Act
            var json = _cache.SerializeToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("1,2", json);
            Assert.Contains("2,3", json);
            Assert.Contains("Frequency", json);
            Assert.Contains("Link", json);
        }

        [Fact]
        public void DeserializeFromJson_ValidJson_RestoresCache()
        {
            // Arrange
            var json = @"{
                ""1,2"": {
                    ""Frequency"": ""5"",
                    ""Link"": ""10""
                },
                ""2,3"": {
                    ""Frequency"": ""3"",
                    ""Link"": ""20""
                }
            }";

            // Act
            _cache.DeserializeFromJson(json);

            // Assert
            Assert.Equal(2, _cache.Count);
            
            var freq12 = _cache.GetFrequency(1, 2);
            var freq23 = _cache.GetFrequency(2, 3);
            
            Assert.NotNull(freq12);
            Assert.NotNull(freq23);
            Assert.Equal(5UL, freq12.Frequency);
            Assert.Equal(10UL, freq12.Link);
            Assert.Equal(3UL, freq23.Frequency);
            Assert.Equal(20UL, freq23.Link);
        }

        [Fact]
        public void SerializeToFile_AndDeserializeFromFile_RoundTrip()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            
            _mockLinks.Setup(x => x.SearchOrDefault(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(0UL);
            _mockCounter.Setup(x => x.Count(It.IsAny<ulong>())).Returns(0UL);

            _cache.IncrementFrequency(1, 2);
            _cache.IncrementFrequency(2, 3);
            var originalCount = _cache.Count;

            try
            {
                // Act
                _cache.SerializeToFile(tempFile);
                _cache.Clear();
                Assert.Equal(0, _cache.Count);
                
                _cache.DeserializeFromFile(tempFile);

                // Assert
                Assert.Equal(originalCount, _cache.Count);
                
                var freq12 = _cache.GetFrequency(1, 2);
                var freq23 = _cache.GetFrequency(2, 3);
                
                Assert.NotNull(freq12);
                Assert.NotNull(freq23);
                Assert.Equal(1UL, freq12.Frequency);
                Assert.Equal(1UL, freq23.Frequency);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void DumpToLinksStorage_WithCachedData_CreatesLinksInStorage()
        {
            // Arrange
            _mockLinks.Setup(x => x.SearchOrDefault(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(0UL);
            _mockLinks.Setup(x => x.GetOrCreate<ulong>(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns((ulong source, ulong target) => source * 10 + target);
            _mockCounter.Setup(x => x.Count(It.IsAny<ulong>())).Returns(0UL);

            _cache.IncrementFrequency(1, 2);
            _cache.IncrementFrequency(2, 3);

            // Act
            var createdCount = _cache.DumpToLinksStorage();

            // Assert
            Assert.Equal(2, createdCount);
            _mockLinks.Verify(x => x.GetOrCreate<ulong>(1, 2), Times.Once);
            _mockLinks.Verify(x => x.GetOrCreate<ulong>(2, 3), Times.Once);
            
            var freq12 = _cache.GetFrequency(1, 2);
            var freq23 = _cache.GetFrequency(2, 3);
            
            Assert.Equal(12UL, freq12.Link); // 1 * 10 + 2
            Assert.Equal(23UL, freq23.Link); // 2 * 10 + 3
        }

        [Fact]
        public void Clear_WithData_RemovesAllEntries()
        {
            // Arrange
            _mockLinks.Setup(x => x.SearchOrDefault(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(0UL);
            _mockCounter.Setup(x => x.Count(It.IsAny<ulong>())).Returns(0UL);

            _cache.IncrementFrequency(1, 2);
            _cache.IncrementFrequency(2, 3);
            Assert.Equal(2, _cache.Count);

            // Act
            _cache.Clear();

            // Assert
            Assert.Equal(0, _cache.Count);
        }

        [Fact]
        public void GetAllEntries_WithData_ReturnsAllCachedEntries()
        {
            // Arrange
            _mockLinks.Setup(x => x.SearchOrDefault(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(0UL);
            _mockCounter.Setup(x => x.Count(It.IsAny<ulong>())).Returns(0UL);

            _cache.IncrementFrequency(1, 2);
            _cache.IncrementFrequency(2, 3);

            // Act
            var entries = _cache.GetAllEntries();

            // Assert
            Assert.Equal(2, entries.Count());
        }
    }

    public class LinkFrequencyTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            // Act
            var frequency = new LinkFrequency<ulong>(5UL, 10UL);

            // Assert
            Assert.Equal(5UL, frequency.Frequency);
            Assert.Equal(10UL, frequency.Link);
        }

        [Fact]
        public void IncrementFrequency_IncrementsValue()
        {
            // Arrange
            var frequency = new LinkFrequency<ulong>(5UL, 10UL);

            // Act
            frequency.IncrementFrequency();

            // Assert
            Assert.Equal(6UL, frequency.Frequency);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var frequency = new LinkFrequency<ulong>(5UL, 10UL);

            // Act
            var result = frequency.ToString();

            // Assert
            Assert.Equal("Link: 10, Frequency: 5", result);
        }
    }

    public class DefaultCounterTests
    {
        [Fact]
        public void Count_AlwaysReturnsZero()
        {
            // Arrange
            var counter = DefaultCounter<string, ulong>.Instance;

            // Act & Assert
            Assert.Equal(0UL, counter.Count("test"));
            Assert.Equal(0UL, counter.Count("another"));
            Assert.Equal(0UL, counter.Count("null"));
        }
    }
}