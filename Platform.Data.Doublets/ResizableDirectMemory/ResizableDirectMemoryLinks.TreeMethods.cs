using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Numbers;
using Platform.Collections.Methods.Trees;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    unsafe partial class ResizableDirectMemoryLinks<TLink>
    {
        private abstract class LinksTreeMethodsBase : SizedAndThreadedAVLBalancedTreeMethods<TLink>
        {
            //private static readonly EqualityComparer<TLink> _equalityComparer = EqualityComparer<TLink>.Default;

            private readonly ResizableDirectMemoryLinks<TLink> _memory;
            private readonly LinksConstants<TLink> _constants;
            protected readonly byte* Links;
            protected readonly byte* Header;

            protected LinksTreeMethodsBase(ResizableDirectMemoryLinks<TLink> memory)
            {
                Links = memory._links;
                Header = memory._header;
                _memory = memory;
                _constants = memory.Constants;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract TLink GetTreeRoot();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract TLink GetBasePartValue(TLink link);

            public TLink this[TLink index]
            {
                get
                {
                    var root = GetTreeRoot();
                    if (GreaterOrEqualThan(index, GetSize(root)))
                    {
                        return GetZero();
                    }
                    while (!EqualToZero(root))
                    {
                        var left = GetLeftOrDefault(root);
                        var leftSize = GetSizeOrZero(left);
                        if (LessThan(index, leftSize))
                        {
                            root = left;
                            continue;
                        }
                        if (IsEquals(index, leftSize))
                        {
                            return root;
                        }
                        root = GetRightOrDefault(root);
                        index = Subtract(index, Increment(leftSize));
                    }
                    return GetZero(); // TODO: Impossible situation exception (only if tree structure broken)
                }
            }

            // TODO: Return indices range instead of references count
            public TLink CountUsages(TLink link)
            {
                var root = GetTreeRoot();
                var total = GetSize(root);
                var totalRightIgnore = GetZero();
                while (!EqualToZero(root))
                {
                    var @base = GetBasePartValue(root);
                    if (LessOrEqualThan(@base, link))
                    {
                        root = GetRightOrDefault(root);
                    }
                    else
                    {
                        totalRightIgnore = Add(totalRightIgnore, Increment(GetRightSize(root)));
                        root = GetLeftOrDefault(root);
                    }
                }
                root = GetTreeRoot();
                var totalLeftIgnore = GetZero();
                while (!EqualToZero(root))
                {
                    var @base = GetBasePartValue(root);
                    if (GreaterOrEqualThan(@base, link))
                    {
                        root = GetLeftOrDefault(root);
                    }
                    else
                    {
                        totalLeftIgnore = Add(totalLeftIgnore, Increment(GetLeftSize(root)));

                        root = GetRightOrDefault(root);
                    }
                }
                return Subtract(Subtract(total, totalRightIgnore), totalLeftIgnore);
            }

            public TLink EachUsage(TLink link, Func<IList<TLink>, TLink> handler)
            {
                var root = GetTreeRoot();
                if (EqualToZero(root))
                {
                    return _constants.Continue;
                }
                TLink first = GetZero(), current = root;
                while (!EqualToZero(current))
                {
                    var @base = GetBasePartValue(current);
                    if (GreaterOrEqualThan(@base, link))
                    {
                        if (IsEquals(@base, link))
                        {
                            first = current;
                        }
                        current = GetLeftOrDefault(current);
                    }
                    else
                    {
                        current = GetRightOrDefault(current);
                    }
                }
                if (!EqualToZero(first))
                {
                    current = first;
                    while (true)
                    {
                        if (IsEquals(handler(_memory.GetLinkStruct(current)), _constants.Break))
                        {
                            return _constants.Break;
                        }
                        current = GetNext(current);
                        if (EqualToZero(current) || !IsEquals(GetBasePartValue(current), link))
                        {
                            break;
                        }
                    }
                }
                return _constants.Continue;
            }

            protected override void PrintNodeValue(TLink node, StringBuilder sb)
            {
                sb.Append(' ');
                sb.Append(Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SourceOffset));
                sb.Append('-');
                sb.Append('>');
                sb.Append(Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.TargetOffset));
            }
        }

        private class LinksSourcesTreeMethods : LinksTreeMethodsBase
        {
            public LinksSourcesTreeMethods(ResizableDirectMemoryLinks<TLink> memory)
                : base(memory)
            {
            }

            protected unsafe override ref TLink GetLeftReference(TLink node) => ref AsRef<TLink>((void*)(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.LeftAsSourceOffset));

            protected unsafe override ref TLink GetRightReference(TLink node) => ref AsRef<TLink>((void*)(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.RightAsSourceOffset));

            protected override TLink GetLeft(TLink node) => Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.LeftAsSourceOffset);

            protected override TLink GetRight(TLink node) => Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.RightAsSourceOffset);

            protected override TLink GetSize(TLink node)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset);
                return Bit<TLink>.PartialRead(previousValue, 5, -5);
            }

            protected override void SetLeft(TLink node, TLink left) => Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.LeftAsSourceOffset, left);

            protected override void SetRight(TLink node, TLink right) => Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.RightAsSourceOffset, right);

            protected override void SetSize(TLink node, TLink size)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset);
                Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset, Bit<TLink>.PartialWrite(previousValue, size, 5, -5));
            }

            protected override bool GetLeftIsChild(TLink node)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset);
                //return (Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 4, 1);
                return !_equalityComparer.Equals(Bit<TLink>.PartialRead(previousValue, 4, 1), default);
            }

            protected override void SetLeftIsChild(TLink node, bool value)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset);
                var modified = Bit<TLink>.PartialWrite(previousValue, (Integer<TLink>)value, 4, 1);
                Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset, modified);
            }

            protected override bool GetRightIsChild(TLink node)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset);
                //return (Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 3, 1);
                return !_equalityComparer.Equals(Bit<TLink>.PartialRead(previousValue, 3, 1), default);
            }

            protected override void SetRightIsChild(TLink node, bool value)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset);
                var modified = Bit<TLink>.PartialWrite(previousValue, (Integer<TLink>)value, 3, 1);
                Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset, modified);
            }

            protected override sbyte GetBalance(TLink node)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset);
                var value = (ulong)(Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 0, 3);
                var unpackedValue = (sbyte)((value & 4) > 0 ? ((value & 4) << 5) | value & 3 | 124 : value & 3);
                return unpackedValue;
            }

            protected override void SetBalance(TLink node, sbyte value)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset);
                var packagedValue = (TLink)(Integer<TLink>)((((byte)value >> 5) & 4) | value & 3);
                var modified = Bit<TLink>.PartialWrite(previousValue, packagedValue, 0, 3);
                Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsSourceOffset, modified);
            }

            protected override bool FirstIsToTheLeftOfSecond(TLink first, TLink second)
            {
                var firstSource = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)first + Link.SourceOffset);
                var secondSource = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)second + Link.SourceOffset);
                return LessThan(firstSource, secondSource) ||
                       (IsEquals(firstSource, secondSource) && LessThan(Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)first + Link.TargetOffset), Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)second + Link.TargetOffset)));
            }

            protected override bool FirstIsToTheRightOfSecond(TLink first, TLink second)
            {
                var firstSource = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)first + Link.SourceOffset);
                var secondSource = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)second + Link.SourceOffset);
                return GreaterThan(firstSource, secondSource) ||
                       (IsEquals(firstSource, secondSource) && GreaterThan(Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)first + Link.TargetOffset), Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)second + Link.TargetOffset)));
            }

            protected override TLink GetTreeRoot() => Read<TLink>(Header + LinksHeader.FirstAsSourceOffset);

            protected override TLink GetBasePartValue(TLink link) => Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)link + Link.SourceOffset);

            /// <summary>
            /// Выполняет поиск и возвращает индекс связи с указанными Source (началом) и Target (концом)
            /// по дереву (индексу) связей, отсортированному по Source, а затем по Target.
            /// </summary>
            /// <param name="source">Индекс связи, которая является началом на искомой связи.</param>
            /// <param name="target">Индекс связи, которая является концом на искомой связи.</param>
            /// <returns>Индекс искомой связи.</returns>
            public TLink Search(TLink source, TLink target)
            {
                var root = GetTreeRoot();
                while (!EqualToZero(root))
                {
                    var rootSource = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)root + Link.SourceOffset);
                    var rootTarget = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)root + Link.TargetOffset);
                    if (FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget)) // node.Key < root.Key
                    {
                        root = GetLeftOrDefault(root);
                    }
                    else if (FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget)) // node.Key > root.Key
                    {
                        root = GetRightOrDefault(root);
                    }
                    else // node.Key == root.Key
                    {
                        return root;
                    }
                }
                return GetZero();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) => LessThan(firstSource, secondSource) || (IsEquals(firstSource, secondSource) && LessThan(firstTarget, secondTarget));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) => GreaterThan(firstSource, secondSource) || (IsEquals(firstSource, secondSource) && GreaterThan(firstTarget, secondTarget));
        }

        private class LinksTargetsTreeMethods : LinksTreeMethodsBase
        {
            public LinksTargetsTreeMethods(ResizableDirectMemoryLinks<TLink> memory)
                : base(memory)
            {
            }

            protected unsafe override ref TLink GetLeftReference(TLink node) => ref AsRef<TLink>((void*)(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.LeftAsTargetOffset));

            protected unsafe override ref TLink GetRightReference(TLink node) => ref AsRef<TLink>((void*)(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.RightAsTargetOffset));

            protected override TLink GetLeft(TLink node) => Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.LeftAsTargetOffset);

            protected override TLink GetRight(TLink node) => Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.RightAsTargetOffset);

            protected override TLink GetSize(TLink node)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset);
                return Bit<TLink>.PartialRead(previousValue, 5, -5);
            }

            protected override void SetLeft(TLink node, TLink left) => Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.LeftAsTargetOffset, left);

            protected override void SetRight(TLink node, TLink right) => Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.RightAsTargetOffset, right);

            protected override void SetSize(TLink node, TLink size)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset);
                Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset, Bit<TLink>.PartialWrite(previousValue, size, 5, -5));
            }

            protected override bool GetLeftIsChild(TLink node)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset);
                //return (Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 4, 1);
                return !_equalityComparer.Equals(Bit<TLink>.PartialRead(previousValue, 4, 1), default);
            }

            protected override void SetLeftIsChild(TLink node, bool value)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset);
                var modified = Bit<TLink>.PartialWrite(previousValue, (Integer<TLink>)value, 4, 1);
                Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset, modified);
            }

            protected override bool GetRightIsChild(TLink node)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset);
                //return (Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 3, 1);
                return !_equalityComparer.Equals(Bit<TLink>.PartialRead(previousValue, 3, 1), default);
            }

            protected override void SetRightIsChild(TLink node, bool value)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset);
                var modified = Bit<TLink>.PartialWrite(previousValue, (Integer<TLink>)value, 3, 1);
                Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset, modified);
            }

            protected override sbyte GetBalance(TLink node)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset);
                var value = (ulong)(Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 0, 3);
                var unpackedValue = (sbyte)((value & 4) > 0 ? ((value & 4) << 5) | value & 3 | 124 : value & 3);
                return unpackedValue;
            }

            protected override void SetBalance(TLink node, sbyte value)
            {
                var previousValue = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset);
                var packagedValue = (TLink)(Integer<TLink>)((((byte)value >> 5) & 4) | value & 3);
                var modified = Bit<TLink>.PartialWrite(previousValue, packagedValue, 0, 3);
                Write(Links + LinkSizeInBytes * (Integer<TLink>)node + Link.SizeAsTargetOffset, modified);
            }

            protected override bool FirstIsToTheLeftOfSecond(TLink first, TLink second)
            {
                var firstTarget = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)first + Link.TargetOffset);
                var secondTarget = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)second + Link.TargetOffset);
                return LessThan(firstTarget, secondTarget) ||
                       (IsEquals(firstTarget, secondTarget) && LessThan(Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)first + Link.SourceOffset), Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)second + Link.SourceOffset)));
            }

            protected override bool FirstIsToTheRightOfSecond(TLink first, TLink second)
            {
                var firstTarget = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)first + Link.TargetOffset);
                var secondTarget = Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)second + Link.TargetOffset);
                return GreaterThan(firstTarget, secondTarget) ||
                       (IsEquals(firstTarget, secondTarget) && GreaterThan(Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)first + Link.SourceOffset), Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)second + Link.SourceOffset)));
            }

            protected override TLink GetTreeRoot() => Read<TLink>(Header + LinksHeader.FirstAsTargetOffset);

            protected override TLink GetBasePartValue(TLink link) => Read<TLink>(Links + LinkSizeInBytes * (Integer<TLink>)link + Link.TargetOffset);
        }
    }
}