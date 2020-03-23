using System.Runtime.CompilerServices;
using Platform.Collections.Stacks;
using Platform.Converters;
using Platform.Numbers;
using Platform.Reflection;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Sequences.Walkers;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Numbers.Raw
{
    public class LongRawNumberSequenceToNumberConverter<TSource, TTarget> : LinksDecoratorBase<TSource>, IConverter<TSource, TTarget>
    {
        private static readonly int _bitsPerRawNumber = NumericType<TSource>.BitsSize - 1;
        private static readonly UncheckedConverter<TSource, TTarget> _sourceToTargetConverter = UncheckedConverter<TSource, TTarget>.Default;

        private readonly IConverter<TSource> _numberToAddressConverter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LongRawNumberSequenceToNumberConverter(ILinks<TSource> links, IConverter<TSource> numberToAddressConverter) : base(links) => _numberToAddressConverter = numberToAddressConverter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TTarget Convert(TSource source)
        {
            var constants = Links.Constants;
            var externalReferencesRange = constants.ExternalReferencesRange;
            if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(source))
            {
                return _sourceToTargetConverter.Convert(_numberToAddressConverter.Convert(source));
            }
            else
            {
                var pair = Links.GetLink(source);
                var walker = new LeftSequenceWalker<TSource>(Links, new DefaultStack<TSource>(), (link) => externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(link));
                TTarget result = default;
                foreach (var element in walker.Walk(source))
                {
                    result = Bit.Or(Bit.ShiftLeft(result, _bitsPerRawNumber), Convert(element));
                }
                return result;
            }
        }
    }
}
