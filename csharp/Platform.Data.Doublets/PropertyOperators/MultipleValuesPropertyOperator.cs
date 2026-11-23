using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.PropertyOperators
{
    /// <summary>
    /// <para>
    /// Represents a property operator that supports multiple values for a single property.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <seealso cref="LinksOperatorBase{TLinkAddress}"/>
    public class MultipleValuesPropertyOperator<TLinkAddress> : LinksOperatorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly TLinkAddress _propertyMarker;
        private readonly TLinkAddress _propertyValueMarker;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="MultipleValuesPropertyOperator"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">
        /// <para>A links.</para>
        /// <para></para>
        /// </param>
        /// <param name="propertyMarker">
        /// <para>A property marker.</para>
        /// <para></para>
        /// </param>
        /// <param name="propertyValueMarker">
        /// <para>A property value marker.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultipleValuesPropertyOperator(ILinks<TLinkAddress> links, TLinkAddress propertyMarker, TLinkAddress propertyValueMarker) : base(links)
        {
            _propertyMarker = propertyMarker;
            _propertyValueMarker = propertyValueMarker;
        }

        /// <summary>
        /// <para>
        /// Gets all values for the specified link property.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A collection of all values for the property</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TLinkAddress> GetAll(TLinkAddress link)
        {
            var property = _links.SearchOrDefault(link, _propertyMarker);
            if (property == default)
            {
                yield break;
            }

            var links = _links;
            var constants = links.Constants;
            var anyConstant = constants.Any;
            var query = new Link<TLinkAddress>(anyConstant, property, anyConstant);

            var values = new List<TLinkAddress>();
            links.Each(candidate =>
            {
                var candidateTarget = links.GetTarget(candidate);
                var valueTarget = links.GetTarget(candidateTarget);
                if (valueTarget == _propertyValueMarker)
                {
                    values.Add(links.GetTarget(links.GetIndex(candidate)));
                }
                return constants.Continue;
            }, query);

            foreach (var value in values)
            {
                yield return value;
            }
        }

        /// <summary>
        /// <para>
        /// Gets the first value for the specified link property.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The first value, or default if no values exist</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress GetFirst(TLinkAddress link)
        {
            foreach (var value in GetAll(link))
            {
                return value;
            }
            return default;
        }

        /// <summary>
        /// <para>
        /// Adds a value to the specified link property.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="value">
        /// <para>The value to add.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TLinkAddress link, TLinkAddress value)
        {
            if (Contains(link, value))
            {
                return; // Value already exists, no need to add
            }

            var links = _links;
            var property = links.GetOrCreate(link, _propertyMarker);
            var valueContainer = links.GetOrCreate(_propertyValueMarker, value);
            links.GetOrCreate(property, valueContainer);
        }

        /// <summary>
        /// <para>
        /// Removes a specific value from the specified link property.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="value">
        /// <para>The value to remove.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(TLinkAddress link, TLinkAddress value)
        {
            var property = _links.SearchOrDefault(link, _propertyMarker);
            if (property == default)
            {
                return;
            }

            var links = _links;
            var constants = links.Constants;
            var anyConstant = constants.Any;
            var valueContainer = links.SearchOrDefault(_propertyValueMarker, value);
            
            if (valueContainer != default)
            {
                var propertyValueLink = links.SearchOrDefault(property, valueContainer);
                if (propertyValueLink != default)
                {
                    links.Delete(propertyValueLink);
                }
            }
        }

        /// <summary>
        /// <para>
        /// Sets all values for the specified link property, replacing any existing values.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="values">
        /// <para>The values to set.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TLinkAddress link, IEnumerable<TLinkAddress> values)
        {
            Clear(link);
            foreach (var value in values)
            {
                Add(link, value);
            }
        }

        /// <summary>
        /// <para>
        /// Clears all values for the specified link property.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(TLinkAddress link)
        {
            var property = _links.SearchOrDefault(link, _propertyMarker);
            if (property == default)
            {
                return;
            }

            var links = _links;
            var constants = links.Constants;
            var anyConstant = constants.Any;
            var query = new Link<TLinkAddress>(anyConstant, property, anyConstant);

            var linksToDelete = new List<TLinkAddress>();
            links.Each(candidate =>
            {
                var candidateTarget = links.GetTarget(candidate);
                var valueTarget = links.GetTarget(candidateTarget);
                if (valueTarget == _propertyValueMarker)
                {
                    linksToDelete.Add(links.GetIndex(candidate));
                }
                return constants.Continue;
            }, query);

            foreach (var linkToDelete in linksToDelete)
            {
                links.Delete(linkToDelete);
            }
        }

        /// <summary>
        /// <para>
        /// Checks if the specified link property contains the specified value.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="value">
        /// <para>The value to check for.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>True if the property contains the value, false otherwise</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TLinkAddress link, TLinkAddress value)
        {
            var property = _links.SearchOrDefault(link, _propertyMarker);
            if (property == default)
            {
                return false;
            }

            var links = _links;
            var valueContainer = links.SearchOrDefault(_propertyValueMarker, value);
            if (valueContainer == default)
            {
                return false;
            }

            return links.SearchOrDefault(property, valueContainer) != default;
        }

        /// <summary>
        /// <para>
        /// Gets the count of values for the specified link property.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of values for the property</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(TLinkAddress link)
        {
            var count = 0;
            foreach (var _ in GetAll(link))
            {
                count++;
            }
            return count;
        }
    }
}