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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Byte EachCallback_UInt8(FfiLink_UInt8 link);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt16 EachCallback_UInt16(FfiLink_UInt16 link);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 EachCallback_UInt32(FfiLink_UInt32 link);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt64 EachCallback_UInt64(FfiLink_UInt64 link);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Byte CreateCallback_UInt8(FfiLink_UInt8 before, FfiLink_UInt8 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt16 CreateCallback_UInt16(FfiLink_UInt16 before, FfiLink_UInt16 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 CreateCallback_UInt32(FfiLink_UInt32 before, FfiLink_UInt32 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt64 CreateCallback_UInt64(FfiLink_UInt64 before, FfiLink_UInt64 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Byte UpdateCallback_UInt8(FfiLink_UInt8 before, FfiLink_UInt8 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt16 UpdateCallback_UInt16(FfiLink_UInt16 before, FfiLink_UInt16 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 UpdateCallback_UInt32(FfiLink_UInt32 before, FfiLink_UInt32 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt64 UpdateCallback_UInt64(FfiLink_UInt64 before, FfiLink_UInt64 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Byte DeleteCallback_UInt8(FfiLink_UInt8 before, FfiLink_UInt8 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt16 DeleteCallback_UInt16(FfiLink_UInt16 before, FfiLink_UInt16 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 DeleteCallback_UInt32(FfiLink_UInt32 before, FfiLink_UInt32 after);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
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

        public TLink Count(IList<TLink>? restriction)
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
                        var byteRestrictionArray = (IList<byte>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = byteRestrictionArray[i];
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Count(_ptr, restrictionArray, (nuint)restrictionLength));
                    }
                    case ushort:
                    {
                        var restrictionArray = stackalloc ushort[restrictionLength];
                        var ushortRestrictionArray = (IList<ushort>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = ushortRestrictionArray[i];
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Count(_ptr, restrictionArray, (nuint)restrictionLength));
                    }
                    case uint:
                    {
                        var restrictionArray = stackalloc uint[restrictionLength];
                        var uintRestrictionArray = (IList<uint>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = uintRestrictionArray[i];
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Count(_ptr, restrictionArray, (nuint)restrictionLength));
                    }
                    case ulong:
                    {
                        {
                            var restrictionArray = stackalloc ulong[restrictionLength];
                            var ulongRestrictionArray = (IList<ulong>)restriction;
                            for (var i = 0; i < restrictionLength; i++)
                            {
                                restrictionArray[i] = ulongRestrictionArray[i];
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
                        byte Callback(FfiLink_UInt8 link) => (byte)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u8.Convert(link.Index), from_u8.Convert(link.Source), from_u8.Convert(link.Target))) : Constants.Continue);
                        var restrictionArray = stackalloc byte[restrictionLength];
                        var byteRestrictionArray = (IList<byte>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = byteRestrictionArray[i];
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Each(_ptr, restrictionArray, (nuint)restrictionLength, Callback));
                    }
                    case ushort:
                    {
                        ushort Callback(FfiLink_UInt16 link) => (ushort)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u16.Convert(link.Index), from_u16.Convert(link.Source), from_u16.Convert(link.Target))) : Constants.Continue);
                        var restrictionArray = stackalloc ushort[restrictionLength];
                        var ushortRestrictionArray = (IList<ushort>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = ushortRestrictionArray[i];
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Each(_ptr, restrictionArray, (nuint)restrictionLength, Callback));
                    }
                    case uint:
                    {
                        uint Callback(FfiLink_UInt32 link) => (uint)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u32.Convert(link.Index), from_u32.Convert(link.Source), from_u32.Convert(link.Target))) : Constants.Continue);
                        var restrictionArray = stackalloc uint[restrictionLength];
                        var uintRestrictionArray = (IList<uint>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = uintRestrictionArray[i];
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Each(_ptr, restrictionArray, (nuint)restrictionLength, Callback));
                    }
                    case ulong:
                    {
                        {
                            ulong Callback(FfiLink_UInt64 link) => from_t.Convert(handler != null ? handler(new Link<TLink>(from_u64.Convert(link.Index), from_u64.Convert(link.Source), from_u64.Convert(link.Target))) : Constants.Continue);
                            var restrictionArray = stackalloc UInt64[restrictionLength];
                            var ulongRestrictionArray = (IList<ulong>)restriction;
                            for (var i = 0; i < restrictionLength; i++)
                            {
                                restrictionArray[i] = ulongRestrictionArray[i];
                            }
                            return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Each(_ptr, restrictionArray, (nuint)restrictionLength, Callback));
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
                        byte Callback(FfiLink_UInt8 before, FfiLink_UInt8 after) => (byte)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u8.Convert(before.Index), from_u8.Convert(before.Source), from_u8.Convert(before.Target)), new Link<TLink>(from_u8.Convert(after.Index), from_u8.Convert(after.Source), from_u8.Convert(after.Target))) : Constants.Continue);
                        var substitutionArray = stackalloc byte[substitutionLength];
                        var byteSubstitutionArray = (IList<byte>)substitution;
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = byteSubstitutionArray[i];
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Create(_ptr, substitutionArray, (nuint)(substitution?.Count ?? 0), Callback));
                    }
                    case ushort:
                    {
                        ushort Callback(FfiLink_UInt16 before, FfiLink_UInt16 after) => (ushort)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u16.Convert(before.Index), from_u16.Convert(before.Source), from_u16.Convert(before.Target)), new Link<TLink>(from_u16.Convert(after.Index), from_u16.Convert(after.Source), from_u16.Convert(after.Target))) : Constants.Continue);
                        var substitutionArray = stackalloc ushort[substitutionLength];
                        var ushortSubstitutionArray = (IList<ushort>)substitution;
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = ushortSubstitutionArray[i];
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Create(_ptr, substitutionArray, (nuint)(substitution?.Count ?? 0), Callback));
                    }
                    case uint:
                    {
                        uint Callback(FfiLink_UInt32 before, FfiLink_UInt32 after) => (uint)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u32.Convert(before.Index), from_u32.Convert(before.Source), from_u32.Convert(before.Target)), new Link<TLink>(from_u32.Convert(after.Index), from_u32.Convert(after.Source), from_u32.Convert(after.Target))) : Constants.Continue);
                        var substitutionArray = stackalloc uint[substitutionLength];
                        var uintSubstitutionArray = (IList<uint>)substitution;
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = uintSubstitutionArray[i];
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Create(_ptr, substitutionArray, (nuint)(substitution?.Count ?? 0), Callback));
                        
                    }
                    case ulong:
                    {
                        ulong Callback(FfiLink_UInt64 before, FfiLink_UInt64 after) => (ulong)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u64.Convert(before.Index), from_u64.Convert(before.Source), from_u64.Convert(before.Target)), new Link<TLink>(from_u64.Convert(after.Index), from_u64.Convert(after.Source), from_u64.Convert(after.Target))) : Constants.Continue);
                        var substitutionArray = stackalloc ulong[substitutionLength];
                        var ulongSubstitutionArray = (IList<ulong>)substitution;
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = ulongSubstitutionArray[i];
                        }
                        return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Create(_ptr, substitutionArray, (nuint)(substitution?.Count ?? 0), Callback));
                    
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
                        var byteRestrictionArray = (IList<byte>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = byteRestrictionArray[i];
                        }
                        var substitutionArray = stackalloc byte[restrictionLength];
                        var byteSubstitutionArray = (IList<byte>)substitution;
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = byteSubstitutionArray[i];
                        }
                        byte Callback(FfiLink_UInt8 before, FfiLink_UInt8 after) => (byte)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u8.Convert(before.Index), from_u8.Convert(before.Source), from_u8.Convert(before.Target)), new Link<TLink>(from_u8.Convert(after.Index), from_u8.Convert(after.Source), from_u8.Convert(after.Target))) : Constants.Continue);
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Update(_ptr, restrictionArray, (nuint)restrictionLength, substitutionArray, (nuint)(substitution?.Count ?? 0), Callback));
                    }
                    case ushort:
                    {
                        var restrictionArray = stackalloc ushort[restrictionLength];
                        var ushortRestrictionArray = (IList<ushort>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = ushortRestrictionArray[i];
                        }
                        var substitutionArray = stackalloc ushort[restrictionLength];
                        var ushortSubstitutionArray = (IList<ushort>)substitution;
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = ushortSubstitutionArray[i];
                        }
                        ushort Callback(FfiLink_UInt16 before, FfiLink_UInt16 after) => (ushort)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u16.Convert(before.Index), from_u16.Convert(before.Source), from_u16.Convert(before.Target)), new Link<TLink>(from_u16.Convert(after.Index), from_u16.Convert(after.Source), from_u16.Convert(after.Target))) : Constants.Continue);
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Update(_ptr, restrictionArray, (nuint)restrictionLength, substitutionArray, (nuint)(substitution?.Count ?? 0), Callback));
                    }
                    case uint:
                    {
                        var restrictionArray = stackalloc uint[restrictionLength];
                        var uintRestrictionArray = (IList<uint>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = uintRestrictionArray[i];
                        }
                        var substitutionArray = stackalloc uint[restrictionLength];
                        var uintSubstitutionArray = (IList<uint>)substitution;
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = uintSubstitutionArray[i];
                        }
                        uint Callback(FfiLink_UInt32 before, FfiLink_UInt32 after) => (uint)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u32.Convert(before.Index), from_u32.Convert(before.Source), from_u32.Convert(before.Target)), new Link<TLink>(from_u32.Convert(after.Index), from_u32.Convert(after.Source), from_u32.Convert(after.Target))) : Constants.Continue);
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Update(_ptr, restrictionArray, (nuint)restrictionLength, substitutionArray, (nuint)(substitution?.Count ?? 0), Callback));
                    }
                    case ulong:
                    {
                        var restrictionArray = stackalloc ulong[restrictionLength];
                        var ulongRestrictionArray = (IList<ulong>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = ulongRestrictionArray[i];
                        }
                        var substitutionArray = stackalloc ulong[restrictionLength];
                        var ulongSubstitutionArray = (IList<ulong>)substitution;
                        for (var i = 0; i < substitutionLength; i++)
                        {
                            substitutionArray[i] = ulongSubstitutionArray[i];
                        }
                        ulong Callback(FfiLink_UInt64 before, FfiLink_UInt64 after) => (ulong)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u64.Convert(before.Index), from_u64.Convert(before.Source), from_u64.Convert(before.Target)), new Link<TLink>(from_u64.Convert(after.Index), from_u64.Convert(after.Source), from_u64.Convert(after.Target))) : Constants.Continue);
                        return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Update(_ptr, restrictionArray, (nuint)restrictionLength, substitutionArray, (nuint)(substitution?.Count ?? 0), Callback));
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
                        var byteRestrictionArray = (IList<byte>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = byteRestrictionArray[i];
                        }
                        byte Callback(FfiLink_UInt8 before, FfiLink_UInt8 after) => (byte)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u8.Convert(before.Index), from_u8.Convert(before.Source), from_u8.Convert(before.Target)), new Link<TLink>(from_u8.Convert(after.Index), from_u8.Convert(after.Source), from_u8.Convert(after.Target))) : Constants.Continue);
                        return (TLink)(object)Methods.ByteUnitedMemoryLinks_Delete(_ptr, restrictionArray, (nuint)restrictionLength, Callback);
                    }
                    case ushort:
                    {
                        var restrictionArray = stackalloc ushort[restrictionLength];
                        var ushortRestrictionArray = (IList<ushort>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = ushortRestrictionArray[i];
                        }
                        ushort Callback(FfiLink_UInt16 before, FfiLink_UInt16 after) => (ushort)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u16.Convert(before.Index), from_u16.Convert(before.Source), from_u16.Convert(before.Target)), new Link<TLink>(from_u16.Convert(after.Index), from_u16.Convert(after.Source), from_u16.Convert(after.Target))) : Constants.Continue);
                        return (TLink)(object)Methods.UInt16UnitedMemoryLinks_Delete(_ptr, restrictionArray, (nuint)restrictionLength, Callback);
                    }
                    case uint:
                    {
                        var restrictionArray = stackalloc uint[restrictionLength];
                        var uintRestrictionArray = (IList<uint>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = uintRestrictionArray[i];
                        }
                        uint Callback(FfiLink_UInt32 before, FfiLink_UInt32 after) => (uint)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u32.Convert(before.Index), from_u32.Convert(before.Source), from_u32.Convert(before.Target)), new Link<TLink>(from_u32.Convert(after.Index), from_u32.Convert(after.Source), from_u32.Convert(after.Target))) : Constants.Continue);
                        return (TLink)(object)Methods.UInt32UnitedMemoryLinks_Delete(_ptr, restrictionArray, (nuint)restrictionLength, Callback);
                    }
                    case ulong:
                    {
                        var restrictionArray = stackalloc ulong[restrictionLength];
                        var ulongRestrictionArray = (IList<ulong>)restriction;
                        for (var i = 0; i < restrictionLength; i++)
                        {
                            restrictionArray[i] = ulongRestrictionArray[i];
                        }
                        ulong Callback(FfiLink_UInt64 before, FfiLink_UInt64 after) => (ulong)from_t.Convert(handler != null ? handler(new Link<TLink>(from_u64.Convert(before.Index), from_u64.Convert(before.Source), from_u64.Convert(before.Target)), new Link<TLink>(from_u64.Convert(after.Index), from_u64.Convert(after.Source), from_u64.Convert(after.Target))) : Constants.Continue);
                        return (TLink)(object)Methods.UInt64UnitedMemoryLinks_Delete(_ptr, restrictionArray, (nuint)restrictionLength, Callback);
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
