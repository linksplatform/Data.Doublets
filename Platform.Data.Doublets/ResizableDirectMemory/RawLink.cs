using Platform.Unsafe;
using System.Runtime.InteropServices;

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    internal struct RawLink<TLink>
    {
        public static readonly long SizeInBytes = Structure<RawLink<TLink>>.Size;
        public static readonly long SourceOffset = Marshal.OffsetOf(typeof(RawLink<TLink>), nameof(Source)).ToInt64();
        public static readonly long TargetOffset = Marshal.OffsetOf(typeof(RawLink<TLink>), nameof(Target)).ToInt64();
        public static readonly long LeftAsSourceOffset = Marshal.OffsetOf(typeof(RawLink<TLink>), nameof(LeftAsSource)).ToInt64();
        public static readonly long RightAsSourceOffset = Marshal.OffsetOf(typeof(RawLink<TLink>), nameof(RightAsSource)).ToInt64();
        public static readonly long SizeAsSourceOffset = Marshal.OffsetOf(typeof(RawLink<TLink>), nameof(SizeAsSource)).ToInt64();
        public static readonly long LeftAsTargetOffset = Marshal.OffsetOf(typeof(RawLink<TLink>), nameof(LeftAsTarget)).ToInt64();
        public static readonly long RightAsTargetOffset = Marshal.OffsetOf(typeof(RawLink<TLink>), nameof(RightAsTarget)).ToInt64();
        public static readonly long SizeAsTargetOffset = Marshal.OffsetOf(typeof(RawLink<TLink>), nameof(SizeAsTarget)).ToInt64();

        public TLink Source;
        public TLink Target;
        public TLink LeftAsSource;
        public TLink RightAsSource;
        public TLink SizeAsSource;
        public TLink LeftAsTarget;
        public TLink RightAsTarget;
        public TLink SizeAsTarget;
    }
}