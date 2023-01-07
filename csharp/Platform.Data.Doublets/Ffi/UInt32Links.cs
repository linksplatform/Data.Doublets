/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Platform.Converters;
using Platform.Delegates;
using Platform.Disposables;

namespace Platform.Data.Doublets.Ffi
{
    using TLinkAddress = System.UInt32;

    public class UInt32Links : DisposableBase, ILinks<TLinkAddress>
    {
        public LinksConstants<TLinkAddress> Constants { get; }

        private readonly unsafe void* _ptr;

        public UInt32Links(string path)
        {
            unsafe
            {
                _ptr = Methods.UInt32Links_New(path);

                // TODO: Update api
                Constants = new LinksConstants<TLinkAddress>(enableExternalReferencesSupport: true);
            }
        }

        public TLinkAddress Count(IList<TLinkAddress>? restriction)
        {
            unsafe
            {
                var array = stackalloc uint[restriction.Count];
                for (var i = 0; i < restriction.Count; i++)
                {
                    array[i] = restriction[i];
                }
                return Methods.UInt32Links_Count(_ptr, array, (nuint)(restriction?.Count ?? 0));
            }
        }

        public TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
        {
            unsafe
            {
                Methods.EachCallback_UInt32 callback = (link) => handler != null ? handler(new Link<TLinkAddress>(link.Index, link.Source, link.Target)) : Constants.Continue;
                var array = stackalloc uint[restriction.Count];
                for (var i = 0; i < restriction.Count; i++)
                {
                    array[i] = restriction[i];
                }
                return Methods.UInt32Links_Each(_ptr, array, (nuint)(restriction?.Count ?? 0), callback);
            }
        }

        public TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            unsafe
            {
                Methods.CreateCallback_UInt32 callback = (before, after) => handler != null ? handler(new Link<TLinkAddress>(before.Index, before.Source, before.Target), new Link<TLinkAddress>(after.Index, after.Source, after.Target)) : Constants.Continue;
                fixed (uint* substitutionPtr = (uint[])substitution)
                {
                    return Methods.UInt32Links_Create(_ptr, substitutionPtr, (nuint)(substitution?.Count ?? 0), callback);
                }
            }
        }

        public TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            unsafe
            {
                var restrictionArray = stackalloc uint[restriction.Count];
                for (var i = 0; i < restriction.Count; i++)
                {
                    restrictionArray[i] = restriction[i];
                }
                var substitutionArray = stackalloc uint[substitution.Count];
                for (var i = 0; i < restriction.Count; i++)
                {
                    substitutionArray[i] = restriction[i];
                }
                Methods.UpdateCallback_UInt32 callback = (before, after) => handler != null ? handler(new Link<TLinkAddress>(before.Index, before.Source, before.Target), new Link<TLinkAddress>(after.Index, after.Source, after.Target)) : Constants.Continue;
                return Methods.UInt32Links_Update(_ptr, restrictionArray, (nuint)(restriction?.Count ?? 0), substitutionArray, (nuint)(substitution?.Count ?? 0), callback);
            }
        }

        public TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            unsafe
            {
                var restrictionArray = stackalloc uint[restriction.Count];
                for (var i = 0; i < restriction.Count; i++)
                {
                    restrictionArray[i] = restriction[i];
                }
                Methods.DeleteCallback_UInt32 callback = (before, after) => handler != null ? handler(new Link<TLinkAddress>(before.Index, before.Source, before.Target), new Link<TLinkAddress>(after.Index, after.Source, after.Target)) : Constants.Continue;
                return Methods.UInt32Links_Delete(_ptr, restrictionArray, (nuint)(restriction?.Count ?? 0), callback);
            }
        }

        protected override void Dispose(bool manual, bool wasDisposed)
        {
            unsafe
            {
                if (wasDisposed || _ptr == null)
                {
                    return;
                }
                Methods.UInt32Links_Drop(_ptr);
            }
        }
    }
}
*/


