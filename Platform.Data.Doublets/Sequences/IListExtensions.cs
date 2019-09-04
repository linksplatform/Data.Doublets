using Platform.Collections;
using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences
{
    public static class IListExtensions
    {
        public static TLink[] ExtractValues<TLink>(this IList<TLink> restrictions)
        {
            if(restrictions.IsNullOrEmpty() || restrictions.Count == 1)
            {
                return new TLink[0];
            }
            var values = new TLink[restrictions.Count - 1];
            for (int i = 1, j = 0; i < restrictions.Count; i++, j++)
            {
                values[j] = restrictions[i];
            }
            return values;
        }

        public static IList<TLink> ConvertToRestrictionsValues<TLink>(this IList<TLink> list)
        {
            var restrictions = new TLink[list.Count + 1];
            for (int i = 0, j = 1; i < list.Count; i++, j++)
            {
                restrictions[j] = list[i];
            }
            return restrictions;
        }
    }
}
