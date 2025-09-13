using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Memory;
using Platform.Data.Doublets.Memory.United;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.United.Generic
{
    /// <summary>
    /// <para>
    /// Represents a basic united memory links implementation without indexing.
    /// </para>
    /// <para>
    /// This implementation provides core link operations using linear search,
    /// making it slower but simpler than indexed implementations.
    /// </para>
    /// </summary>
    public unsafe class BasicUnitedMemoryLinks<TLinkAddress> : UnitedMemoryLinksBase<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private byte* _header;
        private byte* _links;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="BasicUnitedMemoryLinks"/> instance with file storage.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="address">
        /// <para>The file path for the links database.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BasicUnitedMemoryLinks(string address) : this(address, DefaultLinksSizeStep) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="BasicUnitedMemoryLinks"/> instance with file storage and custom step size.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="address">
        /// <para>The file path for the links database.</para>
        /// <para></para>
        /// </param>
        /// <param name="memoryReservationStep">
        /// <para>The memory reservation step size in bytes.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BasicUnitedMemoryLinks(string address, long memoryReservationStep) 
            : this(new FileMappedResizableDirectMemory(address, memoryReservationStep), memoryReservationStep) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="BasicUnitedMemoryLinks"/> instance with memory storage.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="memory">
        /// <para>The resizable direct memory instance.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BasicUnitedMemoryLinks(IResizableDirectMemory memory) : this(memory, DefaultLinksSizeStep) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="BasicUnitedMemoryLinks"/> instance with memory storage and custom step size.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="memory">
        /// <para>The resizable direct memory instance.</para>
        /// <para></para>
        /// </param>
        /// <param name="memoryReservationStep">
        /// <para>The memory reservation step size in bytes.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BasicUnitedMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep) 
            : this(memory, memoryReservationStep, LinksConstants<TLinkAddress>.Default) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="BasicUnitedMemoryLinks"/> instance with full configuration.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="memory">
        /// <para>The resizable direct memory instance.</para>
        /// <para></para>
        /// </param>
        /// <param name="memoryReservationStep">
        /// <para>The memory reservation step size in bytes.</para>
        /// <para></para>
        /// </param>
        /// <param name="constants">
        /// <para>The links constants.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BasicUnitedMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep, LinksConstants<TLinkAddress> constants) 
            : base(memory, memoryReservationStep, constants)
        {
            Init(memory, memoryReservationStep);
        }

        /// <summary>
        /// <para>
        /// Sets up memory pointers and initializes basic tree methods.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="memory">
        /// <para>The resizable direct memory instance.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetPointers(IResizableDirectMemory memory)
        {
            _links = (byte*)memory.Pointer;
            _header = _links;
            
            // Initialize basic tree methods that perform linear search
            SourcesTreeMethods = new BasicLinksTreeMethods<TLinkAddress>(Constants, _links, _header, indexBySource: true);
            TargetsTreeMethods = new BasicLinksTreeMethods<TLinkAddress>(Constants, _links, _header, indexBySource: false);
            UnusedLinksListMethods = new UnusedLinksListMethods<TLinkAddress>(_links, _header);
        }

        /// <summary>
        /// <para>
        /// Resets memory pointers.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ResetPointers()
        {
            base.ResetPointers();
            _links = null;
            _header = null;
        }

        /// <summary>
        /// <para>
        /// Gets the header reference for direct memory access.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>Reference to the links header.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ref LinksHeader<TLinkAddress> GetHeaderReference()
        {
            return ref System.Runtime.CompilerServices.Unsafe.AsRef<LinksHeader<TLinkAddress>>(_header);
        }

        /// <summary>
        /// <para>
        /// Gets the link reference for the specified index.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkIndex">
        /// <para>The link index.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>Reference to the raw link data.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ref RawLink<TLinkAddress> GetLinkReference(TLinkAddress linkIndex)
        {
            return ref System.Runtime.CompilerServices.Unsafe.AsRef<RawLink<TLinkAddress>>(_links + (RawLink<TLinkAddress>.SizeInBytes * System.Runtime.CompilerServices.Unsafe.As<TLinkAddress, long>(ref linkIndex)));
        }
    }
}