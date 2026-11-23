using System;
using System.IO;
using System.Runtime.InteropServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory;

/// <summary>
///     <para>
///         Provides system memory and storage information.
///     </para>
///     <para></para>
/// </summary>
public static class SystemMemoryInfo
{
    /// <summary>
    ///     <para>
    ///         Gets the total system RAM in bytes.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>Total system RAM in bytes</para>
    ///     <para></para>
    /// </returns>
    public static long GetTotalPhysicalMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetTotalPhysicalMemoryWindows();
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetTotalPhysicalMemoryLinux();
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetTotalPhysicalMemoryMacOS();
        }
        
        // Fallback: use GC memory limit as approximation
        return GC.GetTotalMemory(false) * 10; // Rough approximation
    }

    /// <summary>
    ///     <para>
    ///         Gets the available system RAM in bytes.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>Available system RAM in bytes</para>
    ///     <para></para>
    /// </returns>
    public static long GetAvailablePhysicalMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetAvailablePhysicalMemoryWindows();
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetAvailablePhysicalMemoryLinux();
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetAvailablePhysicalMemoryMacOS();
        }
        
        // Fallback: assume 50% of total memory is available
        return GetTotalPhysicalMemory() / 2;
    }

    /// <summary>
    ///     <para>
    ///         Gets the total disk space for the specified path in bytes.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="path">
    ///     <para>The file or directory path to check</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>Total disk space in bytes</para>
    ///     <para></para>
    /// </returns>
    public static long GetTotalDiskSpace(string path)
    {
        var driveInfo = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path)) ?? "C:\\");
        return driveInfo.TotalSize;
    }

    /// <summary>
    ///     <para>
    ///         Gets the available disk space for the specified path in bytes.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="path">
    ///     <para>The file or directory path to check</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>Available disk space in bytes</para>
    ///     <para></para>
    /// </returns>
    public static long GetAvailableDiskSpace(string path)
    {
        var driveInfo = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path)) ?? "C:\\");
        return driveInfo.AvailableFreeSpace;
    }

    private static long GetTotalPhysicalMemoryWindows()
    {
        try
        {
            var memStatus = new MEMORYSTATUSEX();
            memStatus.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
            if (GlobalMemoryStatusEx(ref memStatus))
            {
                return (long)memStatus.ullTotalPhys;
            }
        }
        catch
        {
            // Fall through to alternative method
        }

        // Alternative method using performance counter
        return Environment.WorkingSet; // This is not ideal but serves as fallback
    }

    private static long GetAvailablePhysicalMemoryWindows()
    {
        try
        {
            var memStatus = new MEMORYSTATUSEX();
            memStatus.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
            if (GlobalMemoryStatusEx(ref memStatus))
            {
                return (long)memStatus.ullAvailPhys;
            }
        }
        catch
        {
            // Fall through to fallback
        }

        return GetTotalPhysicalMemoryWindows() / 2; // Assume 50% available as fallback
    }

    private static long GetTotalPhysicalMemoryLinux()
    {
        try
        {
            var memInfo = File.ReadAllText("/proc/meminfo");
            foreach (var line in memInfo.Split('\n'))
            {
                if (line.StartsWith("MemTotal:", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1], out var memKb))
                    {
                        return memKb * 1024; // Convert KB to bytes
                    }
                }
            }
        }
        catch
        {
            // Fall through to fallback
        }

        return Environment.WorkingSet; // Fallback
    }

    private static long GetAvailablePhysicalMemoryLinux()
    {
        try
        {
            var memInfo = File.ReadAllText("/proc/meminfo");
            long available = 0;
            foreach (var line in memInfo.Split('\n'))
            {
                if (line.StartsWith("MemAvailable:", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1], out var memKb))
                    {
                        return memKb * 1024; // Convert KB to bytes
                    }
                }
            }

            // If MemAvailable is not present, calculate from MemFree + Buffers + Cached
            long memFree = 0, buffers = 0, cached = 0;
            foreach (var line in memInfo.Split('\n'))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && long.TryParse(parts[1], out var value))
                {
                    if (line.StartsWith("MemFree:", StringComparison.OrdinalIgnoreCase))
                        memFree = value;
                    else if (line.StartsWith("Buffers:", StringComparison.OrdinalIgnoreCase))
                        buffers = value;
                    else if (line.StartsWith("Cached:", StringComparison.OrdinalIgnoreCase))
                        cached = value;
                }
            }
            return (memFree + buffers + cached) * 1024; // Convert KB to bytes
        }
        catch
        {
            // Fall through to fallback
        }

        return GetTotalPhysicalMemoryLinux() / 2; // Assume 50% available as fallback
    }

    private static long GetTotalPhysicalMemoryMacOS()
    {
        // For macOS, we would need to use sysctlbyname or similar
        // For now, use a simple fallback
        return Environment.WorkingSet * 4; // Rough approximation
    }

    private static long GetAvailablePhysicalMemoryMacOS()
    {
        return GetTotalPhysicalMemoryMacOS() / 2; // Assume 50% available as fallback
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
}