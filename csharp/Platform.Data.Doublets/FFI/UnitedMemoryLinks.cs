using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Platform.Converters;
using Platform.Delegates;
using Platform.Disposables;

namespace Platform.Data.Doublets.FFI
{
    struct FfiLink_UInt8
    {
        public Byte Index;
        public Byte Source;
        public Byte Target;
    }

    struct FfiLink_UInt16
    {
        public UInt16 Index;
        public UInt16 Source;
        public UInt16 Target;
    }

    struct FfiLink_UInt32
    {
        public UInt32 Index;
        public UInt32 Source;
        public UInt32 Target;
    }

    struct FfiLink_UInt64
    {
        public UInt64 Index;
        public UInt64 Source;
        public UInt64 Target;
    }

    unsafe static class Methods
    {
        private const string DllName = "Platform.Doublets";

        public delegate Byte EachCallback_UInt8(FfiLink_UInt8 link);

        public delegate UInt16 EachCallback_UInt16(FfiLink_UInt16 link);

        public delegate UInt32 EachCallback_UInt32(FfiLink_UInt32 link);

        public delegate UInt64 EachCallback_UInt64(FfiLink_UInt64 link);

        public delegate Byte CreateCallback_UInt8(FfiLink_UInt8 before, FfiLink_UInt8 after);

        public delegate UInt16 CreateCallback_UInt16(FfiLink_UInt16 before, FfiLink_UInt16 after);

        public delegate UInt32 CreateCallback_UInt32(FfiLink_UInt32 before, FfiLink_UInt32 after);

        public delegate UInt64 CreateCallback_UInt64(FfiLink_UInt64 before, FfiLink_UInt64 after);

        public delegate Byte UpdateCallback_UInt8(FfiLink_UInt8 before, FfiLink_UInt8 after);

        public delegate UInt16 UpdateCallback_UInt16(FfiLink_UInt16 before, FfiLink_UInt16 after);

        public delegate UInt32 UpdateCallback_UInt32(FfiLink_UInt32 before, FfiLink_UInt32 after);

        public delegate UInt64 UpdateCallback_UInt64(FfiLink_UInt64 before, FfiLink_UInt64 after);

        public delegate Byte DeleteCallback_UInt8(FfiLink_UInt8 before, FfiLink_UInt8 after);

        public delegate UInt16 DeleteCallback_UInt16(FfiLink_UInt16 before, FfiLink_UInt16 after);

        public delegate UInt32 DeleteCallback_UInt32(FfiLink_UInt32 before, FfiLink_UInt32 after);

        public delegate UInt64 DeleteCallback_UInt64(FfiLink_UInt64 before, FfiLink_UInt64 after);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* ByteUnitedMemoryLinks_New(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* UInt16UnitedMemoryLinks_New(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* UInt32UnitedMemoryLinks_New(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* UInt64UnitedMemoryLinks_New(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ByteUnitedMemoryLinks_Drop(void* self);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void UInt16UnitedMemoryLinks_Drop(void* self);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void UInt32UnitedMemoryLinks_Drop(void* self);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void UInt64UnitedMemoryLinks_Drop(void* self);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Create(void* self, byte* substitution, nuint substitutionLength, CreateCallback_UInt8 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Create(void* self, ushort* substitution, nuint substitutionLength, CreateCallback_UInt16 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Create(void* self, uint* substitution, nuint substitutionLength, CreateCallback_UInt32 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Create(void* self, ulong* substitution, nuint substitutionLength, CreateCallback_UInt64 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Count(void* self, byte* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Count(void* self, ushort* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Count(void* self, uint* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Count(void* self, ulong* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Each(void* self, byte* restriction, nuint len, EachCallback_UInt8 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Each(void* self, ushort* restriction, nuint len, EachCallback_UInt16 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Each(void* self, uint* restriction, nuint len, EachCallback_UInt32 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Each(void* self, ulong* restriction, nuint len, EachCallback_UInt64 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Update(void* self, byte* restriction, nuint restrictionLength,  byte* substitution, nuint substitutionLength, UpdateCallback_UInt8 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Update(void* self, ushort* restriction, nuint restrictionLength,  ushort* substitution, nuint substitutionLength, UpdateCallback_UInt16 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Update(void* self, uint* restriction, nuint restrictionLength,  uint* substitution, nuint substitutionLength, UpdateCallback_UInt32 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Update(void* self, ulong* restriction, nuint restrictionLength,  ulong* substitution, nuint substitutionLength, UpdateCallback_UInt64 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Delete(void* self, byte* restriction, nuint restrictionLength, DeleteCallback_UInt8 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Delete(void* self, ushort* restriction, nuint len, DeleteCallback_UInt16 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Delete(void* self, uint* restriction, nuint len, DeleteCallback_UInt32 callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Delete(void* self, ulong* restriction, nuint len, DeleteCallback_UInt64 callback);
    }

    public class UnitedMemoryLinks<TLink> : DisposableBase, ILinks<TLink>
    {
        private static readonly UncheckedConverter<byte, TLink> from_u8 = UncheckedConverter<byte, TLink>.Default;
        private static readonly UncheckedConverter<ushort, TLink> from_u16 = UncheckedConverter<ushort, TLink>.Default;
        private static readonly UncheckedConverter<uint, TLink> from_u32 = UncheckedConverter<uint, TLink>.Default;
        private static readonly UncheckedConverter<ulong, TLink> from_u64 = UncheckedConverter<ulong, TLink>.Default;
        private static readonly UncheckedConverter<TLink, ulong> from_t = UncheckedConverter<TLink, ulong>.Default;

        public LinksConstants<TLink> Constants { get; }

        private readonly unsafe void* _ptr;

        public UnitedMemoryLinks(string path)
        {
            TLink t = default;
            unsafe
            {
                _ptr = t switch
                {
                    byte => Methods.ByteUnitedMemoryLinks_New(path),
                    ushort => Methods.UInt16UnitedMemoryLinks_New(path),
                    uint => Methods.UInt32UnitedMemoryLinks_New(path),
                    ulong => Methods.UInt64UnitedMemoryLinks_New(path),
                    _ => throw new NotImplementedException()
                };

                // TODO: Update api
                Constants = new LinksConstants<TLink>(enableExternalReferencesSupport: true);
            }
        }

        public TLink Count(IList<TLink> restriction)
        {
            var restrictionLength = restriction?.Count ?? 0;
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        var restrictionArray = stackalloc byte[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (byte)(object)restriction[i];
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Count(_ptr, restrictionArray, (nuint)restrictionLength));
                    }
                    case ushort:
                    {
                        var restrictionArray = stackalloc ushort[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (ushort)(object)restriction[i];
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Count(_ptr, restrictionArray, (nuint)restrictionLength));
                    }
                    case uint:
                    {
                        var restrictionArray = stackalloc uint[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (uint)(object)restriction[i];
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Count(_ptr, restrictionArray, (nuint)restrictionLength));
                    }
                    case ulong:
                    {
                        {
                            var restrictionArray = stackalloc ulong[restrictionLength];
                            for (var i = 0; i < restrictionLength; i++)
                            {
                                restrictionArray[i] = (ulong)(object)restriction[i];
                            }
                            return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Count(_ptr, restrictionArray, (nuint)restrictionLength));
                        }
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public TLink Each(IList<TLink> restriction, ReadHandler<TLink> handler)
        {
            var restrictionLength = restriction?.Count ?? 0;
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        Methods.EachCallback_UInt8 callback = (link) => (byte)from_t.Convert(handler != null? handler(new Link<TLink>(from_u8.Convert(link.Index), from_u8.Convert(link.Source), from_u8.Convert(link.Target))) : Constants.Continue);
                        var restrictionArray = stackalloc byte[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (byte)(object)restriction[i];
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Each(_ptr, restrictionArray, (nuint)restrictionLength, callback));
                    }
                    case ushort:
                    {
                        Methods.EachCallback_UInt16 callback = (link) => (ushort)from_t.Convert(handler != null? handler(new Link<TLink>(from_u16.Convert(link.Index), from_u16.Convert(link.Source), from_u16.Convert(link.Target))) : Constants.Continue);
                        var restrictionArray = stackalloc ushort[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (ushort)(object)restriction[i];
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Each(_ptr, restrictionArray, (nuint)restrictionLength, callback));
                    }
                    case uint:
                    {
                        Methods.EachCallback_UInt32 callback = (link) => (uint)from_t.Convert(handler != null? handler(new Link<TLink>(from_u32.Convert(link.Index), from_u32.Convert(link.Source), from_u32.Convert(link.Target))) : Constants.Continue);
                        var restrictionArray = stackalloc uint[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (uint)(object)restriction[i];
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Each(_ptr, restrictionArray, (nuint)restrictionLength, callback));
                    }
                    case ulong:
                    {
                        {
                            Methods.EachCallback_UInt64 callback = (link) => from_t.Convert(handler != null? handler(new Link<TLink>(from_u64.Convert(link.Index), from_u64.Convert(link.Source), from_u64.Convert(link.Target))) : Constants.Continue);
                            var restrictionArray = stackalloc UInt64[restrictionLength];
                            for (var i = 0; i < restrictionLength; i++)
                            {
                                restrictionArray[i] = (ulong)(object)restriction[i];;
                            }
                            return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Each(_ptr, restrictionArray, (nuint)restrictionLength, callback));
                        }
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public TLink Create(IList<TLink> substitution, WriteHandler<TLink> handler)
        {
            var substitutionLength = substitution?.Count ?? 0;
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        Methods.CreateCallback_UInt8 callback = (before, after) => (byte)from_t.Convert(handler != null? handler(new Link<TLink>(from_u8.Convert(before.Index), from_u8.Convert(before.Source), from_u8.Convert(before.Target)), new Link<TLink>(from_u8.Convert(after.Index), from_u8.Convert(after.Source), from_u8.Convert(after.Target))) : Constants.Continue);
                        var substitutionArray = stackalloc byte[substitution?.Count ?? 0];
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = (byte)(object)substitution[i];
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Create(_ptr, substitutionArray, (nuint)(substitution?.Count ?? 0), callback));
                    }
                    case ushort:
                    {
                        Methods.CreateCallback_UInt16 callback = (before, after) => (ushort)from_t.Convert(handler != null? handler(new Link<TLink>(from_u16.Convert(before.Index), from_u16.Convert(before.Source), from_u16.Convert(before.Target)), new Link<TLink>(from_u16.Convert(after.Index), from_u16.Convert(after.Source), from_u16.Convert(after.Target))) : Constants.Continue);
                        var substitutionArray = stackalloc ushort[substitution?.Count ?? 0];
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = (ushort)(object)substitution[i];
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Create(_ptr, substitutionArray, (nuint)(substitution?.Count ?? 0), callback));
                    }
                    case uint:
                    {
                        Methods.CreateCallback_UInt32 callback = (before, after) => (uint)from_t.Convert(handler != null? handler(new Link<TLink>(from_u32.Convert(before.Index), from_u32.Convert(before.Source), from_u32.Convert(before.Target)), new Link<TLink>(from_u32.Convert(after.Index), from_u32.Convert(after.Source), from_u32.Convert(after.Target))) : Constants.Continue);
                        var substitutionArray = stackalloc uint[substitution?.Count ?? 0];
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = (uint)(object)substitution[i];
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Create(_ptr, substitutionArray, (nuint)(substitution?.Count ?? 0), callback));
                        
                    }
                    case ulong:
                    {
                        Methods.CreateCallback_UInt64 callback = (before, after) => (ulong)from_t.Convert(handler != null? handler(new Link<TLink>(from_u64.Convert(before.Index), from_u64.Convert(before.Source), from_u64.Convert(before.Target)), new Link<TLink>(from_u64.Convert(after.Index), from_u64.Convert(after.Source), from_u64.Convert(after.Target))) : Constants.Continue);
                        var substitutionArray = stackalloc ulong[substitution?.Count ?? 0];
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = (ulong)(object)substitution[i];
                        }
                        return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Create(_ptr, substitutionArray, (nuint)(substitution?.Count ?? 0), callback));
                    
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                };
            }
        }

        public TLink Update(IList<TLink> restriction, IList<TLink> substitution, WriteHandler<TLink> handler)
        {
            var restrictionLength = restriction?.Count ?? 0;
            var substitutionLength = substitution?.Count ?? 0;
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        var restrictionArray = stackalloc byte[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (byte)(object)restriction[i];
                        }
                        var substitutionArray = stackalloc byte[restrictionLength];
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = (byte)(object)substitution[i];
                        }  
                        Methods.UpdateCallback_UInt8 callback = (before, after) => (byte)from_t.Convert(handler != null? handler(new Link<TLink>(from_u8.Convert(before.Index), from_u8.Convert(before.Source), from_u8.Convert(before.Target)), new Link<TLink>(from_u8.Convert(after.Index), from_u8.Convert(after.Source), from_u8.Convert(after.Target))) : Constants.Continue);
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Update(_ptr, restrictionArray, (nuint)restrictionLength, substitutionArray, (nuint)(substitution?.Count ?? 0), callback));
                    }
                    case ushort:
                    {
                        var restrictionArray = stackalloc ushort[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (ushort)(object)restriction[i];
                        }
                        var substitutionArray = stackalloc ushort[restrictionLength];
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = (ushort)(object)substitution[i];
                        }  
                        Methods.UpdateCallback_UInt16 callback = (before, after) => (ushort)from_t.Convert(handler != null? handler(new Link<TLink>(from_u16.Convert(before.Index), from_u16.Convert(before.Source), from_u16.Convert(before.Target)), new Link<TLink>(from_u16.Convert(after.Index), from_u16.Convert(after.Source), from_u16.Convert(after.Target))) : Constants.Continue);
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Update(_ptr, restrictionArray, (nuint)restrictionLength, substitutionArray, (nuint)(substitution?.Count ?? 0), callback));
                    }
                    case uint:
                    {
                        var restrictionArray = stackalloc uint[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (uint)(object)restriction[i];
                        }
                        var substitutionArray = stackalloc uint[restrictionLength];
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = (uint)(object)substitution[i];
                        }  
                        Methods.UpdateCallback_UInt32 callback = (before, after) => (uint)from_t.Convert(handler != null? handler(new Link<TLink>(from_u32.Convert(before.Index), from_u32.Convert(before.Source), from_u32.Convert(before.Target)), new Link<TLink>(from_u32.Convert(after.Index), from_u32.Convert(after.Source), from_u32.Convert(after.Target))) : Constants.Continue);
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Update(_ptr, restrictionArray, (nuint)restrictionLength, substitutionArray, (nuint)(substitution?.Count ?? 0), callback));
                    }
                    case ulong:
                    {
                        var restrictionArray = stackalloc ulong[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (ulong)(object)restriction[i];
                        }
                        var substitutionArray = stackalloc ulong[restrictionLength];
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = (ulong)(object)substitution[i];
                        }  
                        Methods.UpdateCallback_UInt64 callback = (before, after) => (ulong)from_t.Convert(handler != null? handler(new Link<TLink>(from_u64.Convert(before.Index), from_u64.Convert(before.Source), from_u64.Convert(before.Target)), new Link<TLink>(from_u64.Convert(after.Index), from_u64.Convert(after.Source), from_u64.Convert(after.Target))) : Constants.Continue);
                        return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Update(_ptr, restrictionArray, (nuint)restrictionLength, substitutionArray, (nuint)(substitution?.Count ?? 0), callback));
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public TLink Delete(IList<TLink> restriction, WriteHandler<TLink> handler)
        {
            var restrictionLength = restriction?.Count ?? 0;
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        var restrictionArray = stackalloc byte[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (byte)(object)restriction[i];
                        }
                        Methods.DeleteCallback_UInt8 callback = (before, after) => (byte)from_t.Convert(handler != null? handler(new Link<TLink>(from_u8.Convert(before.Index), from_u8.Convert(before.Source), from_u8.Convert(before.Target)), new Link<TLink>(from_u8.Convert(after.Index), from_u8.Convert(after.Source), from_u8.Convert(after.Target))) : Constants.Continue);
                        return (TLink)(object)Methods.ByteUnitedMemoryLinks_Delete(_ptr, restrictionArray, (nuint)restrictionLength, callback);
                    }
                    case ushort:
                    {
                        var restrictionArray = stackalloc ushort[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (ushort)(object)restriction[i];
                        }
                        Methods.DeleteCallback_UInt16 callback = (before, after) => (ushort)from_t.Convert(handler != null? handler(new Link<TLink>(from_u16.Convert(before.Index), from_u16.Convert(before.Source), from_u16.Convert(before.Target)), new Link<TLink>(from_u16.Convert(after.Index), from_u16.Convert(after.Source), from_u16.Convert(after.Target))) : Constants.Continue);
                        return (TLink)(object)Methods.UInt16UnitedMemoryLinks_Delete(_ptr, restrictionArray, (nuint)restrictionLength, callback);
                    }
                    case uint:
                    {
                        var restrictionArray = stackalloc uint[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (uint)(object)restriction[i];
                        }
                        Methods.DeleteCallback_UInt32 callback = (before, after) => (uint)from_t.Convert(handler != null? handler(new Link<TLink>(from_u32.Convert(before.Index), from_u32.Convert(before.Source), from_u32.Convert(before.Target)), new Link<TLink>(from_u32.Convert(after.Index), from_u32.Convert(after.Source), from_u32.Convert(after.Target))) : Constants.Continue);
                        return (TLink)(object)Methods.UInt32UnitedMemoryLinks_Delete(_ptr, restrictionArray, (nuint)restrictionLength, callback);
                    }
                    case ulong:
                    {
                        var restrictionArray = stackalloc ulong[restrictionLength];
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = (ulong)(object)restriction[i];
                        }
                        Methods.DeleteCallback_UInt64 callback = (before, after) => (ulong)from_t.Convert(handler != null? handler(new Link<TLink>(from_u64.Convert(before.Index), from_u64.Convert(before.Source), from_u64.Convert(before.Target)), new Link<TLink>(from_u64.Convert(after.Index), from_u64.Convert(after.Source), from_u64.Convert(after.Target))) : Constants.Continue);
                        return (TLink)(object)Methods.UInt64UnitedMemoryLinks_Delete(_ptr, restrictionArray, (nuint)restrictionLength, callback);
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        protected override void Dispose(bool manual, bool wasDisposed)
        {
            unsafe
            {
                if (wasDisposed && _ptr != null)
                {
                    return;
                }
                TLink t = default;
                switch (t)
                    {
                        case byte:
                            Methods.ByteUnitedMemoryLinks_Drop(_ptr);
                            break;
                        case ushort:
                            Methods.UInt16UnitedMemoryLinks_Drop(_ptr);
                            break;
                        case uint:
                            Methods.UInt32UnitedMemoryLinks_Drop(_ptr);
                            break;
                        case ulong:
                            Methods.UInt64UnitedMemoryLinks_Drop(_ptr);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
            }
        }
    }
}
