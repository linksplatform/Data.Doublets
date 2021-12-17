using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Platform.Converters;
using Platform.Disposables;
using IDisposable = Platform.Disposables.IDisposable;

namespace Platform.Data.Doublets.FFI
{
    struct FfiLink_Uint8
    {
        public Byte Index;
        public Byte Source;
        public Byte Target;
    }

    struct FfiLink_Uint16
    {
        public UInt16 Index;
        public UInt16 Source;
        public UInt16 Target;
    }

    struct FfiLink_Uint32
    {
        public UInt32 Index;
        public UInt32 Source;
        public UInt32 Target;
    }

    struct FfiLink_Uint64
    {
        public UInt64 Index;
        public UInt64 Source;
        public UInt64 Target;
    }

    unsafe static class Methods
    {
        private const string DllName = "Platform.Doublets";

        public delegate Byte EachCallback_Uint8(FfiLink_Uint8 link);

        public delegate UInt16 EachCallback_Uint16(FfiLink_Uint16 link);

        public delegate UInt32 EachCallback_Uint32(FfiLink_Uint32 link);

        public delegate UInt64 EachCallback_Uint64(FfiLink_Uint64 link);

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
        public static extern byte ByteUnitedMemoryLinks_Create(void* self);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Create(void* self);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Create(void* self);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Create(void* self);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Count(void* self, byte* query, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Count(void* self, ushort* query, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Count(void* self, uint* query, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Count(void* self, ulong* query, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Each(void* self, EachCallback_Uint8 callback, byte* query, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Each(void* self, EachCallback_Uint16 callback, ushort* query, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Each(void* self, EachCallback_Uint32 callback, uint* query, nuint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Each(void* self, EachCallback_Uint64 callback, ulong* query, nuint len);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Update(void* self, byte index, byte source, byte target);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Update(void* self, ushort index, ushort source, ushort target);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Update(void* self, uint index, uint source, uint target);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Update(void* self, ulong index, ulong source, ulong target);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte ByteUnitedMemoryLinks_Delete(void* self, byte index);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort UInt16UnitedMemoryLinks_Delete(void* self, ushort index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint UInt32UnitedMemoryLinks_Delete(void* self, uint index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong UInt64UnitedMemoryLinks_Delete(void* self, ulong index);
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

        public TLink Count(IList<TLink> restrictions)
        {
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        var array = stackalloc byte[restrictions.Count];
                        for (var i = 0; i < restrictions.Count; i++)
                        {
                            array[i] = (byte)from_t.Convert(restrictions[i]);
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Count(_body, array, (nuint)restrictions.Count));
                    }
                    case ushort:
                    {
                        var array = stackalloc ushort[restrictions.Count];
                        for (var i = 0; i < restrictions.Count; i++)
                        {
                            array[i] = (ushort)from_t.Convert(restrictions[i]);
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Count(_body, array, (nuint)restrictions.Count));
                    }
                    case uint:
                    {
                        var array = stackalloc uint[restrictions.Count];
                        for (var i = 0; i < restrictions.Count; i++)
                        {
                            array[i] = (uint)from_t.Convert(restrictions[i]);
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Count(_body, array, (nuint)restrictions.Count));
                    }
                    case ulong:
                    {
                        {
                            var array = stackalloc UInt64[restrictions.Count];
                            for (var i = 0; i < restrictions.Count; i++)
                            {
                                array[i] = from_t.Convert(restrictions[i]);
                            }
                            return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Count(_body, array, (nuint)restrictions.Count));
                        }
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> restrictions)
        {
            unsafe
            {
                TLink t = default;
                switch (t)
                {
                    case byte:
                    {
                        Methods.EachCallback_Uint8 callback = (link) => (byte)from_t.Convert(handler(new Link<TLink>(from_u8.Convert(link.Index), from_u8.Convert(link.Source), from_u8.Convert(link.Target))));

                        var array = stackalloc byte[restrictions.Count];
                        for (var i = 0; i < restrictions.Count; i++)
                        {
                            array[i] = (byte)from_t.Convert(restrictions[i]);
                        }
                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Each(_body, callback, array, (nuint)restrictions.Count));
                    }
                    case ushort:
                    {
                        Methods.EachCallback_Uint16 callback = (link) => (ushort)from_t.Convert(handler(new Link<TLink>(from_u16.Convert(link.Index), from_u16.Convert(link.Source), from_u16.Convert(link.Target))));

                        var array = stackalloc ushort[restrictions.Count];
                        for (var i = 0; i < restrictions.Count; i++)
                        {
                            array[i] = (ushort)from_t.Convert(restrictions[i]);
                        }
                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Each(_body, callback, array, (nuint)restrictions.Count));
                    }
                    case uint:
                    {
                        Methods.EachCallback_Uint32 callback = (link) => (ushort)from_t.Convert(handler(new Link<TLink>(from_u32.Convert(link.Index), from_u32.Convert(link.Source), from_u32.Convert(link.Target))));

                        var array = stackalloc uint[restrictions.Count];
                        for (var i = 0; i < restrictions.Count; i++)
                        {
                            array[i] = (uint)from_t.Convert(restrictions[i]);
                        }
                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Each(_body, callback, array, (nuint)restrictions.Count));
                    }
                    case ulong:
                    {
                        {
                            Methods.EachCallback_Uint64 callback = (link) => from_t.Convert(handler(new Link<TLink>(from_u64.Convert(link.Index), from_u64.Convert(link.Source), from_u64.Convert(link.Target))));

                            var array = stackalloc UInt64[restrictions.Count];
                            for (var i = 0; i < restrictions.Count; i++)
                            {
                                array[i] = from_t.Convert(restrictions[i]);
                            }
                            return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Each(_body, callback, array, (nuint)restrictions.Count));
                        }
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

                        public TLink Create(IList<TLink> restrictions)
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

                        public TLink Update(IList<TLink> restrictions, IList<TLink> substitution)
                        {
                            unsafe
                            {
                                TLink t = default;
                                switch (t)
                                {
                                    case Byte _:
                                        return from_u8.Convert(Methods.ByteUnitedMemoryLinks_Update(_body, (Byte)from_t.Convert(restrictions[0]), (Byte)from_t.Convert(substitution[1]), (Byte)from_t.Convert(substitution[2])));
                                    case UInt16 _:
                                        return from_u16.Convert(Methods.UInt16UnitedMemoryLinks_Update(_body, (UInt16)from_t.Convert(restrictions[0]), (UInt16)from_t.Convert(substitution[1]), (UInt16)from_t.Convert(substitution[2])));
                                    case UInt32 _:
                                        return from_u32.Convert(Methods.UInt32UnitedMemoryLinks_Update(_body, (UInt32)from_t.Convert(restrictions[0]), (UInt32)from_t.Convert(substitution[1]), (UInt32)from_t.Convert(substitution[2])));
                                    case UInt64:
                                        _:
                                        return from_u64.Convert(Methods.UInt64UnitedMemoryLinks_Update(_body, (UInt64)from_t.Convert(restrictions[0]), (UInt64)from_t.Convert(substitution[1]), (UInt64)from_t.Convert(substitution[2])));
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                        }

                        public void Delete(IList<TLink> restrictions)
                        {
                            unsafe
                            {
                                TLink t = default;
                                switch (t)
                                {
                                    case Byte _:
                                        Methods.ByteUnitedMemoryLinks_Delete(_body, (Byte)from_t.Convert(restrictions[0]));
                                        return;
                                    case UInt16 _:
                                        Methods.UInt16UnitedMemoryLinks_Delete(_body, (UInt16)from_t.Convert(restrictions[0]));
                                        return;
                                    case UInt32 _:
                                        Methods.UInt32UnitedMemoryLinks_Delete(_body, (UInt32)from_t.Convert(restrictions[0]));
                                        return;
                                    case UInt64:
                                        _:
                                        Methods.UInt64UnitedMemoryLinks_Delete(_body, (UInt64)from_t.Convert(restrictions[0]));
                                        return;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                        }

        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (wasDisposed) 
            {
            return;
            }
            TLink t = default;
            unsafe
            {
                switch (t)
                {
                    case Byte t8:
                        Methods.ByteUnitedMemoryLinks_Drop(_body);
                        break;
                    case UInt16 t16:
                        Methods.UInt16UnitedMemoryLinks_Drop(_body);
                        break;
                    case UInt32 t32:
                        Methods.UInt32UnitedMemoryLinks_Drop(_body);
                        break;
                    case UInt64:
                        t64:
                        Methods.UInt64UnitedMemoryLinks_Drop(_body);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
