using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;
using Platform.Numbers;
using Platform.Converters;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Converters;

/// <summary>
/// <para>
/// Represents a converter that transforms unary numbers to address add operations without caching.
/// This implementation creates links on-demand only, without precreation.
/// </para>
/// <para></para>
/// </summary>
/// <typeparam name="TLinkAddress">
/// <para>The link address type.</para>
/// <para></para>
/// </typeparam>
public class UnaryNumberToAddressAddOperationConverter<TLinkAddress> : IConverter<TLinkAddress>
    where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{
    /// <summary>
    /// <para>
    /// The links storage.
    /// </para>
    /// <para></para>
    /// </summary>
    protected readonly ILinks<TLinkAddress> _links;

    /// <summary>
    /// <para>
    /// The constants.
    /// </para>
    /// <para></para>
    /// </summary>
    protected readonly LinksConstants<TLinkAddress> _constants;

    /// <summary>
    /// <para>
    /// Initializes a new <see cref="UnaryNumberToAddressAddOperationConverter{TLinkAddress}"/> instance.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <param name="links">
    /// <para>The links storage.</para>
    /// <para></para>
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnaryNumberToAddressAddOperationConverter(ILinks<TLinkAddress> links)
    {
        _links = links;
        _constants = links.Constants;
    }

    /// <summary>
    /// <para>
    /// Converts a unary number to its address representation without using a cache.
    /// Creates links on-demand only.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <param name="unaryNumber">
    /// <para>The unary number representation to convert.</para>
    /// <para></para>
    /// </param>
    /// <returns>
    /// <para>The address representation of the unary number.</para>
    /// <para></para>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TLinkAddress Convert(TLinkAddress unaryNumber)
    {
        // Handle null/default case
        if (unaryNumber == _constants.Null)
        {
            return TLinkAddress.Zero;
        }

        // Create the unary "one" (self-referencing link) on-demand
        var unaryOne = _links.GetOrCreate(_constants.Itself, _constants.Itself);
        if (unaryNumber == unaryOne)
        {
            return TLinkAddress.One;
        }

        // Get the source and target of the unary number
        var source = _links.GetSource(unaryNumber);
        var target = _links.GetTarget(unaryNumber);

        // If source equals target, this is a simple power of two case
        if (source == target)
        {
            // Calculate the power of two value on-demand instead of using cache
            return CalculatePowerOfTwoValue(unaryNumber, unaryOne);
        }
        else
        {
            // More complex case: accumulate result by traversing the structure
            var result = CalculatePowerOfTwoValue(source, unaryOne);
            
            // Traverse the target structure and accumulate values
            while (target != unaryOne && target != _constants.Null)
            {
                // Check if target is a power of two (as mentioned in the issue)
                if (!Platform.Numbers.Math.IsPowerOfTwo(ulong.CreateTruncating(target)))
                {
                    throw new InvalidOperationException($"Target {target} is not a power of two.");
                }
                
                source = _links.GetSource(target);
                result = result + CalculatePowerOfTwoValue(source, unaryOne);
                target = _links.GetTarget(target);
            }
            
            return result;
        }
    }

    /// <summary>
    /// <para>
    /// Calculates the power of two value for a unary number without using cache.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <param name="unary">
    /// <para>The unary number.</para>
    /// <para></para>
    /// </param>
    /// <param name="unaryOne">
    /// <para>The unary "one" value.</para>
    /// <para></para>
    /// </param>
    /// <returns>
    /// <para>The power of two value.</para>
    /// <para></para>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TLinkAddress CalculatePowerOfTwoValue(TLinkAddress unary, TLinkAddress unaryOne)
    {
        if (unary == unaryOne)
        {
            return TLinkAddress.One;
        }

        // Count the depth to determine the power of two
        var depth = 0;
        var current = unary;
        
        while (current != unaryOne && current != _constants.Null)
        {
            var source = _links.GetSource(current);
            var target = _links.GetTarget(current);
            
            if (source == target) // This means it's a power-of-two link
            {
                depth++;
                current = source;
            }
            else
            {
                break;
            }
        }
        
        // Calculate 2^depth
        var result = TLinkAddress.One;
        for (int i = 0; i < depth; i++)
        {
            result += result; // Double the value (multiply by 2)
        }
        
        return result;
    }

}