namespace Platform.Data.Doublets.ResizableDirectMemory
{
    internal struct UInt64RawLink
    {
        public ulong Source;
        public ulong Target;
        public ulong LeftAsSource;
        public ulong RightAsSource;
        public ulong SizeAsSource;
        public ulong LeftAsTarget;
        public ulong RightAsTarget;
        public ulong SizeAsTarget;
    }
}
