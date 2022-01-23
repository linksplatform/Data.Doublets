using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Platform.Converters;
using Platform.Delegates;
using Platform.Disposables;

namespace Platform.Data.Doublets.FFI
{
    using TLink = System.UInt32;

    public class UInt32UnitedMemoryLinks : DisposableBase, ILinks<TLink>
    {
        public LinksConstants<TLink> Constants { get; }

        private readonly unsafe void* _ptr;

        public UInt32UnitedMemoryLinks(string path)
        {
            unsafe
            {
                _ptr = Methods.UInt32UnitedMemoryLinks_New(path);

                // TODO: Update api
                Constants = new LinksConstants<TLink>(enableExternalReferencesSupport: true);
            }
        }

        public TLink Count(IList<TLink> restriction)
        {
            unsafe
            {
                var array = stackalloc uint[restriction.Count];
                for (var i = 0; i < restriction.Count; i++)
                {
                    array[i] = restriction[i];
                }
                return Methods.UInt32UnitedMemoryLinks_Count(_ptr, array, (nuint)(restriction?.Count ?? 0));
            }
        }

        public TLink Each(IList<TLink> restriction, ReadHandler<TLink> handler)
        {
            unsafe
            {
                Methods.EachCallback_UInt32 callback = (link) => handler?.Invoke(new Link<TLink>(link.Index, link.Source, link.Target)) ?? Constants.Continue;
                var array = stackalloc uint[restriction.Count];
                for (var i = 0; i < restriction.Count; i++)
                {
                    array[i] = restriction[i];
                }
                return Methods.UInt32UnitedMemoryLinks_Each(_ptr, array, (nuint)(restriction?.Count ?? 0), callback);
            }
        }

        public TLink Create(IList<TLink> substitution, WriteHandler<TLink> handler)
        {
            unsafe
            {
                Methods.CreateCallback_UInt32 callback = (before, after) => handler != null ? handler(new Link<TLink>(before.Index, before.Source, before.Target), new Link<TLink>(after.Index, after.Source, after.Target)) : Constants.Continue;
                fixed (uint* substitutionPtr = (uint[])substitution)
                {
                    return Methods.UInt32UnitedMemoryLinks_Create(_ptr, substitutionPtr, (nuint)(substitution?.Count ?? 0), callback);
                }
            }
        }

        public TLink Update(IList<TLink> restriction, IList<TLink> substitution, WriteHandler<TLink> handler)
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
                Methods.UpdateCallback_UInt32 callback = (before, after) => handler?.Invoke(new Link<TLink>(before.Index, before.Source, before.Target), new Link<TLink>(after.Index, after.Source, after.Target)) ?? Constants.Continue;
                return Methods.UInt32UnitedMemoryLinks_Update(_ptr, restrictionArray, (nuint)(restriction?.Count ?? 0), substitutionArray, (nuint)(substitution?.Count ?? 0), callback);
            }
        }

        public TLink Delete(IList<TLink> restriction, WriteHandler<TLink> handler)
        {
            unsafe
            {
                var restrictionArray = stackalloc uint[restriction.Count];
                for (var i = 0; i < restriction.Count; i++)
                {
                    restrictionArray[i] = restriction[i];
                }
                Methods.DeleteCallback_UInt32 callback = (before, after) => handler?.Invoke(new Link<TLink>(before.Index, before.Source, before.Target), new Link<TLink>(after.Index, after.Source, after.Target)) ?? Constants.Continue;
                return Methods.UInt32UnitedMemoryLinks_Delete(_ptr, restrictionArray, (nuint)(restriction?.Count ?? 0), callback);
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
                Methods.UInt32UnitedMemoryLinks_Drop(_ptr);
            }
        }
    }
}
