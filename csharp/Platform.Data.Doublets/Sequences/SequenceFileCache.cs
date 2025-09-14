using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences
{
    /// <summary>
    /// <para>
    /// Represents a file-based cache for sequences with expiration support.
    /// </para>
    /// <para>
    /// This file should be created each time the sequence that does not have such file is read.
    /// We also can store a file with read sequence for a cache duration (1 sec or greater).
    /// </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class SequenceFileCache<TLinkAddress> : IDisposable where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly string _cacheDirectory;
        private readonly TimeSpan _cacheDuration;
        private readonly ConcurrentDictionary<TLinkAddress, string> _addressToFilePathCache;
        private readonly Timer _cleanupTimer;
        private readonly object _disposeLock = new();
        private bool _disposed;

        /// <summary>
        /// Gets the cache directory path.
        /// </summary>
        public string CacheDirectory => _cacheDirectory;

        /// <summary>
        /// Gets the cache duration.
        /// </summary>
        public TimeSpan CacheDuration => _cacheDuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceFileCache{TLinkAddress}"/> class.
        /// </summary>
        /// <param name="cacheDirectory">The directory to store cache files.</param>
        /// <param name="cacheDuration">The duration to keep cache files. Default is 1 second.</param>
        public SequenceFileCache(string cacheDirectory = null, TimeSpan? cacheDuration = null)
        {
            _cacheDirectory = cacheDirectory ?? Path.Combine(Path.GetTempPath(), "DoubletsSequenceCache");
            _cacheDuration = cacheDuration ?? TimeSpan.FromSeconds(1);
            _addressToFilePathCache = new ConcurrentDictionary<TLinkAddress, string>();

            // Ensure cache directory exists
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }

            // Start cleanup timer to remove expired files every cache duration
            _cleanupTimer = new Timer(CleanupExpiredFiles, null, _cacheDuration, _cacheDuration);
        }

        /// <summary>
        /// Tries to get a cached sequence from file.
        /// </summary>
        /// <param name="address">The sequence address.</param>
        /// <param name="sequence">The cached sequence if found and not expired.</param>
        /// <returns>True if the sequence was found and is still valid, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSequence(TLinkAddress address, out TLinkAddress[]? sequence)
        {
            sequence = null;
            
            if (_disposed)
            {
                return false;
            }

            var filePath = GetCacheFilePath(address);
            
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                var fileInfo = new FileInfo(filePath);
                if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc > _cacheDuration)
                {
                    // File is expired, remove it
                    File.Delete(filePath);
                    _addressToFilePathCache.TryRemove(address, out _);
                    return false;
                }

                var json = File.ReadAllText(filePath);
                var cacheEntry = JsonSerializer.Deserialize<SequenceCacheEntry>(json);
                
                if (cacheEntry?.Elements != null)
                {
                    sequence = cacheEntry.Elements.Select(ConvertFromString).ToArray();
                    return true;
                }
            }
            catch (Exception)
            {
                // If there's any error reading the file, consider it as cache miss
                // and try to remove the corrupted file
                try
                {
                    File.Delete(filePath);
                    _addressToFilePathCache.TryRemove(address, out _);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            return false;
        }

        /// <summary>
        /// Caches a sequence to file asynchronously.
        /// </summary>
        /// <param name="address">The sequence address.</param>
        /// <param name="sequence">The sequence elements.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task CacheSequenceAsync(TLinkAddress address, TLinkAddress[] sequence)
        {
            if (_disposed || sequence == null || sequence.Length == 0)
            {
                return;
            }

            try
            {
                var filePath = GetCacheFilePath(address);
                var cacheEntry = new SequenceCacheEntry
                {
                    Address = ConvertToString(address),
                    Elements = sequence.Select(ConvertToString).ToArray(),
                    CreatedAt = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(cacheEntry, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
                _addressToFilePathCache.TryAdd(address, filePath);
            }
            catch (Exception)
            {
                // Ignore caching errors - the system should work without file cache
            }
        }

        /// <summary>
        /// Caches a sequence to file synchronously.
        /// </summary>
        /// <param name="address">The sequence address.</param>
        /// <param name="sequence">The sequence elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CacheSequence(TLinkAddress address, TLinkAddress[] sequence)
        {
            if (_disposed || sequence == null || sequence.Length == 0)
            {
                return;
            }

            try
            {
                var filePath = GetCacheFilePath(address);
                var cacheEntry = new SequenceCacheEntry
                {
                    Address = ConvertToString(address),
                    Elements = sequence.Select(ConvertToString).ToArray(),
                    CreatedAt = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(cacheEntry, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                File.WriteAllText(filePath, json);
                _addressToFilePathCache.TryAdd(address, filePath);
            }
            catch (Exception)
            {
                // Ignore caching errors - the system should work without file cache
            }
        }

        /// <summary>
        /// Removes a cached sequence file.
        /// </summary>
        /// <param name="address">The sequence address.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSequence(TLinkAddress address)
        {
            if (_disposed)
            {
                return;
            }

            var filePath = GetCacheFilePath(address);
            
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                _addressToFilePathCache.TryRemove(address, out _);
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Clears all cached files.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (Directory.Exists(_cacheDirectory))
                {
                    var files = Directory.GetFiles(_cacheDirectory, "seq_*.json");
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception)
                        {
                            // Ignore individual file deletion errors
                        }
                    }
                }
                _addressToFilePathCache.Clear();
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Gets the cache file path for a given address.
        /// </summary>
        /// <param name="address">The sequence address.</param>
        /// <returns>The file path for caching the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetCacheFilePath(TLinkAddress address)
        {
            if (_addressToFilePathCache.TryGetValue(address, out var cachedPath))
            {
                return cachedPath;
            }

            var fileName = $"seq_{ConvertToString(address)}.json";
            return Path.Combine(_cacheDirectory, fileName);
        }

        /// <summary>
        /// Cleanup timer callback to remove expired files.
        /// </summary>
        /// <param name="state">Timer state (unused).</param>
        private void CleanupExpiredFiles(object? state)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (!Directory.Exists(_cacheDirectory))
                {
                    return;
                }

                var files = Directory.GetFiles(_cacheDirectory, "seq_*.json");
                var now = DateTime.UtcNow;

                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (now - fileInfo.LastWriteTimeUtc > _cacheDuration)
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore individual file cleanup errors
                    }
                }
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Converts TLinkAddress to string for serialization.
        /// </summary>
        /// <param name="address">The address to convert.</param>
        /// <returns>String representation of the address.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ConvertToString(TLinkAddress address)
        {
            return address.ToString() ?? "";
        }

        /// <summary>
        /// Converts string back to TLinkAddress for deserialization.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The converted address.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TLinkAddress ConvertFromString(string value)
        {
            return TLinkAddress.Parse(value, null);
        }

        /// <summary>
        /// Disposes the file cache and cleanup timer.
        /// </summary>
        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _cleanupTimer?.Dispose();
                }
            }
        }

        /// <summary>
        /// Represents a cache entry for serialization.
        /// </summary>
        private class SequenceCacheEntry
        {
            public string Address { get; set; } = "";
            public string[] Elements { get; set; } = Array.Empty<string>();
            public DateTime CreatedAt { get; set; }
        }
    }
}