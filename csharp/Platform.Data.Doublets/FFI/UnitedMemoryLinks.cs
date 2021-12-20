using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Platform.Converters;
using Platform.Disposables;
using IDisposable = Platform.Disposables.IDisposable;
using static System.Runtime.CompilerServices.Unsafe;

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

        public delegate Byte EachCallback_Uint8(FfiLink_UInt8 link);

        public delegate UInt16 EachCallback_Uint16(FfiLink_UInt16 link);

        public delegate UInt32 EachCallback_Uint32(FfiLink_UInt32 link);

        public delegate UInt64 EachCallback_Uint64(FfiLink_UInt64 link);

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
        public static extern byte ByteUnitedMemoryLinks_Create(void* self, byte* substitution, nuint substitutionLength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Create(void* self, ushort* substitution, nuint substitutionLength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Create(void* self, uint* substitution, nuint substitutionLength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Create(void* self, ulong* substitution, nuint substitutionLength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Count(void* self, byte* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Count(void* self, ushort* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Count(void* self, uint* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Count(void* self, ulong* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Each(void* self, EachCallback_Uint8 callback, byte* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Each(void* self, EachCallback_Uint16 callback, ushort* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Each(void* self, EachCallback_Uint32 callback, uint* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Each(void* self, EachCallback_Uint64 callback, ulong* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Update(void* self, byte* restriction, nuint restrictionLength,  byte* substitution, nuint substitutionLength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Update(void* self, ushort* restriction, nuint restrictionLength,  ushort* substitution, nuint substitutionLength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Update(void* self, uint* restriction, nuint restrictionLength,  uint* substitution, nuint substitutionLength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Update(void* self, ulong* restriction, nuint restrictionLength,  ulong* substitution, nuint substitutionLength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Delete(void* self, byte* restriction, nuint restrictionLength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Delete(void* self, ushort* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Delete(void* self, uint* restriction, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Delete(void* self, ulong* restriction, nuint len);
    }

    public class UnitedMemoryLinks<TLink> : DisposableBase, ILinks<TLink>
    {
        private static readonly UncheckedConverter<byte, TLink> from_u8 = UncheckedConverter<byte, TLink>.Default;
        private static readonly UncheckedConverter<ushort, TLink> from_u16 = UncheckedConverter<ushort, TLink>.Default;
        private static readonly UncheckedConverter<uint, TLink> from_u32 = UncheckedConverter<uint, TLink>.Default;
        private static readonly UncheckedConverter<ulong, TLink> from_u64 = UncheckedConverter<ulong, TLink>.Default;
        private static readonly UncheckedConverter<TLink, ulong> from_t = UncheckedConverter<TLink, ulong>.Default;

        public LinksConstants<TLink> Constants { get; }

        private readonly unsafe void* _body;

        public UnitedMemoryLinks(string path)
        {
            TLink t = default;
            unsafe
            {
                _body = t switch
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
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        var array = stackalloc byte[restriction.Count];
                        for (var i = 0; i < restriction.Count; i++)
                        {
                            array[i] = (byte)from_t.Convert(restriction[i]);
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Count(_body, array, (nuint)restriction.Count));
                    }
                    case ushort:
                    {
                        var array = stackalloc ushort[restriction.Count];
                        for (var i = 0; i < restriction.Count; i++)
                        {
                            array[i] = (ushort)from_t.Convert(restriction[i]);
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Count(_body, array, (nuint)restriction.Count));
                    }
                    case uint:
                    {
                        var array = stackalloc uint[restriction.Count];
                        for (var i = 0; i < restriction.Count; i++)
                        {
                            array[i] = (uint)from_t.Convert(restriction[i]);
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Count(_body, array, (nuint)restriction.Count));
                    }
                    case ulong:
                    {
                        {
                            var array = stackalloc UInt64[restriction.Count];
                            for (var i = 0; i < restriction.Count; i++)
                            {
                                array[i] = from_t.Convert(restriction[i]);
                            }
                            return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Count(_body, array, (nuint)restriction.Count));
                        }
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> restriction)
        {
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        Methods.EachCallback_Uint8 callback = (link) => (byte)from_t.Convert(handler(new Link<TLink>(from_u8.Convert(link.Index), from_u8.Convert(link.Source), from_u8.Convert(link.Target))));
                        var array = stackalloc byte[restriction.Count];
                        for (var i = 0; i < restriction.Count; i++)
                        {
                            array[i] = (byte)from_t.Convert(restriction[i]);
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Each(_body, callback, array, (nuint)restriction.Count));
                    }
                    case ushort:
                    {
                        Methods.EachCallback_Uint16 callback = (link) => (ushort)from_t.Convert(handler(new Link<TLink>(from_u16.Convert(link.Index), from_u16.Convert(link.Source), from_u16.Convert(link.Target))));
                        var array = stackalloc ushort[restriction.Count];
                        for (var i = 0; i < restriction.Count; i++)
                        {
                            array[i] = (ushort)from_t.Convert(restriction[i]);
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Each(_body, callback, array, (nuint)restriction.Count));
                    }
                    case uint:
                    {
                        Methods.EachCallback_Uint32 callback = (link) => (uint)from_t.Convert(handler(new Link<TLink>(from_u32.Convert(link.Index), from_u32.Convert(link.Source), from_u32.Convert(link.Target))));
                        var array = stackalloc uint[restriction.Count];
                        for (var i = 0; i < restriction.Count; i++)
                        {
                            array[i] = (uint)from_t.Convert(restriction[i]);
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Each(_body, callback, array, (nuint)restriction.Count));
                    }
                    case ulong:
                    {
                        {
                            Methods.EachCallback_Uint64 callback = (link) => from_t.Convert(handler(new Link<TLink>(from_u64.Convert(link.Index), from_u64.Convert(link.Source), from_u64.Convert(link.Target))));
                            var array = stackalloc UInt64[restriction.Count];
                            for (var i = 0; i < restriction.Count; i++)
                            {
                                array[i] = from_t.Convert(restriction[i]);
                            }
                            return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Each(_body, callback, array, (nuint)restriction.Count));
                        }
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public TLink Create(IList<TLink> restriction)
        {
            unsafe
            {
                TLink t = default;
                return t switch
                {
                    byte => from_u8.Convert(Methods.ByteUnitedMemoryLinks_Create(_body)),
                    ushort => from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Create(_body)),
                    uint => from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Create(_body)),
                    ulong => from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Create(_body)),
                    _ => throw new NotImplementedException()
                };
            }
        }

        public TLink Update(IList<TLink> restriction, IList<TLink> substitution)
        {
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        var restrictionArray = restriction.ToArray();
                        var substitutionArray = substitution.ToArray();
                        fixed (byte* restrictionPointer = (byte[])(object)restrictionArray, substitutionPointer = (byte[])(object)substitutionArray)
                        {
                            return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Update(_body, restrictionPointer, (nuint)restrictionArray.Length, substitutionPointer, (nuint)substitutionArray.Length));
                        }
                    }
                    case ushort:
                    {
                        var restrictionArray = restriction.ToArray();
                        var substitutionArray = substitution.ToArray();
                        fixed (ushort* restrictionPointer = (ushort[])(object)restrictionArray, substitutionPointer = (ushort[])(object)substitutionArray)
                        {
                            return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Update(_body, restrictionPointer, (nuint)restrictionArray.Length, substitutionPointer, (nuint)substitutionArray.Length));
                        }
                    }
                    case uint:
                    {
                        var restrictionArray = restriction.ToArray();
                        var substitutionArray = substitution.ToArray();
                        fixed (uint* restrictionPointer = (uint[])(object)restrictionArray, substitutionPointer = (uint[])(object)substitutionArray)
                        {
                            return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Update(_body, restrictionPointer, (nuint)restrictionArray.Length, substitutionPointer, (nuint)substitutionArray.Length));
                        }
                    }
                    case ulong:
                    {
                        var restrictionArray = restriction.ToArray();
                        var substitutionArray = substitution.ToArray();
                        fixed (ulong* restrictionPointer = (ulong[])(object)restrictionArray, substitutionPointer = (ulong[])(object)substitutionArray)
                        {
                            return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Update(_body, restrictionPointer, (nuint)restrictionArray.Length, substitutionPointer, (nuint)substitutionArray.Length));
                        }
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public TLink Delete(IList<TLink> restriction)
        {
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        var restrictionArray = restriction.ToArray();
                        fixed (byte* restrictionPointer = (byte[])(object)restrictionArray)
                        {
                            return (TLink)(object)Methods.ByteUnitedMemoryLinks_Delete(_body, restrictionPointer, (nuint)restrictionArray.Length);
                        }
                    }
                    case ushort:
                    {
                        var restrictionArray = restriction.ToArray();
                        fixed (ushort* restrictionPointer = (ushort[])(object)restrictionArray)
                        {
                            return (TLink)(object)Methods.UInt16UnitedMemoryLinks_Delete(_body, restrictionPointer, (nuint)restrictionArray.Length);
                        }
                    }
                    case uint:
                    {
                        var restrictionArray = restriction.ToArray();
                        fixed (uint* restrictionPointer = (uint[])(object)restrictionArray)
                        {
                            return (TLink)(object)Methods.UInt32UnitedMemoryLinks_Delete(_body, restrictionPointer, (nuint)restrictionArray.Length);
                        }
                    }
                    case ulong:
                    {
                        var restrictionArray = restriction.ToArray();
                        fixed (ulong* restrictionPointer = (ulong[])(object)restrictionArray)
                        {
                            return (TLink)(object)Methods.UInt64UnitedMemoryLinks_Delete(_body, restrictionPointer, (nuint)restrictionArray.Length);
                        }
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
            if (wasDisposed && _body != null)
            {
                return;
            }
            TLink t = default;
            unsafe
            {
                switch (t)
                {
                    case byte:
                        Methods.ByteUnitedMemoryLinks_Drop(_body);
                        break;
                    case ushort:
                        Methods.UInt16UnitedMemoryLinks_Drop(_body);
                        break;
                    case uint:
                        Methods.UInt32UnitedMemoryLinks_Drop(_body);
                        break;
                    case ulong:
                        Methods.UInt64UnitedMemoryLinks_Drop(_body);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
