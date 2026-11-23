using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using Platform.Memory;
using Platform.Disposables;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory
{
    /// <summary>
    /// <para>
    /// Represents a memory block stored as a file on disk with shared access support for multiple processes.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <seealso cref="DisposableBase"/>
    /// <seealso cref="IResizableDirectMemory"/>
    public unsafe class SharedAccessFileMappedResizableDirectMemory : DisposableBase, IResizableDirectMemory
    {
        private readonly string _filename;
        private readonly long _initialSize;
        private FileStream _fileStream;
        private MemoryMappedFile _memoryMappedFile;
        private MemoryMappedViewAccessor _accessor;
        private byte* _pointer;
        private long _reservedCapacity;
        private long _usedCapacity;

        /// <summary>
        /// <para>
        /// Gets the pointer to the memory block.
        /// </para>
        /// <para></para>
        /// </summary>
        public IntPtr Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new IntPtr(_pointer);
        }

        /// <summary>
        /// <para>
        /// Gets or sets the reserved capacity of the memory block.
        /// </para>
        /// <para></para>
        /// </summary>
        public long ReservedCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _reservedCapacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => OnReservedCapacityChanged(value);
        }

        /// <summary>
        /// <para>
        /// Gets or sets the used capacity of the memory block.
        /// </para>
        /// <para></para>
        /// </summary>
        public long UsedCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _usedCapacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _usedCapacity = value;
        }

        /// <summary>
        /// <para>
        /// Gets the size of the memory block.
        /// </para>
        /// <para></para>
        /// </summary>
        public long Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _reservedCapacity;
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SharedAccessFileMappedResizableDirectMemory"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="filename">
        /// <para>The path to the file.</para>
        /// <para></para>
        /// </param>
        /// <param name="initialSize">
        /// <para>The initial size of the memory block.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedAccessFileMappedResizableDirectMemory(string filename, long initialSize)
        {
            _filename = filename;
            _initialSize = initialSize;
            
            InitializeFile();
            InitializeMemoryMapping();
        }

        /// <summary>
        /// <para>
        /// Initializes the file for shared access.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeFile()
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_filename);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Open or create the file with shared read/write access
            _fileStream = new FileStream(_filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            
            // Set initial size if file is empty
            if (_fileStream.Length == 0)
            {
                _fileStream.SetLength(_initialSize);
            }
            
            _reservedCapacity = _fileStream.Length;
            _usedCapacity = _fileStream.Length;
        }

        /// <summary>
        /// <para>
        /// Initializes the memory mapping.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeMemoryMapping()
        {
            // On Windows, try to use named memory-mapped files for better sharing
            // On other platforms, use anonymous memory-mapped files backed by the file
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var mapName = $"LinksDoublets_Map_{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_filename)).Replace("/", "_").Replace("+", "-").Replace("=", "")}";
                
                try
                {
                    // Try to open existing memory-mapped file first
                    _memoryMappedFile = MemoryMappedFile.OpenExisting(mapName, MemoryMappedFileRights.ReadWrite);
                }
                catch (FileNotFoundException)
                {
                    // Create new memory-mapped file if it doesn't exist
                    _memoryMappedFile = MemoryMappedFile.CreateFromFile(_fileStream, mapName, _reservedCapacity, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
                }
            }
            else
            {
                // On non-Windows platforms, create memory-mapped file without name (anonymous)
                _memoryMappedFile = MemoryMappedFile.CreateFromFile(_fileStream, null, _reservedCapacity, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
            }

            // Create accessor for the entire file
            _accessor = _memoryMappedFile.CreateViewAccessor(0, _reservedCapacity, MemoryMappedFileAccess.ReadWrite);
            _pointer = (byte*)_accessor.SafeMemoryMappedViewHandle.DangerousGetHandle().ToPointer();
        }

        /// <summary>
        /// <para>
        /// Handles changes to the reserved capacity.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="newReservedCapacity">
        /// <para>The new reserved capacity.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnReservedCapacityChanged(long newReservedCapacity)
        {
            if (newReservedCapacity <= _reservedCapacity)
            {
                return;
            }

            // Dispose current mapping
            DisposeMapping();

            // Resize the file
            _fileStream.SetLength(newReservedCapacity);
            _reservedCapacity = newReservedCapacity;

            // Recreate memory mapping with new size
            // On non-Windows platforms, need to recreate the memory mapping
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                _memoryMappedFile = MemoryMappedFile.CreateFromFile(_fileStream, null, _reservedCapacity, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
                _accessor = _memoryMappedFile.CreateViewAccessor(0, _reservedCapacity, MemoryMappedFileAccess.ReadWrite);
                _pointer = (byte*)_accessor.SafeMemoryMappedViewHandle.DangerousGetHandle().ToPointer();
            }
            else
            {
                InitializeMemoryMapping();
            }
        }

        /// <summary>
        /// <para>
        /// Disposes the current memory mapping.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisposeMapping()
        {
            _pointer = null;
            _accessor?.Dispose();
            _memoryMappedFile?.Dispose();
        }

        /// <summary>
        /// <para>
        /// Disposes the shared access memory resources.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="manual">
        /// <para>True if called manually, false if called by finalizer.</para>
        /// <para></para>
        /// </param>
        /// <param name="disposeBase">
        /// <para>True to dispose the base class, false otherwise.</para>
        /// <para></para>
        /// </param>
        protected override void Dispose(bool manual, bool disposeBase)
        {
            if (manual)
            {
                DisposeMapping();
                _fileStream?.Dispose();
            }
        }
    }
}