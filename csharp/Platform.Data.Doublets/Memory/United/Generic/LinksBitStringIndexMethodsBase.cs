using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.United.Generic
{
    /// <summary>
    ///     <para>
    ///         Represents the links bitstring index methods base.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <seealso cref="IBitStringTreeMethods{TLinkAddress}" />
    public abstract unsafe class LinksBitStringIndexMethodsBase<TLinkAddress> : IBitStringTreeMethods<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        /// <summary>
        ///     <para>
        ///         The break.
        ///     </para>
        ///     <para></para>
        /// </summary>
        protected readonly TLinkAddress Break;
        
        /// <summary>
        ///     <para>
        ///         The continue.
        ///     </para>
        ///     <para></para>
        /// </summary>
        protected readonly TLinkAddress Continue;
        
        /// <summary>
        ///     <para>
        ///         The header.
        ///     </para>
        ///     <para></para>
        /// </summary>
        protected readonly byte* Header;
        
        /// <summary>
        ///     <para>
        ///         The links.
        ///     </para>
        ///     <para></para>
        /// </summary>
        protected readonly byte* Links;

        /// <summary>
        ///     <para>
        ///         The bitstring storage dictionary.
        ///     </para>
        ///     <para></para>
        /// </summary>
        protected readonly Dictionary<TLinkAddress, BitArray> BitStringStorage;

        /// <summary>
        ///     <para>
        ///         The default bitstring size.
        ///     </para>
        ///     <para></para>
        /// </summary>
        protected readonly int DefaultBitStringSize;

        /// <summary>
        ///     <para>
        ///         Initializes a new <see cref="LinksBitStringIndexMethodsBase" /> instance.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="constants">
        ///     <para>A constants.</para>
        ///     <para></para>
        /// </param>
        /// <param name="links">
        ///     <para>A links.</para>
        ///     <para></para>
        /// </param>
        /// <param name="header">
        ///     <para>A header.</para>
        ///     <para></para>
        /// </param>
        /// <param name="defaultBitStringSize">
        ///     <para>The default bitstring size.</para>
        ///     <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected LinksBitStringIndexMethodsBase(LinksConstants<TLinkAddress> constants, byte* links, byte* header, int defaultBitStringSize = 1024)
        {
            Links = links;
            Header = header;
            Break = constants.Break;
            Continue = constants.Continue;
            BitStringStorage = new Dictionary<TLinkAddress, BitArray>();
            DefaultBitStringSize = defaultBitStringSize;
        }

        /// <summary>
        ///     <para>
        ///         Gets the link data at the specified index.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="link">
        ///     <para>The link index.</para>
        ///     <para></para>
        /// </param>
        /// <param name="index">
        ///     <para>The data index.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The link data.</para>
        ///     <para></para>
        /// </returns>
        public TLinkAddress this[TLinkAddress link, TLinkAddress index]
        {
            get
            {
                var linkPointer = (TLinkAddress*)(Links + (long)(ulong.CreateTruncating(link) * (ulong)sizeof(RawLink<TLinkAddress>)));
                return Add(ref AsRef<TLinkAddress>(linkPointer), int.CreateTruncating(index));
            }
            set
            {
                var linkPointer = (TLinkAddress*)(Links + (long)(ulong.CreateTruncating(link) * (ulong)sizeof(RawLink<TLinkAddress>)));
                Add(ref AsRef<TLinkAddress>(linkPointer), int.CreateTruncating(index)) = value;
            }
        }

        /// <summary>
        ///     <para>
        ///         Counts the usages using the specified root.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="root">
        ///     <para>The root.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The link</para>
        ///     <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual TLinkAddress CountUsages(TLinkAddress root)
        {
            if (!BitStringStorage.TryGetValue(root, out var bitString))
            {
                return TLinkAddress.Zero;
            }
            
            var count = 0;
            for (var i = 0; i < bitString.Length; i++)
            {
                if (bitString[i])
                {
                    count++;
                }
            }
            return TLinkAddress.CreateTruncating(count);
        }

        /// <summary>
        ///     <para>
        ///         Searches for a link with the specified source and target.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="source">
        ///     <para>The source.</para>
        ///     <para></para>
        /// </param>
        /// <param name="target">
        ///     <para>The target.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The link</para>
        ///     <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual TLinkAddress Search(TLinkAddress source, TLinkAddress target)
        {
            if (!BitStringStorage.TryGetValue(source, out var sourceBits) || 
                !BitStringStorage.TryGetValue(target, out var targetBits))
            {
                return TLinkAddress.Zero;
            }

            var intersection = new BitArray(sourceBits).And(targetBits);
            
            for (var i = 0; i < intersection.Length; i++)
            {
                if (intersection[i])
                {
                    return TLinkAddress.CreateTruncating(i + 1);
                }
            }
            
            return TLinkAddress.Zero;
        }

        /// <summary>
        ///     <para>
        ///         Executes the handler for each usage.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="root">
        ///     <para>The root.</para>
        ///     <para></para>
        /// </param>
        /// <param name="handler">
        ///     <para>The handler.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The link</para>
        ///     <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual TLinkAddress EachUsage(TLinkAddress root, ReadHandler<TLinkAddress>? handler)
        {
            if (!BitStringStorage.TryGetValue(root, out var bitString) || handler == null)
            {
                return Continue;
            }

            for (var i = 0; i < bitString.Length; i++)
            {
                if (bitString[i])
                {
                    var link = TLinkAddress.CreateTruncating(i + 1);
                    var result = handler(new TLinkAddress[] { link, root, root });
                    if (result == Break)
                    {
                        return Break;
                    }
                }
            }
            
            return Continue;
        }

        /// <summary>
        ///     <para>
        ///         Detaches the root.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="root">
        ///     <para>The root.</para>
        ///     <para></para>
        /// </param>
        /// <param name="linkIndex">
        ///     <para>The link index.</para>
        ///     <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Detach(ref TLinkAddress root, TLinkAddress linkIndex)
        {
            if (!BitStringStorage.TryGetValue(root, out var bitString))
            {
                return;
            }

            var position = int.CreateTruncating(linkIndex) - 1;
            if (position >= 0 && position < bitString.Length)
            {
                bitString[position] = false;
            }
        }

        /// <summary>
        ///     <para>
        ///         Attaches the root.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="root">
        ///     <para>The root.</para>
        ///     <para></para>
        /// </param>
        /// <param name="linkIndex">
        ///     <para>The link index.</para>
        ///     <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Attach(ref TLinkAddress root, TLinkAddress linkIndex)
        {
            if (!BitStringStorage.TryGetValue(root, out var bitString))
            {
                bitString = new BitArray(DefaultBitStringSize);
                BitStringStorage[root] = bitString;
            }

            var position = int.CreateTruncating(linkIndex) - 1;
            if (position >= bitString.Length)
            {
                var newSize = Math.Max(position + 1, bitString.Length * 2);
                var newBitString = new BitArray(newSize);
                
                for (var i = 0; i < bitString.Length; i++)
                {
                    newBitString[i] = bitString[i];
                }
                
                bitString = newBitString;
                BitStringStorage[root] = bitString;
            }

            if (position >= 0)
            {
                bitString[position] = true;
            }
        }

        /// <summary>
        ///     <para>
        ///         Gets the bitstring for the specified key.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="key">
        ///     <para>The key.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The bitstring as BitArray.</para>
        ///     <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual BitArray GetBitString(TLinkAddress key)
        {
            if (BitStringStorage.TryGetValue(key, out var bitString))
            {
                return new BitArray(bitString);
            }
            return new BitArray(DefaultBitStringSize);
        }

        /// <summary>
        ///     <para>
        ///         Sets the bit at the specified position in the bitstring for the given key.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="key">
        ///     <para>The key.</para>
        ///     <para></para>
        /// </param>
        /// <param name="position">
        ///     <para>The bit position.</para>
        ///     <para></para>
        /// </param>
        /// <param name="value">
        ///     <para>The bit value.</para>
        ///     <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetBit(TLinkAddress key, int position, bool value)
        {
            if (!BitStringStorage.TryGetValue(key, out var bitString))
            {
                bitString = new BitArray(Math.Max(position + 1, DefaultBitStringSize));
                BitStringStorage[key] = bitString;
            }
            else if (position >= bitString.Length)
            {
                var newSize = Math.Max(position + 1, bitString.Length * 2);
                var newBitString = new BitArray(newSize);
                
                for (var i = 0; i < bitString.Length; i++)
                {
                    newBitString[i] = bitString[i];
                }
                
                bitString = newBitString;
                BitStringStorage[key] = bitString;
            }

            bitString[position] = value;
        }

        /// <summary>
        ///     <para>
        ///         Gets the bit at the specified position in the bitstring for the given key.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="key">
        ///     <para>The key.</para>
        ///     <para></para>
        /// </param>
        /// <param name="position">
        ///     <para>The bit position.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The bit value.</para>
        ///     <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool GetBit(TLinkAddress key, int position)
        {
            if (!BitStringStorage.TryGetValue(key, out var bitString) || position >= bitString.Length || position < 0)
            {
                return false;
            }
            return bitString[position];
        }

        /// <summary>
        ///     <para>
        ///         Performs a bitwise AND operation between two bitstrings.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="key1">
        ///     <para>The first key.</para>
        ///     <para></para>
        /// </param>
        /// <param name="key2">
        ///     <para>The second key.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The result bitstring.</para>
        ///     <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual BitArray BitwiseAnd(TLinkAddress key1, TLinkAddress key2)
        {
            var bits1 = GetBitString(key1);
            var bits2 = GetBitString(key2);
            return bits1.And(bits2);
        }

        /// <summary>
        ///     <para>
        ///         Performs a bitwise OR operation between two bitstrings.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="key1">
        ///     <para>The first key.</para>
        ///     <para></para>
        /// </param>
        /// <param name="key2">
        ///     <para>The second key.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The result bitstring.</para>
        ///     <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual BitArray BitwiseOr(TLinkAddress key1, TLinkAddress key2)
        {
            var bits1 = GetBitString(key1);
            var bits2 = GetBitString(key2);
            return bits1.Or(bits2);
        }

        /// <summary>
        ///     <para>
        ///         Performs a bitwise XOR operation between two bitstrings.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="key1">
        ///     <para>The first key.</para>
        ///     <para></para>
        /// </param>
        /// <param name="key2">
        ///     <para>The second key.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The result bitstring.</para>
        ///     <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual BitArray BitwiseXor(TLinkAddress key1, TLinkAddress key2)
        {
            var bits1 = GetBitString(key1);
            var bits2 = GetBitString(key2);
            return bits1.Xor(bits2);
        }

        /// <summary>
        ///     <para>
        ///         Counts the number of set bits in the bitstring for the given key.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="key">
        ///     <para>The key.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The number of set bits.</para>
        ///     <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual int CountSetBits(TLinkAddress key)
        {
            if (!BitStringStorage.TryGetValue(key, out var bitString))
            {
                return 0;
            }
            
            var count = 0;
            for (var i = 0; i < bitString.Length; i++)
            {
                if (bitString[i])
                {
                    count++;
                }
            }
            return count;
        }
    }
}