using System.Runtime.CompilerServices;
using Platform.Numbers;
using Platform.Memory;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    public unsafe partial class ResizableDirectMemoryLinks<TLink> : ResizableDirectMemoryLinksBase<TLink>
    {
        public static readonly long LinkHeaderSizeInBytes = LinksHeader<TLink>.SizeInBytes;
        public static readonly long DefaultLinksSizeStep = LinkSizeInBytes * 1024 * 1024;

        private byte* _header;
        private byte* _links;

        public ResizableDirectMemoryLinks(string address) : this(address, DefaultLinksSizeStep) { }

        /// <summary>
        /// Создаёт экземпляр базы данных Links в файле по указанному адресу, с указанным минимальным шагом расширения базы данных.
        /// </summary>
        /// <param name="address">Полный пусть к файлу базы данных.</param>
        /// <param name="memoryReservationStep">Минимальный шаг расширения базы данных в байтах.</param>
        public ResizableDirectMemoryLinks(string address, long memoryReservationStep) : this(new FileMappedResizableDirectMemory(address, memoryReservationStep), memoryReservationStep) { }

        public ResizableDirectMemoryLinks(IResizableDirectMemory memory) : this(memory, DefaultLinksSizeStep) { }

        public ResizableDirectMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep)
            : base(memory, memoryReservationStep)
        {
            if (memory.ReservedCapacity < memoryReservationStep)
            {
                memory.ReservedCapacity = memoryReservationStep;
            }
            SetPointers(_memory);
            ref var header = ref GetHeaderReference();
            // Гарантия корректности _memory.UsedCapacity относительно _header->AllocatedLinks
            _memory.UsedCapacity = ((Integer<TLink>)header.AllocatedLinks * LinkSizeInBytes) + LinkHeaderSizeInBytes;
            // Гарантия корректности _header->ReservedLinks относительно _memory.ReservedCapacity
            header.ReservedLinks = (Integer<TLink>)((_memory.ReservedCapacity - LinkHeaderSizeInBytes) / LinkSizeInBytes);
        }

        /// <remarks>
        /// TODO: Возможно это должно быть событием, вызываемым из IMemory, в том случае, если адрес реально поменялся
        /// 
        /// Указатель this.links может быть в том же месте, 
        /// так как 0-я связь не используется и имеет такой же размер как Header,
        /// поэтому header размещается в том же месте, что и 0-я связь
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetPointers(IResizableDirectMemory memory)
        {
            if (memory == null)
            {
                _links = null;
                _header = _links;
                SourcesTreeMethods = null;
                TargetsTreeMethods = null;
                UnusedLinksListMethods = null;
            }
            else
            {
                _links = (byte*)memory.Pointer;
                _header = _links;
                SourcesTreeMethods = new LinksSourcesAVLBalancedTreeMethods<TLink>(Constants, _links, _header);
                TargetsTreeMethods = new LinksTargetsAVLBalancedTreeMethods<TLink>(Constants, _links, _header);
                UnusedLinksListMethods = new UnusedLinksListMethods<TLink>(_links, _header);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ref LinksHeader<TLink> GetHeaderReference() => ref AsRef<LinksHeader<TLink>>(_header);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ref RawLink<TLink> GetLinkReference(TLink linkIndex) => ref AsRef<RawLink<TLink>>(_links + LinkSizeInBytes * (Integer<TLink>)linkIndex);
    }
}