using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.PropertyOperators
{
    /// <summary>
    /// <para>
    /// Represents a properties operator that supports multiple values for object-property combinations.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <seealso cref="LinksOperatorBase{TLinkAddress}"/>
    public class MultipleValuesPropertiesOperator<TLinkAddress> : LinksOperatorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Initializes a new <see cref="MultipleValuesPropertiesOperator"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">
        /// <para>A links.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultipleValuesPropertiesOperator(ILinks<TLinkAddress> links) : base(links) { }

        /// <summary>
        /// <para>
        /// Gets all values for the specified object-property combination.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="object">
        /// <para>The object.</para>
        /// <para></para>
        /// </param>
        /// <param name="property">
        /// <para>The property.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A collection of all values for the object-property combination</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TLinkAddress> GetAllValues(TLinkAddress @object, TLinkAddress property)
        {
            var links = _links;
            var objectProperty = links.SearchOrDefault(@object, property);
            if (objectProperty == default)
            {
                yield break;
            }

            var constants = links.Constants;
            var any = constants.Any;
            var query = new Link<TLinkAddress>(any, objectProperty, any);

            var values = new List<TLinkAddress>();
            links.Each(candidate =>
            {
                values.Add(links.GetTarget(links.GetIndex(candidate)));
                return constants.Continue;
            }, query);

            foreach (var value in values)
            {
                yield return value;
            }
        }

        /// <summary>
        /// <para>
        /// Gets the first value for the specified object-property combination.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="object">
        /// <para>The object.</para>
        /// <para></para>
        /// </param>
        /// <param name="property">
        /// <para>The property.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The first value, or default if no values exist</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress GetFirstValue(TLinkAddress @object, TLinkAddress property)
        {
            foreach (var value in GetAllValues(@object, property))
            {
                return value;
            }
            return default;
        }

        /// <summary>
        /// <para>
        /// Adds a value to the specified object-property combination.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="object">
        /// <para>The object.</para>
        /// <para></para>
        /// </param>
        /// <param name="property">
        /// <para>The property.</para>
        /// <para></para>
        /// </param>
        /// <param name="value">
        /// <para>The value to add.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddValue(TLinkAddress @object, TLinkAddress property, TLinkAddress value)
        {
            if (ContainsValue(@object, property, value))
            {
                return; // Value already exists, no need to add
            }

            var links = _links;
            var objectProperty = links.GetOrCreate(@object, property);
            links.GetOrCreate(objectProperty, value);
        }

        /// <summary>
        /// <para>
        /// Removes a specific value from the specified object-property combination.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="object">
        /// <para>The object.</para>
        /// <para></para>
        /// </param>
        /// <param name="property">
        /// <para>The property.</para>
        /// <para></para>
        /// </param>
        /// <param name="value">
        /// <para>The value to remove.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveValue(TLinkAddress @object, TLinkAddress property, TLinkAddress value)
        {
            var links = _links;
            var objectProperty = links.SearchOrDefault(@object, property);
            if (objectProperty == default)
            {
                return;
            }

            var propertyValueLink = links.SearchOrDefault(objectProperty, value);
            if (propertyValueLink != default)
            {
                links.Delete(propertyValueLink);
            }
        }

        /// <summary>
        /// <para>
        /// Sets all values for the specified object-property combination, replacing any existing values.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="object">
        /// <para>The object.</para>
        /// <para></para>
        /// </param>
        /// <param name="property">
        /// <para>The property.</para>
        /// <para></para>
        /// </param>
        /// <param name="values">
        /// <para>The values to set.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAllValues(TLinkAddress @object, TLinkAddress property, IEnumerable<TLinkAddress> values)
        {
            ClearAllValues(@object, property);
            foreach (var value in values)
            {
                AddValue(@object, property, value);
            }
        }

        /// <summary>
        /// <para>
        /// Clears all values for the specified object-property combination.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="object">
        /// <para>The object.</para>
        /// <para></para>
        /// </param>
        /// <param name="property">
        /// <para>The property.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearAllValues(TLinkAddress @object, TLinkAddress property)
        {
            var links = _links;
            var objectProperty = links.SearchOrDefault(@object, property);
            if (objectProperty == default)
            {
                return;
            }

            var constants = links.Constants;
            var any = constants.Any;
            var linksToDelete = new List<TLinkAddress>();

            links.Each(candidate =>
            {
                linksToDelete.Add(links.GetIndex(candidate));
                return constants.Continue;
            }, new Link<TLinkAddress>(any, objectProperty, any));

            foreach (var linkToDelete in linksToDelete)
            {
                links.Delete(linkToDelete);
            }
        }

        /// <summary>
        /// <para>
        /// Checks if the specified object-property combination contains the specified value.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="object">
        /// <para>The object.</para>
        /// <para></para>
        /// </param>
        /// <param name="property">
        /// <para>The property.</para>
        /// <para></para>
        /// </param>
        /// <param name="value">
        /// <para>The value to check for.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>True if the object-property combination contains the value, false otherwise</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(TLinkAddress @object, TLinkAddress property, TLinkAddress value)
        {
            var links = _links;
            var objectProperty = links.SearchOrDefault(@object, property);
            if (objectProperty == default)
            {
                return false;
            }

            return links.SearchOrDefault(objectProperty, value) != default;
        }

        /// <summary>
        /// <para>
        /// Gets the count of values for the specified object-property combination.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="object">
        /// <para>The object.</para>
        /// <para></para>
        /// </param>
        /// <param name="property">
        /// <para>The property.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of values for the object-property combination</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountValues(TLinkAddress @object, TLinkAddress property)
        {
            var count = 0;
            foreach (var _ in GetAllValues(@object, property))
            {
                count++;
            }
            return count;
        }
    }
}