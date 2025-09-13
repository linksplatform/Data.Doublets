using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Platform.Memory;
using Platform.Singletons;
using Platform.Data.Doublets.Memory;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.United.Generic
{
    /// <summary>
    /// <para>
    /// Represents the shared access united memory links that allows multiple processes to access the same database file simultaneously.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <seealso cref="UnitedMemoryLinks{TLinkAddress}"/>
    public unsafe class SharedAccessUnitedMemoryLinks<TLinkAddress> : UnitedMemoryLinks<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress,int,TLinkAddress>, IBitwiseOperators<TLinkAddress,TLinkAddress,TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private readonly Mutex _globalMutex;
        private readonly string _mutexName;
        private readonly string _databasePath;
        private volatile bool _isDisposed;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SharedAccessUnitedMemoryLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="address">
        /// <para>The full path to the database file.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedAccessUnitedMemoryLinks(string address) : this(address, DefaultLinksSizeStep) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SharedAccessUnitedMemoryLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="address">
        /// <para>The full path to the database file.</para>
        /// <para></para>
        /// </param>
        /// <param name="memoryReservationStep">
        /// <para>The minimum database expansion step in bytes.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedAccessUnitedMemoryLinks(string address, long memoryReservationStep) 
            : this(new SharedAccessFileMappedResizableDirectMemory(address, memoryReservationStep), memoryReservationStep, address) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="SharedAccessUnitedMemoryLinks"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="memory">
        /// <para>A shared access memory adapter.</para>
        /// <para></para>
        /// </param>
        /// <param name="memoryReservationStep">
        /// <para>A memory reservation step.</para>
        /// <para></para>
        /// </param>
        /// <param name="databasePath">
        /// <para>The path to the database file for mutex naming.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SharedAccessUnitedMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep, string databasePath) 
            : base(memory, memoryReservationStep, Default<LinksConstants<TLinkAddress>>.Instance, IndexTreeType.Default)
        {
            _databasePath = databasePath;
            _mutexName = $"Global\\LinksDoublets_{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(databasePath)).Replace("/", "_").Replace("+", "-").Replace("=", "")}";
            
            // Create or open a named mutex for inter-process synchronization
            _globalMutex = new Mutex(false, _mutexName);
            
            // Initialize the shared lock information in the database header
            InitializeSharedLock();
        }

        /// <summary>
        /// <para>
        /// Initializes the shared lock information in the database header.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeSharedLock()
        {
            _globalMutex.WaitOne();
            try
            {
                ref var header = ref GetHeaderReference();
                
                // Use Reserved8 field to store lock state information
                // Bit 0: Shared access enabled flag (1 = enabled, 0 = disabled)
                // Bits 1-31: Process counter (number of processes accessing the file)
                var lockValue = TLinkAddress.CreateTruncating(header.Reserved8);
                var lockValueAsUlong = Convert.ToUInt64(lockValue);
                
                // Check if this is the first time accessing this database with shared access
                if ((lockValueAsUlong & 1) == 0)
                {
                    // Enable shared access and set process counter to 1
                    lockValueAsUlong = 1 | (1UL << 1); // Set enabled bit and process counter to 1
                }
                else
                {
                    // Increment process counter
                    var processCount = (lockValueAsUlong >> 1) + 1;
                    lockValueAsUlong = 1 | (processCount << 1);
                }
                
                header.Reserved8 = TLinkAddress.CreateTruncating(lockValueAsUlong);
            }
            finally
            {
                _globalMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// <para>
        /// Executes a read operation with inter-process synchronization.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="operation">
        /// <para>The read operation to execute.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The result of the read operation.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult ExecuteRead<TResult>(Func<TResult> operation)
        {
            _globalMutex.WaitOne();
            try
            {
                return operation();
            }
            finally
            {
                _globalMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// <para>
        /// Executes a write operation with inter-process synchronization.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="operation">
        /// <para>The write operation to execute.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The result of the write operation.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult ExecuteWrite<TResult>(Func<TResult> operation)
        {
            _globalMutex.WaitOne();
            try
            {
                return operation();
            }
            finally
            {
                _globalMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// <para>
        /// Gets the header reference for testing purposes.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>A reference to the links header.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new ref LinksHeader<TLinkAddress> GetHeaderReference()
        {
            return ref base.GetHeaderReference();
        }

        /// <summary>
        /// <para>
        /// Disposes the shared access resources.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="manual">
        /// <para>A manual.</para>
        /// <para></para>
        /// </param>
        protected override void Dispose(bool manual)
        {
            if (!_isDisposed)
            {
                if (manual)
                {
                    // Decrement the process counter in the database header
                    _globalMutex?.WaitOne();
                    try
                    {
                        ref var header = ref GetHeaderReference();
                        var lockValue = TLinkAddress.CreateTruncating(header.Reserved8);
                        var lockValueAsUlong = Convert.ToUInt64(lockValue);
                        
                        if ((lockValueAsUlong & 1) == 1) // If shared access is enabled
                        {
                            var processCount = (lockValueAsUlong >> 1);
                            if (processCount > 1)
                            {
                                // Decrement process counter
                                processCount--;
                                lockValueAsUlong = 1 | (processCount << 1);
                            }
                            else
                            {
                                // Last process - disable shared access
                                lockValueAsUlong = 0;
                            }
                            
                            header.Reserved8 = TLinkAddress.CreateTruncating(lockValueAsUlong);
                        }
                    }
                    finally
                    {
                        _globalMutex?.ReleaseMutex();
                    }

                    _globalMutex?.Close();
                }
                
                _isDisposed = true;
            }

            base.Dispose(manual);
        }
    }
}