using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory
{
    /// <summary>
    /// <para>
    /// Defines the bitstring tree methods interface.
    /// </para>
    /// <para></para>
    /// </summary>
    public interface IBitStringTreeMethods<TLinkAddress> : ILinksTreeMethods<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the bitstring for the specified key.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="key">
        /// <para>The key.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bitstring as BitArray.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        BitArray GetBitString(TLinkAddress key);

        /// <summary>
        /// <para>
        /// Sets the bit at the specified position in the bitstring for the given key.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="key">
        /// <para>The key.</para>
        /// <para></para>
        /// </param>
        /// <param name="position">
        /// <para>The bit position.</para>
        /// <para></para>
        /// </param>
        /// <param name="value">
        /// <para>The bit value.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetBit(TLinkAddress key, int position, bool value);

        /// <summary>
        /// <para>
        /// Gets the bit at the specified position in the bitstring for the given key.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="key">
        /// <para>The key.</para>
        /// <para></para>
        /// </param>
        /// <param name="position">
        /// <para>The bit position.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bit value.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool GetBit(TLinkAddress key, int position);

        /// <summary>
        /// <para>
        /// Performs a bitwise AND operation between two bitstrings.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="key1">
        /// <para>The first key.</para>
        /// <para></para>
        /// </param>
        /// <param name="key2">
        /// <para>The second key.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The result bitstring.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        BitArray BitwiseAnd(TLinkAddress key1, TLinkAddress key2);

        /// <summary>
        /// <para>
        /// Performs a bitwise OR operation between two bitstrings.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="key1">
        /// <para>The first key.</para>
        /// <para></para>
        /// </param>
        /// <param name="key2">
        /// <para>The second key.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The result bitstring.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        BitArray BitwiseOr(TLinkAddress key1, TLinkAddress key2);

        /// <summary>
        /// <para>
        /// Performs a bitwise XOR operation between two bitstrings.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="key1">
        /// <para>The first key.</para>
        /// <para></para>
        /// </param>
        /// <param name="key2">
        /// <para>The second key.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The result bitstring.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        BitArray BitwiseXor(TLinkAddress key1, TLinkAddress key2);

        /// <summary>
        /// <para>
        /// Counts the number of set bits in the bitstring for the given key.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="key">
        /// <para>The key.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of set bits.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CountSetBits(TLinkAddress key);
    }
}