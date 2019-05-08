﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Math;

namespace BenLib.Standard
{
    internal class IntOrdinalValueHelper : OrdinalValueHelper<int>
    {
        public override bool IsInteger => true;
        public override int Compare(int left, int right, out int equalityValue)
        {
            int comp = left.CompareTo(right).Trim(-1, 1);
            equalityValue = comp == 0 ? left : default;
            return comp * (comp < 0 && left + 1 == right || comp > 0 && right + 1 == left ? 1 : 2);
        }
        protected override (int newValue, int newLevel) ComputeLevelCore(int value, int level)
        {
            try
            {
                int newValue = value + level;
                return (newValue, 0);
            }
            catch { return (value, level); }
        }
    }
    internal class UIntOrdinalValueHelper : OrdinalValueHelper<uint>
    {
        public override bool IsInteger => true;
        public override int Compare(uint left, uint right, out uint equalityValue)
        {
            int comp = left.CompareTo(right).Trim(-1, 1);
            equalityValue = comp == 0 ? left : default;
            return comp * (comp < 0 && left + 1 == right || comp > 0 && right + 1 == left ? 1 : 2);
        }
        protected override (uint newValue, int newLevel) ComputeLevelCore(uint value, int level)
        {
            try
            {
                uint newValue = (uint)(value + level);
                return (newValue, 0);
            }
            catch { return (value, level); }
        }
    }
    internal class LongOrdinalValueHelper : OrdinalValueHelper<long>
    {
        public override bool IsInteger => true;
        public override int Compare(long left, long right, out long equalityValue)
        {
            int comp = left.CompareTo(right).Trim(-1, 1);
            equalityValue = comp == 0 ? left : default;
            return comp * (comp < 0 && left + 1 == right || comp > 0 && right + 1 == left ? 1 : 2);
        }
        protected override (long newValue, int newLevel) ComputeLevelCore(long value, int level)
        {
            try
            {
                long newValue = value + level;
                return (newValue, 0);
            }
            catch { return (value, level); }
        }
    }
    internal class ULongOrdinalValueHelper : OrdinalValueHelper<ulong>
    {
        public override bool IsInteger => true;
        public override int Compare(ulong left, ulong right, out ulong equalityValue)
        {
            int comp = left.CompareTo(right).Trim(-1, 1);
            equalityValue = comp == 0 ? left : default;
            return comp * (comp < 0 && left + 1 == right || comp > 0 && right + 1 == left ? 1 : 2);
        }
        protected override (ulong newValue, int newLevel) ComputeLevelCore(ulong value, int level)
        {
            try
            {
                ulong newValue = value + (ulong)level;
                return (newValue, 0);
            }
            catch { return (value, level); }
        }
    }
    internal class ShortOrdinalValueHelper : OrdinalValueHelper<short>
    {
        public override bool IsInteger => true;
        public override int Compare(short left, short right, out short equalityValue)
        {
            int comp = left.CompareTo(right).Trim(-1, 1);
            equalityValue = comp == 0 ? left : default;
            return comp * (comp < 0 && left + 1 == right || comp > 0 && right + 1 == left ? 1 : 2);
        }
        protected override (short newValue, int newLevel) ComputeLevelCore(short value, int level)
        {
            try
            {
                short newValue = (short)(value + level);
                return (newValue, 0);
            }
            catch { return (value, level); }
        }
    }
    internal class UShortOrdinalValueHelper : OrdinalValueHelper<ushort>
    {
        public override bool IsInteger => true;
        public override int Compare(ushort left, ushort right, out ushort equalityValue)
        {
            int comp = left.CompareTo(right).Trim(-1, 1);
            equalityValue = comp == 0 ? left : default;
            return comp * (comp < 0 && left + 1 == right || comp > 0 && right + 1 == left ? 1 : 2);
        }
        protected override (ushort newValue, int newLevel) ComputeLevelCore(ushort value, int level)
        {
            try
            {
                ushort newValue = (ushort)(value + level);
                return (newValue, 0);
            }
            catch { return (value, level); }
        }
    }
    internal class ByteOrdinalValueHelper : OrdinalValueHelper<byte>
    {
        public override bool IsInteger => true;
        public override int Compare(byte left, byte right, out byte equalityValue)
        {
            int comp = left.CompareTo(right).Trim(-1, 1);
            equalityValue = comp == 0 ? left : default;
            return comp * (comp < 0 && left + 1 == right || comp > 0 && right + 1 == left ? 1 : 2);
        }
        protected override (byte newValue, int newLevel) ComputeLevelCore(byte value, int level)
        {
            try
            {
                byte newValue = (byte)(value + level);
                return (newValue, 0);
            }
            catch { return (value, level); }
        }
    }
    internal class SByteOrdinalValueHelper : OrdinalValueHelper<sbyte>
    {
        public override bool IsInteger => true;
        public override int Compare(sbyte left, sbyte right, out sbyte equalityValue)
        {
            int comp = left.CompareTo(right).Trim(-1, 1);
            equalityValue = comp == 0 ? left : default;
            return comp * (comp < 0 && left + 1 == right || comp > 0 && right + 1 == left ? 1 : 2);
        }
        protected override (sbyte newValue, int newLevel) ComputeLevelCore(sbyte value, int level)
        {
            try
            {
                sbyte newValue = (sbyte)(value + level);
                return (newValue, 0);
            }
            catch { return (value, level); }
        }
    }

    public class OrdinalValueHelper<T> where T : IComparable<T>
    {
        private static OrdinalValueHelper<T> m_default;
        public static OrdinalValueHelper<T> Default { get => m_default ?? CreateDefault(); set => m_default = value; }

        public virtual (T newValue, int newLevel) ComputeLevel(T value, int level) => level == 0 ? (value, level) : ComputeLevelCore(value, level);
        protected virtual (T newValue, int newLevel) ComputeLevelCore(T value, int level) => (value, level);
        public virtual int Compare(T left, T right, out T equalityValue)
        {
            int result = 2 * left.CompareTo(right).Trim(-1, 1);
            equalityValue = result == 0 ? left : default;
            return result;
        }

        public virtual T Zero => default;
        public virtual bool IsInteger => false;
        private static OrdinalValueHelper<T> CreateDefault()
        {
            var t = typeof(T);
            RuntimeHelpers.RunClassConstructor(t.TypeHandle);
            return m_default ?? (OrdinalValueHelper<T>)(
                t == typeof(int) ? new IntOrdinalValueHelper() :
                t == typeof(uint) ? new UIntOrdinalValueHelper() :
                t == typeof(long) ? new LongOrdinalValueHelper() :
                t == typeof(ulong) ? new ULongOrdinalValueHelper() :
                t == typeof(short) ? new ShortOrdinalValueHelper() :
                t == typeof(ushort) ? new UShortOrdinalValueHelper() :
                t == typeof(byte) ? new ByteOrdinalValueHelper() :
                t == typeof(sbyte) ? new SByteOrdinalValueHelper() :
                (object)new OrdinalValueHelper<T>());
        }
    }

    public readonly struct Ordinal<T> : IComparable<Ordinal<T>>, IEquatable<Ordinal<T>> where T : IComparable<T>
    {
        private Ordinal(T value, int level, bool isNaN, bool isPositiveInfinity, bool isNegativeInfinity)
        {
            (Value, Level) = (isNaN || isPositiveInfinity || isNegativeInfinity) ? (value, level) : OrdinalValueHelper<T>.Default.ComputeLevel(value, level);
            IsNaN = isNaN;
            IsPositiveInfinity = isPositiveInfinity;
            IsNegativeInfinity = isNegativeInfinity;
        }

        public Ordinal(T value, int level) : this(value, level, false, false, false) { }

        public T Value { get; }
        public int Level { get; }

        public bool IsNaN { get; }
        public bool IsPositiveInfinity { get; }
        public bool IsNegativeInfinity { get; }

        public bool IsReal => !(IsNaN || IsPositiveInfinity || IsNegativeInfinity);

        public Ordinal<T> Antecedent => IsNaN ? NaN : IsPositiveInfinity ? PositiveInfinity : IsNegativeInfinity ? NegativeInfinity : new Ordinal<T>(Value, Level - 1, false, false, false);
        public Ordinal<T> Next => IsNaN ? NaN : IsPositiveInfinity ? PositiveInfinity : IsNegativeInfinity ? NegativeInfinity : new Ordinal<T>(Value, Level + 1, false, false, false);

        int IComparable<Ordinal<T>>.CompareTo(Ordinal<T> other) => CompareTo(other);
        public int CompareTo(in Ordinal<T> other) => CompareTo(other, out _);
        public int CompareTo(in Ordinal<T> other, out T equalityValue)
        {
            equalityValue = default;

            int result = IsNaN || other.IsNaN ? 0 : (IsPositiveInfinity, IsNegativeInfinity, other.IsPositiveInfinity, other.IsNegativeInfinity) switch
            {
                (true, false, true, false) => 0, //ω == ω 
                (false, true, false, true) => 0, //-ω == -ω

                (false, false, true, false) => -2, //x < ω
                (false, true, false, false) => -2, //-ω < x
                (false, true, true, false) => -2, //-ω < ω

                (true, false, false, false) => 2, //ω > x
                (false, false, false, true) => 2, //x > -ω
                (true, false, false, true) => 2, //ω > -ω

                _ => 3
            };

            if (result == 3)
            {
                int comp = OrdinalValueHelper<T>.Default.Compare(Value, other.Value, out equalityValue);
                result = Abs(comp) switch
                {
                    0 => (Level - other.Level).Trim(-2, 2),
                    1 => comp + (Level - other.Level).Trim(-1, 1),
                    _ => comp
                };
            }

            return result;
        }

        public bool IsFarBefore(in Ordinal<T> other) => CompareTo(other) == -2;
        public bool IsFarAfter(in Ordinal<T> other) => CompareTo(other) == 2;
        public bool IsAround(in Ordinal<T> other) => Abs(CompareTo(other)) < 2;

        public Ordinal<TResult> Convert<TResult>() where TResult : IComparable<TResult> => new Ordinal<TResult>(IsReal ? (TResult)(object)Value : default, Level, IsNaN, IsPositiveInfinity, IsNegativeInfinity);
        public Ordinal<TResult> Convert<TResult>(Func<T, TResult> converter) where TResult : IComparable<TResult> => new Ordinal<TResult>(IsReal ? converter(Value) : default, Level, IsNaN, IsPositiveInfinity, IsNegativeInfinity);
        public Ordinal<TResult> Convert<TResult>(Func<T, TResult> valueConverter, Func<T, int> levelConverter) where TResult : IComparable<TResult> => new Ordinal<TResult>(IsReal ? valueConverter(Value) : default, IsReal ? levelConverter(Value) : Level, IsNaN, IsPositiveInfinity, IsNegativeInfinity);

        public static bool operator !=(Ordinal<T> left, Ordinal<T> right) => left.CompareTo(right) != 0;
        public static bool operator ==(Ordinal<T> left, Ordinal<T> right) => left.CompareTo(right) == 0;
        public static bool operator <(Ordinal<T> left, Ordinal<T> right) => left.CompareTo(right) < 0;
        public static bool operator >(Ordinal<T> left, Ordinal<T> right) => left.CompareTo(right) > 0;
        public static bool operator <=(Ordinal<T> left, Ordinal<T> right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Ordinal<T> left, Ordinal<T> right) => left.CompareTo(right) >= 0;

        public static implicit operator Ordinal<T>(T value) => new Ordinal<T>(value, 0);

        public static Ordinal<T> Zero = new Ordinal<T>(OrdinalValueHelper<T>.Default.Zero, 0, false, false, false);
        public static Ordinal<T> NaN = new Ordinal<T>(default, 0, true, false, false);
        public static Ordinal<T> PositiveInfinity = new Ordinal<T>(default, -1, false, true, false);
        public static Ordinal<T> NegativeInfinity = new Ordinal<T>(default, 1, false, false, true);

        public static Ordinal<T> Min(in Ordinal<T> left, in Ordinal<T> right) => left < right ? left : right;
        public static Ordinal<T> Max(in Ordinal<T> left, in Ordinal<T> right) => left > right ? left : right;

        public override string ToString() => ToString(true);
        public string ToString(bool level) => IsNaN ? "NaN" : IsPositiveInfinity ? "+∞" : IsNegativeInfinity ? "-∞" : Value.ToString() + (!level || Level == 0 ? string.Empty : $"₍{(Level > 0 ? "₊" + Level.ToSubscript() : Level.ToSubscript())}₎");

        public override bool Equals(object obj) => obj is Ordinal<T> ordinal && Equals(ordinal);
        public bool Equals(Ordinal<T> other) => EqualityComparer<T>.Default.Equals(Value, other.Value) && Level == other.Level && IsNaN == other.IsNaN && IsPositiveInfinity == other.IsPositiveInfinity && IsNegativeInfinity == other.IsNegativeInfinity;

        public override int GetHashCode()
        {
            int hashCode = 435058893;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + Level.GetHashCode();
            hashCode = hashCode * -1521134295 + IsNaN.GetHashCode();
            hashCode = hashCode * -1521134295 + IsPositiveInfinity.GetHashCode();
            hashCode = hashCode * -1521134295 + IsNegativeInfinity.GetHashCode();
            return hashCode;
        }
    }

    public static class Interval
    {
        public static Range<T> OO<T>(in Ordinal<T> start, in Ordinal<T> end) where T : IComparable<T> => new Range<T>(start.Next, end.Antecedent);
        public static Range<T> OC<T>(in Ordinal<T> start, in Ordinal<T> end) where T : IComparable<T> => new Range<T>(start.Next, end);
        public static Range<T> CO<T>(in Ordinal<T> start, in Ordinal<T> end) where T : IComparable<T> => new Range<T>(start, end.Antecedent);
        public static Range<T> CC<T>(in Ordinal<T> start, in Ordinal<T> end) where T : IComparable<T> => new Range<T>(start, end);

        public static Range<T> Single<T>(in Ordinal<T> value) where T : IComparable<T> => new Range<T>(value, value);

        public static Range<T> OO<T>(in Ordinal<T>? start, in Ordinal<T>? end) where T : IComparable<T> => new Range<T>(start?.Next ?? Ordinal<T>.NegativeInfinity, end?.Antecedent ?? Ordinal<T>.PositiveInfinity);
        public static Range<T> OC<T>(in Ordinal<T>? start, in Ordinal<T>? end) where T : IComparable<T> => new Range<T>(start?.Next ?? Ordinal<T>.NegativeInfinity, end ?? Ordinal<T>.PositiveInfinity);
        public static Range<T> CO<T>(in Ordinal<T>? start, in Ordinal<T>? end) where T : IComparable<T> => new Range<T>(start ?? Ordinal<T>.NegativeInfinity, end?.Antecedent ?? Ordinal<T>.PositiveInfinity);
        public static Range<T> CC<T>(in Ordinal<T>? start, in Ordinal<T>? end) where T : IComparable<T> => new Range<T>(start ?? Ordinal<T>.NegativeInfinity, end ?? Ordinal<T>.PositiveInfinity);

        public static Range<T> EmptySet<T>() where T : IComparable<T> => Interval<T>.EmptySet;
        public static Range<T> NegativeReals<T>() where T : IComparable<T> => Interval<T>.NegativeReals;
        public static Range<T> PositiveReals<T>() where T : IComparable<T> => Interval<T>.PositiveReals;
        public static Range<T> Reals<T>() where T : IComparable<T> => Interval<T>.Reals;
        public static Range<T> NegativeRealsNoZero<T>() where T : IComparable<T> => Interval<T>.NegativeRealsNoZero;
        public static Range<T> PositiveRealsNoZero<T>() where T : IComparable<T> => Interval<T>.PositiveRealsNoZero;
        public static MultiRange<T> RealsNoZero<T>() where T : IComparable<T> => Interval<T>.RealsNoZero;
    }

    public abstract class Interval<T> where T : IComparable<T>
    {
        public abstract bool IsEmpty { get; }
        protected abstract Interval<T> Invert { get; }
        public abstract IEnumerable<Range<T>> Ranges { get; }

        public Range<T> Container => new Range<T>(Ranges.First().m_start, Ranges.Last().m_end);

        protected abstract bool Contains(Ordinal<T> value, bool allowBounds);
        protected abstract bool Contains(Interval<T> value, bool allowBounds);

        protected abstract Interval<T> Union(Interval<T> value);
        protected abstract Interval<T> Inter(Interval<T> value);

        public Interval<TResult> Convert<TResult>() where TResult : IComparable<TResult> => MultiRange<TResult>.Create(Ranges.Select(r => r.Convert<TResult>()));
        public Interval<TResult> Convert<TResult>(Func<T, TResult> converter) where TResult : IComparable<TResult> => MultiRange<TResult>.Create(Ranges.Select(r => r.Convert(converter)));

        public static bool operator <(Interval<T> left, Interval<T> right) => right > left;
        public static bool operator >(Interval<T> left, Interval<T> right) => left.Contains(right, false);
        public static bool operator <=(Interval<T> left, Interval<T> right) => right >= left;
        public static bool operator >=(Interval<T> left, Interval<T> right) => left.Contains(right, true);

        public static bool operator <(Interval<T> left, Ordinal<T> right) => right > left;
        public static bool operator >(Interval<T> left, Ordinal<T> right) => left.Contains(right, false);
        public static bool operator <=(Interval<T> left, Ordinal<T> right) => right >= left;
        public static bool operator >=(Interval<T> left, Ordinal<T> right) => left.Contains(right, true);
        public static bool operator <(Ordinal<T> left, Interval<T> right) => right > left;
        public static bool operator >(Ordinal<T> left, Interval<T> right) => right < left;
        public static bool operator <=(Ordinal<T> left, Interval<T> right) => right >= left;
        public static bool operator >=(Ordinal<T> left, Interval<T> right) => right <= left;

        public static Interval<T> operator |(Interval<T> left, Interval<T> right) => left.Union(right);
        public static Interval<T> operator &(Interval<T> left, Interval<T> right) => left.Inter(right);
        public static Interval<T> operator /(Interval<T> left, Interval<T> right) => left & !right;
        public static Interval<T> operator ^(Interval<T> left, Interval<T> right) => left / right | right / left;
        public static Interval<T> operator !(Interval<T> value) => value.Invert;

        public static Range<T> OO(in Ordinal<T> start, in Ordinal<T> end) => new Range<T>(start.Next, end.Antecedent);
        public static Range<T> OC(in Ordinal<T> start, in Ordinal<T> end) => new Range<T>(start.Next, end);
        public static Range<T> CO(in Ordinal<T> start, in Ordinal<T> end) => new Range<T>(start, end.Antecedent);
        public static Range<T> CC(in Ordinal<T> start, in Ordinal<T> end) => new Range<T>(start, end);

        public static Range<T> Single(in Ordinal<T> value) => new Range<T>(value, value);

        public static Range<T> OO(in Ordinal<T>? start, in Ordinal<T>? end) => new Range<T>(start?.Next ?? Ordinal<T>.NegativeInfinity, end?.Antecedent ?? Ordinal<T>.PositiveInfinity);
        public static Range<T> OC(in Ordinal<T>? start, in Ordinal<T>? end) => new Range<T>(start?.Next ?? Ordinal<T>.NegativeInfinity, end ?? Ordinal<T>.PositiveInfinity);
        public static Range<T> CO(in Ordinal<T>? start, in Ordinal<T>? end) => new Range<T>(start ?? Ordinal<T>.NegativeInfinity, end?.Antecedent ?? Ordinal<T>.PositiveInfinity);
        public static Range<T> CC(in Ordinal<T>? start, in Ordinal<T>? end) => new Range<T>(start ?? Ordinal<T>.NegativeInfinity, end ?? Ordinal<T>.PositiveInfinity);

        public static implicit operator Interval<T>(Ordinal<T> value) => Single(value);
        public static implicit operator Interval<T>((Ordinal<T> start, Ordinal<T> end) ends) => CC(ends.start, ends.end);
        public static implicit operator Interval<T>((Ordinal<T>? start, Ordinal<T>? end) ends) => CC(ends.start, ends.end);

        public bool IsNegativeReals => Equals(NegativeReals);
        public bool IsPositiveReals => Equals(PositiveReals);
        public bool IsReals => Equals(Reals);
        public bool IsNegativeRealsNoZero => Equals(NegativeRealsNoZero);
        public bool IsPositiveRealsNoZero => Equals(PositiveRealsNoZero);
        public bool IsRealsNoZero => Equals(RealsNoZero);

        public static Range<T> EmptySet = new Range<T>(Ordinal<T>.NaN, Ordinal<T>.NaN);
        public static Range<T> NegativeReals = new Range<T>(Ordinal<T>.NegativeInfinity, Ordinal<T>.Zero);
        public static Range<T> PositiveReals = new Range<T>(Ordinal<T>.Zero, Ordinal<T>.PositiveInfinity);
        public static Range<T> Reals = new Range<T>(Ordinal<T>.NegativeInfinity, Ordinal<T>.PositiveInfinity);
        public static Range<T> NegativeRealsNoZero = CO(Ordinal<T>.NegativeInfinity, Ordinal<T>.Zero);
        public static Range<T> PositiveRealsNoZero = OC(Ordinal<T>.Zero, Ordinal<T>.PositiveInfinity);
        public static MultiRange<T> RealsNoZero = (MultiRange<T>)(NegativeRealsNoZero | PositiveRealsNoZero);
    }

    public class Range<T> : Interval<T>, IEquatable<Range<T>> where T : IComparable<T>
    {
        internal readonly Ordinal<T> m_start = Ordinal<T>.NaN;
        internal readonly Ordinal<T> m_end = Ordinal<T>.NaN;

        internal Range(in Ordinal<T> start, in Ordinal<T> end)
        {
            int comp = start.CompareTo(end, out var equalityValue);
            if (comp < 0/* && (comp != 0 || start.IsReal)*/) (m_start, m_end) = (start, end);
            else if (comp == 0 && start.IsReal && end.IsReal) (m_start, m_end) = (equalityValue, equalityValue);
        }

        public Ordinal<T> Start => m_start;
        public Ordinal<T> End => m_end;
        public override IEnumerable<Range<T>> Ranges { get { yield return this; } }
        public bool IsInteger => OrdinalValueHelper<T>.Default.IsInteger;

        public override bool IsEmpty => m_start.IsNaN || m_end.IsNaN;
        public bool IsSingle => m_start == m_end;

        protected override Interval<T> Invert => IsEmpty ? Reals : MultiRange<T>.Create(new[] { OO(Ordinal<T>.NegativeInfinity, m_start), OO(m_end, Ordinal<T>.PositiveInfinity) });

        protected override bool Contains(Ordinal<T> value, bool allowBounds) => !IsEmpty && allowBounds ? m_start.CompareTo(value) <= 0 && value.CompareTo(m_end) <= 0 : m_start.CompareTo(value) < 0 && value.CompareTo(m_end) < 0;
        protected override bool Contains(Interval<T> value, bool allowBounds) => !IsEmpty && value.Ranges.All(range => Contains(range.m_start, allowBounds) && Contains(range.m_end, allowBounds));

        protected override Interval<T> Union(Interval<T> value) => MultiRange<T>.Create(value.Ranges.Prepend(this).ToArray());
        protected override Interval<T> Inter(Interval<T> value) => IsEmpty ? EmptySet : MultiRange<T>.Create(value.Ranges.Select(r => r.IsEmpty ? EmptySet : new Range<T>(Ordinal<T>.Max(m_start, r.m_start), Ordinal<T>.Min(m_end, r.m_end))));

        public new Range<TResult> Convert<TResult>() where TResult : IComparable<TResult> => new Range<TResult>(m_start.Convert<TResult>(), m_end.Convert<TResult>());
        public new Range<TResult> Convert<TResult>(Func<T, TResult> converter) where TResult : IComparable<TResult> => new Range<TResult>(m_start.Convert(converter), m_end.Convert(converter));

        public char LeftBracket => IsInteger ? m_start.Level > 0 ? '⟧' : '⟦' : m_start.Level > 0 ? ']' : '[';
        public char RightBracket => IsInteger ? m_end.Level < 0 ? '⟦' : '⟧' : m_end.Level < 0 ? '[' : ']';

        public override string ToString() =>
            IsEmpty ? "∅" :
            this == Reals ? IsInteger ? "ℤ" : "ℝ" :
            this == NegativeReals ? IsInteger ? "ℤ₋" : "ℝ₋" :
            this == PositiveReals ? IsInteger ? "ℕ" : "ℝ₊" :
            this == NegativeRealsNoZero ? IsInteger ? "ℤ₋*" : "ℝ₋*" :
            this == PositiveRealsNoZero ? IsInteger ? "ℕ*" : "ℝ₊*" :
            IsSingle ? "{" + m_start + "}" :
            $"{LeftBracket}{(m_start.Level > 0 ? m_start.Antecedent : m_start)} ; {(m_end.Level < 0 ? m_end.Next : m_end)}{RightBracket}";

        public override bool Equals(object obj) => Equals(obj as Range<T>);
        public bool Equals(Range<T> other) => other != null && (IsEmpty && other.IsEmpty || m_start.Equals(other.m_start) && m_end.Equals(other.m_end));

        public override int GetHashCode()
        {
            int hashCode = -1676728671;
            hashCode = hashCode * -1521134295 + EqualityComparer<Ordinal<T>>.Default.GetHashCode(m_start);
            hashCode = hashCode * -1521134295 + EqualityComparer<Ordinal<T>>.Default.GetHashCode(m_end);
            return hashCode;
        }

        public static implicit operator Range<T>((Ordinal<T> start, Ordinal<T> end) ends) => CC(ends.start, ends.end);
        public static implicit operator Range<T>((Ordinal<T>? start, Ordinal<T>? end) ends) => CC(ends.start, ends.end);

        public static bool operator ==(Range<T> left, Range<T> right) => EqualityComparer<Range<T>>.Default.Equals(left, right);
        public static bool operator !=(Range<T> left, Range<T> right) => !(left == right);
    }

    public class MultiRange<T> : Interval<T>, IEquatable<MultiRange<T>> where T : IComparable<T>
    {
        private readonly Range<T>[] m_ranges;

        private MultiRange(params Range<T>[] orderedRanges) => m_ranges = orderedRanges;

        public override IEnumerable<Range<T>> Ranges => m_ranges;
        public override bool IsEmpty => false;

        protected override Interval<T> Invert => m_ranges.Select(r => !r).Inter();

        protected override bool Contains(Ordinal<T> value, bool allowBounds) => m_ranges.Any(r => allowBounds ? r >= value : r > value);
        protected override bool Contains(Interval<T> value, bool allowBounds) => value.Ranges.All(ra => m_ranges.Any(r => allowBounds ? r >= ra : r > ra));

        protected override Interval<T> Inter(Interval<T> value) => Create(m_ranges.SelectMany(r => (value & r).Ranges).ToArray());
        protected override Interval<T> Union(Interval<T> value) => Create(value.Ranges.Concat(m_ranges).ToArray());

        internal static Interval<T> Create(IEnumerable<Range<T>> ranges)
        {
            var rs = MergeRanges(ranges).ToArray();
            return rs.Length switch
            {
                0 => (Interval<T>)EmptySet,
                1 => rs[0],
                _ => new MultiRange<T>(rs)
            };

            static IEnumerable<Range<T>> MergeRanges(IEnumerable<Range<T>> ranges)
            {
                var t = EmptySet;
                foreach (var range in ranges.Where(r => !r.IsEmpty).OrderBy(r => r.m_start))
                {
                    if (t.IsEmpty) t = new Range<T>(range.m_start, range.m_end);
                    else if (range.m_start.CompareTo(t.m_end) != 2) t = new Range<T>(t.m_start, Ordinal<T>.Max(t.m_end, range.m_end));
                    else
                    {
                        yield return t;
                        t = new Range<T>(range.m_start, range.m_end);
                    }
                }
                if (!t.IsEmpty) yield return t;
            }
        }

        public override string ToString() =>
            this == RealsNoZero ? "ℝ*" :
            "{" + string.Join(" ; ", m_ranges.Where(r => r.IsSingle).Select(r => r.m_start)) + "} ∪ " + string.Join(" ∪ ", m_ranges.Where(r => !r.IsSingle));

        public override bool Equals(object obj) => Equals(obj as MultiRange<T>);
        public bool Equals(MultiRange<T> other) => other != null && (m_ranges == other.m_ranges || m_ranges.SequenceEqual(other.m_ranges));
        public override int GetHashCode() => -1877571317 + EqualityComparer<Range<T>[]>.Default.GetHashCode(m_ranges);

        public static bool operator ==(MultiRange<T> left, MultiRange<T> right) => EqualityComparer<MultiRange<T>>.Default.Equals(left, right);
        public static bool operator !=(MultiRange<T> left, MultiRange<T> right) => !(left == right);
    }

    public static partial class Extensions
    {
        public static Interval<T> Union<T>(this IEnumerable<Interval<T>> source) where T : IComparable<T>
        {
            Interval<T> result = Range<T>.EmptySet;
            foreach (var interval in source) result |= interval;
            return result;
        }
        public static Interval<T> Union<TSource, T>(this IEnumerable<TSource> source, Func<TSource, Interval<T>> selector) where T : IComparable<T>
        {
            Interval<T> result = Range<T>.EmptySet;
            foreach (var interval in source) result |= selector(interval);
            return result;
        }

        public static Interval<T> Inter<T>(this IEnumerable<Interval<T>> source) where T : IComparable<T>
        {
            Interval<T> result = Range<T>.Reals;
            foreach (var interval in source) result &= interval;
            return result;
        }
        public static Interval<T> Inter<TSource, T>(this IEnumerable<TSource> source, Func<TSource, Interval<T>> selector) where T : IComparable<T>
        {
            Interval<T> result = Range<T>.Reals;
            foreach (var interval in source) result &= selector(interval);
            return result;
        }

        public static int? Length(this Range<int> range) => range.m_start.IsNegativeInfinity || range.m_end.IsPositiveInfinity ? null : (int?)(range.m_end.Value - range.m_start.Value + 1);
        public static long? Length(this Range<long> range) => range.m_start.IsNegativeInfinity || range.m_end.IsPositiveInfinity ? null : (long?)(range.m_end.Value - range.m_start.Value + 1);
        public static uint? Length(this Range<uint> range) => range.m_start.IsNegativeInfinity || range.m_end.IsPositiveInfinity ? null : (uint?)(range.m_end.Value - range.m_start.Value + 1);
        public static ulong? Length(this Range<ulong> range) => range.m_start.IsNegativeInfinity || range.m_end.IsPositiveInfinity ? null : (ulong?)(range.m_end.Value - range.m_start.Value + 1);
        public static short? Length(this Range<short> range) => range.m_start.IsNegativeInfinity || range.m_end.IsPositiveInfinity ? null : (short?)(range.m_end.Value - range.m_start.Value + 1);
        public static ushort? Length(this Range<ushort> range) => range.m_start.IsNegativeInfinity || range.m_end.IsPositiveInfinity ? null : (ushort?)(range.m_end.Value - range.m_start.Value + 1);
        public static byte? Length(this Range<byte> range) => range.m_start.IsNegativeInfinity || range.m_end.IsPositiveInfinity ? null : (byte?)(range.m_end.Value - range.m_start.Value + 1);
        public static sbyte? Length(this Range<sbyte> range) => range.m_start.IsNegativeInfinity || range.m_end.IsPositiveInfinity ? null : (sbyte?)(range.m_end.Value - range.m_start.Value + 1);

        public static IEnumerable<int> Numbers(this Range<int> range, int start = int.MinValue, int end = int.MaxValue)
        {
            if (range.IsEmpty) yield break;
            int endValue = range.End.IsReal ? range.End.Value : end;
            for (int i = range.Start.IsReal ? range.Start.Value : start; i <= endValue; i++) yield return i;
        }
        public static IEnumerable<uint> Numbers(this Range<uint> range, uint start = uint.MinValue, uint end = uint.MaxValue)
        {
            if (range.IsEmpty) yield break;
            uint endValue = range.End.IsReal ? range.End.Value : end;
            for (uint i = range.Start.IsReal ? range.Start.Value : start; i <= endValue; i++) yield return i;
        }
        public static IEnumerable<long> Numbers(this Range<long> range, long start = long.MinValue, long end = long.MaxValue)
        {
            if (range.IsEmpty) yield break;
            long endValue = range.End.IsReal ? range.End.Value : end;
            for (long i = range.Start.IsReal ? range.Start.Value : start; i <= endValue; i++) yield return i;
        }
        public static IEnumerable<ulong> Numbers(this Range<ulong> range, ulong start = ulong.MinValue, ulong end = ulong.MaxValue)
        {
            if (range.IsEmpty) yield break;
            ulong endValue = range.End.IsReal ? range.End.Value : end;
            for (ulong i = range.Start.IsReal ? range.Start.Value : start; i <= endValue; i++) yield return i;
        }
        public static IEnumerable<short> Numbers(this Range<short> range, short start = short.MinValue, short end = short.MaxValue)
        {
            if (range.IsEmpty) yield break;
            short endValue = range.End.IsReal ? range.End.Value : end;
            for (short i = range.Start.IsReal ? range.Start.Value : start; i <= endValue; i++) yield return i;
        }
        public static IEnumerable<ushort> Numbers(this Range<ushort> range, ushort start = ushort.MinValue, ushort end = ushort.MaxValue)
        {
            if (range.IsEmpty) yield break;
            ushort endValue = range.End.IsReal ? range.End.Value : end;
            for (ushort i = range.Start.IsReal ? range.Start.Value : start; i <= endValue; i++) yield return i;
        }
        public static IEnumerable<byte> Numbers(this Range<byte> range, byte start = byte.MinValue, byte end = byte.MaxValue)
        {
            if (range.IsEmpty) yield break;
            byte endValue = range.End.IsReal ? range.End.Value : end;
            for (byte i = range.Start.IsReal ? range.Start.Value : start; i <= endValue; i++) yield return i;
        }
        public static IEnumerable<sbyte> Numbers(this Range<sbyte> range, sbyte start = sbyte.MinValue, sbyte end = sbyte.MaxValue)
        {
            if (range.IsEmpty) yield break;
            sbyte endValue = range.End.IsReal ? range.End.Value : end;
            for (sbyte i = range.Start.IsReal ? range.Start.Value : start; i <= endValue; i++) yield return i;
        }
        public static IEnumerable<float> Numbers(this Range<float> range, float step, float start = float.MinValue, float end = float.MaxValue)
        {
            if (range.IsEmpty) yield break;
            float endValue = range.End.IsReal ? range.End.Value - range.End.Level < 0 ? step : 0 : end;
            for (float i = range.Start.IsReal ? range.Start.Value + range.Start.Level > 0 ? step : 0 : start; i <= endValue; i += step) yield return i;
        }
        public static IEnumerable<double> Numbers(this Range<double> range, double step, double start = double.MinValue, double end = double.MaxValue)
        {
            if (range.IsEmpty) yield break;
            double endValue = range.End.IsReal ? range.End.Value - range.End.Level < 0 ? step : 0 : end;
            for (double i = range.Start.IsReal ? range.Start.Value + range.Start.Level > 0 ? step : 0 : start; i <= endValue; i += step) yield return i;
        }
        public static IEnumerable<decimal> Numbers(this Range<decimal> range, decimal step, decimal start = decimal.MinValue, decimal end = decimal.MaxValue)
        {
            if (range.IsEmpty) yield break;
            decimal endValue = range.End.IsReal ? range.End.Value - range.End.Level < 0 ? step : 0 : end;
            for (decimal i = range.Start.IsReal ? range.Start.Value + range.Start.Level > 0 ? step : 0 : start; i <= endValue; i += step) yield return i;
        }

        public static IEnumerable<IEnumerable<int>> RangesNumbers(this Interval<int> interval, int start = int.MinValue, int end = int.MaxValue) => interval.Ranges.Select(r => r.Numbers(start, end));
        public static IEnumerable<IEnumerable<uint>> RangesNumbers(this Interval<uint> interval, uint start = uint.MinValue, uint end = uint.MaxValue) => interval.Ranges.Select(r => r.Numbers(start, end));
        public static IEnumerable<IEnumerable<long>> RangesNumbers(this Interval<long> interval, long start = long.MinValue, long end = long.MaxValue) => interval.Ranges.Select(r => r.Numbers(start, end));
        public static IEnumerable<IEnumerable<ulong>> RangesNumbers(this Interval<ulong> interval, ulong start = ulong.MinValue, ulong end = ulong.MaxValue) => interval.Ranges.Select(r => r.Numbers(start, end));
        public static IEnumerable<IEnumerable<short>> RangesNumbers(this Interval<short> interval, short start = short.MinValue, short end = short.MaxValue) => interval.Ranges.Select(r => r.Numbers(start, end));
        public static IEnumerable<IEnumerable<ushort>> RangesNumbers(this Interval<ushort> interval, ushort start = ushort.MinValue, ushort end = ushort.MaxValue) => interval.Ranges.Select(r => r.Numbers(start, end));
        public static IEnumerable<IEnumerable<byte>> RangesNumbers(this Interval<byte> interval, byte start = byte.MinValue, byte end = byte.MaxValue) => interval.Ranges.Select(r => r.Numbers(start, end));
        public static IEnumerable<IEnumerable<sbyte>> RangesNumbers(this Interval<sbyte> interval, sbyte start = sbyte.MinValue, sbyte end = sbyte.MaxValue) => interval.Ranges.Select(r => r.Numbers(start, end));
        public static IEnumerable<IEnumerable<float>> RangesNumbers(this Interval<float> interval, float step, float start = float.MinValue, float end = float.MaxValue) => interval.Ranges.Select(r => r.Numbers(step, start, end));
        public static IEnumerable<IEnumerable<double>> RangesNumbers(this Interval<double> interval, double step, double start = double.MinValue, double end = double.MaxValue) => interval.Ranges.Select(r => r.Numbers(step, start, end));
        public static IEnumerable<IEnumerable<decimal>> RangesNumbers(this Interval<decimal> interval, decimal step, decimal start = decimal.MinValue, decimal end = decimal.MaxValue) => interval.Ranges.Select(r => r.Numbers(step, start, end));

        public static IEnumerable<int> Numbers(this Interval<int> interval, int start = int.MinValue, int end = int.MaxValue) => interval.RangesNumbers(start, end).SelectMany(n => n);
        public static IEnumerable<uint> Numbers(this Interval<uint> interval, uint start = uint.MinValue, uint end = uint.MaxValue) => interval.RangesNumbers(start, end).SelectMany(n => n);
        public static IEnumerable<long> Numbers(this Interval<long> interval, long start = long.MinValue, long end = long.MaxValue) => interval.RangesNumbers(start, end).SelectMany(n => n);
        public static IEnumerable<ulong> Numbers(this Interval<ulong> interval, ulong start = ulong.MinValue, ulong end = ulong.MaxValue) => interval.RangesNumbers(start, end).SelectMany(n => n);
        public static IEnumerable<short> Numbers(this Interval<short> interval, short start = short.MinValue, short end = short.MaxValue) => interval.RangesNumbers(start, end).SelectMany(n => n);
        public static IEnumerable<ushort> Numbers(this Interval<ushort> interval, ushort start = ushort.MinValue, ushort end = ushort.MaxValue) => interval.RangesNumbers(start, end).SelectMany(n => n);
        public static IEnumerable<byte> Numbers(this Interval<byte> interval, byte start = byte.MinValue, byte end = byte.MaxValue) => interval.RangesNumbers(start, end).SelectMany(n => n);
        public static IEnumerable<sbyte> Numbers(this Interval<sbyte> interval, sbyte start = sbyte.MinValue, sbyte end = sbyte.MaxValue) => interval.RangesNumbers(start, end).SelectMany(n => n);
        public static IEnumerable<float> Numbers(this Interval<float> interval, float step, float start = float.MinValue, float end = float.MaxValue) => interval.RangesNumbers(step, start, end).SelectMany(n => n);
        public static IEnumerable<double> Numbers(this Interval<double> interval, double step, double start = double.MinValue, double end = double.MaxValue) => interval.RangesNumbers(step, start, end).SelectMany(n => n);
        public static IEnumerable<decimal> Numbers(this Interval<decimal> interval, decimal step, decimal start = decimal.MinValue, decimal end = decimal.MaxValue) => interval.RangesNumbers(step, start, end).SelectMany(n => n);
    }
}
