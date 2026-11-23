using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Platform.Data.Doublets.Examples
{
    /// <summary>
    /// Implementation using System.Collections.BitArray for tracking deleted links
    /// </summary>
    /// <typeparam name="TLinkAddress">The link address type</typeparam>
    public class BitArrayDeletedLinksTracker<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IConvertible
    {
        private BitArray _deletedLinks;
        private readonly object _lock = new object();

        public BitArrayDeletedLinksTracker(int initialCapacity = 1024)
        {
            _deletedLinks = new BitArray(initialCapacity);
        }

        public void MarkAsDeleted(TLinkAddress link)
        {
            var index = Convert.ToInt32(link);
            lock (_lock)
            {
                EnsureCapacity(index + 1);
                _deletedLinks[index] = true;
            }
        }

        public void MarkAsUndeleted(TLinkAddress link)
        {
            var index = Convert.ToInt32(link);
            lock (_lock)
            {
                if (index < _deletedLinks.Length)
                    _deletedLinks[index] = false;
            }
        }

        public bool IsDeleted(TLinkAddress link)
        {
            var index = Convert.ToInt32(link);
            lock (_lock)
            {
                return index < _deletedLinks.Length && _deletedLinks[index];
            }
        }

        public (bool found, TLinkAddress value) GetFirstDeleted()
        {
            lock (_lock)
            {
                for (int i = 0; i < _deletedLinks.Length; i++)
                {
                    if (_deletedLinks[i])
                        return (true, TLinkAddress.CreateTruncating(i));
                }
                return (false, default(TLinkAddress));
            }
        }

        public int CountDeleted()
        {
            lock (_lock)
            {
                int count = 0;
                for (int i = 0; i < _deletedLinks.Length; i++)
                {
                    if (_deletedLinks[i]) count++;
                }
                return count;
            }
        }

        private void EnsureCapacity(int requiredCapacity)
        {
            if (_deletedLinks.Length < requiredCapacity)
            {
                var newSize = Math.Max(requiredCapacity, _deletedLinks.Length * 2);
                _deletedLinks.Length = newSize;
            }
        }
    }

    /// <summary>
    /// High-performance implementation using custom bitstring approach with ulong arrays
    /// </summary>
    /// <typeparam name="TLinkAddress">The link address type</typeparam>
    public unsafe class CustomBitstringDeletedLinksTracker<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IConvertible
    {
        private ulong[] _bits;
        private int _capacity;
        private readonly object _lock = new object();
        private const int BitsPerUlong = 64;

        public CustomBitstringDeletedLinksTracker(int initialCapacity = 1024)
        {
            _capacity = ((initialCapacity + BitsPerUlong - 1) / BitsPerUlong) * BitsPerUlong;
            _bits = new ulong[_capacity / BitsPerUlong];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkAsDeleted(TLinkAddress link)
        {
            var index = Convert.ToInt32(link);
            lock (_lock)
            {
                EnsureCapacity(index + 1);
                var wordIndex = index / BitsPerUlong;
                var bitIndex = index % BitsPerUlong;
                _bits[wordIndex] |= (1UL << bitIndex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkAsUndeleted(TLinkAddress link)
        {
            var index = Convert.ToInt32(link);
            lock (_lock)
            {
                if (index < _capacity)
                {
                    var wordIndex = index / BitsPerUlong;
                    var bitIndex = index % BitsPerUlong;
                    _bits[wordIndex] &= ~(1UL << bitIndex);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDeleted(TLinkAddress link)
        {
            var index = Convert.ToInt32(link);
            lock (_lock)
            {
                if (index >= _capacity) return false;
                var wordIndex = index / BitsPerUlong;
                var bitIndex = index % BitsPerUlong;
                return (_bits[wordIndex] & (1UL << bitIndex)) != 0;
            }
        }

        public (bool found, TLinkAddress value) GetFirstDeleted()
        {
            lock (_lock)
            {
                for (int wordIndex = 0; wordIndex < _bits.Length; wordIndex++)
                {
                    if (_bits[wordIndex] != 0)
                    {
                        // Find first set bit using bit manipulation
                        var word = _bits[wordIndex];
                        var trailingZeros = BitOperations.TrailingZeroCount(word);
                        return (true, TLinkAddress.CreateTruncating(wordIndex * BitsPerUlong + trailingZeros));
                    }
                }
                return (false, default(TLinkAddress));
            }
        }

        public int CountDeleted()
        {
            lock (_lock)
            {
                int count = 0;
                for (int i = 0; i < _bits.Length; i++)
                {
                    count += BitOperations.PopCount(_bits[i]);
                }
                return count;
            }
        }

        private void EnsureCapacity(int requiredCapacity)
        {
            if (_capacity < requiredCapacity)
            {
                var newCapacity = Math.Max(requiredCapacity, _capacity * 2);
                newCapacity = ((newCapacity + BitsPerUlong - 1) / BitsPerUlong) * BitsPerUlong;
                Array.Resize(ref _bits, newCapacity / BitsPerUlong);
                _capacity = newCapacity;
            }
        }
    }

    /// <summary>
    /// Traditional linked list approach (simulation of current implementation)
    /// </summary>
    /// <typeparam name="TLinkAddress">The link address type</typeparam>
    public class LinkedListDeletedLinksTracker<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IConvertible
    {
        private readonly HashSet<TLinkAddress> _deletedLinks = new();
        private readonly LinkedList<TLinkAddress> _deletedLinksList = new();
        private readonly object _lock = new object();

        public void MarkAsDeleted(TLinkAddress link)
        {
            lock (_lock)
            {
                if (_deletedLinks.Add(link))
                    _deletedLinksList.AddFirst(link);
            }
        }

        public void MarkAsUndeleted(TLinkAddress link)
        {
            lock (_lock)
            {
                if (_deletedLinks.Remove(link))
                    _deletedLinksList.Remove(link);
            }
        }

        public bool IsDeleted(TLinkAddress link)
        {
            lock (_lock)
            {
                return _deletedLinks.Contains(link);
            }
        }

        public (bool found, TLinkAddress value) GetFirstDeleted()
        {
            lock (_lock)
            {
                var first = _deletedLinksList.First;
                return first != null ? (true, first.Value) : (false, default(TLinkAddress));
            }
        }

        public int CountDeleted()
        {
            lock (_lock)
            {
                return _deletedLinks.Count;
            }
        }
    }

    /// <summary>
    /// Performance comparison between different deleted links tracking approaches
    /// </summary>
    public class DeletedLinksTrackingBenchmark
    {
        public static void RunComparison()
        {
            const int linkCount = 100_000;
            const int operations = 50_000;
            var random = new Random(42);

            Console.WriteLine("Deleted Links Tracking Performance Comparison");
            Console.WriteLine("=" + new string('=', 50));

            // Test BitArray approach
            var bitArrayTracker = new BitArrayDeletedLinksTracker<uint>();
            var bitArrayTime = MeasurePerformance("BitArray", bitArrayTracker, linkCount, operations, random);

            // Test custom bitstring approach
            var customBitstringTracker = new CustomBitstringDeletedLinksTracker<uint>();
            var customBitstringTime = MeasurePerformance("Custom Bitstring", customBitstringTracker, linkCount, operations, random);

            // Test linked list approach (current implementation simulation)
            var linkedListTracker = new LinkedListDeletedLinksTracker<uint>();
            var linkedListTime = MeasurePerformance("Linked List", linkedListTracker, linkCount, operations, random);

            // Results
            Console.WriteLine("\nResults Summary:");
            Console.WriteLine("=" + new string('=', 50));
            Console.WriteLine($"BitArray:         {bitArrayTime.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Custom Bitstring: {customBitstringTime.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Linked List:      {linkedListTime.TotalMilliseconds:F2} ms");

            // Speed comparison
            var fastest = Math.Min(Math.Min(bitArrayTime.TotalMilliseconds, customBitstringTime.TotalMilliseconds), linkedListTime.TotalMilliseconds);
            Console.WriteLine("\nSpeed Comparison (vs fastest):");
            Console.WriteLine($"BitArray:         {(bitArrayTime.TotalMilliseconds / fastest):F2}x");
            Console.WriteLine($"Custom Bitstring: {(customBitstringTime.TotalMilliseconds / fastest):F2}x");
            Console.WriteLine($"Linked List:      {(linkedListTime.TotalMilliseconds / fastest):F2}x");
        }

        private static TimeSpan MeasurePerformance<T>(string name, T tracker, int linkCount, int operations, Random random) where T : class
        {
            Console.WriteLine($"\nTesting {name}...");

            var sw = Stopwatch.StartNew();

            // Mark half as deleted
            for (uint i = 0; i < linkCount / 2; i++)
            {
                if (tracker is BitArrayDeletedLinksTracker<uint> ba)
                    ba.MarkAsDeleted(i);
                else if (tracker is CustomBitstringDeletedLinksTracker<uint> cb)
                    cb.MarkAsDeleted(i);
                else if (tracker is LinkedListDeletedLinksTracker<uint> ll)
                    ll.MarkAsDeleted(i);
            }

            // Perform random operations
            for (int op = 0; op < operations; op++)
            {
                var link = (uint)random.Next(0, linkCount);
                var operation = random.Next(0, 4);

                switch (operation)
                {
                    case 0: // Mark as deleted
                        if (tracker is BitArrayDeletedLinksTracker<uint> ba1)
                            ba1.MarkAsDeleted(link);
                        else if (tracker is CustomBitstringDeletedLinksTracker<uint> cb1)
                            cb1.MarkAsDeleted(link);
                        else if (tracker is LinkedListDeletedLinksTracker<uint> ll1)
                            ll1.MarkAsDeleted(link);
                        break;
                    case 1: // Mark as undeleted
                        if (tracker is BitArrayDeletedLinksTracker<uint> ba2)
                            ba2.MarkAsUndeleted(link);
                        else if (tracker is CustomBitstringDeletedLinksTracker<uint> cb2)
                            cb2.MarkAsUndeleted(link);
                        else if (tracker is LinkedListDeletedLinksTracker<uint> ll2)
                            ll2.MarkAsUndeleted(link);
                        break;
                    case 2: // Check if deleted
                        if (tracker is BitArrayDeletedLinksTracker<uint> ba3)
                            ba3.IsDeleted(link);
                        else if (tracker is CustomBitstringDeletedLinksTracker<uint> cb3)
                            cb3.IsDeleted(link);
                        else if (tracker is LinkedListDeletedLinksTracker<uint> ll3)
                            ll3.IsDeleted(link);
                        break;
                    case 3: // Get first deleted
                        if (tracker is BitArrayDeletedLinksTracker<uint> ba4)
                            ba4.GetFirstDeleted();
                        else if (tracker is CustomBitstringDeletedLinksTracker<uint> cb4)
                            cb4.GetFirstDeleted();
                        else if (tracker is LinkedListDeletedLinksTracker<uint> ll4)
                            ll4.GetFirstDeleted();
                        break;
                }
            }

            sw.Stop();
            Console.WriteLine($"Completed in {sw.ElapsedMilliseconds} ms");
            return sw.Elapsed;
        }
    }
}