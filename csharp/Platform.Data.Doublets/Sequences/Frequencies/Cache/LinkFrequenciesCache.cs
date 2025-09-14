using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.IO;
using System.Text.Json;
using System.Linq;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences.Frequencies.Cache
{
    /// <remarks>
    /// Can be used to operate with many CompressingConverters (to keep global frequencies data between them).
    /// TODO: Extract interface to implement frequencies storage inside Links storage
    /// </remarks>
    public class LinkFrequenciesCache<TLink> : LinksOperatorBase<TLink> where TLink : IUnsignedNumber<TLink>
    {
        private static readonly EqualityComparer<TLink> _equalityComparer = EqualityComparer<TLink>.Default;
        private static readonly Comparer<TLink> _comparer = Comparer<TLink>.Default;

        private readonly Dictionary<Doublet<TLink>, LinkFrequency<TLink>> _doubletsCache;
        private readonly ICounter<TLink, TLink> _frequencyCounter;

        public LinkFrequenciesCache(ILinks<TLink> links, ICounter<TLink, TLink> frequencyCounter)
            : base(links)
        {
            _doubletsCache = new Dictionary<Doublet<TLink>, LinkFrequency<TLink>>(4096, DoubletComparer<TLink>.Default);
            _frequencyCounter = frequencyCounter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkFrequency<TLink> GetFrequency(TLink source, TLink target)
        {
            var doublet = new Doublet<TLink>(source, target);
            return GetFrequency(ref doublet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkFrequency<TLink> GetFrequency(ref Doublet<TLink> doublet)
        {
            _doubletsCache.TryGetValue(doublet, out LinkFrequency<TLink> data);
            return data;
        }

        public void IncrementFrequencies(IList<TLink> sequence)
        {
            for (var i = 1; i < sequence.Count; i++)
            {
                IncrementFrequency(sequence[i - 1], sequence[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkFrequency<TLink> IncrementFrequency(TLink source, TLink target)
        {
            var doublet = new Doublet<TLink>(source, target);
            return IncrementFrequency(ref doublet);
        }

        public void PrintFrequencies(IList<TLink> sequence)
        {
            for (var i = 1; i < sequence.Count; i++)
            {
                PrintFrequency(sequence[i - 1], sequence[i]);
            }
        }

        public void PrintFrequency(TLink source, TLink target)
        {
            var frequency = GetFrequency(source, target);
            var number = frequency != null ? frequency.Frequency : TLink.Zero;
            Console.WriteLine("({0},{1}) - {2}", source, target, number);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkFrequency<TLink> IncrementFrequency(ref Doublet<TLink> doublet)
        {
            if (_doubletsCache.TryGetValue(doublet, out LinkFrequency<TLink> data))
            {
                data.IncrementFrequency();
            }
            else
            {
                var link = Links.SearchOrDefault(doublet.Source, doublet.Target);
                data = new LinkFrequency<TLink>(TLink.One, link);
                if (!_equalityComparer.Equals(link, default))
                {
                    data.Frequency = data.Frequency + _frequencyCounter.Count(link);
                }
                _doubletsCache.Add(doublet, data);
            }
            return data;
        }

        public void ValidateFrequencies()
        {
            foreach (var entry in _doubletsCache)
            {
                var value = entry.Value;
                var linkIndex = value.Link;
                if (!_equalityComparer.Equals(linkIndex, default))
                {
                    var frequency = value.Frequency;
                    var count = _frequencyCounter.Count(linkIndex);
                    // TODO: Why `frequency` always greater than `count` by 1?
                    if (((_comparer.Compare(frequency, count) > 0) && (_comparer.Compare(frequency - count, TLink.One) > 0))
                     || ((_comparer.Compare(count, frequency) > 0) && (_comparer.Compare(count - frequency, TLink.One) > 0)))
                    {
                        throw new InvalidOperationException("Frequencies validation failed.");
                    }
                }
            }
        }

        /// <summary>
        /// Serializes the cache data to JSON format.
        /// </summary>
        /// <returns>JSON string representation of the cache.</returns>
        public string SerializeToJson()
        {
            var cacheData = _doubletsCache.ToDictionary(
                kvp => $"{kvp.Key.Source},{kvp.Key.Target}",
                kvp => new { Frequency = kvp.Value.Frequency.ToString(), Link = kvp.Value.Link.ToString() }
            );

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(cacheData, options);
        }

        /// <summary>
        /// Serializes the cache data to a file.
        /// </summary>
        /// <param name="filePath">The path where to save the serialized data.</param>
        public void SerializeToFile(string filePath)
        {
            var json = SerializeToJson();
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Deserializes cache data from JSON format.
        /// </summary>
        /// <param name="json">JSON string containing the cache data.</param>
        public void DeserializeFromJson(string json)
        {
            _doubletsCache.Clear();

            var cacheData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            if (cacheData != null)
            {
                foreach (var kvp in cacheData)
                {
                    var parts = kvp.Key.Split(',');
                    if (parts.Length == 2 && 
                        TLink.TryParse(parts[0], null, out TLink source) && 
                        TLink.TryParse(parts[1], null, out TLink target))
                    {
                        var frequencyStr = kvp.Value.GetProperty("Frequency").GetString();
                        var linkStr = kvp.Value.GetProperty("Link").GetString();
                        
                        if (frequencyStr != null && linkStr != null &&
                            TLink.TryParse(frequencyStr, null, out TLink frequency) && 
                            TLink.TryParse(linkStr, null, out TLink link))
                        {
                            var doublet = new Doublet<TLink>(source, target);
                            var linkFrequency = new LinkFrequency<TLink>(frequency, link);
                            _doubletsCache[doublet] = linkFrequency;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deserializes cache data from a file.
        /// </summary>
        /// <param name="filePath">The path to the file containing serialized data.</param>
        public void DeserializeFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            DeserializeFromJson(json);
        }

        /// <summary>
        /// Dumps the cache data into the Links storage by creating actual links.
        /// </summary>
        /// <returns>Number of links created in the storage.</returns>
        public int DumpToLinksStorage()
        {
            int createdLinks = 0;
            
            foreach (var entry in _doubletsCache)
            {
                var doublet = entry.Key;
                var frequency = entry.Value;
                
                // Only create links that don't already exist
                if (_equalityComparer.Equals(frequency.Link, default))
                {
                    // Create the link in storage
                    var newLinkId = Links.GetOrCreate<TLink>(doublet.Source, doublet.Target);
                    frequency.Link = newLinkId;
                    createdLinks++;
                }
            }
            
            return createdLinks;
        }

        /// <summary>
        /// Gets the total number of cached frequency entries.
        /// </summary>
        public int Count => _doubletsCache.Count;

        /// <summary>
        /// Gets all cached doublets and their frequencies.
        /// </summary>
        /// <returns>Enumerable of all cached entries.</returns>
        public IEnumerable<KeyValuePair<Doublet<TLink>, LinkFrequency<TLink>>> GetAllEntries()
        {
            return _doubletsCache.AsEnumerable();
        }

        /// <summary>
        /// Clears all cached frequency data.
        /// </summary>
        public void Clear()
        {
            _doubletsCache.Clear();
        }
    }
}