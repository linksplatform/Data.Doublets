using System;
using System.Runtime.CompilerServices;
using Platform.Converters;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Time
{
    public class LongRawNumberSequenceToDateTimeConverter<TLink> : IConverter<TLink, DateTime>
    {
        private readonly IConverter<TLink, long> _longRawNumberConverterToInt64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LongRawNumberSequenceToDateTimeConverter(IConverter<TLink, long> longRawNumberConverterToInt64) => _longRawNumberConverterToInt64 = longRawNumberConverterToInt64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime Convert(TLink source) => DateTime.FromFileTimeUtc(_longRawNumberConverterToInt64.Convert(source));
    }
}
