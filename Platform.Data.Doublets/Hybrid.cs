using System;
using System.Reflection;
using Platform.Reflection;
using Platform.Converters;
using Platform.Exceptions;
using Platform.Reflection.Sigil;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    public class Hybrid<T>
    {
        private static readonly Func<object, T> _absAndConvert;
        private static readonly Func<object, T> _absAndNegateAndConvert;

        static Hybrid()
        {
            _absAndConvert = DelegateHelpers.Compile<Func<object, T>>(emiter =>
            {
                Ensure.Always.IsUnsignedInteger<T>();
                emiter.LoadArgument(0);
                var signedVersion = Type<T>.SignedVersion;
                var signedVersionField = typeof(Type<T>).GetTypeInfo().GetField("SignedVersion", BindingFlags.Static | BindingFlags.Public);
                emiter.LoadField(signedVersionField);
                var changeTypeMethod = typeof(Convert).GetTypeInfo().GetMethod("ChangeType", Types<object, Type>.Array);
                emiter.Call(changeTypeMethod);
                emiter.UnboxAny(signedVersion);
                var absMethod = typeof(Math).GetTypeInfo().GetMethod("Abs", new[] { signedVersion });
                emiter.Call(absMethod);
                var unsignedMethod = typeof(To).GetTypeInfo().GetMethod("Unsigned", new[] { signedVersion });
                emiter.Call(unsignedMethod);
                emiter.Return();
            });
            _absAndNegateAndConvert = DelegateHelpers.Compile<Func<object, T>>(emiter =>
            {
                Ensure.Always.IsUnsignedInteger<T>();
                emiter.LoadArgument(0);
                var signedVersion = Type<T>.SignedVersion;
                var signedVersionField = typeof(Type<T>).GetTypeInfo().GetField("SignedVersion", BindingFlags.Static | BindingFlags.Public);
                emiter.LoadField(signedVersionField);
                var changeTypeMethod = typeof(Convert).GetTypeInfo().GetMethod("ChangeType", Types<object, Type>.Array);
                emiter.Call(changeTypeMethod);
                emiter.UnboxAny(signedVersion);
                var absMethod = typeof(Math).GetTypeInfo().GetMethod("Abs", new[] { signedVersion });
                emiter.Call(absMethod);
                var negateMethod = typeof(Platform.Numbers.Math).GetTypeInfo().GetMethod("Negate").MakeGenericMethod(signedVersion);
                emiter.Call(negateMethod);
                var unsignedMethod = typeof(To).GetTypeInfo().GetMethod("Unsigned", new[] { signedVersion });
                emiter.Call(unsignedMethod);
                emiter.Return();
            });
        }

        public readonly T Value;
        public bool IsNothing => Convert.ToInt64(To.Signed(Value)) == 0;
        public bool IsInternal => Convert.ToInt64(To.Signed(Value)) > 0;
        public bool IsExternal => Convert.ToInt64(To.Signed(Value)) < 0;
        public long AbsoluteValue => Platform.Numbers.Math.Abs(Convert.ToInt64(To.Signed(Value)));

        public Hybrid(T value)
        {
            Ensure.OnDebug.IsUnsignedInteger<T>();
            Value = value;
        }

        public Hybrid(object value) => Value = To.UnsignedAs<T>(Convert.ChangeType(value, Type<T>.SignedVersion));

        public Hybrid(object value, bool isExternal)
        {
            //var signedType = Type<T>.SignedVersion;
            //var signedValue = Convert.ChangeType(value, signedType);
            //var abs = typeof(Platform.Numbers.Math).GetTypeInfo().GetMethod("Abs").MakeGenericMethod(signedType);
            //var negate = typeof(Platform.Numbers.Math).GetTypeInfo().GetMethod("Negate").MakeGenericMethod(signedType);
            //var absoluteValue = abs.Invoke(null, new[] { signedValue });
            //var resultValue = isExternal ? negate.Invoke(null, new[] { absoluteValue }) : absoluteValue;
            //Value = To.UnsignedAs<T>(resultValue);
            if (isExternal)
            {
                Value = _absAndNegateAndConvert(value);
            }
            else
            {
                Value = _absAndConvert(value);
            }
        }

        public static implicit operator Hybrid<T>(T integer) => new Hybrid<T>(integer);

        public static explicit operator Hybrid<T>(ulong integer) => new Hybrid<T>(integer);

        public static explicit operator Hybrid<T>(long integer) => new Hybrid<T>(integer);

        public static explicit operator Hybrid<T>(uint integer) => new Hybrid<T>(integer);

        public static explicit operator Hybrid<T>(int integer) => new Hybrid<T>(integer);

        public static explicit operator Hybrid<T>(ushort integer) => new Hybrid<T>(integer);

        public static explicit operator Hybrid<T>(short integer) => new Hybrid<T>(integer);

        public static explicit operator Hybrid<T>(byte integer) => new Hybrid<T>(integer);

        public static explicit operator Hybrid<T>(sbyte integer) => new Hybrid<T>(integer);

        public static implicit operator T(Hybrid<T> hybrid) => hybrid.Value;

        public static explicit operator ulong(Hybrid<T> hybrid) => Convert.ToUInt64(hybrid.Value);

        public static explicit operator long(Hybrid<T> hybrid) => hybrid.AbsoluteValue;

        public static explicit operator uint(Hybrid<T> hybrid) => Convert.ToUInt32(hybrid.Value);

        public static explicit operator int(Hybrid<T> hybrid) => Convert.ToInt32(hybrid.AbsoluteValue);

        public static explicit operator ushort(Hybrid<T> hybrid) => Convert.ToUInt16(hybrid.Value);

        public static explicit operator short(Hybrid<T> hybrid) => Convert.ToInt16(hybrid.AbsoluteValue);

        public static explicit operator byte(Hybrid<T> hybrid) => Convert.ToByte(hybrid.Value);

        public static explicit operator sbyte(Hybrid<T> hybrid) => Convert.ToSByte(hybrid.AbsoluteValue);

        public override string ToString() => IsNothing ? default(T) == null ? "Nothing" : default(T).ToString() : IsExternal ? $"<{AbsoluteValue}>" : Value.ToString();
    }
}
