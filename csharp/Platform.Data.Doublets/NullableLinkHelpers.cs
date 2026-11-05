using System;
using System.Collections.Generic;
using System.Numerics;

namespace Platform.Data.Doublets;

// Platform.Data 0.16.1 annotates these callback snapshots as non-null even
// though creation/deletion legitimately supply null. Keep their semantics
// until the upstream annotations are corrected.
internal static class NullableLinkHelpers
{
    internal static T HandleNullable<T>(this ref WriteHandlerState<T> state, IList<T>? before, IList<T>? after) where T : IUnsignedNumber<T>
    {
        if (state.Handler is { } handler)
        {
            state.Apply(handler(before, after));
        }
        return state.Result;
    }

    internal static IList<T> GetRequiredLink<T>(this ILinks<T> links, T link) where T : IUnsignedNumber<T>
        => links.GetLink(link) ?? throw new KeyNotFoundException($"Link {link} does not exist.");
}
