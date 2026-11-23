using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.NativeLibrary
{
    public static unsafe class DoubletsNativeLibrary
    {
        private static readonly Dictionary<IntPtr, UnitedMemoryLinks<ulong>> _instances = new();
        private static readonly object _lock = new object();
        private static IntPtr _nextHandle = new IntPtr(1);

        [UnmanagedCallersOnly(EntryPoint = "UInt64UnitedMemoryLinks_New")]
        public static IntPtr CreateLinks(IntPtr pathPtr)
        {
            try
            {
                var path = Marshal.PtrToStringAnsi(pathPtr);
                if (string.IsNullOrEmpty(path))
                    return IntPtr.Zero;

                var links = new UnitedMemoryLinks<ulong>(path);
                
                lock (_lock)
                {
                    var handle = _nextHandle;
                    _nextHandle = new IntPtr(_nextHandle.ToInt64() + 1);
                    _instances[handle] = links;
                    return handle;
                }
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "UInt64UnitedMemoryLinks_Drop")]
        public static void DisposeLinks(IntPtr handle)
        {
            try
            {
                lock (_lock)
                {
                    if (_instances.TryGetValue(handle, out var links))
                    {
                        links.Dispose();
                        _instances.Remove(handle);
                    }
                }
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "UInt64UnitedMemoryLinks_Create")]
        public static ulong CreateLink(IntPtr handle, ulong* query, nuint queryLen)
        {
            try
            {
                lock (_lock)
                {
                    if (!_instances.TryGetValue(handle, out var links))
                        return 0;

                    var queryArray = new ulong[queryLen];
                    for (int i = 0; i < (int)queryLen; i++)
                    {
                        queryArray[i] = query[i];
                    }

                    return links.Create(queryArray);
                }
            }
            catch
            {
                return 0;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "UInt64UnitedMemoryLinks_Count")]
        public static ulong CountLinks(IntPtr handle, ulong* query, nuint queryLen)
        {
            try
            {
                lock (_lock)
                {
                    if (!_instances.TryGetValue(handle, out var links))
                        return 0;

                    var queryArray = new ulong[queryLen];
                    for (int i = 0; i < (int)queryLen; i++)
                    {
                        queryArray[i] = query[i];
                    }

                    return links.Count(queryArray);
                }
            }
            catch
            {
                return 0;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "UInt64UnitedMemoryLinks_Update")]
        public static ulong UpdateLink(IntPtr handle, ulong* query, nuint queryLen, ulong* replacement, nuint replacementLen)
        {
            try
            {
                lock (_lock)
                {
                    if (!_instances.TryGetValue(handle, out var links))
                        return 0;

                    var queryArray = new ulong[queryLen];
                    for (int i = 0; i < (int)queryLen; i++)
                    {
                        queryArray[i] = query[i];
                    }

                    var replacementArray = new ulong[replacementLen];
                    for (int i = 0; i < (int)replacementLen; i++)
                    {
                        replacementArray[i] = replacement[i];
                    }

                    return links.Update(queryArray, replacementArray);
                }
            }
            catch
            {
                return 0;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "UInt64UnitedMemoryLinks_Delete")]
        public static ulong DeleteLink(IntPtr handle, ulong* query, nuint queryLen)
        {
            try
            {
                lock (_lock)
                {
                    if (!_instances.TryGetValue(handle, out var links))
                        return 0;

                    var queryArray = new ulong[queryLen];
                    for (int i = 0; i < (int)queryLen; i++)
                    {
                        queryArray[i] = query[i];
                    }

                    return links.Delete(queryArray);
                }
            }
            catch
            {
                return 0;
            }
        }

        // Basic version info export
        [UnmanagedCallersOnly(EntryPoint = "GetLibraryVersion")]
        public static IntPtr GetVersion()
        {
            var version = "0.1.0-nativeaot";
            var ptr = Marshal.StringToHGlobalAnsi(version);
            return ptr;
        }
    }
}